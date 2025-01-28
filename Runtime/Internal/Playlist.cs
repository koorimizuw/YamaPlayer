using UdonSharp;
using UnityEngine;
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
        #region Udon Sync Variables
        [SerializeField, UdonSynced] VideoPlayerType[] _videoPlayerTypes = new VideoPlayerType[] { };
        [SerializeField, UdonSynced] string[] _titles = new string[] { };
        [SerializeField, UdonSynced] VRCUrl[] _urls = new VRCUrl[] { };
        [SerializeField, UdonSynced] string[] _originalUrls = new string[] { };
        #endregion
        DataList<Track> _tracks;
        bool _isLoading;
        bool _loaded;

        private void Start() => GenerateTracks();

        void GenerateTracks()
        {
            _tracks = DataList<Track>.New();
            for (int i = 0; i < _urls.Length; i++)
            {
                Track track = Track.New(_videoPlayerTypes[i], _titles[i], _urls[i], _originalUrls[i]);
                _tracks.Add(track);
            }
        }

        public DataList<Track> Tracks
        {
            get
            {
                if (!Utilities.IsValid(_tracks)) GenerateTracks();
                return _tracks;
            }
        }

        public string PlaylistName => _playlistName;

        public int Length => Tracks.Count();

        public bool IsLoading => _isLoading;

        public bool Loaded => _loaded;

        public Track GetTrack(int index)
        {
            if (index >= Tracks.Count()) return Track.Empty();
            return Tracks.GetValue(index);
        }

        public void AddTrack(Track track)
        {
            string title = track.GetTitle();
            if (string.IsNullOrEmpty(title) && _videoInfo != null) 
                _videoInfo.GetVideoInfo(track.GetVRCUrl());
            Tracks.Add(track);
            SendEvent();
        }

        public void RemoveTrack(int index)
        {
            if (index < 0 || index > Length - 1) return;
            Tracks.RemoveAt(index);
            SendEvent();
        }

        public void MoveUp(int index)
        {
            if (index < 1 || index > Length - 1) return;
            Track track = Tracks.GetValue(index);
            Tracks.RemoveAt(index);
            Tracks.Insert(index - 1, track);
            SendEvent();
        }

        public void MoveDown(int index)
        {
            if (index < 0 || index > Length - 2) return;
            Track track = Tracks.GetValue(index);
            Tracks.RemoveAt(index);
            Tracks.Insert(index + 1, track);
            SendEvent();
        }

        public void LoadPlaylist(VRCUrl url)
        {
            _isLoading = true;
            VRCStringDownloader.LoadUrl(url, (IUdonEventReceiver)this);
            _controller.SendCustomVideoEvent(nameof(Listener.OnPlaylistsUpdated));
        }

        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            _isLoading = false;
            YouTubePlaylist pl = YouTube.GetPlaylistFromHtml(result.Result);
            _playlistName = pl.GetName();
            _tracks = pl.GetTracks();
            _loaded = true;
            _controller.SendCustomVideoEvent(nameof(Listener.OnPlaylistsUpdated));
        }

        public override void OnStringLoadError(IVRCStringDownload result)
        {
            _isLoading = false;
            _controller.SendCustomVideoEvent(nameof(Listener.OnPlaylistsUpdated));
        }

        public override void OnPreSerialization()
        {
            base.OnPreSerialization();
            var videoPlayerTypes = DataList<int>.New();
            var titles = DataList<string>.New();
            var urls = DataList<VRCUrl>.New();
            var originalUrls = DataList<string>.New();
            for (int i = 0; i < Tracks.Count(); i++)
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
            base.OnPreSerialization();
            GenerateTracks();
            SendEvent();
        }

        public void SendEvent()
        {
            if (_isObjectOwner && !_controller.IsLocal) SyncVariables();
            if (_isQueue) _controller.SendCustomVideoEvent(nameof(Listener.OnQueueUpdated));
            if (_isHistory) _controller.SendCustomVideoEvent(nameof(Listener.OnHistoryUpdated));
        }
    }
}