using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
  public enum PlayerState
  {
    Idle,
    Playing,
    Paused,
  }

  [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
  [DefaultExecutionOrder(-1000)]
  [DisallowMultipleComponent]
  public partial class Controller : YamaPlayerListener
  {
    [SerializeField, HideInInspector] private string _version;
    [SerializeField] private PlayerHandler[] _videoPlayerHandlers = new PlayerHandler[0];
    [SerializeField] private bool _allowAutoSwitchHandler = true;
    [SerializeField, Range(0, 10)] private int _useFallbackAfterErrors = 1;
    [SerializeField] private string _timeFormat = @"hh\:mm\:ss";
    [SerializeField] private bool _isLocal;
    [SerializeField, Range(0, 10)] private int _maxErrorRetry = 5;
    [SerializeField, UdonSynced, FieldChangeCallback(nameof(Loop))] private bool _loop;
    [UdonSynced, FieldChangeCallback(nameof(Speed))] private float _speed = 1f;
    [UdonSynced, FieldChangeCallback(nameof(Repeat))] private ulong _repeat;
    [UdonSynced] private byte _syncedState;
    [UdonSynced] private int _handlerIndex;
    [UdonSynced] private string _title = string.Empty;
    [UdonSynced] private VRCUrl _url = VRCUrl.Empty;
    [UdonSynced] private byte[] _trackExtension = new byte[0];
    private object[] _track;
    private PlayerHandler _handler;
    private bool _useFallback;
    private YamaPlayerListener[] _listeners = new YamaPlayerListener[0];
    private int _errorRetryCount;
    private object[] _retryTargetTrack;
    private bool _reloading;
    private int _lastSetTimeFrame = 0;
    private float _lastLoadTime = 0f;
    private bool _checkRepeatRunning = false;

    private const float SAFETY_RETRY_INTERVAL = 5.1f;

    private void Start()
    {
      if (!Utilities.IsValid(_videoPlayerHandlers) || _videoPlayerHandlers.Length == 0)
      {
        PrintError($"Video player handlers are not assigned to {name}");
        return;
      }

      SetupHandlers();
      ReadPlaylists();

      InitializeScreen();
      InitializeAudio();

      RegisterHandlerListeners();
    }

    private void Update()
    {
      if (IsPlaying && Time.time - _lastSync > _syncFrequency)
      {
        EnsureVideoTime();
      }
    }

    public string Version => _version;
    public string TimeFormat => _timeFormat;
    public bool IsLocal => _isLocal;
    public PlayerState SyncedState => (PlayerState)_syncedState;
    public bool IsLoading => ActiveHandler.IsLoading;
    public bool Paused => ActiveHandler.IsPaused;
    public bool Stopped => ActiveHandler.IsStopped;
    public bool IsPlaying => ActiveHandler.IsPlaying;
    public bool IsError => ActiveHandler.IsError;
    public float Duration => ActiveHandler.Duration;
    public float VideoTime => ActiveHandler.Time;
    public bool IsLive => float.IsInfinity(Duration) || float.IsNaN(Duration);
    public string FormatedDuration => IsLive ? string.Empty : TimeSpan.FromSeconds(Duration).ToString(_timeFormat);
    public string FormatedVideoTime => IsLive ? string.Empty : TimeSpan.FromSeconds(VideoTime).ToString(_timeFormat);

    public YamaPlayerListener[] EventListeners
    {
      get => _listeners;
      set => _listeners = value;
    }

    public void AddListener(YamaPlayerListener listener)
    {
      if (!Utilities.IsValid(listener) || Array.IndexOf(_listeners, listener) >= 0) return;
      _listeners = _listeners.Add(listener);
    }

    public void SendCustomVideoEvent(string eventName)
    {
      int len = _listeners.Length;
      for (int i = 0; i < len; i++)
      {
        _listeners[i].SendCustomEvent(eventName);
      }
    }

    public PlayerHandler Handler
    {
      get
      {
        if (!Utilities.IsValid(_handler))
        {
          _handler = _videoPlayerHandlers[0];
        }
        return _handler;
      }
      set
      {
        SetUseFallback(false);
        _handler = value;
        _handlerIndex = Array.IndexOf(_videoPlayerHandlers, value);
        if (Networking.IsOwner(gameObject) && !_isLocal) RequestSerialization();
        int len = _listeners.Length;
        for (int i = 0; i < len; i++)
        {
          var listener = _listeners[i];
          if (Utilities.IsValid(listener)) listener.AfterPlayerHandlerChanged(_handler.Type);
        }
        PrintLog($"Player handler changed to {_handler.Type.GetString()}.");
      }
    }

    [Obsolete("Use Handler instead")]
    public PlayerHandler VideoPlayerHandle => Handler;

    public PlayerHandler ActiveHandler
    {
      get
      {
        if (_useFallback && Utilities.IsValid(Handler.FallbackHandler)) return Handler.FallbackHandler;
        return Handler;
      }
    }

    private void SetUseFallback(bool value)
    {
      if (_useFallback == value) return;
      if (value)
      {
        if (!Utilities.IsValid(Handler.FallbackHandler)) return;
        _useFallback = true;
        return;
      }
      _useFallback = false;
      var fallback = Handler.FallbackHandler;
      if (Utilities.IsValid(fallback) && !fallback.IsStopped) fallback.Stop();
    }

    private void RegisterHandlerListeners()
    {
      int len = _videoPlayerHandlers.Length;
      for (int i = 0; i < len; i++)
      {
        var handler = _videoPlayerHandlers[i];
        if (Utilities.IsValid(handler)) handler.SetListener(this);
      }
    }

    private void SetupHandlers()
    {
      if (!Utilities.IsValid(_handler)) _handler = _videoPlayerHandlers[0];

      int len = _videoPlayerHandlers.Length;
      for (int i = 0; i < len; i++)
      {
        var handler = _videoPlayerHandlers[i];
        if (Utilities.IsValid(handler)) handler.Loop = _loop;
      }
    }

    public bool AllowAutoSwitchHandler
    {
      get => _allowAutoSwitchHandler;
      set => _allowAutoSwitchHandler = value;
    }

    public void SetPlayerType(VideoPlayerType playerType)
    {
      if (Utilities.IsValid(Handler) && Handler.Type == playerType) return;
      Stop();
      SwitchToHandlerIndex(FindHandlerIndexByType(playerType));
    }

    public void SetPlayerHandler(int index)
    {
      if (index >= 0 && index < _videoPlayerHandlers.Length && _videoPlayerHandlers[index] == Handler) return;
      Stop();
      SwitchToHandlerIndex(index);
    }

    private bool SwitchToHandlerIndex(int index)
    {
      if (index < 0 || index >= _videoPlayerHandlers.Length || !Utilities.IsValid(_videoPlayerHandlers[index]))
      {
        PrintError($"Cannot switch handler: invalid handler index {index}.");
        return false;
      }

      var next = _videoPlayerHandlers[index];
      _handlerIndex = index;
      if (next == _handler) return true;

      StopLocal();
      Handler = next;
      return true;
    }

    private int FindHandlerIndexByType(VideoPlayerType playerType)
    {
      int len = _videoPlayerHandlers.Length;
      for (int i = 0; i < len; i++)
      {
        var handler = _videoPlayerHandlers[i];
        if (Utilities.IsValid(handler) && handler.Type == playerType) return i;
      }
      return -1;
    }

    public int FindHandlerIndexForUrl(VRCUrl url)
    {
      return FindHandlerIndexForTrack(TrackUtils.NewTrack(0, string.Empty, url));
    }

    public int FindHandlerIndexForTrack(object[] track)
    {
      int len = _videoPlayerHandlers.Length;
      for (int i = 0; i < len; i++)
      {
        var handler = _videoPlayerHandlers[i];
        if (Utilities.IsValid(handler) && handler.IsValidTrack(track)) return i;
      }
      return -1;
    }

    private int ResolveHandlerIndexForTrack(object[] track)
    {
      int declared = FindHandlerIndexByType(TrackUtils.GetPlayerType(track));
      if (declared >= 0 && _videoPlayerHandlers[declared].IsValidTrack(track)) return declared;
      if (_allowAutoSwitchHandler) return FindHandlerIndexForTrack(track);
      return -1;
    }

    public void Play(bool force = false)
    {
      if ((Stopped || IsPlaying) && !force) return;
      _syncedState = (byte)PlayerState.Playing;
      ActiveHandler.Play();
    }

    public void Pause(bool force = false)
    {
      if ((Stopped || Paused) && !force) return;
      _syncedState = (byte)PlayerState.Paused;
      ActiveHandler.Pause();
    }

    public void Stop(bool force = false)
    {
      _autoForward = false;
      _reloading = false;
      if (Stopped && !IsError && !force) return;
      _syncedState = (byte)PlayerState.Idle;
      ClearPlaylistIndexes();
      StopLocal();
      if (IsError) AfterVideoStopped();
    }

    private void StopLocal()
    {
      _autoForward = false;
      ActiveHandler.Stop();
    }

    public void Reload()
    {
      if (!Stopped && !IsLoading) ResolveAndLoadTrack(Track, true);
    }

    public bool Loop
    {
      get => _loop;
      set
      {
        _loop = value;
        int handlerLen = _videoPlayerHandlers.Length;
        for (int i = 0; i < handlerLen; i++)
        {
          var handler = _videoPlayerHandlers[i];
          if (Utilities.IsValid(handler)) handler.Loop = _loop;
        }

        if (Networking.IsOwner(gameObject) && !_isLocal) RequestSerialization();
        int listenerLen = _listeners.Length;
        for (int i = 0; i < listenerLen; i++)
        {
          var listener = _listeners[i];
          if (Utilities.IsValid(listener)) listener.AfterLoopChanged(value);
        }
        PrintLog($"Loop changed to {_loop}.");
      }
    }

    public float Speed
    {
      get => _speed;
      set
      {
        _speed = value;
        UpdateSpeed();
        if (Networking.IsOwner(gameObject) && !_isLocal)
        {
          UpdateSyncedVideoTime(VideoTime);
          RequestSerialization();
        }
        int len = _listeners.Length;
        for (int i = 0; i < len; i++)
        {
          var listener = _listeners[i];
          if (Utilities.IsValid(listener)) listener.AfterSpeedChanged(value);
        }
        PrintLog($"Speed changed to {_speed:F2}x.");
      }
    }

    public void UpdateSpeed()
    {
      int len = _videoPlayerHandlers.Length;
      for (int i = 0; i < len; i++)
      {
        var handler = _videoPlayerHandlers[i];
        if (Utilities.IsValid(handler)) handler.Speed = _speed;
      }
      if (!Stopped && ActiveHandler.Type == VideoPlayerType.AVProVideoPlayer)
        SendCustomEventDelayedFrames(nameof(Reload), 0);
      UpdateAudioPitch();
    }

    public ulong Repeat
    {
      get => _repeat;
      set
      {
        _repeat = value;
        CheckRepeat();
        if (Networking.IsOwner(gameObject) && !_isLocal) RequestSerialization();
        int len = _listeners.Length;
        for (int i = 0; i < len; i++)
        {
          var listener = _listeners[i];
          if (Utilities.IsValid(listener)) listener.AfterRepeatChanged(value);
        }

        if (RepeatUtils.IsOn(_repeat)) PrintLog($"Repeat on, start: {RepeatUtils.GetStartTime(_repeat)}, end: {RepeatUtils.GetEndTime(_repeat)}.");
      }
    }

    public void CheckRepeat()
    {
      if (_checkRepeatRunning) return;
      _checkRepeatRunning = true;
      SendCustomEventDelayedFrames(nameof(_CheckRepeat), 0);
    }

    public void _CheckRepeat()
    {
      if (!RepeatUtils.IsOn(_repeat) || IsLive || Stopped)
      {
        _checkRepeatRunning = false;
        return;
      }

      if (IsPlaying)
      {
        var start = RepeatUtils.GetStartTime(_repeat);
        var end = RepeatUtils.GetEndTime(_repeat);
        if (ActiveHandler.Time > end || ActiveHandler.Time < start) SetTime(start);
      }

      SendCustomEventDelayedSeconds(nameof(_CheckRepeat), 0.5f);
    }

    public void SetTime(float time)
    {
      if (IsLive || Time.frameCount == _lastSetTimeFrame) return;
      _lastSetTimeFrame = Time.frameCount;

      ActiveHandler.Time = time;
      if (Networking.IsOwner(gameObject) && !_isLocal)
      {
        UpdateSyncedVideoTime(time);
        RequestSerialization();
      }

      int len = _listeners.Length;
      for (int i = 0; i < len; i++)
      {
        var listener = _listeners[i];
        if (Utilities.IsValid(listener)) listener.AfterTimeChanged(time);
      }
      PrintLog($"{ActiveHandler.Type.GetString()}: Set video time: {time}.");
    }

    public object[] Track
    {
      get
      {
        if (!Utilities.IsValid(_track))
        {
          _track = TrackUtils.CreateEmptyTrack();
        }
        return _track;
      }
      set
      {
        _track = value;
        int len = _listeners.Length;
        for (int i = 0; i < len; i++)
        {
          var listener = _listeners[i];
          if (Utilities.IsValid(listener)) listener.AfterTrackUpdated();
        }
      }
    }

    public void PlayTrack(object[] track)
    {
      if (!Utilities.IsValid(track)) return;

      if (ResolveHandlerIndexForTrack(track) < 0)
      {
        PrintError($"Track is not playable: url={TrackUtils.GetUrl(track).Get()}, extension={TrackUtils.GetExtensionString(track)}.");
        return;
      }

      if (IsPlaying && (Networking.IsOwner(gameObject) || _isLocal))
      {
        Stop();
      }

      ClearPlaylistIndexes();

      _syncedState = (byte)PlayerState.Playing;
      ResolveAndLoadTrack(track);
    }

    private void ResolveAndLoadTrack(object[] track, bool isReload = false)
    {
      _reloading = isReload;

      int index = ResolveHandlerIndexForTrack(track);
      if (index >= 0) SwitchToHandlerIndex(index);

      LoadTrackLocal(track, isReload);

      if (Networking.IsOwner(gameObject) && !_isLocal && !isReload)
      {
        _trackVersion++;
        _appliedTrackVersion = _trackVersion;
        RequestSerialization();
      }
    }

    private void LoadTrackLocal(object[] track, bool isReload)
    {
      _reloading = isReload;
      StopLocal();

      if (!isReload) Track = track;
      ActiveHandler.LoadTrack(track);
      _lastLoadTime = Time.time;

      int len = _listeners.Length;
      for (int i = 0; i < len; i++)
      {
        var listener = _listeners[i];
        if (Utilities.IsValid(listener)) listener.AfterTrackLoaded();
      }
      PrintLog($"Load url: {TrackUtils.GetUrl(track)}.");
    }
  }
}
