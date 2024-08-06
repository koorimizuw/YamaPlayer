
using System;
using UnityEngine;
using UdonSharp;

#if AUDIOLINK_V1
using AudioLink;
#endif

namespace Yamadev.YamaStream
{
    public partial class Controller : Listener
    {
#if AUDIOLINK_V1
        [SerializeField] AudioLink.AudioLink _audioLink;
#else
        [SerializeField] UdonSharpBehaviour _audioLink;
#endif
        [SerializeField] bool _useAudioLink;
        [SerializeField] bool _mute;
        [SerializeField, Range(0f, 1f)] float _volume;
        // [SerializeField, Range(-6f, 6f)] float _pitch = 1f;
        [SerializeField] AudioSource[] _audioSources = new AudioSource[] { };

        public void AddAudioSource(AudioSource audioSource)
        {
            if (Array.IndexOf(_audioSources, audioSource) >= 0) return;
            _audioSources = _audioSources.Add(audioSource);
            UpdateAudio();
            UpdateAudioLink();
        }

        public AudioSource MainAudioSource => _audioSources.Length > 0 ? _audioSources[0] : null;

        public void UpdateAudio()
        {
            foreach (AudioSource audioSource in _audioSources)
            {
                if (audioSource == null) continue;
                audioSource.volume = _volume;
                audioSource.mute = _mute;
                audioSource.pitch = IsLive ? 1 : _speed; // * Mathf.Pow(2, _pitch / 12f);
            }
        }

        public void UpdateAudioLink()
        {
#if AUDIOLINK_V1
            if (_audioLink == null) return;
            if (_useAudioLink)
            {
                _audioLink.audioSource = MainAudioSource;
                _audioLink.EnableAudioLink();
                _audioLink.SetMediaVolume(_mute ? 0 : _volume);
                return;
            }
            if (AudioLinked)
            {
                _audioLink.audioSource = null;
                _audioLink.DisableAudioLink();
            }
#endif
        }

#if AUDIOLINK_V1
        public AudioLink.AudioLink AudioLink => _audioLink;
#else
        public UdonSharpBehaviour AudioLink => _audioLink;
#endif

        public bool UseAudioLink
        {
#if AUDIOLINK_V1
            get => _useAudioLink;
            set
            {
                _useAudioLink = value;
                UpdateAudioLink();
                foreach (Listener listener in _listeners) listener.OnUseAudioLinkChanged();
            }
#else
            get => false;
            set => _useAudioLink = false;
#endif
        }

        public bool AudioLinked
        {
#if AUDIOLINK_V1
            get
            {
                if (_audioLink == null) return false;
                return (bool)_audioLink.GetProgramVariable("_audioLinkEnabled") && _audioLink.audioSource == MainAudioSource;
            }
#else
            get => false;
#endif
        }

        public float Volume
        {
            get => _volume;
            set
            {
                _volume = Mathf.Clamp01(value);
                UpdateAudio();
                UpdateAudioLink();
                foreach (Listener listener in _listeners) listener.OnVolumeChanged();
            }
        }

        public bool Mute
        {
            get => _mute;
            set
            {
                _mute = value;
                UpdateAudio();
                UpdateAudioLink();
                foreach (Listener listener in _listeners) listener.OnMuteChanged();
            }
        }

        /*
        public float Pitch
        {
            get => _pitch;
            set
            {
                _pitch = value;
                UpdateAudio();
            }
        }
        */
    }
}