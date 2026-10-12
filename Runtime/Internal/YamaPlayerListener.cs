using UnityEngine;
using VRC.SDK3.Components.Video;

namespace Yamadev.YamaStream
{
  public abstract class YamaPlayerListener : YamaPlayerBehaviour
  {
    #region Core Events

    public virtual void AfterVideoReady() { }
    public virtual void AfterVideoStarted() { }
    public virtual void AfterVideoErrorOccurred(VideoError videoError) { }
    public virtual void AfterVideoLooped() { }
    public virtual void AfterVideoPlayed() { }
    public virtual void AfterVideoPaused() { }
    public virtual void AfterVideoEnded() { }
    public virtual void AfterVideoStopped() { }
    public virtual void AfterVideoReloaded() { }

    #endregion

    #region Playback Events

    public virtual void AfterPlayerHandlerChanged(VideoPlayerType playerType) { }
    public virtual void AfterTimeChanged(float time) { }
    public virtual void AfterLoopChanged(bool loop) { }
    public virtual void AfterShufflePlayChanged(bool shufflePlay) { }
    public virtual void AfterSpeedChanged(float speed) { }
    public virtual void AfterRepeatChanged(ulong repeat) { }
    public virtual void AfterLocalDelayChanged(float localDelay) { }

    #endregion

    #region Screen Events

    public virtual void AfterTextureUpdated(Texture texture) { }
    public virtual void AfterMirrorFlipChanged(bool mirrorFlip) { }
    public virtual void AfterBrightnessChanged(float brightness) { }
    public virtual void AfterMaxResolutionChanged(int maxResolution) { }

    #endregion

    #region Audio Events

    public virtual void AfterVolumeChanged(float volume) { }
    public virtual void AfterMuteChanged(bool mute) { }

    #endregion

    #region Other Events

    public virtual void AfterTrackUpdated() { }
    public virtual void AfterQueueUpdated() { }
    public virtual void AfterHistoryUpdated() { }
    public virtual void AfterPlaylistsUpdated() { }
    public virtual void AfterTrackSynced() { }
    public virtual void AfterTrackLoaded() { }
    public virtual void AfterOwnerChanged() { }
    public virtual void AfterVideoRetry() { }

    #endregion
  }
}
