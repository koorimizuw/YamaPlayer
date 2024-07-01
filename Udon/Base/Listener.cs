
using UdonSharp;
using VRC.SDK3.Components.Video;

namespace Yamadev.YamaStream
{
    public abstract class Listener : UdonSharpBehaviour
    {
        #region Video Event
        public override void OnVideoPlay() { }
        public override void OnVideoPause() { }
        public override void OnVideoEnd() { }
        public override void OnVideoError(VideoError videoError) { }
        public override void OnVideoLoop() { }
        public override void OnVideoReady() { }
        public override void OnVideoStart() { }
        public virtual void OnVideoStop() { }
        public virtual void OnVideoReloaded() { }
        #endregion

        #region Playback Event
        public virtual void OnSetTime(float time) { }
        public virtual void OnSpeedChanged() { }
        public virtual void OnRepeatChanged() { }
        public virtual void OnLocalDelayChanged() { }
        #endregion

        #region Screen Event
        public virtual void OnMirrorInversionChanged() { }
        public virtual void OnEmissionChanged() { }
        public virtual void OnMaxResolutionChanged() { }
        #endregion

        #region Audio Event
        public virtual void OnVolumeChanged() { }
        public virtual void OnMuteChanged() { }
        #endregion

        public virtual void OnVideoInfoLoaded() { }
        public virtual void OnTrackUpdated() { }
        public virtual void OnQueueUpdated() { }
        public virtual void OnHistoryUpdated() { }
        public virtual void OnPlaylistsUpdated() { }
        public virtual void OnTrackSynced(string url) { }
        public virtual void OnUrlChanged() { }
        public virtual void OnLoopChanged() { }
        public virtual void OnShufflePlayChanged() { }
        public virtual void OnOwnerChanged() { }
        public virtual void OnPlayerChanged() { }
        public virtual void OnVideoRetry() { }
        public virtual void OnLanguageChanged() { }
        public virtual void OnKaraokeModeChanged() { }
        public virtual void OnKaraokeMemberChanged() { }
        public virtual void OnPermissionChanged() { }
    }
}