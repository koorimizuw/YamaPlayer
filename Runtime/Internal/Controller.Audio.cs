using System;
using UnityEngine;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
  public partial class Controller
  {
    [SerializeField] private bool _mute;
    [SerializeField, Range(0f, 1f)] private float _volume;
    [SerializeField] private AudioSource[] _audioSources = new AudioSource[0];

    private void InitializeAudio()
    {
      UpdateAudioVolume();
      UpdateAudioPitch();
    }

    public AudioSource[] AudioSources
    {
      get => _audioSources;
      set => _audioSources = value;
    }

    public float Volume
    {
      get => _volume;
      set
      {
        _volume = Mathf.Clamp01(value);
        UpdateAudioVolume();
        PrintLog($"Volume changed to {_volume * 100}%.");
        int len = _listeners.Length;
        for (int i = 0; i < len; i++)
        {
          var listener = _listeners[i];
          if (Utilities.IsValid(listener)) listener.AfterVolumeChanged(_volume);
        }
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
        int len = _listeners.Length;
        for (int i = 0; i < len; i++)
        {
          var listener = _listeners[i];
          if (Utilities.IsValid(listener)) listener.AfterMuteChanged(_mute);
        }
      }
    }

    public void AddAudioSource(AudioSource audioSource)
    {
      if (Array.IndexOf(AudioSources, audioSource) >= 0) return;

      AudioSources = AudioSources.Add(audioSource);
      UpdateAudioVolume();
      UpdateAudioPitch();
    }

    private void UpdateAudioVolume()
    {
      int len = _audioSources.Length;
      for (int i = 0; i < len; i++)
      {
        var audioSource = _audioSources[i];
        if (!Utilities.IsValid(audioSource)) continue;

        audioSource.volume = _volume;
        audioSource.mute = _mute;
      }
    }

    private void UpdateAudioPitch()
    {
      var pitch = IsLive || ActiveHandler.Type == VideoPlayerType.UnityVideoPlayer ? 1f : _speed;
      int len = _audioSources.Length;
      for (int i = 0; i < len; i++)
      {
        var audioSource = _audioSources[i];
        if (!Utilities.IsValid(audioSource)) continue;
        audioSource.pitch = pitch;
      }
    }
  }
}