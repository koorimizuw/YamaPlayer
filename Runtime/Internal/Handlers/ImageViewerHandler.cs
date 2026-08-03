using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Image;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace Yamadev.YamaStream
{
  public class ImageViewerHandler : PlayerHandler
  {
    private Texture _texture;
    private bool _isReady;
    private bool _isPlaying;
    private bool _loop;
    private bool _playImmediately;
    private bool _isError;
    private VRCImageDownloader _imageDownloader;

    private void Start()
    {
      _imageDownloader = new VRCImageDownloader();
    }

    public override bool IsLoading => _loading;

    public override bool IsPlaying => _isPlaying;

    public override bool IsPaused
    {
      get
      {
        if (!_isReady || _loading) return false;
        return !_isPlaying;
      }
    }

    public override bool IsStopped => !_isReady && !_loading;

    public override bool IsError => _isError;

    public VRCImageDownloader ImageDownloader
    {
      get
      {
        if (!Utilities.IsValid(_imageDownloader))
        {
          _imageDownloader = new VRCImageDownloader();
        }
        return _imageDownloader;
      }
    }

    public override bool Loop
    {
      get => _loop;
      set => _loop = value;
    }

    public override float Speed => 1f;

    public override bool IsReady => _isReady;

    public override int VideoWidth
    {
      get
      {
        if (!Utilities.IsValid(_texture)) return 0;
        return _texture.width;
      }
    }

    public override int VideoHeight
    {
      get
      {
        if (!Utilities.IsValid(_texture)) return 0;
        return _texture.height;
      }
    }

    public override void PlayUrl(VRCUrl url)
    {
      ImageDownloader.DownloadImage(url, null, (IUdonEventReceiver)this);
      _loadedUrl = url;
      _playImmediately = true;
      _loading = true;
      _isError = false;
    }

    public override void LoadUrl(VRCUrl url)
    {
      ImageDownloader.DownloadImage(url, null, (IUdonEventReceiver)this);
      _loadedUrl = url;
      _playImmediately = false;
      _loading = true;
      _isError = false;
    }

    public override void Play()
    {
      if (_isPlaying) return;
      _isPlaying = true;
      if (Utilities.IsValid(_listener)) _listener.AfterVideoPlayed();
    }

    public override void Pause()
    {
      if (!_isPlaying) return;
      _isPlaying = false;
      if (Utilities.IsValid(_listener)) _listener.AfterVideoPaused();
    }

    public override void Stop()
    {
      _isReady = false;
      _loadedUrl = VRCUrl.Empty;
      _loading = false;
      _isPlaying = false;
      _isError = false;
      _texture = null;
      if (Utilities.IsValid(_imageDownloader))
      {
        _imageDownloader.Dispose();
        _imageDownloader = null;
      }
      if (Utilities.IsValid(_listener)) _listener.AfterTextureUpdated(null);
      if (Utilities.IsValid(_listener)) _listener.AfterVideoStopped();
    }

    public override Texture Texture => _texture;

    public override void OnImageLoadSuccess(IVRCImageDownload result)
    {
      if (!result.Url.Equals(_loadedUrl)) return;

      _texture = result.Result;
      _isReady = true;
      _loading = false;
      if (Utilities.IsValid(_listener)) _listener.AfterVideoReady();
      if (_playImmediately)
      {
        Play();
        if (Utilities.IsValid(_listener)) _listener.AfterVideoStarted();
      }
      if (Utilities.IsValid(_listener)) _listener.AfterTextureUpdated(_texture);
    }

    public override void OnImageLoadError(IVRCImageDownload result)
    {
      if (!result.Url.Equals(_loadedUrl)) return;

      _loading = false;
      _isError = true;
      VideoError videoError;
      switch (result.Error)
      {
        case VRCImageDownloadError.AccessDenied:
          videoError = VideoError.AccessDenied;
          break;
        case VRCImageDownloadError.InvalidURL:
          videoError = VideoError.InvalidURL;
          break;
        case VRCImageDownloadError.DownloadError:
          videoError = VideoError.PlayerError;
          break;
        case VRCImageDownloadError.TooManyRequests:
          videoError = VideoError.RateLimited;
          break;
        default:
          videoError = VideoError.Unknown;
          break;
      }
      if (Utilities.IsValid(_listener)) _listener.AfterVideoErrorOccurred(videoError);
    }
  }
}