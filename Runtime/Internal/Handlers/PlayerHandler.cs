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
    [SerializeField] protected PlayerHandler _fallbackHandler;
    protected VRCUrl _loadedUrl = VRCUrl.Empty;
    protected float _speed = 1f;
    protected bool _loading;
    protected YamaPlayerListener _listener;
    protected bool _useFallbackHandler;

    public void SetListener(YamaPlayerListener listener)
    {
      _listener = listener;
    }

    public VideoPlayerType Type => _type;

    public PlayerHandler FallbackHandler => _fallbackHandler;

    public virtual VRCUrl LoadedUrl => _loadedUrl;

    public virtual bool IsValidTrack(object[] track)
    {
      return IsValidUrl(TrackUtils.GetUrl(track));
    }

    protected virtual bool IsValidUrl(VRCUrl url)
    {
      if (!Utilities.IsValid(url)) return false;
      string u = url.Get().ToLower();
      return u.StartsWith("http://") || u.StartsWith("https://");
    }

    public virtual bool IsLoading { get; }

    public virtual bool IsPlaying { get; }

    public virtual bool IsPaused { get; }

    public virtual bool IsStopped { get; }

    public virtual bool IsError { get; }

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

    public virtual void LoadTrack(object[] track)
    {
      LoadUrl(TrackUtils.GetUrl(track));
    }

    public virtual void Play() { }

    public virtual void Pause() { }

    public virtual void Stop() { }

    public virtual Texture Texture { get; }

    public virtual int MaxResolution { get; set; }
  }
}