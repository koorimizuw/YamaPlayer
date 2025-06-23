using System;
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
        [SerializeField, Range(0, 60)] float _forwardInterval = 0;
        [SerializeField, UdonSynced, FieldChangeCallback(nameof(ShufflePlay))] bool _shuffle = false;
        [UdonSynced] int _activePlaylistIndex = -1;
        [UdonSynced] int _playingTrackIndex = -1;
        [UdonSynced, FieldChangeCallback(nameof(DynamicUrls))] VRCUrl[] _dynamicUrls = new VRCUrl[] { };
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

        public VRCUrl[] DynamicUrls
        {
            get => _dynamicUrls;
            set
            {
                _dynamicUrls = value;
                GenerateDynamicPlaylists();
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

        private void GenerateDynamicPlaylists()
        {
            Playlist[] newDynamicPlaylists = new Playlist[_dynamicUrls.Length];
            for (int i = 0; i < _dynamicUrls.Length; i++)
            {
                newDynamicPlaylists[i] = FindMatchingPlaylist(_dynamicUrls[i]);
                if (!Utilities.IsValid(newDynamicPlaylists[i]))
                {
                    newDynamicPlaylists[i] = CreateNewDynamicPlaylist(i);
                }
            }

            DestroyUnusedPlaylists(newDynamicPlaylists);

            _dynamicPlaylists = newDynamicPlaylists;
            SendCustomVideoEvent(nameof(Listener.OnPlaylistsUpdated));
        }

        private Playlist CreateNewDynamicPlaylist(int index)
        {
            GameObject go = Instantiate(_playlistTemplate.gameObject, _playlistTemplate.transform.parent);
            go.SetActive(true);

            Playlist newPlaylist = go.GetComponent<Playlist>();
            newPlaylist.LoadPlaylist(_dynamicUrls[index]);

            return newPlaylist;
        }

        private Playlist FindMatchingPlaylist(VRCUrl targetUrl)
        {
            string targetUrlString = targetUrl.Get();
            foreach (Playlist existing in _dynamicPlaylists)
            {
                if (!Utilities.IsValid(existing)) continue;
                if (existing.PlaylistUrl.Contains(targetUrlString)) return existing;
            }

            return null;
        }

        private void DestroyUnusedPlaylists(Playlist[] newPlaylists)
        {
            foreach (Playlist old in _dynamicPlaylists)
            {
                if (!Utilities.IsValid(old)) continue;

                bool isUsed = false;
                foreach (Playlist newPl in newPlaylists)
                {
                    if (old == newPl)
                    {
                        isUsed = true;
                        break;
                    }
                }

                if (!isUsed) DestroyImmediate(old.gameObject);
            }
        }

        public void ClearPlaylistIndexes()
        {
            _activePlaylistIndex = -1;
            _playingTrackIndex = -1;
        }

        public void PlayTrack(Playlist playlist, int index)
        {
            if (!Utilities.IsValid(playlist))
            {
                PrintError("Cannot play track: playlist is null");
                return;
            }

            if (index < 0 || index >= playlist.Length)
            {
                PrintError($"Cannot play track: invalid index {index} for playlist with {playlist.Length} tracks");
                return;
            }

            Track track = playlist.GetTrack(index);
            if (!Utilities.IsValid(track))
            {
                PrintError($"Cannot play track: failed to get track at index {index}");
                return;
            }

            bool isQueueOrHistory = playlist == _queue || playlist == _history;
            _activePlaylistIndex = isQueueOrHistory ? -1 : Array.IndexOf(Playlists, playlist);
            _playingTrackIndex = isQueueOrHistory ? -1 : index;

            PlayTrack(track);
        }

        public void Backward()
        {
            if (!Utilities.IsValid(ActivePlaylist) || _playingTrackIndex < 0 || ActivePlaylist.Length == 0) return;
            int next = _playingTrackIndex - 1 < 0 ? ActivePlaylist.Length - 1 : _playingTrackIndex - 1;
            PlayTrack(ActivePlaylist, next);
        }

        public void PlayRandomTrack(Playlist playlist, int exclude = -1)
        {
            if (!Utilities.IsValid(playlist) || playlist.Length == 0) return;
            if (playlist.Length == 1)
            {
                PlayTrack(playlist, 0);
                return;
            }

            var r = new System.Random();
            bool hasExclude = exclude != -1;
            int next = r.Next(0, hasExclude ? playlist.Length - 1 : playlist.Length);
            if (hasExclude)
            {
                PlayTrack(playlist, next < exclude ? next : next + 1);
                return;
            }
            PlayTrack(playlist, next);
        }

        public void Forward()
        {
            if (Utilities.IsValid(_queue) && _queue.Length > 0)
            {
                PlayTrack(_queue, 0);
                _queue.RemoveTrack(0);
                return;
            }

            if (!Utilities.IsValid(ActivePlaylist) || _playingTrackIndex < 0 || ActivePlaylist.Length == 0) return;

            if (_shuffle)
            {
                PlayRandomTrack(ActivePlaylist, _playingTrackIndex);
            }
            else
            {
                int next = _playingTrackIndex + 1 < ActivePlaylist.Length ? _playingTrackIndex + 1 : 0;
                PlayTrack(ActivePlaylist, next);
            }
        }
    }
}