
using System;
using System.Reflection;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using Yamadev.YamaStream.Libraries.GenericDataContainer;

namespace Yamadev.YamaStream
{
    public partial class Controller
    {
        [SerializeField] Playlist _queue;
        [SerializeField] Playlist _history;
        [SerializeField] Playlist[] _playlists;
        [SerializeField] Playlist _playlistTemplate;
        [SerializeField] float _forwardInterval = 0;
        [SerializeField, UdonSynced, FieldChangeCallback(nameof(ShufflePlay))] bool _shuffle = false;
        [UdonSynced] int _activePlaylistIndex = -1;
        [UdonSynced] int _playingTrackIndex = -1;
        [UdonSynced] VRCUrl[] _dynamicUrls = new VRCUrl[] { };
        Playlist[] _dynamicPlaylists = new Playlist[] { };

        public Playlist[] Playlists
        {
            get
            {
                var pl = DataList<Playlist>.New(_playlists);
                pl.AddRange(DataList<Playlist>.New(_dynamicPlaylists));
                return pl.ToArray();
            }
        }

        public Playlist ActivePlaylist
        {
            get
            {
                if (_activePlaylistIndex < 0 || _activePlaylistIndex >= Playlists.Length) return null;
                return Playlists[_activePlaylistIndex];
            }
        }

        public int PlayingTrackIndex => _playingTrackIndex;

        public Playlist Queue => _queue;

        public Playlist History => _history;

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
            _dynamicUrls = _dynamicUrls.Add(url);
            GenerateDynamicPlaylists();
            if (Networking.IsOwner(gameObject) && !_isLocal) RequestSerialization();
        }

        public void GenerateDynamicPlaylists()
        {
            if (_dynamicPlaylists.Length >= _dynamicUrls.Length) return;
            for (int i = _dynamicPlaylists.Length; i < _dynamicUrls.Length; i++)
            {
                GameObject go = Instantiate(_playlistTemplate.gameObject, _playlistTemplate.transform.parent);
                go.SetActive(true);
                Playlist newPlaylist = go.GetComponent<Playlist>();
                _dynamicPlaylists = _dynamicPlaylists.Add(newPlaylist);
                newPlaylist.LoadPlaylist(_dynamicUrls[i]);
            }
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

        public void PlayRandomTrack(Playlist playlist, int exclude = -1)
        {
            if (playlist == null || playlist.Length == 0) return;
            if (playlist.Length == 1) PlayTrack(ActivePlaylist, 0);

            var r = new System.Random();
            bool hasExclude = exclude != -1;
            int next = r.Next(0, hasExclude ? playlist.Length - 1 : playlist.Length);
            int max = hasExclude ? playlist.Length : playlist.Length - 1;
            if (hasExclude)
            {
                PlayTrack(playlist, next < exclude ? next : next + 1);
                return;
            }
            PlayTrack(playlist, next);
        }

        public void RunForward()
        {
            if (IsPlaying || IsLoading) return;
            Forward();
        }

        public void Forward()
        {
            if (_queue.Length > 0)
            {
                PlayTrack(_queue, 0);
                _queue.RemoveTrack(0);
                return;
            }
            if (ActivePlaylist == null || _playingTrackIndex < 0 || ActivePlaylist.Length == 0) return;
            if (_shuffle) PlayRandomTrack(ActivePlaylist, _playingTrackIndex);
            else PlayTrack(ActivePlaylist, _playingTrackIndex + 1 < ActivePlaylist.Length ? _playingTrackIndex + 1 : 0);
        }
    }
}