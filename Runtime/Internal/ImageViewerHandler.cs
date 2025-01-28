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

        public override bool IsPlaying => _isPlaying;

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
            new VRCImageDownloader().DownloadImage(url, null, (IUdonEventReceiver)this);
            _loadedUrl = url;
            _playImmediately = true;
            _loading = true;
        }

        public override void LoadUrl(VRCUrl url)
        {
            new VRCImageDownloader().DownloadImage(url, null, (IUdonEventReceiver)this);
            _loadedUrl = url;
            _playImmediately = false;
            _loading = true;
        }

        public override void Play()
        {
            if (!_isPlaying) _isPlaying = true;
        }

        public override void Pause()
        {
            if (_isPlaying) _isPlaying = false;
        }

        public override void Stop()
        {
            _isReady = false;
            _loadedUrl = VRCUrl.Empty;
            _loading = false;
            _isPlaying = false;
            _texture = null;
            if (Utilities.IsValid(_listener)) _listener.OnTextureUpdated(null);
        }

        public override Texture Texture => _texture;

        public override void OnImageLoadSuccess(IVRCImageDownload result)
        {
            if (!result.Url.Equals(_loadedUrl)) return;

            _texture = result.Result;
            _isReady = true;
            _loading = false;
            if (Utilities.IsValid(_listener)) _listener.OnVideoReady();
            if (_playImmediately)
            {
                Play();
                if (Utilities.IsValid(_listener)) _listener.OnVideoStart();
            }
            if (Utilities.IsValid(_listener)) _listener.OnTextureUpdated(_texture);
        }

        public override void OnImageLoadError(IVRCImageDownload result)
        {
            if (!result.Url.Equals(_loadedUrl)) return;

            _loading = false;
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
            };
            if (Utilities.IsValid(_listener)) _listener.OnVideoError(videoError);
        }
    }
}