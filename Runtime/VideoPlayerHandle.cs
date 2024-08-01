
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Video.Components.Base;
using VRC.SDKBase;
using VRC.Udon.Common.Enums;

namespace Yamadev.YamaStream
{
    [RequireComponent(typeof(BaseVRCVideoPlayer))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VideoPlayerHandle : UdonSharpBehaviour
    {
        [SerializeField] VideoPlayerType _videoPlayerType;
        [SerializeField] string _textureName = "_MainTex";
        [SerializeField] bool _useMaterial;
        [SerializeField] bool _isAvPro;
        [SerializeField] bool _fixFlicker;
        [SerializeField] Material _blitMaterial;
        BaseVRCVideoPlayer _baseVideoPlayer;
        Renderer _renderer;
        MaterialPropertyBlock _properties;
        Texture _texture;
        RenderTexture _blitTexture;
        Listener _listener;

        VRCUrl _url = VRCUrl.Empty;
        bool _stopped = true;
        bool _loading;
        float _lastLoaded;

        void Start()
        {
            _renderer = GetComponentInChildren<Renderer>();
            _properties = new MaterialPropertyBlock();
        }

#if UNITY_EDITOR && AVPRO_DEBUG
        private void Update()
        {
            if (_isAvPro && _stopped && BaseVideoPlayer.IsPlaying)
            {
                OnVideoStart();
            }
        }
#endif

        BaseVRCVideoPlayer BaseVideoPlayer
        {
            get
            {
                if (_baseVideoPlayer == null) _baseVideoPlayer = GetComponent<BaseVRCVideoPlayer>();
                return _baseVideoPlayer;
            }
        }

        public Listener Listener
        {
            get => _listener;
            set => _listener = value;
        }

        public float LastLoaded => _lastLoaded;

        public bool Loop
        {
            get => BaseVideoPlayer.Loop;
            set => BaseVideoPlayer.Loop = value;
        }

        public float Time
        {
            get => BaseVideoPlayer.GetTime();
            set => BaseVideoPlayer.SetTime(value);
        }

        #region ListenerEvents
        public override void OnVideoReady()
        {
            if (_listener != null) _listener.OnVideoReady();
        }

        public override void OnVideoStart()
        {
            if (_stopped && !_loading)
            {
                BaseVideoPlayer.Stop();
                return;
            }
            if (_listener != null && _stopped)
            {
                _loading = false;
                _stopped = false;
                _listener.OnVideoStart();
                GetVideoTexture();
            }
        }

        public override void OnVideoEnd()
        {
            if (IsLive || Duration == 0) return;
            _url = VRCUrl.Empty;
            if (_listener != null) _listener.OnVideoEnd();
            Stop();
        }

        public override void OnVideoError(VideoError videoError)
        {
            if (_listener != null) _listener.OnVideoError(videoError);
        }

        public override void OnVideoLoop()
        {
            if (_listener != null) _listener.OnVideoLoop();
        }
        #endregion

        public void PlayUrl(VRCUrl url)
        {
            _url = url;
            _loading = true;
            _lastLoaded = UnityEngine.Time.time;
            BaseVideoPlayer.PlayURL(_url);
        }

        public void Play()
        {
            if (_stopped || BaseVideoPlayer.IsPlaying) return;
            BaseVideoPlayer.Play();
            if (_listener != null) _listener.OnVideoPlay();
        }

        public void Pause()
        {
            if (_stopped || !BaseVideoPlayer.IsPlaying) return;
            BaseVideoPlayer.Pause();
            if (_listener != null) _listener.OnVideoPause();
        }

        public void Stop()
        {
            if (_stopped && !_loading) return;
            BaseVideoPlayer.Stop();
            _stopped = true;
            _loading = false;
            if (_listener != null) _listener.OnVideoStop();
        }

        public VideoPlayerType VideoPlayerType => _videoPlayerType;

        void createBlitTexture(int width, int height)
        {
            _blitTexture = VRCRenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB64, RenderTextureReadWrite.sRGB, 1);
            _blitTexture.filterMode = FilterMode.Bilinear;
            _blitTexture.wrapMode = TextureWrapMode.Clamp;
        }

        public Texture Texture => _texture != null ? _blitTexture != null ? _blitTexture : _texture : null;

        void resetTexture()
        {
            _texture = null;
            if (_blitTexture != null)
            {
                _blitTexture.Release();
                _blitTexture = null;
            }
        }

        public void GetVideoTexture()
        {
            if (_renderer == null || _stopped)
            {
                resetTexture();
                return;
            }

            if (_useMaterial) _texture = _renderer.sharedMaterial.GetTexture(_textureName);
            else
            {
                _renderer.GetPropertyBlock(_properties);
                _texture = _properties.GetTexture(_textureName);
            }

            if (_isAvPro && _fixFlicker && _texture != null)
                SendCustomEventDelayedFrames(nameof(BlitLastUpdate), 0, EventTiming.LateUpdate);

            if (_listener != null) _listener.OnTextureUpdated();
            SendCustomEventDelayedFrames(nameof(GetVideoTexture), 1);
        }

        public void BlitLastUpdate()
        {
            if (_texture == null) return;
            if (_blitTexture == null || _blitTexture.width != _texture.width || _blitTexture.height != _texture.height)
                createBlitTexture(_texture.width, _texture.height);
            VRCGraphics.Blit(_texture, _blitTexture, _blitMaterial);
        }

        public bool IsPlaying
        {
            get
            {
                if (BaseVideoPlayer == null) return false;
                return BaseVideoPlayer.IsPlaying;
            }
        }

        public bool IsLive => float.IsInfinity(Duration);
        public float Duration
        {
            get => BaseVideoPlayer.GetDuration();
        }
    }
}
