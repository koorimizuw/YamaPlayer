
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
    public partial class Controller : Listener
    {
        [SerializeField] StandardPlaylist _queue;
        [SerializeField] StandardPlaylist _history;
        [SerializeField] Playlist[] _playlists;
        [SerializeField] Transform _dynamicPlaylistsContainer;
        [SerializeField, UdonSynced, FieldChangeCallback(nameof(ShufflePlay))] bool _shuffle = false;
        [UdonSynced] int _activePlaylistIndex = -1;
        [UdonSynced] int _playingTrackIndex = -1;
        DynamicPlaylist[] _dynamics = new DynamicPlaylist[] { };

        public Playlist[] Playlists
        {
            get
            {
                Playlist[] results = new Playlist[_playlists.Length];
                Array.Copy(_playlists, results, _playlists.Length);
                foreach (var item in _dynamics) results = results.Add(item);
                return results;
            }
        }
        public Playlist ActivePlaylist => _activePlaylistIndex >= 0 ? _activePlaylistIndex < Playlists.Length ? Playlists[_activePlaylistIndex] : null : null;
        public int PlayingTrackIndex => _playingTrackIndex;
        public StandardPlaylist Queue => _queue;
        public StandardPlaylist History => _history;

        public bool ShufflePlay
        {
            get => _shuffle;
            set
            {
                _shuffle = value;
                if (Networking.IsOwner(gameObject) && !_isLocal) RequestSerialization();
                SendCustomVideoEvent(nameof(Listener.OnShufflePlayChanged));
            }
        }

        public void AddDynamicPlaylist(VRCUrl url)
        {
            DynamicPlaylist[] dynamics = _dynamicPlaylistsContainer.GetComponentsInChildren<DynamicPlaylist>();
            foreach (var item in dynamics)
            {
                if (item.IsLoading || item.Loaded) continue;
                item.Load(url);
                SendCustomVideoEvent(nameof(Listener.OnPlaylistsUpdated));
                return;
            }
        }

        public void UpdatePlaylists()
        {
            DynamicPlaylist[] dynamics = _dynamicPlaylistsContainer.GetComponentsInChildren<DynamicPlaylist>();
            DynamicPlaylist[] results = new DynamicPlaylist[] { };
            foreach (DynamicPlaylist item in dynamics)
            {
                if (item.IsLoading || item.Loaded) results = results.Add(item);
                else break;
            }
            _dynamics = results;
            SendCustomVideoEvent(nameof(Listener.OnPlaylistsUpdated));
        }

        public void PlayTrack(Playlist playlist, int index)
        {
            bool isQueueOrHistory = playlist == _queue || playlist == _history;
            _activePlaylistIndex = isQueueOrHistory ? -1 : Array.IndexOf(Playlists, playlist);
            _playingTrackIndex = isQueueOrHistory ? -1 : index;
            Track track = playlist.GetTrack(index);
            PlayTrack(track);
        }

        public void Backward()
        {
            if (ActivePlaylist == null || _playingTrackIndex < 0 || ActivePlaylist.Length == 0) return;
            int next = _playingTrackIndex - 1 < 0 ? ActivePlaylist.Length - 1 : _playingTrackIndex - 1;
            PlayTrack(ActivePlaylist, next);
        }

        public int GetRandomIndex(int length, int exclude = -1)
        {
            if (length == 1) return 0;
            while (true)
            {
                int result = UnityEngine.Random.Range(0, length);
                if (result != exclude) return result;
            }
        }

        public void Forward()
        {
            if (_queue.Length > 0)
            {
                PlayTrack(_queue, 0);
                _queue.Remove(0);
                return;
            }
            if (ActivePlaylist == null || _playingTrackIndex < 0 || ActivePlaylist.Length == 0) return;
            int next = _shuffle ? GetRandomIndex(ActivePlaylist.Length, _playingTrackIndex) : _playingTrackIndex + 1 < ActivePlaylist.Length ? _playingTrackIndex + 1 : 0;
            PlayTrack(ActivePlaylist, next);
        }
    }
}