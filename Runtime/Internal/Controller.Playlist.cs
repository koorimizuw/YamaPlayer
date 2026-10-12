using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
  public partial class Controller
  {
    [SerializeField] private QueueList _queue;
    [SerializeField] private HistoryList _history;
    [SerializeField] private Playlist[] _playlists = new Playlist[0];
    [SerializeField, Range(0, 60)] private float _forwardInterval = 0;
    [SerializeField, UdonSynced, FieldChangeCallback(nameof(ShufflePlay))] private bool _shuffle = false;
    [UdonSynced] private int _activePlaylistIndex = -1;
    [UdonSynced] private int _playingTrackIndex = -1;
    private bool _autoForward = false;

    public Playlist[] Playlists => _playlists;

    public int PlaylistCount => _playlists.Length;

    public Playlist ActivePlaylist
    {
      get
      {
        if (_activePlaylistIndex < 0 || _activePlaylistIndex >= PlaylistCount) return null;
        return _playlists[_activePlaylistIndex];
      }
    }

    public int ActivePlaylistIndex => _activePlaylistIndex;

    public int PlayingTrackIndex => _playingTrackIndex;

    public QueueList Queue => _queue;

    public HistoryList History => _history;

    public bool ShufflePlay
    {
      get => _shuffle;
      set
      {
        _shuffle = value;
        if (Networking.IsOwner(gameObject) && !_isLocal) RequestSerialization();
        int len = _listeners.Length;
        for (int i = 0; i < len; i++)
        {
          var listener = _listeners[i];
          if (Utilities.IsValid(listener)) listener.AfterShufflePlayChanged(_shuffle);
        }
      }
    }

    public void ClearPlaylistIndexes()
    {
      _activePlaylistIndex = -1;
      _playingTrackIndex = -1;
    }

    private void ReadPlaylists()
    {
      _playlists = GetComponentsInChildren<Playlist>();
    }

    public void AddPlaylist(Playlist playlist)
    {
      _playlists = _playlists.Add(playlist);
      int len = _listeners.Length;
      for (int i = 0; i < len; i++)
      {
        var listener = _listeners[i];
        if (Utilities.IsValid(listener)) listener.AfterPlaylistsUpdated();
      }
    }

    public void PlayTrackFromQueue()
    {
      if (!Utilities.IsValid(_queue) || _queue.TrackCount == 0) return;

      var track = _queue.GetTrack(0);
      PlayTrack(track);

      _queue.RemoveTrack(0);
    }

    public void PlayTrackFromHistory(int index)
    {
      if (!Utilities.IsValid(_history) || index < 0 || index >= _history.TrackCount) return;

      var track = _history.GetTrack(index);
      PlayTrack(track);
    }

    public void PlayTrack(Playlist playlist, int index)
    {
      if (!Utilities.IsValid(playlist) || index < 0 || index >= playlist.TrackCount) return;

      var track = playlist.GetTrack(index);
      if (!Utilities.IsValid(track)) return;

      var url = TrackUtils.GetUrl(track);
      if (ResolveHandlerIndexForTrack(track) < 0)
      {
        PrintError($"URL {url.Get()} is not valid.");
        return;
      }

      if (IsPlaying && (Networking.IsOwner(gameObject) || _isLocal))
      {
        Stop();
      }

      _activePlaylistIndex = Array.IndexOf(_playlists, playlist);
      _playingTrackIndex = index;

      _syncedState = (byte)PlayerState.Playing;
      ResolveAndLoadTrack(track);
    }

    public void Backward()
    {
      if (!Utilities.IsValid(ActivePlaylist) || _playingTrackIndex < 0 || ActivePlaylist.TrackCount == 0) return;
      int next = _playingTrackIndex - 1 < 0 ? ActivePlaylist.TrackCount - 1 : _playingTrackIndex - 1;
      PlayTrack(ActivePlaylist, next);
    }

    public int GetRandomIndex(int trackCount, int exclude = -1)
    {
      if (trackCount == 0) return -1;
      if (trackCount == 1) return 0;

      var r = new System.Random();
      bool hasExclude = exclude != -1;
      int next = r.Next(0, hasExclude ? trackCount - 1 : trackCount);
      if (hasExclude)
      {
        return next < exclude ? next : next + 1;
      }
      return next;
    }

    public void PlayRandomTrack(Playlist playlist)
    {
      if (!Utilities.IsValid(playlist) || playlist.TrackCount == 0) return;
      var index = GetRandomIndex(playlist.TrackCount);
      PlayTrack(playlist, index);
    }

    public void AutoForward()
    {
      if (!_autoForward) return;
      Forward();
    }

    public void Forward()
    {
      if (Utilities.IsValid(_queue) && _queue.TrackCount > 0)
      {
        PlayTrackFromQueue();
        return;
      }

      if (!Utilities.IsValid(ActivePlaylist) || _playingTrackIndex < 0 || ActivePlaylist.TrackCount == 0) return;

      int next;
      if (_shuffle)
      {
        next = GetRandomIndex(ActivePlaylist.TrackCount, _playingTrackIndex);
      }
      else
      {
        next = _playingTrackIndex + 1 < ActivePlaylist.TrackCount ? _playingTrackIndex + 1 : 0;
      }
      PlayTrack(ActivePlaylist, next);
    }
  }
}