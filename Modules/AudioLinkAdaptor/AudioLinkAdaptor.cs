using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using System;

#if AUDIOLINK_V1
using AudioLink;
#endif

namespace Yamadev.YamaStream.Modules.AudioLinkAdaptor
{
  [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
  public class AudioLinkAdaptor : YamaPlayerModule
  {
#if AUDIOLINK_V1
    [SerializeField] private AudioLink.AudioLink _audioLink;
#else
    [SerializeField] private UdonSharpBehaviour _audioLink;
#endif

#if AUDIOLINK_V1
    [SerializeField, TranslationKey("module.audioLinkAdaptor.defaultAudioLinkEnabled")] private bool _defaultAudioLinkEnabled = false;
    private YamaPlayerListener[] _listeners = new YamaPlayerListener[0];
    public AudioLink.AudioLink AudioLinkInstance => _audioLink;

    public override void Start()
    {
      base.Start();
      if (!Utilities.IsValid(_audioLink))
      {
        Debug.LogError("AudioLink is not valid, please set up the AudioLink component in the scene.");
        return;
      }

      _audioLink.autoSetMediaState = false;
      AudioLinkEnabled = _defaultAudioLinkEnabled;
    }

    public void AddListener(YamaPlayerListener listener)
    {
      if (!Utilities.IsValid(listener) || Array.IndexOf(_listeners, listener) >= 0) return;
      _listeners = _listeners.Add(listener);
    }

    public bool AudioLinkEnabled
    {
      get
      {
        if (!Utilities.IsValid(_audioLink)) return false;
        if (!Utilities.IsValid(_controller) || _controller.AudioSources.Length == 0) return false;
        return _audioLink.audioSource == _controller.AudioSources[0];
      }
      set
      {
        Debug.Log("AudioLinkEnabled: " + value);
        if (!Utilities.IsValid(_audioLink)) return;
        if (!Utilities.IsValid(_controller) || _controller.AudioSources.Length == 0) return;

        if (value)
        {
          _audioLink.audioSource = _controller.AudioSources[0];
          _audioLink.EnableAudioLink();
          UpdateMediaState();
          _controller.SendCustomVideoEvent("AfterAudioLinkStateChanged");
        }
        else
        {
          _audioLink.audioSource = null;
          _audioLink.DisableAudioLink();
          UpdateMediaState();
          _controller.SendCustomVideoEvent("AfterAudioLinkStateChanged");
        }
      }
    }

    public void EnableAudioLink() => AudioLinkEnabled = true;

    public void DisableAudioLink() => AudioLinkEnabled = false;

    private void UpdateMediaState()
    {
      if (!AudioLinkEnabled) return;

      if (_controller.Stopped)
      {
        _audioLink.SetMediaPlaying(MediaPlaying.Stopped);
      }
      else if (_controller.Paused)
      {
        _audioLink.SetMediaPlaying(MediaPlaying.Paused);
      }
      else if (_controller.IsLoading)
      {
        _audioLink.SetMediaPlaying(MediaPlaying.Loading);
      }
      else if (_controller.IsLive)
      {
        _audioLink.SetMediaPlaying(MediaPlaying.Streaming);
      }
      else if (_controller.IsPlaying)
      {
        _audioLink.SetMediaPlaying(MediaPlaying.Playing);
      }
    }

    private void UpdateMediaVolume()
    {
      if (!AudioLinkEnabled) return;

      float volume = _controller.Mute ? 0f : _controller.Volume;
      _audioLink.SetMediaVolume(volume);
    }

    private void UpdateMediaLoop()
    {
      if (!AudioLinkEnabled) return;

      if (_controller.Loop)
      {
        _audioLink.SetMediaLoop(MediaLoop.Loop);
      }
      else
      {
        _audioLink.SetMediaLoop(MediaLoop.None);
      }
    }

    public override void AfterVideoPlayed()
    {
      UpdateMediaState();
    }

    public override void AfterVideoPaused()
    {
      UpdateMediaState();
    }

    public override void AfterVideoStopped()
    {
      UpdateMediaState();
    }

    public override void AfterVideoReady()
    {
      UpdateMediaState();
    }

    public override void AfterVideoStarted()
    {
      UpdateMediaState();
      UpdateMediaLoop();
    }

    public override void AfterVolumeChanged(float volume)
    {
      UpdateMediaVolume();
    }

    public override void AfterMuteChanged(bool mute)
    {
      UpdateMediaVolume();
    }

    public override void AfterLoopChanged(bool loop)
    {
      UpdateMediaLoop();
    }

#endif
  }
}
