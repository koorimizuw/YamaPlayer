using System;
using UnityEngine;
using UdonSharp;

namespace Yamadev.YamaStream
{
    public partial class Controller
    {
        [SerializeField] UdonSharpBehaviour _audioLink;
        [SerializeField] bool _useAudioLink;
        [SerializeField] bool _mute;
        [SerializeField, Range(0f, 1f)] float _volume;
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
            }
        }

        public void UpdateAudioLink()
        {
            if (_audioLink == null) return;
            if (_useAudioLink)
            {
                _audioLink.SetProgramVariable("audioSource", MainAudioSource); // _audioLink.audioSource = MainAudioSource;
                _audioLink.SendCustomEvent("EnableAudioLink"); // _audioLink.EnableAudioLink();
                ((Material)_audioLink.GetProgramVariable("audioMaterial")).SetFloat("_MediaVolume", _mute ? 0 : _volume); // _audioLink.SetMediaVolume(_mute ? 0 : _volume);
                return;
            }
            if (AudioLinked)
            {
                _audioLink.SetProgramVariable("audioSource", null); // _audioLink.audioSource = null;
                _audioLink.SendCustomEvent("DisableAudioLink"); // _audioLink.DisableAudioLink();
            }
        }

        public UdonSharpBehaviour AudioLink => _audioLink;

        public bool UseAudioLink
        {
            get => _useAudioLink;
            set
            {
                _useAudioLink = value;
                UpdateAudioLink();
                foreach (Listener listener in _listeners) listener.OnUseAudioLinkChanged();
            }
        }

        public bool AudioLinked
        {
            get
            {
                if (_audioLink == null) return false;
                return (bool)_audioLink.GetProgramVariable("_audioLinkEnabled") && (AudioSource)_audioLink.GetProgramVariable("audioSource") == MainAudioSource;
            }
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
                PrintLog($"Volume changed {_volume * 100}%.");
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
                if (_mute) PrintLog($"Muted.");
                else PrintLog($"Mute off.");
            }
        }
    }
}