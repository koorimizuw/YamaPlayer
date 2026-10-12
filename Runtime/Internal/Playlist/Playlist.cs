using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using System;

namespace Yamadev.YamaStream
{
  [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
  public class Playlist : YamaPlayerListener
  {
    [SerializeField] private string _playlistName;
    [SerializeField] private VideoPlayerType[] _videoPlayerTypes = new VideoPlayerType[0];
    [SerializeField] private string[] _titles = new string[0];
    [SerializeField] private VRCUrl[] _urls = new VRCUrl[0];
    private object[][] _tracks;
    private bool _initialized;

    private void EnsureInitialized()
    {
      if (_initialized) return;
      _initialized = true;
      GenerateTracks();
    }

    private void GenerateTracks()
    {
      var trackCount = _urls.Length;
      _tracks = new object[trackCount][];
      for (int i = 0; i < trackCount; i++)
      {
        _tracks[i] = TrackUtils.NewTrack(_videoPlayerTypes[i], _titles[i], _urls[i]);
      }
    }

    public object[][] Tracks
    {
      get
      {
        EnsureInitialized();
        return _tracks;
      }
    }

    public string PlaylistName => _playlistName;

    public int TrackCount
    {
      get
      {
        EnsureInitialized();
        return _tracks.Length;
      }
    }

    public object[] GetTrack(int index)
    {
      EnsureInitialized();
      if (index < 0 || index >= TrackCount) return TrackUtils.CreateEmptyTrack();
      return _tracks[index];
    }

    public void SetTracks(string playlistName, object[][] tracks)
    {
      _playlistName = playlistName;
      _tracks = tracks == null ? new object[0][] : tracks;
      _initialized = true;
    }
  }
}
