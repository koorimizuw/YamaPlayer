
using System;
using UnityEngine;

namespace Yamadev.YamaStream
{
    public partial class Controller : Listener
    {
        [SerializeField] bool _mute;
        [SerializeField, Range(0f, 1f)] float _volume;
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
            }
            foreach (Listener listener in _listeners) listener.OnVolumeChanged();
        }

        public float Volume
        {
            get { return _volume; }
            set
            {
                _volume = Mathf.Clamp01(value);
                UpdateAudio();
            }
        }

        public bool Mute
        {
            get { return _mute; }
            set
            {
                _mute = value;
                UpdateAudio();
            }
        }
    }
}