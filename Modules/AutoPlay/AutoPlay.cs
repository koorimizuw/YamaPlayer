using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Yamadev.YamaStream.Modules.AutoPlay
{
  public enum AutoPlayMode
  {
    Off,
    FromTrack,
    FromPlaylist,
  }

  [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
  public class AutoPlay : YamaPlayerModule
  {
    [SerializeField] private AutoPlayMode _autoPlayMode = AutoPlayMode.Off;
    [SerializeField, Range(0, 60)] private float _delay;
    [SerializeField] private VideoPlayerType _videoPlayerType;
    [SerializeField] private string _title;
    [SerializeField] private VRCUrl _url;
    [SerializeField] private int _playlistIndex = 0;
    [SerializeField] private int _playlistTrackIndex = 0;

    public override void Start()
    {
      base.Start();
      if (!Utilities.IsValid(_controller)) return;
      if (!Networking.IsMaster && !_controller.IsLocal) return;
      SendCustomEventDelayedSeconds(nameof(PlayDefaultTrack), _delay);
    }

    public void PlayDefaultTrack()
    {
      if (!Utilities.IsValid(_controller) || _controller.IsPlaying) return;
      switch (_autoPlayMode)
      {
        case AutoPlayMode.FromTrack:
          PlayFromTrack();
          break;
        case AutoPlayMode.FromPlaylist:
          PlayFromPlaylist();
          break;
        case AutoPlayMode.Off:
          break;
        default:
          break;
      }
    }

    private void PlayFromTrack()
    {
      if (string.IsNullOrEmpty(_url.Get())) return;
      object[] track = TrackUtils.NewTrack(_videoPlayerType, _title, _url);
      _controller.PlayTrack(track);
    }

    private void PlayFromPlaylist()
    {
      if (_playlistIndex < 0 || _playlistIndex >= _controller.Playlists.Length) return;

      var playlist = _controller.Playlists[_playlistIndex];
      if (!Utilities.IsValid(playlist)) return;

      if (_playlistTrackIndex < 0)
      {
        _controller.PlayRandomTrack(playlist);
      }
      else
      {
        _controller.PlayTrack(playlist, _playlistTrackIndex);
      }
    }
  }
}