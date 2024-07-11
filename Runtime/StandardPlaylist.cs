
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class StandardPlaylist : Playlist
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

        public override string PlaylistName => _playlistName;
        public override int Length => _urls.Length;

        public override Track GetTrack(int index)
        {
            if (index > Length) return null;
            if (_titles[index] == string.Empty && _urls[index].Get() != string.Empty && _videoInfo != null)
            {
                string title = _videoInfo.GetVideoInfo(_urls[index]);
                if (title != string.Empty) _titles[index] = title;
            }
            return Track.New(_videoPlayerTypes[index], _titles[index], _urls[index], _originalUrls[index]);
        }

        public override void AddTrack(Track track)
        {
            string title = track.GetTitle();
            if (track.GetTitle() == string.Empty && track.GetVRCUrl().Get() != string.Empty && _videoInfo != null)
                title = _videoInfo.GetVideoInfo(track.GetVRCUrl());
            _videoPlayerTypes = _videoPlayerTypes.Add(track.GetPlayer());
            _titles = _titles.Add(title);
            _urls = _urls.Add(track.GetVRCUrl());
            _originalUrls = _originalUrls.Add(track.GetOriginalUrl());
            SendEvent();
        }

        public override void Remove(int index)
        {
            if (index < 0 || index > Length - 1) return;
            _videoPlayerTypes = _videoPlayerTypes.Remove(index);
            _titles = _titles.Remove(index);
            _originalUrls = _originalUrls.Remove(index);
            _urls = _urls.Remove(index);
            SendEvent();
        }

        public void MoveUp(int index)
        {
            if (index < 1 || index > Length - 1) return;
            Track track = GetTrack(index - 1);
            _videoPlayerTypes[index - 1] = _videoPlayerTypes[index];
            _titles[index - 1] = _titles[index];
            _urls[index - 1] = _urls[index];
            _originalUrls[index - 1] = _originalUrls[index];
            _videoPlayerTypes[index] = track.GetPlayer();
            _titles[index] = track.GetTitle();
            _urls[index] = track.GetVRCUrl();
            _originalUrls[index] = track.GetOriginalUrl();
            SendEvent();
        }

        public void MoveDown(int index)
        {
            if (index < 0 || index > Length - 2) return;
            Track track = GetTrack(index + 1);
            _videoPlayerTypes[index + 1] = _videoPlayerTypes[index];
            _titles[index + 1] = _titles[index];
            _urls[index + 1] = _urls[index];
            _originalUrls[index + 1] = _originalUrls[index];
            _videoPlayerTypes[index] = track.GetPlayer();
            _titles[index] = track.GetTitle();
            _urls[index] = track.GetVRCUrl();
            _originalUrls[index] = track.GetOriginalUrl();
            SendEvent();
        }

        public void SendEvent()
        {
            if (Networking.IsOwner(_controller.gameObject) && !_controller.IsLocal) this.SyncVariables();
            if (_isQueue) _controller.SendCustomVideoEvent(nameof(Listener.OnQueueUpdated));
            if (_isHistory) _controller.SendCustomVideoEvent(nameof(Listener.OnHistoryUpdated));
        }

        public override void OnDeserialization()
        {
            SendEvent();
        }
    }
}