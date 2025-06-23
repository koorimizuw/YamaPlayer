using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

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
        [UdonSynced, FieldChangeCallback(nameof(SyncedState))] byte _state;
        [UdonSynced, FieldChangeCallback(nameof(Speed))] float _speed = 1f;
        [UdonSynced, FieldChangeCallback(nameof(Repeat))] ulong _repeat;
        private PlayerHandler _handler;
        private Listener[] _listeners;
        private int _errorRetryCount = 0;
        private bool _reloading;

        private void Start()
        {
            EnsurePlayerHandler();
            SetupHandlers();

            UpdateScreenMaterial();
            if (Utilities.IsValid(Handler)) Handler.MaxResolution = _maxResolution;

            UpdateAudioVolume();
            UpdateAudioPitch();

            RegisterHandlerListeners();
        }

        private void Update()
        {
            if (IsPlaying && Time.time - _syncFrequency > _lastSync)
            {
                EnsureVideoTime();
            }
        }

        public Permission Permission => _permission;

        public string Version => _version;

        public bool IsLocal => _isLocal;

        public PlayerPermission PlayerPermission => Utilities.IsValid(_permission) ? PlayerPermission.Editor : _permission.PlayerPermission;

        public Listener[] EventListeners
        {
            get
            {
                if (!Utilities.IsValid(_listeners))
                {
                    _listeners = new Listener[0];
                }

                return _listeners;
            }
            set => _listeners = value;
        }

        public void AddListener(Listener listener)
        {
            if (Array.IndexOf(EventListeners, listener) >= 0) return;
            EventListeners = EventListeners.Add(listener);
        }

        private void RegisterHandlerListeners()
        {
            foreach (PlayerHandler handler in _videoPlayerHandlers)
            {
                if (Utilities.IsValid(handler)) handler.SetListener(this);
            }
        }

        private void SetupHandlers()
        {
            foreach (PlayerHandler handler in _videoPlayerHandlers)
            {
                if (Utilities.IsValid(handler)) handler.Loop = _loop;
            }
        }

        public PlayerHandler Handler
        {
            get
            {
                if (!Utilities.IsValid(_handler)) EnsurePlayerHandler();
                return _handler;
            }
        }

        public VideoPlayerType PlayerType
        {
            get => _playerType;
            set
            {
                if (_playerType == value) return;
                Stop();

                var oldPlayerType = _playerType;
                _playerType = value;

                if (Networking.IsOwner(gameObject) && !_isLocal)
                {
                    RequestSerialization();
                }

                EnsurePlayerHandler();
                foreach (Listener listener in EventListeners) listener.OnPlayerHandlerChanged();
                PrintLog($"Video player changed from {oldPlayerType.GetString()} to {_playerType.GetString()}.");
            }
        }

        private void EnsurePlayerHandler()
        {
            foreach (PlayerHandler handler in _videoPlayerHandlers)
            {
                if (!Utilities.IsValid(handler)) continue;
                if (handler.Type == _playerType)
                {
                    _handler = handler;
                    return;
                }
            }
            _handler = null;
            PrintError("No player handler found.");
        }

        public PlayerState State => (PlayerState)_state;

        private byte SyncedState
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
            if (KaraokeMode != KaraokeMode.None) SendCustomEventDelayedSeconds(nameof(EnsureVideoTime), 1f);

            UpdateAudioLinkMaterial("_MediaPlaying", IsLive ? 5 : 1);
            PlayTimeline(SyncedVideoTime);

            if (Networking.IsOwner(gameObject) && !_isLocal && !_reloading)
            {
                SyncedVideoTime = VideoTime - VideoStandardDelay;
                RequestSerialization();
            }

            foreach (Listener listener in EventListeners) listener.OnVideoPlay();
            PrintLog($"{_playerType.GetString()}: Video play.");
        }

        public void Pause()
        {
            if (State == PlayerState.Paused) return;
            Handler.Pause();
            _state = (byte)PlayerState.Paused;

            PauseTimeline(SyncedVideoTime);
            UpdateAudioLinkMaterial("_MediaPlaying", 2);

            if (Networking.IsOwner(gameObject) && !_isLocal)
            {
                SyncedVideoTime = VideoTime - VideoStandardDelay;
                RequestSerialization();
            }

            foreach (Listener listener in EventListeners) listener.OnVideoPause();
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

            StopTimeline();
            UpdateAudioLinkMaterial("_MediaPlaying", 3);

            if (!string.IsNullOrEmpty(Track.GetUrl())) _history.AddTrack(Track);
            Track = Track.New(_playerType, string.Empty, VRCUrl.Empty);

            if (Networking.IsOwner(gameObject) && !_isLocal)
            {
                _syncedVideoTime = 0f;
                _serverTimeMilliseconds = 0;
                RequestSerialization();
            }

            foreach (Listener listener in EventListeners) listener.OnVideoStop();
            PrintLog($"{_playerType.GetString()}: Video stop.");
        }

        public bool Loop
        {
            get => _loop;
            set
            {
                _loop = value;
                foreach (PlayerHandler handler in _videoPlayerHandlers) handler.Loop = _loop;
                if (Utilities.IsValid(_audioLink) && _useAudioLink)
                {
                    ((Material)_audioLink.GetProgramVariable("audioMaterial")).SetFloat("_MediaPlaying", _loop ? 2 : 0);
                }

                if (Networking.IsOwner(gameObject) && !_isLocal) RequestSerialization();
                foreach (Listener listener in EventListeners) listener.OnLoopChanged();
                PrintLog($"Loop changed {_loop}.");
            }
        }

        public void UpdateSpeed()
        {
            foreach (PlayerHandler handler in _videoPlayerHandlers) handler.Speed = _speed;
            if (!Stopped && _playerType == VideoPlayerType.AVProVideoPlayer)
                SendCustomEventDelayedFrames(nameof(Reload), 1);
            UpdateAudioVolume();
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
                    SyncedVideoTime = VideoTime - VideoStandardDelay;
                    RequestSerialization();
                }
                foreach (Listener listener in EventListeners) listener.OnSpeedChanged();
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

        private ulong Repeat
        {
            get => _repeat;
            set
            {
                _repeat = value;
                SendCustomEventDelayedFrames(nameof(CheckRepeat), 0);
                if (Networking.IsOwner(gameObject) && !_isLocal) RequestSerialization();
                foreach (Listener listener in EventListeners) listener.OnRepeatChanged();
                RepeatStatus status = RepeatStatus.New(_repeat);
                if (status.IsOn()) PrintLog($"Repeat on, start: {status.GetStartTime()}, end: {status.GetEndTime()}.");
            }
        }

        public RepeatStatus RepeatStatus
        {
            get => RepeatStatus.New(_repeat);
            set => Repeat = value.Pack();
        }

        public bool IsPlaying => Handler.IsPlaying;

        public float Duration => Handler.Duration;

        public float VideoTime => Handler.Time;

        public bool IsLoading => Handler.IsLoading;

        public bool IsLive => float.IsInfinity(Duration);

        public void SetTime(float time)
        {
            if (IsLive)
            {
                PrintError("Cannot set time for live video");
                return;
            }

            Handler.Time = time;
            SetTimelineTime(time);
            if (Networking.IsOwner(gameObject) && !_isLocal)
            {
                SyncedVideoTime = time - VideoStandardDelay;
                RequestSerialization();
            }

            foreach (Listener listener in EventListeners) listener.OnSetTime(time);
            PrintLog($"{_playerType.GetString()}: Set video time: {time}.");
        }

        public void SendCustomVideoEvent(string eventName)
        {
            foreach (Listener listener in EventListeners)
            {
                if (!Utilities.IsValid(listener)) continue;
                listener.SendCustomEvent(eventName);
            }
        }

        public override void OnDeserialization()
        {
            Track track;
            if (Utilities.IsValid(ActivePlaylist) && _playingTrackIndex >= 0)
            {
                track = ActivePlaylist.GetTrack(_playingTrackIndex);
            }
            else
            {
                track = Track.New(_targetPlayer, _title, _url, _originalUrl);
            }
            foreach (Listener listener in EventListeners) listener.OnTrackSynced(track.GetUrl());

            if (track.GetUrl() != Track.GetUrl())
            {
                StopTimeline();
                LoadTrack(track);
            }

            EnsureVideoTime();
        }
    }

    public enum PlayerState
    {
        Idle,
        Playing,
        Paused,
    }
}