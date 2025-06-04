using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
    [DefaultExecutionOrder(-1200)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DisallowMultipleComponent]
    public abstract class PlayerHandler : YamaPlayerBehaviour
    {
        [SerializeField] protected VideoPlayerType _type;
        protected VRCUrl _loadedUrl = VRCUrl.Empty;
        protected float _speed = 1f;
        protected bool _loading;
        protected Listener _listener;

        public void SetListener(Listener listener)
        {
            _listener = listener;
        }

        public VideoPlayerType Type => _type;

        public bool IsLoading => _loading;

        public VRCUrl LoadedUrl => _loadedUrl;

        public virtual bool IsPlaying { get; }

        public virtual bool Loop { get; set; }

        public virtual float Speed { get; set; }

        public virtual bool IsReady { get; }

        public virtual int VideoWidth { get; }

        public virtual int VideoHeight { get; }

        public virtual float Time { get; set; }

        public virtual float Duration { get; }

        public virtual bool IsLive { get; }

        public virtual void PlayUrl(VRCUrl url) { }

        public virtual void LoadUrl(VRCUrl url) { }

        public virtual void Play() { }

        public virtual void Pause() { }

        public virtual void Stop() { }

        public virtual Texture Texture { get; }

        public virtual int MaxResolution { get; set; }
    }
}