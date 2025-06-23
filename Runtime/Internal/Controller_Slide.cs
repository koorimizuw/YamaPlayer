using VRC.SDKBase;
using UdonSharp;
using UnityEngine;
using System;

namespace Yamadev.YamaStream
{
    public partial class Controller
    {
        [SerializeField, UdonSynced, FieldChangeCallback(nameof(SlideMode))] bool _slideMode;
        [SerializeField, UdonSynced, FieldChangeCallback(nameof(SlideSeconds))] int _slideSeconds = 1;

        public int SlidePage => _slideMode && !Stopped ? Mathf.FloorToInt(VideoTime) / _slideSeconds + 1 : 0;

        public int SlidePageCount => _slideMode ? Mathf.FloorToInt(Duration) / _slideSeconds : 0;

        public bool SlideMode
        {
            get => _slideMode;
            set
            {
                if (_slideMode == value) return;

                _slideMode = value;
                if (_slideMode) Pause();
                if (Networking.IsOwner(gameObject) && !_isLocal) RequestSerialization();
                foreach (Listener listener in EventListeners) listener.OnSlideModeChanged();
                PrintLog($"Slide mode changed to {_slideMode}.");
            }
        }

        public int SlideSeconds
        {
            get => _slideSeconds;
            set
            {
                if (_slideSeconds == value) return;

                _slideSeconds = value;
                if (Networking.IsOwner(gameObject) && !_isLocal) RequestSerialization();
                foreach (Listener listener in EventListeners) listener.OnSlideModeChanged();
                PrintLog($"Slide seconds changed to {_slideSeconds}.");
            }
        }

        public void SetSlidePage(int page)
        {
            if (!_slideMode || page < 1 || page > SlidePageCount)
            {
                PrintError($"Invalid slide page: {page}");
                return;
            }

            SetTime(page * _slideSeconds - 0.5f);
        }

        [Obsolete("Use SetPage instead")]
        public void SetPage(int page)
        {
            SetSlidePage(page);
        }
    }
}