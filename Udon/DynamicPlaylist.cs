
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace Yamadev.YamaStream
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class DynamicPlaylist : Playlist
    {
        [SerializeField] Controller _controller;
        [UdonSynced] VRCUrl _playlistLink;
        string _playlistName;
        VideoPlayerType[] _videoPlayerTypes = new VideoPlayerType[] { };
        string[] _titles = new string[] { };
        VRCUrl[] _urls = new VRCUrl[] { };
        string[] _originalUrls = new string[] { };
        bool _isLoading;
        bool _loaded;

        public override string PlaylistName => _playlistName;
        public override int Length => _urls.Length;

        public override bool IsLoading => _isLoading;
        public override bool Loaded => _loaded;

        public override Track GetTrack(int index)
        {
            if (index > Length) return null;
            return Track.New(_videoPlayerTypes[index], _titles[index], _urls[index], _originalUrls[index]);
        }

        public void Load(VRCUrl url)
        {
            _playlistLink = url;
            LoadPlaylist();
            this.SyncVariables();
        }

        public void LoadPlaylist()
        {
            if (_playlistLink == null) return;
            _isLoading = true;
            _controller.UpdatePlaylists();
            VRCStringDownloader.LoadUrl(_playlistLink, (IUdonEventReceiver)this);
        }

        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            int index = result.Result.IndexOf("{\"playlist\":");
            if (index >= 0)
            {
                string playlistString = Utils.FindPairBrackets(result.Result, index);
                if (playlistString != string.Empty && VRCJson.TryDeserializeFromJson(playlistString, out var json))
                {
                    DataDictionary dict = json.DataDictionary["playlist"].DataDictionary;
                    _playlistName = dict["title"].String;
                    DataList contents = dict["contents"].DataList;
                    for (int i = 0; i < contents.Count; i++)
                    {
                        if (contents[i].DataDictionary.TryGetValue("playlistPanelVideoRenderer", out var renderer))
                        {
                            bool isLive = renderer.DataDictionary.TryGetValue("badges", out var badges) &&
                                badges.DataList.TryGetValue(0, out var badge) &&
                                badge.DataDictionary["metadataBadgeRenderer"].DataDictionary["icon"].DataDictionary["iconType"].String == "LIVE";
                            _videoPlayerTypes = _videoPlayerTypes.Add(VideoPlayerType.AVProVideoPlayer);
                            _titles = _titles.Add(renderer.DataDictionary["title"].DataDictionary["simpleText"].String);
                            _urls = _urls.Add(VRCUrl.Empty);
                            _originalUrls = _originalUrls.Add($"https://www.youtube.com/watch?v={renderer.DataDictionary["videoId"].String}");
                        }
                    }
                    _isLoading = false;
                    _loaded = true;
                    _controller.SendCustomEventDelayedSeconds(nameof(_controller.UpdatePlaylists), 0.1f);
                }
            } else
            {
                int playlistIndex = result.Result.IndexOf("{\"playlistVideoListRenderer\":");
                if (playlistIndex >= 0)
                {
                    int headerIndex = result.Result.IndexOf("{\"playlistHeaderRenderer\":");
                    string headerString = Utils.FindPairBrackets(result.Result, headerIndex);
                    if (headerString != string.Empty && VRCJson.TryDeserializeFromJson(headerString, out var headerJson))
                        _playlistName = headerJson.DataDictionary["playlistHeaderRenderer"].DataDictionary["title"].DataDictionary["simpleText"].String;
                    string playlistString = Utils.FindPairBrackets(result.Result, playlistIndex);
                    if (playlistString != string.Empty && VRCJson.TryDeserializeFromJson(playlistString, out var playlistJson))
                    {
                        DataList contents = playlistJson.DataDictionary["playlistVideoListRenderer"].DataDictionary["contents"].DataList;
                        for (int i = 0; i < contents.Count; i++)
                        {
                            DataDictionary renderer = contents[i].DataDictionary["playlistVideoRenderer"].DataDictionary;
                            //VRCUrl.TryCreateAllowlistedVRCUrl($"https://www.youtube.com/watch?v={renderer["videoId"].String}", out VRCUrl outputUrl);
                            bool isLive = renderer["thumbnailOverlays"].DataList[0].DataDictionary.TryGetValue("thumbnailOverlayTimeStatusRenderer", out var thu) &&
                                thu.DataDictionary["style"] == "LIVE";
                            _videoPlayerTypes = _videoPlayerTypes.Add(VideoPlayerType.AVProVideoPlayer);
                            _titles = _titles.Add(renderer["title"].DataDictionary["runs"].DataList[0].DataDictionary["text"].String);
                            _urls = _urls.Add(VRCUrl.Empty);
                            _originalUrls = _originalUrls.Add($"https://www.youtube.com/watch?v={renderer["videoId"].String}");
                        }
                        _isLoading = false;
                        _loaded = true;
                        _controller.SendCustomEventDelayedSeconds(nameof(_controller.UpdatePlaylists), 0.1f);
                    }
                }
            }
        }

        public override void OnStringLoadError(IVRCStringDownload result)
        {
            _isLoading = false;
            _controller.UpdatePlaylists();
        }

        public override void OnDeserialization() => LoadPlaylist();
    }
}