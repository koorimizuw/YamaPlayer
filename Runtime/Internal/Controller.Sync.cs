using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
  public partial class Controller
  {
    [SerializeField, Range(1f, 10f)] private float _syncFrequency = 5.0f;
    [SerializeField, Range(0f, 1f)] private float _syncMargin = 0.3f;
    [UdonSynced] private float _syncedVideoTime = 0;
    [UdonSynced] private long _networkDataTimeTicks = 0;
    [UdonSynced] private int _trackVersion = 0;
    private int _appliedTrackVersion = 0;
    private float _lastSync = 0;
    private float _localDelay = 0;

    private void ApplySyncedState()
    {
      if (SyncedState == PlayerState.Idle)
      {
        if (Stopped && !IsError) return;
        StopLocal();
        if (IsError) AfterVideoStopped();
        return;
      }
      if (!ActiveHandler.IsReady) return;
      switch (SyncedState)
      {
        case PlayerState.Playing:
          if (IsPlaying) return;
          ActiveHandler.Play();
          break;
        case PlayerState.Paused:
          if (Paused) return;
          ActiveHandler.Pause();
          break;
      }
    }

    public float LocalDelay
    {
      get => _localDelay;
      set
      {
        _localDelay = value;
        EnsureVideoTime(true);
        int len = _listeners.Length;
        for (int i = 0; i < len; i++)
        {
          var listener = _listeners[i];
          if (Utilities.IsValid(listener)) listener.AfterLocalDelayChanged(value);
        }
      }
    }

    private void UpdateSyncedVideoTime(float time)
    {
      _syncedVideoTime = Mathf.Clamp(time - _localDelay, 0f, Duration);
      _networkDataTimeTicks = Networking.GetNetworkDateTime().Ticks;
    }

    private void ResetSyncedVideoTime()
    {
      _syncedVideoTime = 0;
      _networkDataTimeTicks = 0;
    }

    private void EnsureVideoTime(bool force = false)
    {
      if (IsLive || Stopped || _networkDataTimeTicks == 0)
      {
        _lastSync = Time.time;
        return;
      }

      float offset = Paused ? 0 : (float)(Networking.GetNetworkDateTime().Ticks - _networkDataTimeTicks) / TimeSpan.TicksPerSecond * Speed;
      float targetTime = Mathf.Clamp(_syncedVideoTime + offset + _localDelay, 0, Duration);
      if (force || Mathf.Abs(VideoTime - targetTime) >= _syncMargin)
      {
        SetTime(targetTime);
      }

      _lastSync = Time.time;
    }

    public override void OnPreSerialization()
    {
      _title = TrackUtils.GetTitle(Track);
      _url = TrackUtils.GetUrl(Track);
      _trackExtension = TrackUtils.GetExtension(Track);
    }

    public override void OnDeserialization()
    {
      int handlerIndex = _handlerIndex;
      bool syncedIndexValid = handlerIndex >= 0 && handlerIndex < _videoPlayerHandlers.Length && Utilities.IsValid(_videoPlayerHandlers[handlerIndex]);
      if (!syncedIndexValid)
      {
        handlerIndex = Mathf.Max(Array.IndexOf(_videoPlayerHandlers, Handler), 0);
      }

      bool hasSyncedExtension = _trackExtension != null && _trackExtension.Length > 0;
      object[] track = hasSyncedExtension
        ? TrackUtils.NewTrackWithExtension(_videoPlayerHandlers[handlerIndex].Type, _title, _url, _trackExtension)
        : TrackUtils.NewTrack(_videoPlayerHandlers[handlerIndex].Type, _title, _url);

      int len = _listeners.Length;
      for (int i = 0; i < len; i++)
      {
        var listener = _listeners[i];
        if (Utilities.IsValid(listener)) listener.AfterTrackSynced();
      }

      bool trackChanged = _trackVersion != _appliedTrackVersion;
      _appliedTrackVersion = _trackVersion;

      SwitchToHandlerIndex(handlerIndex);
      if (SyncedState != PlayerState.Idle && trackChanged)
      {
        LoadTrackLocal(track, false);
      }

      ApplySyncedState();
      EnsureVideoTime();
    }
  }
}
