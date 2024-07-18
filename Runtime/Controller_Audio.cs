
using System;
using UnityEngine;

namespace Yamadev.YamaStream
{
    public partial class Controller : Listener
    {
        [SerializeField] bool _mute;
        [SerializeField, Range(0f, 1f)] float _volume;
        [SerializeField, Range(-6f, 6f)] float _pitch = 0f;
        [SerializeField] AudioSource _audioSource;
        [SerializeField] AudioSource _pitchAudio;
        AudioClip _avProOutput;
        int _bufferSize = 4096;
        float[] _inputBufferLeft;
        float[] _inputBufferRight;
        long _lastDspTime = -1;
        int _head = 0;

        public void InitializePitchAudio()
        {
            _avProOutput = AudioClip.Create("AVPro Output", _bufferSize * 2, 2, AudioSettings.outputSampleRate, false);
            _inputBufferLeft = new float[_bufferSize];
            _inputBufferRight = new float[_bufferSize];
            _pitchAudio.loop = true;
            _pitchAudio.clip = _avProOutput;
        }

        public void UpdateAudio()
        {
            _audioSource.volume = _pitch == 0 ? _volume : 0;
            _audioSource.mute = _mute;
            _audioSource.pitch = _speed;
            foreach (Listener listener in _listeners) listener.OnVolumeChanged();
        }

        public void GetAVProAudio()
        {
            long dspTime = (long)Math.Floor(AudioSettings.dspTime * AudioSettings.outputSampleRate);
            if (_lastDspTime < 0)
            {
                _lastDspTime = dspTime;
                return;
            }

            int length = (int)(dspTime - _lastDspTime);
            _lastDspTime = dspTime;

            if (length > _bufferSize || !IsPlaying)
            {
                _head = 0;
                _pitchAudio.Stop();
                return;
            }

            int index = _bufferSize - length;
            float[] data = new float[_bufferSize * 2];
            for (var frame = 0; frame < length; frame++)
            {
                data[frame * 2] = _inputBufferLeft[index + frame];
                data[frame * 2 + 1] = _inputBufferRight[index + frame];
            }

            _audioSource.GetOutputData(_inputBufferLeft, 0);
            _audioSource.GetOutputData(_inputBufferRight, 1);
            _avProOutput.SetData(data, _head);
            _head += length;

            if (!_pitchAudio.isPlaying && _head >= _bufferSize) _pitchAudio.Play();
            int target = _head - _bufferSize >= 0 ? _head - _bufferSize : _head + _bufferSize;
            int foo = _pitchAudio.timeSamples >= _bufferSize ? _pitchAudio.timeSamples : _pitchAudio.timeSamples + _bufferSize * 2;
            if (_pitchAudio.timeSamples < foo ||
                _pitchAudio.timeSamples + _bufferSize >= foo + _bufferSize)
                _pitchAudio.timeSamples = target;

            if (_head >= _avProOutput.samples) _head -= _avProOutput.samples;
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

        public float Pitch
        {
            get => _pitch;
            set
            {
                _pitch = value;
                _pitchAudio.pitch = Mathf.Pow(2, _pitch / 12f);
                if (_pitch == 0 && _pitchAudio.isPlaying) _pitchAudio.Stop();
                UpdateAudio();
            }
        }
    }
}