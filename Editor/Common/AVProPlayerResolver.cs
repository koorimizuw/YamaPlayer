using System;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Video.Components.AVPro;
using VRC.SDK3.Video.Interfaces.AVPro;
using VRC.SDKBase;

#if AVPRO_DEBUG
using RenderHeads.Media.AVProVideo;
using static RenderHeads.Media.AVProVideo.MediaPlayer;
#endif

namespace Yamadev.YamaStream.Editor
{
#if AVPRO_DEBUG
    public class AVProPlayerResolver : IAVProVideoPlayerInternal
    {
        public VRCAVProVideoPlayer BasePlayer;
        MediaPlayer _player;
        private bool _isInitialized = false;

        public MediaPlayer MediaPlayer
        {
            get => _player;
            set
            {
                if (_player != null && _isInitialized)
                {
                    UnsubscribeFromEvents();
                }

                _player = value;

                if (_player != null && BasePlayer != null)
                {
                    SubscribeToEvents();
                    _isInitialized = true;
                }
            }
        }

        private void SubscribeToEvents()
        {
            if (MediaPlayer?.Events != null)
            {
                MediaPlayer.Events.AddListener(OnAVProVideoEvent);
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (MediaPlayer?.Events != null)
            {
                MediaPlayer.Events.RemoveListener(OnAVProVideoEvent);
            }
        }

        public void Dispose()
        {
            if (_isInitialized)
            {
                UnsubscribeFromEvents();
                _isInitialized = false;
            }
        }

        public bool Loop
        {
            get => MediaPlayer != null && MediaPlayer.Loop;
            set
            {
                if (MediaPlayer != null)
                    MediaPlayer.Loop = value;
            }
        }

        public bool IsPlaying
        {
            get => MediaPlayer?.Control?.IsPlaying() ?? false;
        }

        public bool IsReady
        {
            get => MediaPlayer != null &&
                   MediaPlayer.Control != null &&
                   MediaPlayer.Control.CanPlay() &&
                   MediaPlayer.Control.HasMetaData();
        }

        public int VideoWidth
        {
            get
            {
                try
                {
                    if (MediaPlayer?.TextureProducer == null) return 0;

                    Texture frame = MediaPlayer.TextureProducer.GetTexture(0);
                    return frame?.width ?? 0;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to get video width: {ex.Message}");
                    return 0;
                }
            }
        }

        public int VideoHeight
        {
            get
            {
                try
                {
                    if (MediaPlayer?.TextureProducer == null) return 0;

                    Texture frame = MediaPlayer.TextureProducer.GetTexture(0);
                    return frame?.height ?? 0;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to get video height: {ex.Message}");
                    return 0;
                }
            }
        }

        public bool UseLowLatency
        {
            get
            {
#if UNITY_EDITOR_WIN
                try
                {
                    if (MediaPlayer == null) return false;

                    var platformOptions = MediaPlayer.GetCurrentPlatformOptions();
                    if (platformOptions is OptionsWindows windowsOptions)
                    {
                        return windowsOptions.useLowLatency;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to get low latency setting: {ex.Message}");
                    return false;
                }
#else
                return false;
#endif
            }
        }

        public static Action<VRCUrl, int, UnityEngine.Object, Action<string>, Action<VideoError>> StartResolveURLCoroutine { get; set; }

        public void LoadURL(VRCUrl url)
        {
            try
            {
                if (StartResolveURLCoroutine != null)
                {
                    StartResolveURLCoroutine(url, BasePlayer.MaximumResolution, BasePlayer, PlayVideo, HandleVideoError);
                }
                else
                {
                    PlayVideo(url.Get());
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load URL: {ex.Message}");
                HandleVideoError(VideoError.InvalidURL);
            }

            void PlayVideo(string resolvedURL)
            {
                try
                {
                    if (string.IsNullOrEmpty(resolvedURL))
                    {
                        Debug.LogError("Resolved URL is null or empty");
                        HandleVideoError(VideoError.InvalidURL);
                        return;
                    }

                    MediaPlayer.OpenMedia(MediaPathType.AbsolutePathOrURL, resolvedURL, false);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to open media: {ex.Message}");
                    HandleVideoError(VideoError.PlayerError);
                }
            }
        }

        public void PlayURL(VRCUrl url)
        {
            try
            {
                if (StartResolveURLCoroutine != null)
                {
                    StartResolveURLCoroutine(url, BasePlayer.MaximumResolution, BasePlayer, PlayVideo, HandleVideoError);
                }
                else
                {
                    PlayVideo(url.Get());
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to play URL: {ex.Message}");
                HandleVideoError(VideoError.InvalidURL);
            }

            void PlayVideo(string resolvedURL)
            {
                try
                {
                    if (string.IsNullOrEmpty(resolvedURL))
                    {
                        Debug.LogError("Resolved URL is null or empty");
                        HandleVideoError(VideoError.InvalidURL);
                        return;
                    }

                    MediaPlayer.OpenMedia(MediaPathType.AbsolutePathOrURL, resolvedURL, true);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to open and play media: {ex.Message}");
                    HandleVideoError(VideoError.PlayerError);
                }
            }
        }

        public void Play()
        {
            try
            {
                MediaPlayer?.Play();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to play: {ex.Message}");
            }
        }

        public void Pause()
        {
            try
            {
                MediaPlayer?.Pause();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to pause: {ex.Message}");
            }
        }

        public void Stop()
        {
            try
            {
                MediaPlayer?.CloseMedia();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to stop: {ex.Message}");
            }
        }

        public void SetTime(float value)
        {
            try
            {
                if (MediaPlayer?.Control != null)
                {
                    MediaPlayer.Control.Seek(value);
                }
                else
                {
                    Debug.LogWarning("Cannot set time: MediaPlayer or Control is null");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to set time: {ex.Message}");
            }
        }

        public float GetTime()
        {
            try
            {
                if (MediaPlayer?.Control != null)
                {
                    return (float)MediaPlayer.Control.GetCurrentTime();
                }
                else
                {
                    Debug.LogWarning("Cannot get time: MediaPlayer or Control is null");
                    return 0f;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to get time: {ex.Message}");
                return 0f;
            }
        }

        public float GetDuration()
        {
            try
            {
                if (MediaPlayer?.Info != null)
                {
                    return (float)MediaPlayer.Info.GetDuration();
                }
                else
                {
                    Debug.LogWarning("Cannot get duration: MediaPlayer or Info is null");
                    return 0f;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to get duration: {ex.Message}");
                return 0f;
            }
        }

        #region Event Handlers
        private void OnAVProVideoEvent(MediaPlayer mediaPlayer, MediaPlayerEvent.EventType eventType, ErrorCode errorCode)
        {
            try
            {
                switch (eventType)
                {
                    case MediaPlayerEvent.EventType.ReadyToPlay:
                        HandleOnVideoReady();
                        break;
                    case MediaPlayerEvent.EventType.Started:
                        HandleOnVideoStart();
                        break;
                    case MediaPlayerEvent.EventType.Error:
                        HandleVideoError(VideoError.PlayerError);
                        break;
                    case MediaPlayerEvent.EventType.FinishedPlaying:
                        HandleOnVideoFinished();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to handle AVProVideo event {eventType}: {ex.Message}");
            }
        }

        private void HandleOnVideoReady()
        {
            try
            {
                BasePlayer?.OnVideoReady();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to handle OnVideoReady: {ex.Message}");
            }
        }

        private void HandleOnVideoStart()
        {
            try
            {
                BasePlayer?.OnVideoStart();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to handle OnVideoStart: {ex.Message}");
            }
        }

        private void HandleOnVideoFinished()
        {
            try
            {
                BasePlayer?.OnVideoEnd();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to handle OnVideoFinished: {ex.Message}");
            }
        }

        private void HandleVideoError(VideoError error)
        {
            try
            {
                BasePlayer?.OnVideoError(error);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to handle video error: {ex.Message}");
            }
        }


        // No event for video loop in AVPro.
        private void HandleOnVideoLoop()
        {
            try
            {
                BasePlayer?.OnVideoLoop();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to handle OnVideoLoop: {ex.Message}");
            }
        }
        #endregion
    }
#endif
}