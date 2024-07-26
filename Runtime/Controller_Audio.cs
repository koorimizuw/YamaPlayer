
using System;
using UnityEngine;

namespace Yamadev.YamaStream
{
    public partial class Controller : Listener
    {
        [SerializeField] bool _mute;
        [SerializeField, Range(0f, 1f)] float _volume;
        // [SerializeField, Range(-6f, 6f)] float _pitch = 1f;
        [SerializeField] AudioSource[] _audioSources = new AudioSource[] { };

        public void AddAudioSource(AudioSource audioSource)
        {
            if (Array.IndexOf(_audioSources, audioSource) >= 0) return;
            _audioSources = _audioSources.Add(audioSource);
            UpdateAudio();
        }

        public void UpdateAudio()
        {
            foreach (AudioSource audioSource in _audioSources)
            {
                if (audioSource == null) continue;
                audioSource.volume = _volume;
                audioSource.mute = _mute;
                audioSource.pitch = _speed; // * Mathf.Pow(2, _pitch / 12f);
            }
            foreach (Listener listener in _listeners) listener.OnVolumeChanged();
        }

        public float Volume
        {
            get => _volume;
            set
            {
                _volume = Mathf.Clamp01(value);
                UpdateAudio();
            }
        }

        public bool Mute
        {
            get => _mute;
            set
            {
                _mute = value;
                UpdateAudio();
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