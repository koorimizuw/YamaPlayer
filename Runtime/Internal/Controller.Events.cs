using VRC.SDKBase;
using VRC.SDK3.Components.Video;
using UnityEngine;

namespace Yamadev.YamaStream
{
  public partial class Controller
  {
    public override void AfterVideoPlayed()
    {
      if (Networking.IsOwner(gameObject) && !_isLocal && !_reloading)
      {
        UpdateSyncedVideoTime(VideoTime);
        RequestSerialization();
      }

      int len = _listeners.Length;
      for (int i = 0; i < len; i++)
      {
        var listener = _listeners[i];
        if (Utilities.IsValid(listener)) listener.AfterVideoPlayed();
      }
      PrintLog($"{_handler.Type.GetString()}: Video play.");
    }

    public override void AfterVideoPaused()
    {
      if (Networking.IsOwner(gameObject) && !_isLocal && !_reloading)
      {
        UpdateSyncedVideoTime(VideoTime);
        RequestSerialization();
      }

      int len = _listeners.Length;
      for (int i = 0; i < len; i++)
      {
        var listener = _listeners[i];
        if (Utilities.IsValid(listener)) listener.AfterVideoPaused();
      }
      PrintLog($"{_handler.Type.GetString()}: Video pause.");
    }

    public override void AfterVideoStopped()
    {
      _errorRetryCount = 0;
      _retryTargetTrack = null;

      if (!_reloading)
      {
        _repeat = 0;
        SetUseFallback(false);

        if (Networking.IsOwner(gameObject) && !_isLocal)
        {
          if (!string.IsNullOrEmpty(TrackUtils.GetUrl(Track).Get()) && Utilities.IsValid(_history))
          {
            _history.AddTrack(Track);
          }
          Track = TrackUtils.CreateEmptyTrack();
          ResetSyncedVideoTime();
          RequestSerialization();
        }
        else
        {
          Track = TrackUtils.CreateEmptyTrack();
        }
      }

      int len = _listeners.Length;
      for (int i = 0; i < len; i++)
      {
        var listener = _listeners[i];
        if (Utilities.IsValid(listener)) listener.AfterVideoStopped();
      }
      PrintLog($"{_handler.Type.GetString()}: Video stop.");
    }

    public override void AfterVideoReady()
    {
      _errorRetryCount = 0;
      _retryTargetTrack = null;
      if (SyncedState == PlayerState.Playing) ActiveHandler.Play();
      if (SyncedState == PlayerState.Idle) ActiveHandler.Stop();
      CheckRepeat();

      if (Networking.IsOwner(gameObject) && !_isLocal && !_reloading)
      {
        UpdateSyncedVideoTime(0f);
        RequestSerialization();
      }
      else EnsureVideoTime();

      int len = _listeners.Length;
      for (int i = 0; i < len; i++)
      {
        var listener = _listeners[i];
        if (Utilities.IsValid(listener)) listener.AfterVideoReady();
      }
      PrintLog($"{_handler.Type.GetString()}: Video ready.");
      _reloading = false;
    }

    public override void AfterVideoStarted()
    {
      int len = _listeners.Length;
      for (int i = 0; i < len; i++)
      {
        var listener = _listeners[i];
        if (Utilities.IsValid(listener)) listener.AfterVideoStarted();
      }
      PrintLog($"{_handler.Type.GetString()}: Video start.");
    }

    public override void AfterVideoLooped()
    {
      if (Networking.IsOwner(gameObject) && !_isLocal)
      {
        UpdateSyncedVideoTime(0f);
        RequestSerialization();
      }

      int len = _listeners.Length;
      for (int i = 0; i < len; i++)
      {
        var listener = _listeners[i];
        if (Utilities.IsValid(listener)) listener.AfterVideoLooped();
      }
      PrintLog($"{_handler.Type.GetString()}: Video loop.");
    }

    public override void AfterVideoEnded()
    {
      if (Networking.IsOwner(gameObject) || _isLocal)
      {
        if (_forwardInterval >= 0)
        {
          _autoForward = true;
          SendCustomEventDelayedSeconds(nameof(AutoForward), _forwardInterval);
        }
        else
        {
          ClearPlaylistIndexes();
        }
        _syncedState = (byte)PlayerState.Idle;
        ActiveHandler.Stop();
      }

      int len = _listeners.Length;
      for (int i = 0; i < len; i++)
      {
        var listener = _listeners[i];
        if (Utilities.IsValid(listener)) listener.AfterVideoEnded();
      }
      PrintLog($"{_handler.Type.GetString()}: Video end.");
    }

    public override void AfterVideoErrorOccurred(VideoError videoError)
    {
      PrintLog($"{_handler.Type.GetString()}: Video error {videoError}.");

      HandleErrorRetry(videoError);
      int len = _listeners.Length;
      for (int i = 0; i < len; i++)
      {
        var listener = _listeners[i];
        if (Utilities.IsValid(listener)) listener.AfterVideoErrorOccurred(videoError);
      }
    }

    private void HandleErrorRetry(VideoError videoError)
    {
      switch (videoError)
      {
        case VideoError.AccessDenied:
          PrintError("Access denied - no retry will be attempted");
          _errorRetryCount = 0;
          _retryTargetTrack = null;
          _reloading = false;
          return;
        case VideoError.InvalidURL:
          PrintError("Invalid URL - no retry will be attempted");
          _errorRetryCount = 0;
          _retryTargetTrack = null;
          _reloading = false;
          return;
        case VideoError.PlayerError:
          if (_useFallbackAfterErrors > 0 && _errorRetryCount == _useFallbackAfterErrors - 1)
          {
            if (Utilities.IsValid(Handler.FallbackHandler))
            {
              SetUseFallback(true);
              PrintLog($"Switching to fallback handler: {Handler.FallbackHandler.Type.GetString()}");
            }
          }
          else
          {
            SetUseFallback(false);
          }
          break;
      }

      if (_errorRetryCount < _maxErrorRetry)
      {
        _errorRetryCount++;
        _retryTargetTrack = Track;

        var nextSafeRetryTime = _lastLoadTime + SAFETY_RETRY_INTERVAL;
        var delay = Time.time >= nextSafeRetryTime ? 0 : nextSafeRetryTime - Time.time;
        SendCustomEventDelayedSeconds(nameof(ErrorRetry), delay);
        PrintLog($"Scheduling retry {_errorRetryCount}/{_maxErrorRetry} in {delay} seconds");
      }
      else
      {
        _errorRetryCount = 0;
        _retryTargetTrack = null;
        _reloading = false;
        PrintError($"Maximum retry count ({_maxErrorRetry}) reached. Stopping retry attempts.");
      }
    }

    public void ErrorRetry()
    {
      if (_retryTargetTrack == null || !TrackUtils.Equals(_retryTargetTrack, Track))
      {
        _errorRetryCount = 0;
        _retryTargetTrack = null;
        PrintLog("Retry cancelled: track has changed.");
        return;
      }

      if (IsPlaying || !ActiveHandler.IsValidTrack(Track))
      {
        _retryTargetTrack = null;
        return;
      }

      ActiveHandler.LoadTrack(Track);
      _lastLoadTime = Time.time;

      int len = _listeners.Length;
      for (int i = 0; i < len; i++)
      {
        var listener = _listeners[i];
        if (Utilities.IsValid(listener)) listener.AfterVideoRetry();
      }
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
      int len = _listeners.Length;
      for (int i = 0; i < len; i++)
      {
        var listener = _listeners[i];
        if (Utilities.IsValid(listener)) listener.AfterOwnerChanged();
      }
    }
  }
}