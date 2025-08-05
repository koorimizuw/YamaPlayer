using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
    public partial class Controller
    {
        [SerializeField, Range(1f, 10f)] float _syncFrequency = 5.0f;
        [SerializeField, Range(0f, 1f)] float _syncMargin = 0.3f;
        [UdonSynced] float _syncedVideoTime = 0f;
        [UdonSynced] int _serverTimeMilliseconds = 0;
        float _lastSync = 0f;
        float _localDelay = 0f;

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
            float targetTime = Mathf.Clamp(_syncedVideoTime + offset + _localDelay, 0f, Duration);
            if (force || Mathf.Abs(VideoTime - targetTime) >= _syncMargin)
            {
                SetTime(targetTime);
            }

            _lastSync = Time.time;
        }

        [Obsolete("Use EnsureVideoTime instead.")]
        public void DoSync(bool force = false) => EnsureVideoTime(force);

        [Obsolete("Use EnsureVideoTime instead.")]
        public void ForceSync() => EnsureVideoTime(true);

        [Obsolete("Use SyncedVideoTime instead.")]
        public float SyncTime => SyncedVideoTime;

        [Obsolete("Use LocalDelay instead.")]
        public float VideoStandardDelay => _localDelay;
    }
}