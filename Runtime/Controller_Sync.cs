
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
    public partial class Controller : Listener
    {
        [SerializeField] LatencyManager _latencyManager;
        [SerializeField, Range(1f, 10f)] float _syncFrequency = 5.0f;
        [SerializeField, Range(0f, 1f)] float _syncMargin = 0.3f;
        [UdonSynced] float _syncTime = 0f;
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
                DoSync(true);
                foreach (Listener listener in _listeners) listener.OnLocalDelayChanged();
            }
        }

        public bool IsKaraokeMember => Networking.LocalPlayer != null ? Array.IndexOf(_karaokeMembers, Networking.LocalPlayer.displayName) >= 0 : false;

        public KaraokeMode KaraokeMode
        {
            get => _karaokeMode;
            set
            {
                _karaokeMode = value;
                if (value == KaraokeMode.None) KaraokeMembers = new string[0];
                if (_karaokeMode != KaraokeMode.None && _latencyManager != null) _latencyManager.RequestRecord();
                if (!_isLocal) DoSync(true);
                if (Networking.IsOwner(gameObject) && !_isLocal) RequestSerialization();
                foreach (Listener listener in _listeners) listener.OnKaraokeModeChanged();
            }
        }

        public string[] KaraokeMembers
        {
            get => _karaokeMembers;
            set
            {
                if (IsKaraokeMember ^ Array.IndexOf(value, Networking.LocalPlayer.displayName) >= 0)
                {
                    _karaokeMembers = value;
                    ForceSync();
                } else _karaokeMembers = value;
                if (Networking.IsOwner(gameObject) && !_isLocal) RequestSerialization();
                foreach (Listener listener in _listeners) listener.OnKaraokeMemberChanged();
            }
        }

        public float NetworkDelay => _latencyManager != null ? _networkDelay : 0.1f;
        public float KaraokeDelay => _karaokeMode == KaraokeMode.None ? 0f :
                !IsKaraokeMember ? -NetworkDelay :
                NetworkDelay == 0f ? _defaultKaraokeDelay :
                _karaokeMode == KaraokeMode.Karaoke ? _defaultKaraokeDelay + NetworkDelay * 2 :
                _karaokeMode == KaraokeMode.Dance ? NetworkDelay : 0f;

        public float VideoStandardDelay => KaraokeDelay + _localDelay;

        public float SyncTime
        {
            get => _syncTime;
            set
            {
                _syncTime = Mathf.Clamp(value, 0f, Duration);
                _serverTimeMilliseconds = Networking.GetServerTimeInMilliseconds();
            }
        }

        public void ClearSync()
        {
            _syncTime = 0f;
            _serverTimeMilliseconds = 0;
        }

        public float NetworkOffset => Paused ? 0 : (Networking.GetServerTimeInMilliseconds() - _serverTimeMilliseconds) / 1000f * Speed;
        public void ForceSync() => DoSync(true);
        public void DoSync(bool force = false)
        {
            if (IsLive || Stopped || _serverTimeMilliseconds == 0) return;
            float targetTime = Mathf.Clamp(_syncTime + NetworkOffset + VideoStandardDelay, 0f, Duration);
            float timeMargin = Mathf.Abs(VideoTime - targetTime);
            if (force || timeMargin >= _syncMargin) VideoPlayerHandle.Time = targetTime;
            _lastSync = Time.time;
        }

        public void UpdateNetworkDelay()
        {
            if (_latencyManager == null) return;
            _networkDelay = Mathf.Clamp(_latencyManager.GetServerDelayseconds(), 0, 1);
        }
    }
}