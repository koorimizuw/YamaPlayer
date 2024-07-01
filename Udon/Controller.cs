
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public partial class Controller : Listener
    {
        [SerializeField] string _version;
        [SerializeField] Animator _videoPlayerAnimator;
        [SerializeField] VideoPlayerHandle[] _videoPlayerHandles;
        [SerializeField] Permission _permission;
        [SerializeField] float _retryAfterSeconds = 5;
        [SerializeField] int _maxErrorRetry = 3;
        [SerializeField] string _timeFormat = @"hh\:mm\:ss";
        [SerializeField, UdonSynced, FieldChangeCallback(nameof(VideoPlayerType))] VideoPlayerType _videoPlayerType;
        [SerializeField, UdonSynced, FieldChangeCallback(nameof(Loop))] bool _loop = false;
        [UdonSynced, FieldChangeCallback(nameof(Paused))] bool _paused = false;
        [UdonSynced, FieldChangeCallback(nameof(Stopped))] bool _stopped = true;
        [UdonSynced, FieldChangeCallback(nameof(Speed))] float _speed = 1f;
        [UdonSynced, FieldChangeCallback(nameof(Repeat))] Vector3 _repeat = new Vector3(0f, 0f, 999999f);
        Listener[] _listeners = { };
        bool _isLocal = false;
        int _errorRetryCount = 0;
        bool _loading = false;
        bool _isReload = false;
        float _lastSetTime = 0f;
        float _setTimeCooling = 0.6f; 
        bool _initialized = false;

        void Start() => initialize();

        void Update()
        {
            checkRepeat();
            renderScreen();
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
            initializeTrack();
            initializeScreen();
            UpdateAudio();
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
                updateProperties();
                if (value != VideoPlayerType.UnityVideoPlayer) Speed = 1f;
                if (Networking.IsOwner(gameObject) && !_isLocal) RequestSerialization();
                foreach (Listener listener in _listeners) listener.OnPlayerChanged();
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
                if (value) VideoPlayerHandle.Stop();
                if (Networking.IsOwner(gameObject) && !_isLocal) RequestSerialization();
            }
        }

        public bool Loop
        {
            get => _loop;
            set
            {
                _loop = value;
                foreach (VideoPlayerHandle handle in _videoPlayerHandles) handle.Loop = value;
                if (Networking.IsOwner(gameObject) && !_isLocal) RequestSerialization();
                foreach (Listener listener in _listeners) listener.OnLoopChanged();
            }
        }

        public float Speed
        {
            get => _speed;
            set
            {
                _speed = VideoPlayerType == VideoPlayerType.UnityVideoPlayer ? value : 1f;
                if (!_stopped) _videoPlayerAnimator.SetFloat("Speed", value);
                if (Networking.IsOwner(gameObject) && !_isLocal)
                {
                    SyncTime = VideoTime - VideoStandardDelay;
                    RequestSerialization();
                }
                foreach (Listener listener in _listeners) listener.OnSpeedChanged();
            }
        }

        void checkRepeat()
        {
            if (!IsPlaying || !Repeat.ToRepeatStatus().IsOn()) return;
            if (VideoTime > Repeat.ToRepeatStatus().GetEndTime() || VideoTime < Repeat.ToRepeatStatus().GetStartTime()) SetTime(Repeat.ToRepeatStatus().GetStartTime());
        }

        public Vector3 Repeat
        {
            get => _repeat;
            set
            {
                _repeat = value;
                if (Networking.IsOwner(gameObject) && !_isLocal) RequestSerialization();
                foreach (Listener listener in _listeners) listener.OnRepeatChanged();
            }
        }
        public bool IsPlaying => VideoPlayerHandle.IsPlaying;
        public float Duration => VideoPlayerHandle.Duration;
        public float VideoTime => VideoPlayerHandle.Time;
        public bool IsLoading => _loading;
        public bool IsReload => _isReload;
        public bool IsLive => float.IsInfinity(Duration);

        public void Reload()
        {
            if (!Stopped && !IsLoading) PlayTrack(Track, true);
        }

        public void ErrorRetry()
        {
            if (IsPlaying) return;
            _resolveTrack.Invoke();
            foreach (Listener listener in _listeners) listener.OnVideoRetry();
        }

        public void SetTime(float time)
        {
            if (IsLive || Time.time - _lastSetTime < _setTimeCooling) return;
            if (Repeat.ToRepeatStatus().IsOn() && (time < Repeat.ToRepeatStatus().GetStartTime() || time > Repeat.ToRepeatStatus().GetEndTime())) return;
            VideoPlayerHandle.Time = time;
            _lastSetTime = Time.time;
            if (Networking.IsOwner(gameObject) && !_isLocal)
            {
                SyncTime = time - VideoStandardDelay;
                RequestSerialization();
            }
            foreach (Listener listener in _listeners) listener.OnSetTime(time);
        }

        public void SendCustomVideoEvent(string eventName)
        {
            foreach (Listener listener in _listeners) listener.SendCustomEvent(eventName);
        }

        public override void OnDeserialization()
        {
            Track track = Track.New(_targetPlayer, _title, _url, _originalUrl);
            foreach (Listener listener in _listeners) listener.OnTrackSynced(track.GetUrl());
            if (track.GetUrl() != Track.GetUrl()) PlayTrack(track);
            DoSync(true);
        }

        #region Video Event
        public override void OnVideoReady()
        {
            _loading = false;
            if (_paused) VideoPlayerHandle.Pause();
            else VideoPlayerHandle.Play();
            _videoPlayerAnimator.SetFloat("Speed", _speed);
            foreach (Listener listener in _listeners) listener.OnVideoReady();
        }

        public override void OnVideoStart() 
        {
            _errorRetryCount = 0;
            _loading = false;
            _stopped = false;
            if (Networking.IsOwner(gameObject) && !_isLocal && !_isReload)
            {
                SyncTime = 0f;
                RequestSerialization();
            }
            else DoSync();
            if (KaraokeMode != KaraokeMode.None) SendCustomEventDelayedSeconds(nameof(ForceSync), 1f);
            foreach (Listener listener in _listeners) listener.OnVideoStart();
            _isReload = false;
        }

        public override void OnVideoPlay()
        {
            _paused = false;
            if (KaraokeMode != KaraokeMode.None) SendCustomEventDelayedSeconds(nameof(ForceSync), 1f);
            foreach (Listener listener in _listeners) listener.OnVideoPlay();
        }

        public override void OnVideoPause()
        {
            _paused = true;
            foreach (Listener listener in _listeners) listener.OnVideoPause();
        }

        public override void OnVideoStop()
        {
            if (!_isReload)
            {
                _paused = false;
                _stopped = true;
                _repeat = new Vector3(0f, 0f, 999999f);
                if (!string.IsNullOrEmpty(Track.GetUrl())) _history.AddTrack(Track);
                Track = Track.New(_videoPlayerType, string.Empty, VRCUrl.Empty);
                if (Networking.IsOwner(gameObject) && !_isLocal)
                {
                    ClearSync();
                    RequestSerialization();
                }
            }
            foreach (Listener listener in _listeners) listener.OnVideoStop();
        }

        public override void OnVideoLoop()
        {
            if (Networking.IsOwner(gameObject) && !_isLocal)
            {
                SyncTime = 0f;
                RequestSerialization();
            }
            foreach (Listener listener in _listeners) listener.OnVideoLoop();
        }
        public override void OnVideoEnd()
        {
            if (Networking.IsOwner(gameObject) && !_isLocal) SendCustomEventDelayedFrames(nameof(Forward), 0);
            foreach (Listener listener in _listeners) listener.OnVideoEnd();
        }

        public override void OnVideoError(VideoError videoError)
        {
            _loading = false;
            if (videoError != VideoError.InvalidURL && videoError != VideoError.AccessDenied && videoError != VideoError.PlayerError)
            {
                if (_errorRetryCount < _maxErrorRetry)
                {
                    _errorRetryCount++;
                    SendCustomEventDelayedSeconds(nameof(ErrorRetry), _retryAfterSeconds);
                } else _errorRetryCount = 0;
            }
            foreach (Listener listener in _listeners) listener.OnVideoError(videoError);
        }
        #endregion
    }
}