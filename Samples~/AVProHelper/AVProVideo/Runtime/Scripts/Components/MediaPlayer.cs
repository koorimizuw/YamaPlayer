using UnityEngine;

namespace RenderHeads.Media.AVProVideo
{
    public class MediaPlayer : MonoBehaviour
    {
        [SerializeField] float _playbackRate = 1.0f;

        public virtual ITextureProducer TextureProducer => null;
        public virtual IMediaControl Control => null;
        public virtual IMediaInfo Info => null;
        public bool Loop { get => false; set { } }
        public void Play() { }
        public void Pause() { }
        public bool OpenMedia(MediaPath path, bool autoPlay = true) => false;
        public bool OpenMedia(MediaPathType pathType, string path, bool autoPlay = true) => false;
        public bool OpenMedia(MediaReference mediaReference, bool autoPlay = true) => false;
        public void CloseMedia() { }
        public PlatformOptions GetCurrentPlatformOptions() => null;

    }

    public class MediaPath { }
    public class MediaReference { }
    public class PlatformOptions { }
    public class Windows
    {
        public enum AudioOutput
        {
            System,                     // Default
            Unity,                      // Media Foundation API only
            FacebookAudio360,           // Media Foundation API only
            None,                       // Media Foundation API only
        }
    }
    public class OptionsWindows: PlatformOptions
    {
        public bool useLowLatency;
        public Windows.AudioOutput _audioMode = Windows.AudioOutput.System;
    }
    public interface ITextureProducer
    {
        Texture GetTexture(int index = 0);
    }
    public interface IMediaControl
    {
        bool IsPlaying();
        void Seek(double time);
        double GetCurrentTime();
    }
    public interface IMediaInfo
    {
        double GetDuration();
    }
    public enum MediaPathType
    {
        AbsolutePathOrURL,
        RelativeToProjectFolder,
        RelativeToStreamingAssetsFolder,
        RelativeToDataFolder,
        RelativeToPersistentDataFolder,
    }
}