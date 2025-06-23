using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Video.Components.Base;
using VRC.SDKBase;
using VRC.Udon.Common.Enums;

namespace Yamadev.YamaStream
{
    [RequireComponent(typeof(Renderer))]
    [RequireComponent(typeof(BaseVRCVideoPlayer))]
    [RequireComponent(typeof(Animator))]
    public sealed class BaseVideoPlayerHandler : PlayerHandler
    {
        [SerializeField] private string _textureName = "_MainTex";
        [SerializeField] private bool _useMaterial;
        [SerializeField] private Material _blitMaterial;
        private BaseVRCVideoPlayer _baseVideoPlayer;
        private Animator _animator;
        private Renderer _renderer;
        private MaterialPropertyBlock _properties;
        private Texture _videoTexture;
        private RenderTexture _blitTexture;
        private bool _stopped = true;

        private void Start()
        {
            _animator = GetComponent<Animator>();
            _animator.Rebind();
            _baseVideoPlayer = GetComponent<BaseVRCVideoPlayer>();
            _renderer = GetComponent<Renderer>();
            _properties = new MaterialPropertyBlock();
        }

        private void Update()
        {
            if (!_renderer || _stopped) return;
            if (_useMaterial) _videoTexture = _renderer.sharedMaterial.GetTexture(_textureName);
            else
            {
                _renderer.GetPropertyBlock(_properties);
                _videoTexture = _properties.GetTexture(_textureName);
            }

            if (!_videoTexture)
            {
                if (Utilities.IsValid(_listener)) _listener.OnTextureUpdated(null);
                return;
            }

#if UNITY_STANDALONE_WIN
            if (_type == VideoPlayerType.AVProVideoPlayer)
            {
                SendCustomEventDelayedFrames(nameof(BlitLastUpdate), 0, EventTiming.LateUpdate);
                return;
            }
#endif

            if (Utilities.IsValid(_listener)) _listener.OnTextureUpdated(_videoTexture);
        }

        public override bool IsPlaying
        {
            get
            {
                if (!_baseVideoPlayer) return false;
                return _baseVideoPlayer.IsPlaying;
            }
        }

        public override bool Loop
        {
            get
            {
                if (!_baseVideoPlayer) return false;
                return _baseVideoPlayer.Loop;
            }
            set
            {
                if (!_baseVideoPlayer) return;
                _baseVideoPlayer.Loop = value;
            }
        }

        public override float Speed
        {
            get => _speed;
            set
            {
                if (!Utilities.IsValid(_animator)) return;
                _speed = value;
                _animator.SetFloat("Speed", _speed);
                _animator.Update(0f);
            }
        }

        public override bool IsReady
        {
            get
            {
                if (!Utilities.IsValid(_baseVideoPlayer)) return false;
                return _baseVideoPlayer.IsReady;
            }
        }

        public override int VideoWidth
        {
            get
            {
                if (!Utilities.IsValid(_baseVideoPlayer)) return 0;
                return _baseVideoPlayer.VideoWidth;
            }
        }

        public override int VideoHeight
        {
            get
            {
                if (!Utilities.IsValid(_baseVideoPlayer)) return 0;
                return _baseVideoPlayer.VideoHeight;
            }
        }

        public override int MaxResolution
        {
            set
            {
                if (!Utilities.IsValid(_animator)) return;
                _animator.SetFloat("Resolution", value / 4320f);
                _animator.Update(0f);
            }
        }

        public override float Time
        {
            get
            {
                if (!Utilities.IsValid(_baseVideoPlayer)) return 0;
                return _baseVideoPlayer.GetTime();
            }
            set
            {
                if (!Utilities.IsValid(_baseVideoPlayer)) return;
                _baseVideoPlayer.SetTime(value);
            }
        }

        public override float Duration
        {
            get
            {
                if (!Utilities.IsValid(_baseVideoPlayer)) return 0;
                return _baseVideoPlayer.GetDuration();
            }
        }

        public override bool IsLive => float.IsInfinity(Duration);

        public override void PlayUrl(VRCUrl url)
        {
            if (!Utilities.IsValid(_baseVideoPlayer)) return;
            _baseVideoPlayer.PlayURL(url);
            _loadedUrl = url;
            _loading = true;
            _stopped = false;
        }

        public override void LoadUrl(VRCUrl url)
        {
            if (!Utilities.IsValid(_baseVideoPlayer)) return;
            _baseVideoPlayer.LoadURL(url);
            _loadedUrl = url;
            _loading = true;
            _stopped = false;
        }

        public override void Play()
        {
            if (IsPlaying || !Utilities.IsValid(_baseVideoPlayer)) return;
            _baseVideoPlayer.Play();
        }

        public override void Pause()
        {
            if (!IsPlaying || !Utilities.IsValid(_baseVideoPlayer)) return;
            _baseVideoPlayer.Pause();
        }

        public override void Stop()
        {
            if (!Utilities.IsValid(_baseVideoPlayer)) return;
            _baseVideoPlayer.Stop();
            _loadedUrl = VRCUrl.Empty;
            _loading = false;
            _stopped = true;
            if (!_videoTexture && Utilities.IsValid(_blitTexture))
            {
                _blitTexture.Release();
                _blitTexture = null;
                if (Utilities.IsValid(_listener)) _listener.OnTextureUpdated(null);
                return;
            }
        }

        public override Texture Texture => _blitTexture ?? _videoTexture;

        public void BlitLastUpdate()
        {
            if (!Utilities.IsValid(_videoTexture))
            {
                if (Utilities.IsValid(_listener)) _listener.OnTextureUpdated(null);
                return;
            }

            var width = _videoTexture.width;
            var height = _videoTexture.height;
            if (!Utilities.IsValid(_blitTexture) || _blitTexture.width != width || _blitTexture.height != height)
            {
                _blitTexture = VRCRenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB64, RenderTextureReadWrite.sRGB, 1);
                _blitTexture.filterMode = FilterMode.Bilinear;
                _blitTexture.wrapMode = TextureWrapMode.Clamp;
            }

            VRCGraphics.Blit(_videoTexture, _blitTexture, _blitMaterial);
            if (Utilities.IsValid(_listener)) _listener.OnTextureUpdated(_blitTexture);
        }

        #region ListenerEvents
        public override void OnVideoReady()
        {
            _loading = false;
            if (Utilities.IsValid(_listener)) _listener.OnVideoReady();
        }

        public override void OnVideoStart()
        {
            _loading = false;
            if (Utilities.IsValid(_listener)) _listener.OnVideoStart();
        }

        public override void OnVideoEnd()
        {
            if (IsLive || Duration == 0) return;
            if (Utilities.IsValid(_listener)) _listener.OnVideoEnd();
        }

        public override void OnVideoError(VideoError videoError)
        {
            _loading = false;
            if (Utilities.IsValid(_listener)) _listener.OnVideoError(videoError);
        }

        public override void OnVideoLoop()
        {
            if (Utilities.IsValid(_listener)) _listener.OnVideoLoop();
        }
        #endregion
    }
}