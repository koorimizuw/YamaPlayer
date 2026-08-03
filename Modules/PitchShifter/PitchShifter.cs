using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Yamadev.YamaStream.Modules.PitchShifter
{
  [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
  public class PitchShifter : YamaPlayerModule
  {
    [SerializeField, Range(-12f, 12f), TranslationKey("module.pitchshifter.semitones"), UdonSynced, FieldChangeCallback(nameof(Semitones))] private float _semitones = 0f;
    [SerializeField, TranslationKey("module.pitchshifter.bufferLength")] private int _bufferLength = 1024 * 2;
    [SerializeField, HideInInspector] private AudioSource[] _outputSources;

    private const float VOLUME_SCALE = 0.001f;
    private const float VOLUME_MULTIPLIER = 1000f;

    protected override bool IsSyncedModule => true;

    private AudioSource _inputSource;
    private AudioClip[] _outputClips;
    private float[] _readBufferL;
    private float[] _readBufferR;
    private float[][] _writeBuffers;

    private long _previousDspTimeSample = -1;
    private float _readPhase = 0f;
    private int[] _outputClipWriteHeads;
    private int _outputClipFrames;
    private bool _isInitialized = false;
    private YamaPlayerListener[] _listeners = new YamaPlayerListener[0];

    private bool IsPitchActive => !Mathf.Approximately(_semitones, 0f);

    public void AddListener(YamaPlayerListener listener)
    {
      if (!Utilities.IsValid(listener) || Array.IndexOf(_listeners, listener) >= 0) return;
      _listeners = _listeners.Add(listener);
    }

    public float Semitones
    {
      get => _semitones;
      set
      {
        float newValue = Mathf.Clamp(value, -12f, 12f);
        if (Mathf.Approximately(_semitones, newValue)) return;

        bool wasZero = Mathf.Approximately(_semitones, 0f);
        bool isZero = Mathf.Approximately(newValue, 0f);
        _semitones = newValue;

        if (wasZero && !isZero)
        {
          ActivatePitchShift();
        }
        else if (!wasZero && isZero)
        {
          DeactivatePitchShift();
        }

        if (Networking.IsOwner(_controller.gameObject) && !_controller.IsLocal) RequestSerialization();
        int len = _listeners.Length;
        for (int i = 0; i < len; i++)
        {
          var listener = _listeners[i];
          if (Utilities.IsValid(listener)) listener.SendCustomEvent("AfterSemitonesChanged");
        }

      }
    }

    public float PitchRatio => Mathf.Pow(2f, _semitones / 12f);

    public override void Start()
    {
      base.Start();
      Initialize();
    }

    private void Initialize()
    {
      if (!Utilities.IsValid(_controller) || !Utilities.IsValid(_controller.AudioSources) || _controller.AudioSources.Length == 0)
      {
        PrintError("No input source available from Controller.");
        return;
      }

      _inputSource = _controller.AudioSources[0];

      if (_outputSources == null || _outputSources.Length == 0)
      {
        PrintError("No output sources configured.");
        return;
      }

      int sourceCount = _outputSources.Length;
      _outputClipFrames = _bufferLength * 2;

      _outputClips = new AudioClip[sourceCount];
      _writeBuffers = new float[sourceCount][];
      _outputClipWriteHeads = new int[sourceCount];

      _readBufferL = new float[_bufferLength];
      _readBufferR = new float[_bufferLength];

      for (int i = 0; i < sourceCount; i++)
      {
        _outputClips[i] = AudioClip.Create(
            $"PitchShiftedClip_{i}",
            _outputClipFrames,
            2,
            AudioSettings.outputSampleRate,
            false
        );

        _outputSources[i].clip = _outputClips[i];
        _outputSources[i].loop = true;
        _outputSources[i].playOnAwake = false;

        _writeBuffers[i] = new float[_bufferLength * 2];
        _outputClipWriteHeads[i] = 0;
      }

      _isInitialized = true;

      if (IsPitchActive)
      {
        ActivatePitchShift();
      }
    }

    private void ActivatePitchShift()
    {
      if (!_isInitialized) return;

      SetInputSourcesVolume(false, VOLUME_SCALE);
      ApplyVolumeToOutputs();
    }

    private void SetInputSourcesVolume(bool mute, float volume)
    {
      if (!Utilities.IsValid(_controller) || !Utilities.IsValid(_controller.AudioSources)) return;

      var audioSources = _controller.AudioSources;
      int len = audioSources.Length;
      for (int i = 0; i < len; i++)
      {
        var source = audioSources[i];
        if (Utilities.IsValid(source))
        {
          source.mute = mute;
          source.volume = volume;
        }
      }
    }

    private void DeactivatePitchShift()
    {
      if (!_isInitialized) return;

      float volume = Utilities.IsValid(_controller) ? _controller.Volume : 1f;
      bool mute = Utilities.IsValid(_controller) && _controller.Mute;

      SetInputSourcesVolume(mute, volume);

      for (int i = 0; i < _outputSources.Length; i++)
      {
        if (Utilities.IsValid(_outputSources[i]) && _outputSources[i].isPlaying)
        {
          _outputSources[i].Stop();
        }
        _outputClipWriteHeads[i] = 0;
      }

      _previousDspTimeSample = -1;
    }

    private void ApplyVolumeToOutputs()
    {
      if (!IsPitchActive) return;

      float volume = Utilities.IsValid(_controller) ? _controller.Volume : 1f;
      bool mute = Utilities.IsValid(_controller) && _controller.Mute;

      for (int i = 0; i < _outputSources.Length; i++)
      {
        if (Utilities.IsValid(_outputSources[i]))
        {
          _outputSources[i].volume = mute ? 0f : volume;
          _outputSources[i].mute = false;
        }
      }
    }

    private void Update()
    {
      if (!_isInitialized || !IsPitchActive) return;
      if (Utilities.IsValid(_controller) && _controller.Mute) return;

      if (!Utilities.IsValid(_inputSource) || !_inputSource.isPlaying)
      {
        StopAllOutputs();
        return;
      }

      long currentDspTimeSample = (long)Math.Floor(AudioSettings.dspTime * AudioSettings.outputSampleRate);

      if (_previousDspTimeSample < 0)
      {
        _previousDspTimeSample = currentDspTimeSample;
        return;
      }

      int freshDataFrames = (int)(currentDspTimeSample - _previousDspTimeSample);
      if (freshDataFrames <= 0) return;

      if (freshDataFrames > _bufferLength)
      {
        _previousDspTimeSample = currentDspTimeSample;
        ResetAllWriteHeads();
        return;
      }

      int readBeginIndex = _bufferLength - freshDataFrames;
      _previousDspTimeSample = currentDspTimeSample;

      _inputSource.GetOutputData(_readBufferL, 0);
      _inputSource.GetOutputData(_readBufferR, 1);

      for (int i = 0; i < _bufferLength; i++)
      {
        _readBufferL[i] *= VOLUME_MULTIPLIER;
        _readBufferR[i] *= VOLUME_MULTIPLIER;
      }

      for (int i = 0; i < _outputSources.Length; i++)
      {
        if (!Utilities.IsValid(_outputSources[i])) continue;
        if (!_outputSources[i].gameObject.activeInHierarchy) continue;

        ProcessOutput(i, readBeginIndex, freshDataFrames);
      }
    }

    private void ProcessOutput(int index, int readBeginIndex, int freshDataFrames)
    {
      var outputSource = _outputSources[index];
      var writeBuffer = _writeBuffers[index];
      var outputClip = _outputClips[index];

      if (outputSource.isPlaying)
      {
        int timeSamples = outputSource.timeSamples;
        int writeHead = _outputClipWriteHeads[index];
        bool exhausted =
            (writeHead <= timeSamples && timeSamples < writeHead + _bufferLength)
            || (timeSamples < writeHead && timeSamples + _outputClipFrames < writeHead + _bufferLength);

        if (exhausted)
        {
          _outputClipWriteHeads[index] = 0;
          outputSource.Stop();
          return;
        }
      }

      ProcessPitchShift(writeBuffer, readBeginIndex, freshDataFrames);

      outputClip.SetData(writeBuffer, _outputClipWriteHeads[index]);
      _outputClipWriteHeads[index] += freshDataFrames;

      if (!outputSource.isPlaying && _outputClipWriteHeads[index] >= _bufferLength)
      {
        outputSource.Play();
      }

      if (_outputClipWriteHeads[index] >= _outputClipFrames)
      {
        _outputClipWriteHeads[index] -= _outputClipFrames;
      }
    }

    private void ProcessPitchShift(float[] writeBuf, int readBeginIndex, int samplesToProcess)
    {
      float pitchRatio = PitchRatio;
      int bufLen = _bufferLength;

      if (Mathf.Approximately(pitchRatio, 1f))
      {
        for (int i = 0; i < samplesToProcess; i++)
        {
          int srcIdx = readBeginIndex + i;
          int dstIdx = i * 2;
          writeBuf[dstIdx] = _readBufferL[srcIdx];
          writeBuf[dstIdx + 1] = _readBufferR[srcIdx];
        }
        _readPhase = 0f;
        return;
      }

      for (int outIdx = 0; outIdx < samplesToProcess; outIdx++)
      {
        float inputPos = readBeginIndex + _readPhase;

        int idx0 = Mathf.FloorToInt(inputPos);
        idx0 = ((idx0 % bufLen) + bufLen) % bufLen;

        int idx1 = (idx0 + 1) % bufLen;
        float frac = inputPos - Mathf.Floor(inputPos);

        int dstIdx = outIdx * 2;
        writeBuf[dstIdx] = Mathf.LerpUnclamped(_readBufferL[idx0], _readBufferL[idx1], frac);
        writeBuf[dstIdx + 1] = Mathf.LerpUnclamped(_readBufferR[idx0], _readBufferR[idx1], frac);

        _readPhase += pitchRatio;
      }

      _readPhase -= samplesToProcess;
    }

    private void StopAllOutputs()
    {
      for (int i = 0; i < _outputSources.Length; i++)
      {
        if (Utilities.IsValid(_outputSources[i]) && _outputSources[i].isPlaying)
        {
          _outputSources[i].Stop();
        }
        _outputClipWriteHeads[i] = 0;
      }
      _previousDspTimeSample = -1;
      _readPhase = 0f;
    }

    private void ResetAllWriteHeads()
    {
      for (int i = 0; i < _outputSources.Length; i++)
      {
        _outputClipWriteHeads[i] = 0;
        if (Utilities.IsValid(_outputSources[i]) && _outputSources[i].isPlaying)
        {
          _outputSources[i].Stop();
        }
      }
    }

    public void PitchUp()
    {
      Semitones += 1f;
    }

    public void PitchDown()
    {
      Semitones -= 1f;
    }

    public void ResetPitch()
    {
      Semitones = 0f;
    }

    public override void AfterVolumeChanged(float volume)
    {
      if (!IsPitchActive) return;

      SetInputSourcesVolume(false, VOLUME_SCALE);
      ApplyVolumeToOutputs();
    }

    public override void AfterMuteChanged(bool mute)
    {
      if (!IsPitchActive) return;

      if (mute)
      {
        StopAllOutputs();
      }
      else
      {
        SetInputSourcesVolume(false, VOLUME_SCALE);
        ApplyVolumeToOutputs();
      }
    }

    public override void AfterVideoStopped()
    {
      StopAllOutputs();
      if (IsPitchActive)
      {
        DeactivatePitchShift();
      }
    }

    public override void AfterVideoEnded()
    {
      AfterVideoStopped();
    }

    public override void AfterVideoPaused()
    {
      StopAllOutputs();
    }

    public override void AfterVideoStarted()
    {
      if (IsPitchActive)
      {
        ActivatePitchShift();
      }
    }

    public override void AfterTimeChanged(float time)
    {
      StopAllOutputs();
    }
  }
}
