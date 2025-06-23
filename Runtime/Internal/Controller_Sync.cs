using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
    public partial class Controller
    {
        [SerializeField] LatencyManager _latencyManager;
        [SerializeField, Range(1f, 10f)] float _syncFrequency = 5.0f;
        [SerializeField, Range(0f, 1f)] float _syncMargin = 0.3f;
        [UdonSynced] float _syncedVideoTime = 0f;
        [UdonSynced] int _serverTimeMilliseconds = 0;
        [UdonSynced, FieldChangeCallback(nameof(KaraokeMode))] KaraokeMode _karaokeMode = KaraokeMode.None;
        [UdonSynced, FieldChangeCallback(nameof(KaraokeMembers))] string[] _karaokeMembers = new string[0];
        float _lastSync = 0f;
        float _localDelay = 0f;
        float _networkDelay = 0f;
        float _defaultKaraokeDelay = 0.5f;

        public float LocalDelay
        {
            get => _localDelay;
            set
            {
                _localDelay = value;
                EnsureVideoTime(true);
                foreach (Listener listener in EventListeners) listener.OnLocalDelayChanged();
            }
        }

        public bool IsKaraokeMember
        {
            get
            {
                if (!Utilities.IsValid(Networking.LocalPlayer)) return false;
                return Array.IndexOf(_karaokeMembers, Networking.LocalPlayer.displayName) >= 0;
            }
        }

        public string[] KaraokeMembers
        {
            get => _karaokeMembers;
            set
            {
                if (_karaokeMode == KaraokeMode.None) return;
                _karaokeMembers = value;
                if (Networking.IsOwner(gameObject) && !_isLocal) RequestSerialization();
                foreach (Listener listener in EventListeners) listener.OnKaraokeMemberChanged();
            }
        }

        public KaraokeMode KaraokeMode
        {
            get => _karaokeMode;
            set
            {
                _karaokeMode = value;
                if (value == KaraokeMode.None) KaraokeMembers = new string[0];
                if (_karaokeMode != KaraokeMode.None && Utilities.IsValid(_latencyManager)) _latencyManager.RequestRecord();
                if (!_isLocal) EnsureVideoTime(true);
                if (Networking.IsOwner(gameObject) && !_isLocal) RequestSerialization();
                foreach (Listener listener in EventListeners) listener.OnKaraokeModeChanged();
            }
        }

        public float NetworkDelay => _latencyManager != null ? _networkDelay : 0.1f;
        public float KaraokeDelay => _karaokeMode == KaraokeMode.None ? 0f :
                !IsKaraokeMember ? -NetworkDelay :
                NetworkDelay == 0f ? _defaultKaraokeDelay :
                _karaokeMode == KaraokeMode.Karaoke ? _defaultKaraokeDelay + NetworkDelay * 2 :
                _karaokeMode == KaraokeMode.Dance ? NetworkDelay : 0f;

        public float VideoStandardDelay => KaraokeDelay + _localDelay;

        public float SyncedVideoTime
        {
            get => _syncedVideoTime;
            set
            {
                _syncedVideoTime = Mathf.Clamp(value, 0f, Duration);
                _serverTimeMilliseconds = Networking.GetServerTimeInMilliseconds();
            }
        }

        public void EnsureVideoTime(bool force = false)
        {
            if (IsLive || Stopped || _serverTimeMilliseconds == 0) return;

            float offset = Paused ? 0 : (Networking.GetServerTimeInMilliseconds() - _serverTimeMilliseconds) / 1000f * Speed;
            float targetTime = Mathf.Clamp(_syncedVideoTime + offset + VideoStandardDelay, 0f, Duration);
            if (force || Mathf.Abs(VideoTime - targetTime) >= _syncMargin)
            {
                SetTime(targetTime);
            }

            _lastSync = Time.time;
        }

        public void UpdateNetworkDelay()
        {
            if (_latencyManager == null) return;
            _networkDelay = Mathf.Clamp(_latencyManager.GetServerDelayseconds(), 0, 1);
        }

        [Obsolete("Use EnsureVideoTime instead.")]
        public void DoSync(bool force = false) => EnsureVideoTime(force);

        [Obsolete("Use EnsureVideoTime instead.")]
        public void ForceSync() => EnsureVideoTime(true);

        [Obsolete("Use SyncedVideoTime instead.")]
        public float SyncTime => SyncedVideoTime;
    }
}