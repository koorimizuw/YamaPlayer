
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

namespace Yamadev.YamaStream.Script
{
#if AVPRO_DEBUG
    public class AVProPlayerResolver : IAVProVideoPlayerInternal
    {
        public VRCAVProVideoPlayer BasePlayer;
        MediaPlayer _player;

        public MediaPlayer MediaPlayer
        {
            get => _player;
            set
            {
                _player = value;
            }
        }

        public bool Loop
        {
            get => MediaPlayer.Loop;
            set => MediaPlayer.Loop = value;
        }

        public bool IsPlaying
        {
            get => MediaPlayer.Control.IsPlaying();
        }

        public bool IsReady
        {
            get => true;
        }

        public int VideoWidth
        {
            get
            {
                Texture frame = MediaPlayer.TextureProducer.GetTexture(0);
                return frame.width;
            }
        }

        public int VideoHeight
        {
            get
            {
                Texture frame = MediaPlayer.TextureProducer.GetTexture(0);
                return frame.height;
            }
        }

        public bool UseLowLatency
        {
            get
            {
#if UNITY_EDITOR_WIN
                return ((OptionsWindows)MediaPlayer.GetCurrentPlatformOptions()).useLowLatency;
#else
                return false;
#endif
            }
        }

        public static Action<VRCUrl, int, UnityEngine.Object, Action<string>, Action<VideoError>> StartResolveURLCoroutine { get; set; }

        public void LoadURL(VRCUrl url)
        {
            if (StartResolveURLCoroutine != null)
            {
                StartResolveURLCoroutine(url, BasePlayer.MaximumResolution, BasePlayer, PlayVideo, BasePlayer.OnVideoError);
            }
            else
            {
                PlayVideo(url.Get());
            }

            void PlayVideo(string resolvedURL)
            {
                MediaPlayer.OpenMedia(MediaPathType.AbsolutePathOrURL, resolvedURL, false);
            }
        }

        public void PlayURL(VRCUrl url)
        {
            if (StartResolveURLCoroutine != null)
            {
                StartResolveURLCoroutine(url, BasePlayer.MaximumResolution, BasePlayer, PlayVideo, BasePlayer.OnVideoError);
            }
            else
            {
                PlayVideo(url.Get());
            }

            void PlayVideo(string resolvedURL)
            {
                MediaPlayer.OpenMedia(MediaPathType.AbsolutePathOrURL, resolvedURL, true);
            }
        }

        public void Play()
        {
            MediaPlayer.Play();
        }

        public void Pause()
        {
            MediaPlayer.Pause();
        }

        public void Stop()
        {
            MediaPlayer.CloseMedia();
        }

        public void SetTime(float value)
        {
            MediaPlayer.Control.Seek(value);
        }

        public float GetTime()
        {
            return (float)MediaPlayer.Control.GetCurrentTime();
        }

        public float GetDuration()
        {
            return (float)MediaPlayer.Info.GetDuration();
        }
    }
#endif
}