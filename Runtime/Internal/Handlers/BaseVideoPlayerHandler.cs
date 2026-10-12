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
    [SerializeField] private BaseVRCVideoPlayer _baseVideoPlayer;
    [SerializeField] private Animator _animator;
    private Renderer _renderer;
    private MaterialPropertyBlock _properties;
    private Texture _videoTexture;
    private RenderTexture _blitTexture;
    private bool _stopped = true;
    private bool _isError;

    private void Start()
    {
      if (!Utilities.IsValid(_baseVideoPlayer)) _baseVideoPlayer = GetComponent<BaseVRCVideoPlayer>();
      if (!Utilities.IsValid(_animator)) _animator = GetComponent<Animator>();
      _animator.Rebind();
      if (!Utilities.IsValid(_renderer)) _renderer = GetComponent<Renderer>();
      _properties = new MaterialPropertyBlock();
    }

    protected override bool IsValidUrl(VRCUrl url)
    {
      if (_type != VideoPlayerType.AVProVideoPlayer) return base.IsValidUrl(url);
      if (!Utilities.IsValid(url)) return false;
      string u = url.Get().ToLower();
      return u.StartsWith("http://") || u.StartsWith("https://")
        || u.StartsWith("rtsp://") || u.StartsWith("rtspt://") || u.StartsWith("rtspu://")
        || u.StartsWith("rtmp://") || u.StartsWith("rtmps://") || u.StartsWith("rtsps://");
    }

    private void Update()
    {
      if (!Utilities.IsValid(_renderer) || _stopped || _loading) return;
      if (_useMaterial) _videoTexture = _renderer.sharedMaterial.GetTexture(_textureName);
      else
      {
        _renderer.GetPropertyBlock(_properties);
        _videoTexture = _properties.GetTexture(_textureName);
      }

      if (!Utilities.IsValid(_videoTexture))
      {
        if (Utilities.IsValid(_listener)) _listener.AfterTextureUpdated(null);
        return;
      }

#if UNITY_STANDALONE_WIN
      if (_type == VideoPlayerType.AVProVideoPlayer)
      {
        SendCustomEventDelayedFrames(nameof(BlitLastUpdate), 0, EventTiming.LateUpdate);
        return;
      }
#endif

      if (Utilities.IsValid(_listener)) _listener.AfterTextureUpdated(_videoTexture);
    }

    public override bool IsLoading => _loading;

    public override bool IsPlaying
    {
      get
      {
        if (!Utilities.IsValid(_baseVideoPlayer)) return false;
        return _baseVideoPlayer.IsPlaying;
      }
    }

    public override bool IsPaused
    {
      get
      {
        if (_stopped || _loading) return false;
        return !IsPlaying;
      }
    }

    public override bool IsStopped => _stopped;

    public override bool IsError => _isError;

    public override bool Loop
    {
      get
      {
        if (!Utilities.IsValid(_baseVideoPlayer)) return false;
        return _baseVideoPlayer.Loop;
      }
      set
      {
        if (!Utilities.IsValid(_baseVideoPlayer)) return;
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
      _stopped = false;
      _loading = true;
      _isError = false;
    }

    public override void LoadUrl(VRCUrl url)
    {
      if (!Utilities.IsValid(_baseVideoPlayer)) return;

      _baseVideoPlayer.LoadURL(url);
      _loadedUrl = url;
      _stopped = false;
      _loading = true;
      _isError = false;
    }

    public override void Play()
    {
      if (_stopped || IsPlaying || !Utilities.IsValid(_baseVideoPlayer)) return;
      _baseVideoPlayer.Play();
      if (Utilities.IsValid(_listener)) _listener.AfterVideoPlayed();
    }

    public override void Pause()
    {
      if (_stopped || !IsPlaying || !Utilities.IsValid(_baseVideoPlayer)) return;
      _baseVideoPlayer.Pause();
      if (Utilities.IsValid(_listener)) _listener.AfterVideoPaused();
    }

    public override void Stop()
    {
      if (!Utilities.IsValid(_baseVideoPlayer) || _stopped) return;

      _baseVideoPlayer.Stop();
      _loadedUrl = VRCUrl.Empty;
      _loading = false;
      _stopped = true;
      _isError = false;

      if (!Utilities.IsValid(_videoTexture) && Utilities.IsValid(_blitTexture))
      {
        _blitTexture.Release();
        _blitTexture = null;
      }

      if (Utilities.IsValid(_listener)) _listener.AfterTextureUpdated(null);
      if (Utilities.IsValid(_listener)) _listener.AfterVideoStopped();
    }

    public override Texture Texture => _blitTexture ?? _videoTexture;

    public void BlitLastUpdate()
    {
      if (!Utilities.IsValid(_videoTexture))
      {
        if (Utilities.IsValid(_listener)) _listener.AfterTextureUpdated(null);
        return;
      }

      var width = _videoTexture.width;
      var height = _videoTexture.height;
      if (!Utilities.IsValid(_blitTexture) || _blitTexture.width != width || _blitTexture.height != height)
      {
        if (Utilities.IsValid(_blitTexture)) _blitTexture.Release();
        _blitTexture = VRCRenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB64, RenderTextureReadWrite.sRGB, 1);
        _blitTexture.filterMode = FilterMode.Bilinear;
        _blitTexture.wrapMode = TextureWrapMode.Clamp;
      }

      VRCGraphics.Blit(_videoTexture, _blitTexture, _blitMaterial);
      if (Utilities.IsValid(_listener)) _listener.AfterTextureUpdated(_blitTexture);
    }

    #region VRChat Video Events
    public override void OnVideoReady()
    {
      _loading = false;
      if (_stopped)
      {
        _baseVideoPlayer.Stop();
        return;
      }

      if (Utilities.IsValid(_listener)) _listener.AfterVideoReady();
    }

    public override void OnVideoStart()
    {
      if (Utilities.IsValid(_listener)) _listener.AfterVideoStarted();
    }

    public override void OnVideoEnd()
    {
      if (IsLive || Duration == 0) return;
      if (Utilities.IsValid(_listener)) _listener.AfterVideoEnded();
    }

    public override void OnVideoError(VideoError videoError)
    {
      _loading = false;
      _stopped = true;
      _isError = true;
      if (Utilities.IsValid(_listener)) _listener.AfterVideoErrorOccurred(videoError);
    }

    public override void OnVideoLoop()
    {
      if (Utilities.IsValid(_listener)) _listener.AfterVideoLooped();
    }
    #endregion
  }
}
