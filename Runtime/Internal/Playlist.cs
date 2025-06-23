using UdonSharp;
using UnityEngine;
using UnityEngine.Playables;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using Yamadev.YamaStream.Libraries.GenericDataContainer;

namespace Yamadev.YamaStream
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Playlist : Listener
    {
        [SerializeField] Controller _controller;
        [SerializeField] VideoInfo _videoInfo;
        [SerializeField] bool _isQueue = false;
        [SerializeField] bool _isHistory = false;
        [SerializeField] string _playlistName;
        [SerializeField, UdonSynced] VideoPlayerType[] _videoPlayerTypes = new VideoPlayerType[] { };
        [SerializeField, UdonSynced] string[] _titles = new string[] { };
        [SerializeField, UdonSynced] VRCUrl[] _urls = new VRCUrl[] { };
        [SerializeField, UdonSynced] string[] _originalUrls = new string[] { };
        [SerializeField] PlayableDirector[] _timelines = new PlayableDirector[] { };
        DataList<Track> _tracks;
        string _playlistUrl;
        bool _initialized;
        bool _isLoading;
        bool _loaded;

        private void Start()
        {
            if (!_initialized)
            {
                GenerateTracks();
                _initialized = true;
            }
        }

        private PlayableDirector GetTimelineAtIndex(int index)
        {
            return (index < _timelines.Length) ? _timelines[index] : null;
        }

        private void GenerateTracks()
        {
            if (!Utilities.IsValid(_tracks))
            {
                _tracks = DataList<Track>.New();
            }
            else
            {
                _tracks.Clear();
            }

            int trackCount = _urls.Length;
            for (int i = 0; i < trackCount; i++)
            {
                var timeline = GetTimelineAtIndex(i);
                Track track = Track.New(_videoPlayerTypes[i], _titles[i], _urls[i], _originalUrls[i], timeline);
                _tracks.Add(track);
            }
        }

        public DataList<Track> Tracks
        {
            get
            {
                if (!_initialized)
                {
                    GenerateTracks();
                    _initialized = true;
                }
                return _tracks;
            }
        }

        public string PlaylistName => _playlistName;

        public int Length => Tracks.Count();

        public bool IsLoading => _isLoading;

        public bool Loaded => _loaded;

        private bool IsValidIndex(int index)
        {
            return index >= 0 && index < Length;
        }

        public Track GetTrack(int index)
        {
            if (!IsValidIndex(index)) return Track.Empty();
            return Tracks.GetValue(index);
        }

        public void AddTrack(Track track)
        {
            if (!Utilities.IsValid(track)) return;
            if (string.IsNullOrEmpty(track.GetTitle()) && Utilities.IsValid(_videoInfo))
            {
                _videoInfo.GetVideoInfo(track.GetVRCUrl());
            }
            Tracks.Add(track);
            SendEvent();
        }

        public void RemoveTrack(int index)
        {
            if (!IsValidIndex(index)) return;
            Tracks.RemoveAt(index);
            SendEvent();
        }

        public void MoveUp(int index)
        {
            if (!IsValidIndex(index) || index == 0) return;
            Track track = Tracks.GetValue(index);
            Tracks.RemoveAt(index);
            Tracks.Insert(index - 1, track);
            SendEvent();
        }

        public void MoveDown(int index)
        {
            if (!IsValidIndex(index) || index >= Length - 1) return;
            Track track = Tracks.GetValue(index);
            Tracks.RemoveAt(index);
            Tracks.Insert(index + 1, track);
            SendEvent();
        }

        public string PlaylistUrl => _playlistUrl;

        public void LoadPlaylist(VRCUrl url)
        {
            _isLoading = true;
            _playlistUrl = url.Get();
            VRCStringDownloader.LoadUrl(url, (IUdonEventReceiver)this);
            _controller.SendCustomVideoEvent(nameof(Listener.OnPlaylistsUpdated));
        }

        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            _isLoading = false;

            if (string.IsNullOrEmpty(result.Result))
            {
                Debug.LogError($"Playlist {result.Url.Get()} load failed: {result.Result}");
                OnPlaylistLoadFailed();
                return;
            }

            YouTubePlaylist pl = YouTube.GetPlaylistFromHtml(result.Result);
            if (!Utilities.IsValid(pl) || !Utilities.IsValid(pl.GetTracks()))
            {
                Debug.LogError($"Failed to parse playlist {result.Url.Get()}");
                OnPlaylistLoadFailed();
                return;
            }

            _playlistName = pl.GetName();
            _tracks = pl.GetTracks();
            _initialized = true;
            _loaded = true;
            _controller.SendCustomVideoEvent(nameof(Listener.OnPlaylistsUpdated));
        }

        public override void OnStringLoadError(IVRCStringDownload result)
        {
            _isLoading = false;
            OnPlaylistLoadFailed();
        }

        private void OnPlaylistLoadFailed()
        {
            _loaded = false;
            _controller.SendCustomVideoEvent(nameof(Listener.OnPlaylistsUpdated));
        }

        public override void OnPreSerialization()
        {
            var videoPlayerTypes = DataList<int>.New();
            var titles = DataList<string>.New();
            var urls = DataList<VRCUrl>.New();
            var originalUrls = DataList<string>.New();

            int trackCount = Tracks.Count();
            for (int i = 0; i < trackCount; i++)
            {
                Track track = Tracks.GetValue(i);
                videoPlayerTypes.Add((int)track.GetPlayerType());
                titles.Add(track.GetTitle());
                urls.Add(track.GetVRCUrl());
                originalUrls.Add(track.GetOriginalUrl());
            }

            // It throws error when we convert a enum list to array.
            _videoPlayerTypes = (VideoPlayerType[])(object)videoPlayerTypes.ToArray();
            _titles = titles.ToArray();
            _urls = urls.ToArray();
            _originalUrls = originalUrls.ToArray();
        }

        public override void OnDeserialization()
        {
            GenerateTracks();
            SendEvent();
        }

        private void SendEvent()
        {
            if (_isObjectOwner && !_controller.IsLocal) SyncVariables();
            if (_isQueue) _controller.SendCustomVideoEvent(nameof(Listener.OnQueueUpdated));
            if (_isHistory) _controller.SendCustomVideoEvent(nameof(Listener.OnHistoryUpdated));
        }
    }
}