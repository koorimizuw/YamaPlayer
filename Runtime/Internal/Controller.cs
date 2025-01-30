using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;

#if AUDIOLINK_V1
using AudioLink;
#endif

namespace Yamadev.YamaStream
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    [DefaultExecutionOrder(-1000)]
    [DisallowMultipleComponent]
    public partial class Controller : Listener
    {
        [SerializeField] PlayerHandler[] _videoPlayerHandlers;
        [SerializeField] Permission _permission;
        [SerializeField] float _retryAfterSeconds = 5.1f;
        [SerializeField] int _maxErrorRetry = 5;
        [SerializeField] string _timeFormat = @"hh\:mm\:ss";
        [SerializeField] bool _isLocal;
        [SerializeField] string _version;
        [SerializeField, UdonSynced, FieldChangeCallback(nameof(PlayerType))] VideoPlayerType _playerType;
        [SerializeField, UdonSynced, FieldChangeCallback(nameof(Loop))] bool _loop;
        [SerializeField, UdonSynced, FieldChangeCallback(nameof(SlideMode))] bool _slideMode;
        [SerializeField, UdonSynced, FieldChangeCallback(nameof(SlideSeconds))] int _slideSeconds = 1;
        [UdonSynced, FieldChangeCallback(nameof(SyncedState))] byte _state;
        [UdonSynced, FieldChangeCallback(nameof(Speed))] float _speed = 1f;
        [UdonSynced, FieldChangeCallback(nameof(Repeat))] ulong _repeat;
        PlayerHandler _handler;
        Listener[] _listeners = { };
        int _errorRetryCount = 0;
        bool _reloading;

        private void Start()
        {
            EnsurePlayerHandler();
            foreach (PlayerHandler handler in _videoPlayerHandlers) handler.Loop = _loop;
            InitializeScreen();
            UpdateAudio();
            UpdateAudioLink();
            foreach (PlayerHandler handler in _videoPlayerHandlers)
                handler.SetListener(this);
        }

        private void Update()
        {
            if (IsPlaying && Time.time - _syncFrequency > _lastSync) DoSync();
        }

        public Permission Permission => _permission;

        public string Version => _version;

        public PlayerPermission PlayerPermission => Utilities.IsValid(_permission) ? PlayerPermission.Editor : _permission.PlayerPermission;

        public void AddListener(Listener listener)
        {
            if (Array.IndexOf(_listeners, listener) >= 0) return;
            _listeners = _listeners.Add(listener);
        }

        public bool IsLocal => _isLocal;

        private void EnsurePlayerHandler()
        {
            foreach (PlayerHandler handler in _videoPlayerHandlers)
            {
                if (handler.Type == _playerType)
                {
                    _handler = handler;
                    return;
                }
            }
        }

        public PlayerHandler Handler
        {
            get
            {
                if (!_handler) EnsurePlayerHandler();
                return _handler;
            }
        }

        public VideoPlayerType PlayerType
        {
            get => _playerType;
            set => ChangePlayerTyper(value);
        }

        public void ChangePlayerTyper(VideoPlayerType playerType)
        {
            if (_playerType == playerType) return;
            Stop();
            _playerType = playerType;
            EnsurePlayerHandler();
            if (Networking.IsOwner(gameObject) && !_isLocal) RequestSerialization();
            foreach (Listener listener in _listeners) listener.OnPlayerHandlerChanged();
            PrintLog($"Video player changed to {_playerType.GetString()}.");
        }

        public PlayerState State => (PlayerState)_state;

        public byte SyncedState
        {
            set
            {
                if (_state == value) return;
                switch ((PlayerState)value)
                {
                    case PlayerState.Idle:
                        Stop();
                        break;
                    case PlayerState.Playing: 
                        Play();
                        break;
                    case PlayerState.Paused:
                        Pause();
                        break;
                }
            }
        }

        public bool Paused => State == PlayerState.Paused;

        public bool Stopped => State == PlayerState.Idle;

        public void Play(bool force = false)
        {
            if (State == PlayerState.Playing && !force) return;
            Handler.Play();
            _state = (byte)PlayerState.Playing;
            SendCustomEventDelayedFrames(nameof(CheckRepeat), 0);
            if (KaraokeMode != KaraokeMode.None) SendCustomEventDelayedSeconds(nameof(ForceSync), 1f);
#if AUDIOLINK_V1
                if (Utilities.IsValid(_audioLink) && _useAudioLink)
                    _audioLink.SetMediaPlaying(IsLive ? MediaPlaying.Streaming : MediaPlaying.Playing);
#endif
            if (Networking.IsOwner(gameObject) && !_isLocal)
            {
                SyncTime = VideoTime - VideoStandardDelay;
                RequestSerialization();
            }
            foreach (Listener listener in _listeners) listener.OnVideoPlay();
            PrintLog($"{_playerType.GetString()}: Video play.");
        }

        public void Pause()
        {
            if (State == PlayerState.Paused) return;
            Handler.Pause();
            _state = (byte)PlayerState.Paused;
#if AUDIOLINK_V1
                if (Utilities.IsValid(_audioLink) && _useAudioLink)
                    _audioLink.SetMediaPlaying(MediaPlaying.Paused);
#endif
            if (Networking.IsOwner(gameObject) && !_isLocal)
            {
                SyncTime = VideoTime - VideoStandardDelay;
                RequestSerialization();
            }
            foreach (Listener listener in _listeners) listener.OnVideoPause();
            PrintLog($"{_playerType.GetString()}: Video pause.");
        }

        public void Stop()
        {
            if (State == PlayerState.Idle) return;
            Handler.Stop();
            _state = (byte)PlayerState.Idle;
            _reloading = false;
            _errorRetryCount = 0;
            _repeat = 0;
            if (!string.IsNullOrEmpty(Track.GetUrl())) _history.AddTrack(Track);
            Track = Track.New(_playerType, string.Empty, VRCUrl.Empty);
#if AUDIOLINK_V1
                if (Utilities.IsValid(_audioLink) && _useAudioLink)
                    _audioLink.SetMediaPlaying(MediaPlaying.Stopped);
#endif
            if (Networking.IsOwner(gameObject) && !_isLocal)
            {
                _syncTime = 0f;
                _serverTimeMilliseconds = 0;
                RequestSerialization();
            }
            foreach (Listener listener in _listeners) listener.OnVideoStop();
            PrintLog($"{_playerType.GetString()}: Video stop.");
        }

        public bool SlideMode
        {
            get => _slideMode;
            set
            {
                _slideMode = value;
                if (_slideMode) Pause();
                if (Networking.IsOwner(gameObject) && !_isLocal) RequestSerialization();
                foreach (Listener listener in _listeners) listener.OnSlideModeChanged();
                PrintLog($"Slide mode changed {_slideMode}.");
            }
        }

        public int SlideSeconds
        {
            get => _slideSeconds;
            set
            {
                _slideSeconds = value;
                if (Networking.IsOwner(gameObject) && !_isLocal) RequestSerialization();
                foreach (Listener listener in _listeners) listener.OnSlideModeChanged();
                PrintLog($"Slide seconds changed to {_slideSeconds}.");
            }
        }

        public int SlidePage => _slideMode && !Stopped ? Mathf.FloorToInt(VideoTime) / _slideSeconds + 1 : 0;

        public int SlidePageCount => _slideMode ? Mathf.FloorToInt(Duration) / _slideSeconds : 0;
        
        public bool Loop
        {
            get => _loop;
            set
            {
                _loop = value;
                foreach (PlayerHandler handler in _videoPlayerHandlers) handler.Loop = _loop;
#if AUDIOLINK_V1
                if (Utilities.IsValid(_audioLink) && _useAudioLink)
                    _audioLink.SetMediaLoop(_loop ? MediaLoop.LoopOne : MediaLoop.None);
#endif
                if (Networking.IsOwner(gameObject) && !_isLocal) RequestSerialization();
                foreach (Listener listener in _listeners) listener.OnLoopChanged();
                PrintLog($"Loop changed {_loop}.");
            }
        }

        public void UpdateSpeed()
        {
            foreach (PlayerHandler handler in _videoPlayerHandlers) handler.Speed = _speed;
            if (!Stopped && _playerType == VideoPlayerType.AVProVideoPlayer) 
                SendCustomEventDelayedFrames(nameof(Reload), 1);
            UpdateAudio();
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
                    SyncTime = VideoTime - VideoStandardDelay;
                    RequestSerialization();
                }
                foreach (Listener listener in _listeners) listener.OnSpeedChanged();
                PrintLog($"Speed changed {_speed:F2}x.");
            }
        }

        public void CheckRepeat()
        {
            RepeatStatus status = RepeatStatus.New(_repeat);
            if (!IsPlaying || !status.IsOn()) return;
            if (Handler.Time > status.GetEndTime() || Handler.Time < status.GetStartTime()) SetTime(status.GetStartTime());
            SendCustomEventDelayedSeconds(nameof(CheckRepeat), 0.5f);
        }

        public ulong Repeat
        {
            get => _repeat;
            set
            {
                _repeat = value;
                SendCustomEventDelayedFrames(nameof(CheckRepeat), 0);
                if (Networking.IsOwner(gameObject) && !_isLocal) RequestSerialization();
                foreach (Listener listener in _listeners) listener.OnRepeatChanged();
                RepeatStatus status = RepeatStatus.New(_repeat);
                if (status.IsOn()) PrintLog($"Repeat on, start: {status.GetStartTime()}, end: {status.GetEndTime()}.");
            }
        }

        public bool IsPlaying => Handler.IsPlaying;

        public float Duration => Handler.Duration;

        public float VideoTime => Handler.Time;

        public bool IsLoading => Handler.IsLoading;

        public bool IsReload => _reloading;

        public bool IsLive => float.IsInfinity(Duration);

        public void Reload()
        {
            if (!Stopped && !IsLoading) PlayTrack(Track, true);
        }

        public void ErrorRetry()
        {
            if (IsPlaying || !Track.GetUrl().IsValidUrl()) return;
            _resolveTrack.Invoke();
            foreach (Listener listener in _listeners) listener.OnVideoRetry();
        }

        public void SetPage(int page)
        {
            if (!_slideMode || page < 1 || page > SlidePageCount) return;
            SetTime(page * _slideSeconds - 0.5f);
        }

        public void SetTime(float time)
        {
            if (IsLive) return;
            Handler.Time = time;
            if (Networking.IsOwner(gameObject) && !_isLocal)
            {
                SyncTime = time - VideoStandardDelay;
                RequestSerialization();
            }
            foreach (Listener listener in _listeners) listener.OnSetTime(time);
            PrintLog($"{_playerType.GetString()}: Set video time: {time}.");
        }

        public void SendCustomVideoEvent(string eventName)
        {
            foreach (Listener listener in _listeners)
                if (Utilities.IsValid(listener)) listener.SendCustomEvent(eventName);
        }

        public override void OnDeserialization()
        {
            Track track = Track.New(_targetPlayer, _title, _url, _originalUrl);
            foreach (Listener listener in _listeners) listener.OnTrackSynced(track.GetUrl());
            if (track.GetUrl() != Track.GetUrl()) LoadTrack(track);
            DoSync(true);
        }

        #region Video Event
        public override void OnVideoReady()
        {
            _errorRetryCount = 0;
            if (State == PlayerState.Playing) Play(true);
            UpdateAudio();
            if (Networking.IsOwner(gameObject) && !_isLocal && !_reloading)
            {
                SyncTime = 0f;
                RequestSerialization();
            }
            else DoSync();
            if (KaraokeMode != KaraokeMode.None) SendCustomEventDelayedSeconds(nameof(ForceSync), 1f);
            foreach (Listener listener in _listeners) listener.OnVideoReady();
            PrintLog($"{_playerType.GetString()}: Video ready.");
            _reloading = false;
        }

        public override void OnVideoStart() 
        {
            foreach (Listener listener in _listeners) listener.OnVideoStart();
            PrintLog($"{_playerType.GetString()}: Video start.");
        }

        public override void OnVideoLoop()
        {
            if (Networking.IsOwner(gameObject) && !_isLocal)
            {
                SyncTime = 0f;
                RequestSerialization();
            }
            foreach (Listener listener in _listeners) listener.OnVideoLoop();
            PrintLog($"{_playerType.GetString()}: Video loop.");
        }

        public override void OnVideoEnd()
        {
            if (Networking.IsOwner(gameObject) || _isLocal)
            {
                if ((_activePlaylistIndex >= 0 || _queue.Length > 0) && _forwardInterval >= 0)
                    SendCustomEventDelayedSeconds(nameof(Forward), _forwardInterval);
                else Stop();
            }
            foreach (Listener listener in _listeners) listener.OnVideoEnd();
            PrintLog($"{_playerType.GetString()}: Video end.");
        }

        public override void OnVideoError(VideoError videoError)
        {
#if AUDIOLINK_V1
            if (Utilities.IsValid(_audioLink) && _useAudioLink)
                _audioLink.SetMediaPlaying(MediaPlaying.Error);
#endif
            if (videoError != VideoError.AccessDenied)
            {
                if (_errorRetryCount < _maxErrorRetry)
                {
                    _errorRetryCount++;
                    SendCustomEventDelayedSeconds(nameof(ErrorRetry), _retryAfterSeconds);
                } else _errorRetryCount = 0;
            }
            foreach (Listener listener in _listeners) listener.OnVideoError(videoError);
            PrintLog($"{_playerType.GetString()}: Video error {videoError}.");
        }
        #endregion
    }

    public enum PlayerState
    {
        Idle,
        Playing,
        Paused,
    }
}