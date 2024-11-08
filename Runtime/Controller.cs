
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
    public partial class Controller : Listener
    {
        [SerializeField] string _version;
        [SerializeField] Animator _videoPlayerAnimator;
        [SerializeField] VideoPlayerHandle[] _videoPlayerHandles;
        [SerializeField] Permission _permission;
        [SerializeField] float _retryAfterSeconds = 5.1f;
        [SerializeField] int _maxErrorRetry = 5;
        [SerializeField] string _timeFormat = @"hh\:mm\:ss";
        [SerializeField, UdonSynced, FieldChangeCallback(nameof(VideoPlayerType))] VideoPlayerType _videoPlayerType;
        [SerializeField, UdonSynced, FieldChangeCallback(nameof(Loop))] bool _loop;
        [SerializeField, UdonSynced, FieldChangeCallback(nameof(SlideMode))] bool _slideMode;
        [SerializeField, UdonSynced, FieldChangeCallback(nameof(SlideSeconds))] int _slideSeconds = 1;
        [UdonSynced, FieldChangeCallback(nameof(Paused))] bool _paused;
        [UdonSynced, FieldChangeCallback(nameof(Stopped))] bool _stopped = true;
        [UdonSynced, FieldChangeCallback(nameof(Speed))] float _speed = 1f;
        [UdonSynced, FieldChangeCallback(nameof(Repeat))] Vector3 _repeat = new Vector3(0f, 0f, 999999f);
        Listener[] _listeners = { };
        bool _isLocal = false;
        int _errorRetryCount = 0;
        bool _isReload = false;
        float _lastSetTime = 0f;
        float _repeatCooling = 0.6f; 
        bool _initialized = false;

        void Start() => initialize();

        void Update()
        {
            if (OutOfRepeat(VideoTime) && Time.time - _lastSetTime > _repeatCooling) 
                SetTime(Repeat.ToRepeatStatus().GetStartTime());
            if (IsPlaying && Time.time - _syncFrequency > _lastSync) DoSync();
        }

        public string Version => _version;

        public Permission Permission => _permission;

        public PlayerPermission PlayerPermission => _permission == null ? PlayerPermission.Editor : _permission.PlayerPermission;

        void initialize()
        {
            if (_initialized) return;
            Loop = _loop;
            _videoPlayerAnimator.Rebind();
            initializeScreen();
            UpdateAudio();
            UpdateAudioLink();
            foreach (VideoPlayerHandle handle in _videoPlayerHandles)
                handle.Listener = this;
            _initialized = true;
        }

        public void AddListener(Listener listener)
        {
            if (Array.IndexOf(_listeners, listener) >= 0) return;
            _listeners = _listeners.Add(listener);
        }

        public bool IsLocal => _isLocal;

        public VideoPlayerType VideoPlayerType
        {
            get => _videoPlayerType;
            set 
            {
                if (_videoPlayerType == value) return;
                VideoPlayerHandle.Stop();
                _videoPlayerType = value;
                if (Networking.IsOwner(gameObject) && !_isLocal) RequestSerialization();
                foreach (Listener listener in _listeners) listener.OnPlayerChanged();
                PrintLog($"Video player change to {_videoPlayerType}.");
            }
        }

        public VideoPlayerHandle VideoPlayerHandle
        {
            get
            {
                foreach (VideoPlayerHandle handle in _videoPlayerHandles) 
                    if (handle.VideoPlayerType == _videoPlayerType) return handle;
                return null;
            }
        }

        public bool Paused
        {
            get => _paused;
            set
            {
                _paused = value;
                if (_paused) VideoPlayerHandle.Pause();
                else VideoPlayerHandle.Play();
#if AUDIOLINK_V1
                if (_audioLink != null && _useAudioLink)
                    _audioLink.SetMediaPlaying(_paused ? MediaPlaying.Paused : IsLive ? MediaPlaying.Streaming : MediaPlaying.Playing);
#endif
                if (Networking.IsOwner(gameObject) && !_isLocal)
                {
                    SyncTime = VideoTime - VideoStandardDelay;
                    RequestSerialization();
                }
            }
        }

        public bool Stopped
        {
            get => _stopped;
            set
            {
                _stopped = value;
                _isReload = false;
                if (_stopped) VideoPlayerHandle.Stop();
                if (Networking.IsOwner(gameObject) && !_isLocal) RequestSerialization();
            }
        }

        public bool SlideMode
        {
            get => _slideMode;
            set
            {
                _slideMode = value;
                if (!_paused) Paused = true;
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

        public int SlidePage => _slideMode && !_stopped ? Mathf.FloorToInt(VideoTime) / _slideSeconds + 1 : 0;

        public int SlidePageCount => _slideMode ? Mathf.FloorToInt(Duration) / _slideSeconds : 0;
        
        public bool Loop
        {
            get => _loop;
            set
            {
                _loop = value;
                foreach (VideoPlayerHandle handle in _videoPlayerHandles) handle.Loop = _loop;
#if AUDIOLINK_V1
                if (_audioLink != null && _useAudioLink)
                    _audioLink.SetMediaLoop(_loop ? MediaLoop.LoopOne : MediaLoop.None);
#endif
                if (Networking.IsOwner(gameObject) && !_isLocal) RequestSerialization();
                foreach (Listener listener in _listeners) listener.OnLoopChanged();
                PrintLog($"Loop changed {_loop}.");
            }
        }

        public void UpdateSpeed()
        {
            _videoPlayerAnimator.SetFloat("Speed", _speed);
            _videoPlayerAnimator.Update(0f);
            if (!_stopped && _videoPlayerType == VideoPlayerType.AVProVideoPlayer) 
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

        public bool OutOfRepeat(float targetTime)
        {
            if (!IsPlaying || !Repeat.ToRepeatStatus().IsOn()) return false;
            return targetTime > Repeat.ToRepeatStatus().GetEndTime() || targetTime < Repeat.ToRepeatStatus().GetStartTime();

        }

        public Vector3 Repeat
        {
            get => _repeat;
            set
            {
                _repeat = value;
                if (Networking.IsOwner(gameObject) && !_isLocal) RequestSerialization();
                foreach (Listener listener in _listeners) listener.OnRepeatChanged();
                RepeatStatus status = _repeat.ToRepeatStatus();
                if (status.IsOn()) PrintLog($"Repeat on, start: {status.GetStartTime()}, end: {status.GetEndTime()}.");
                else PrintLog($"Repeat off.");
            }
        }

        public float LastLoaded => VideoPlayerHandle.LastLoaded;
        public bool IsPlaying => VideoPlayerHandle.IsPlaying;
        public float Duration => VideoPlayerHandle.Duration;
        public float VideoTime => VideoPlayerHandle.VideoTime;
        public bool IsLoading => VideoPlayerHandle.IsLoading;
        public bool IsReload => _isReload;
        public bool IsLive => float.IsInfinity(Duration);

        public void Reload()
        {
            if (!Stopped && !IsLoading) PlayTrack(Track, true);
        }

        public void ErrorRetry()
        {
            if (IsPlaying || !Track.GetUrl().IsValidUrl()) return;
            if (Time.time - LastLoaded < _retryAfterSeconds)
            {
                SendCustomEventDelayedFrames(nameof(ErrorRetry), 0);
                return;
            }
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
            if (IsLive || OutOfRepeat(time)) return;
            VideoPlayerHandle.VideoTime = time;
            _lastSetTime = Time.time;
            if (Networking.IsOwner(gameObject) && !_isLocal)
            {
                SyncTime = time - VideoStandardDelay;
                RequestSerialization();
            }
            foreach (Listener listener in _listeners) listener.OnSetTime(time);
            PrintLog($"{_videoPlayerType}: Set video time: {time}.");
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
            if (track.GetUrl() != Track.GetUrl())
            {
                Stopped = true;
                PlayTrack(track);
            }
            DoSync(true);
        }

        #region Video Event
        public override void OnVideoReady()
        {
            foreach (Listener listener in _listeners) listener.OnVideoReady();
            PrintLog($"{_videoPlayerType}: Video ready.");
        }

        public override void OnVideoStart() 
        {
            _errorRetryCount = 0;
            _stopped = false;
            if (_paused || _slideMode) VideoPlayerHandle.Pause();
            else VideoPlayerHandle.Play();
            UpdateAudio();
#if AUDIOLINK_V1
            if (_audioLink != null && _useAudioLink)
                _audioLink.SetMediaPlaying(IsLive ? MediaPlaying.Streaming : MediaPlaying.Playing);
#endif
            if (Networking.IsOwner(gameObject) && !_isLocal && !_isReload)
            {
                SyncTime = 0f;
                RequestSerialization();
            }
            else DoSync();
            if (KaraokeMode != KaraokeMode.None) SendCustomEventDelayedSeconds(nameof(ForceSync), 1f);
            foreach (Listener listener in _listeners) listener.OnVideoStart();
            PrintLog($"{_videoPlayerType}: Video start.");
            _isReload = false;
        }

        public override void OnVideoPlay()
        {
            _paused = false;
            if (KaraokeMode != KaraokeMode.None) SendCustomEventDelayedSeconds(nameof(ForceSync), 1f);
            foreach (Listener listener in _listeners) listener.OnVideoPlay();
            PrintLog($"{_videoPlayerType}: Video play.");
        }

        public override void OnVideoPause()
        {
            _paused = true;
            foreach (Listener listener in _listeners) listener.OnVideoPause();
            PrintLog($"{_videoPlayerType}: Video pause.");
        }

        public override void OnVideoStop()
        {
            if (!_isReload)
            {
                _paused = false;
                _stopped = true;
                _errorRetryCount = 0;
                _repeat = new Vector3(0f, 0f, 999999f);
                if (!string.IsNullOrEmpty(Track.GetUrl())) _history.AddTrack(Track);
                Track = Track.New(_videoPlayerType, string.Empty, VRCUrl.Empty);
#if AUDIOLINK_V1
                if (_audioLink != null && _useAudioLink)
                    _audioLink.SetMediaPlaying(MediaPlaying.Stopped);
#endif
                if (Networking.IsOwner(gameObject) && !_isLocal)
                {
                    ClearSync();
                    RequestSerialization();
                }
            }
            foreach (Listener listener in _listeners) listener.OnVideoStop();
            PrintLog($"{_videoPlayerType}: Video stop.");
        }

        public override void OnVideoLoop()
        {
            if (Networking.IsOwner(gameObject) && !_isLocal)
            {
                SyncTime = 0f;
                RequestSerialization();
            }
            foreach (Listener listener in _listeners) listener.OnVideoLoop();
            PrintLog($"{_videoPlayerType}: Video loop.");
        }

        public override void OnVideoEnd()
        {
            if (Networking.IsOwner(gameObject) && !_isLocal && _forwardInterval >= 0)
                SendCustomEventDelayedSeconds(nameof(RunForward), _forwardInterval);
            foreach (Listener listener in _listeners) listener.OnVideoEnd();
            PrintLog($"{_videoPlayerType}: Video end.");
        }

        public override void OnVideoError(VideoError videoError)
        {
#if AUDIOLINK_V1
            if (_audioLink != null && _useAudioLink)
                _audioLink.SetMediaPlaying(MediaPlaying.Error);
#endif
            if (videoError != VideoError.AccessDenied)
            {
                if (_errorRetryCount < _maxErrorRetry)
                {
                    _errorRetryCount++;
                    SendCustomEventDelayedFrames(nameof(ErrorRetry), 0);
                } else _errorRetryCount = 0;
            }
            foreach (Listener listener in _listeners) listener.OnVideoError(videoError);
            PrintLog($"{_videoPlayerType}: Video error {videoError}.");
        }
        #endregion
    }
}