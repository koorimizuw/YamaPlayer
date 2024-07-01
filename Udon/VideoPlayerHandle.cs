
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Video.Components.Base;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
    [RequireComponent(typeof(BaseVRCVideoPlayer))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VideoPlayerHandle : UdonSharpBehaviour
    {
        [SerializeField] VideoPlayerType _videoPlayerType;
        [SerializeField] string _textureName = "_MainTex";
        [SerializeField] bool _useMaterial;
        Renderer _renderer;
        MaterialPropertyBlock _properties;
        Listener _listener;

        VRCUrl _url = VRCUrl.Empty;
        bool _stopped = true;

        void Start()
        {
            _renderer = GetComponentInChildren<Renderer>();
            _properties = new MaterialPropertyBlock();
        }

        BaseVRCVideoPlayer _baseVideoPlayer => GetComponent<BaseVRCVideoPlayer>();

        public Listener Listener
        {
            get => _listener;
            set => _listener = value;
        }

        public bool Loop
        {
            get => _baseVideoPlayer.Loop;
            set => _baseVideoPlayer.Loop = value;
        }

        public float Time
        {
            get => _baseVideoPlayer.GetTime();
            set => _baseVideoPlayer.SetTime(value);
        }

        #region ListenerEvents
        public override void OnVideoReady()
        {
            if (_listener != null) _listener.OnVideoReady();
        }

        public override void OnVideoStart()
        {
            if (_listener != null && _stopped) _listener.OnVideoStart();
            _stopped = false;
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
            _baseVideoPlayer.PlayURL(_url);
        }

        public void Play()
        {
            if (_baseVideoPlayer.IsPlaying) return;
            _baseVideoPlayer.Play();
            if (_listener != null) _listener.OnVideoPlay();
        }

        public void Pause()
        {
            if (!_baseVideoPlayer.IsPlaying) return;
            _baseVideoPlayer.Pause();
            if (_listener != null) _listener.OnVideoPause();
        }

        public void Stop()
        {
            _baseVideoPlayer.Stop();
            _stopped = true;
            if (_listener != null) _listener.OnVideoStop();
        }

        public VideoPlayerType VideoPlayerType => _videoPlayerType;

        public Texture Texture
        {
            get
            {
                if (_renderer == null ||!_baseVideoPlayer.IsPlaying) return null;
                if (_useMaterial) return _renderer.material.GetTexture(_textureName);
                _renderer.GetPropertyBlock(_properties);
                return _properties.GetTexture(_textureName);
            }
        }

        public bool IsPlaying
        {
            get
            {
                if (_baseVideoPlayer == null) return false;
                return _baseVideoPlayer.IsPlaying;
            }
        }

        public bool IsLive => float.IsInfinity(Duration);
        public float Duration
        {
            get => _baseVideoPlayer.GetDuration();
        }
    }
}
