using System;
using UnityEngine;
using UdonSharp;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
    public partial class Controller
    {
        [SerializeField] private UdonSharpBehaviour _audioLink;
        [SerializeField] private bool _useAudioLink;
        [SerializeField] private bool _mute;
        [SerializeField, Range(0f, 1f)] private float _volume;
        [SerializeField] private AudioSource[] _audioSources;
        private Material _audioLinkMaterial;

        private AudioSource[] AudioSources
        {
            get
            {
                if (!Utilities.IsValid(_audioSources)) _audioSources = new AudioSource[0];
                return _audioSources;
            }
            set => _audioSources = value;
        }

        private AudioSource MainAudioSource => AudioSources.Length > 0 ? AudioSources[0] : null;

        public void AddAudioSource(AudioSource audioSource)
        {
            if (Array.IndexOf(AudioSources, audioSource) >= 0)
            {
                PrintWarning($"Audio source {audioSource.name} already added.");
                return;
            }

            AudioSources = AudioSources.Add(audioSource);
            UpdateAudioVolume();
            UpdateAudioPitch();
        }

        private void UpdateAudioVolume()
        {
            foreach (AudioSource audioSource in AudioSources)
            {
                if (!Utilities.IsValid(audioSource)) continue;

                audioSource.volume = _volume;
                audioSource.mute = _mute;

                UpdateAudioLinkMaterial("_MediaVolume", _mute ? 0 : _volume);
            }
        }

        private void UpdateAudioPitch()
        {
            foreach (AudioSource audioSource in AudioSources)
            {
                if (!Utilities.IsValid(audioSource)) continue;
                audioSource.pitch = IsLive ? 1 : _speed;
            }
        }

        public float Volume
        {
            get => _volume;
            set
            {
                _volume = Mathf.Clamp01(value);
                UpdateAudioVolume();
                PrintLog($"Volume changed to {_volume * 100}%.");
                foreach (Listener listener in EventListeners) listener.OnVolumeChanged();
            }
        }

        public bool Mute
        {
            get => _mute;
            set
            {
                _mute = value;
                UpdateAudioVolume();
                PrintLog($"Mute changed to {_mute}.");
                foreach (Listener listener in EventListeners) listener.OnMuteChanged();
            }
        }

        #region Audio Link
        public bool AudioLinkAssigned => Utilities.IsValid(_audioLink);

        public bool AudioLinkEnabled
        {
            get
            {
                if (!AudioLinkAssigned) return false;

                object audioLinkEnabled = _audioLink.GetProgramVariable("_audioLinkEnabled");
                object audioSource = _audioLink.GetProgramVariable("audioSource");
                if (!Utilities.IsValid(audioLinkEnabled) || !Utilities.IsValid(audioSource)) return false;

                return (bool)audioLinkEnabled && (AudioSource)audioSource == MainAudioSource;
            }
            set
            {
                if (!AudioLinkAssigned) return;

                if (value)
                {
                    _audioLink.SetProgramVariable("audioSource", MainAudioSource);
                    _audioLink.SendCustomEvent("EnableAudioLink");
                }
                else
                {
                    _audioLink.SetProgramVariable("audioSource", null);
                    _audioLink.SendCustomEvent("DisableAudioLink");
                }
            }
        }

        private void UpdateAudioLinkMaterial(string property, float value)
        {
            if (!AudioLinkEnabled) return;

            if (!Utilities.IsValid(_audioLinkMaterial))
            {
                _audioLinkMaterial = (Material)_audioLink.GetProgramVariable("audioMaterial");
            }

            _audioLinkMaterial.SetFloat(property, value);
        }

        [Obsolete("Use AudioLinkAssigned instead.")]
        public bool UseAudioLink => AudioLinkAssigned;

        [Obsolete("Use AudioLinkEnabled instead.")]
        public bool AudioLinked => AudioLinkEnabled;
        #endregion
    }
}