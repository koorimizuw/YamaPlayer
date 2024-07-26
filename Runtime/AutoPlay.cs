
using UnityEngine;
using UdonSharp;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AutoPlay : UdonSharpBehaviour
    {
        [SerializeField] Controller _controller;
        [SerializeField] AutoPlayMode _autoPlayMode = AutoPlayMode.Off;
        [SerializeField, Range(0, 60)] float _delay;
        [SerializeField] VideoPlayerType _videoPlayerType;
        [SerializeField] string _title;
        [SerializeField] VRCUrl _url;
        [SerializeField] int _playlistIndex = 0;
        [SerializeField] int _playlistTrackIndex = 0;

        void Start()
        {
            if (!Networking.IsMaster) return;
            SendCustomEventDelayedSeconds(nameof(PlayDefaultTrack), _delay);
        }

        public void PlayDefaultTrack()
        {
            if (_controller.IsPlaying) return;
            switch(_autoPlayMode)
            {
                case AutoPlayMode.FromTrack:
                    Track track = Track.New(_videoPlayerType, _title, _url);
                    _controller.PlayTrack(track);
                    break;
                case AutoPlayMode.FromPlaylist:
                    _controller.PlayTrack(_controller.Playlists[_playlistIndex], _playlistTrackIndex);
                    break;
            }
        }
    }
}