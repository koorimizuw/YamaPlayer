using VRC.SDKBase;
using VRC.SDK3.Components.Video;

namespace Yamadev.YamaStream
{
    public partial class Controller
    {
        public override void OnVideoReady()
        {
            _errorRetryCount = 0;
            if (State == PlayerState.Playing) Play(true);

            if (Networking.IsOwner(gameObject) && !_isLocal && !_reloading)
            {
                SyncedVideoTime = 0f;
                RequestSerialization();
            }
            else EnsureVideoTime();

            PlayTimeline(0f);

            foreach (Listener listener in EventListeners) listener.OnVideoReady();
            PrintLog($"{_playerType.GetString()}: Video ready.");
            _reloading = false;
        }

        public override void OnVideoStart()
        {
            foreach (Listener listener in EventListeners) listener.OnVideoStart();
            PrintLog($"{_playerType.GetString()}: Video start.");
        }

        public override void OnVideoLoop()
        {
            if (Networking.IsOwner(gameObject) && !_isLocal)
            {
                SyncedVideoTime = 0f;
                RequestSerialization();
            }

            PlayTimeline(0f);
            foreach (Listener listener in EventListeners) listener.OnVideoLoop();
            PrintLog($"{_playerType.GetString()}: Video loop.");
        }

        public override void OnVideoEnd()
        {
            if (Networking.IsOwner(gameObject) || _isLocal)
            {
                PlaylistForward();
                Stop();
            }

            foreach (Listener listener in EventListeners) listener.OnVideoEnd();
            PrintLog($"{_playerType.GetString()}: Video end.");
        }

        private void PlaylistForward()
        {
            if (!Utilities.IsValid(ActivePlaylist) || _forwardInterval < 0) return;
            SendCustomEventDelayedSeconds(nameof(Forward), _forwardInterval);
        }

        public override void OnVideoError(VideoError videoError)
        {
            PrintLog($"{_playerType.GetString()}: Video error {videoError}.");

            HandleErrorRetry(videoError);
            UpdateAudioLinkMaterial("_MediaPlaying", 6);
            foreach (Listener listener in EventListeners) listener.OnVideoError(videoError);
        }


        public void ErrorRetry()
        {
            if (IsPlaying || !Track.GetUrl().IsValidUrl()) return;
            _resolveTrack.Invoke();
            foreach (Listener listener in EventListeners) listener.OnVideoRetry();
        }


        private void HandleErrorRetry(VideoError videoError)
        {
            if (videoError == VideoError.AccessDenied)
            {
                PrintError("Access denied - no retry will be attempted");
                _errorRetryCount = 0;
                return;
            }


            if (_errorRetryCount < _maxErrorRetry)
            {
                _errorRetryCount++;
                PrintLog($"Scheduling retry {_errorRetryCount}/{_maxErrorRetry} in {_retryAfterSeconds} seconds");
                SendCustomEventDelayedSeconds(nameof(ErrorRetry), _retryAfterSeconds);
            }
            else
            {
                _errorRetryCount = 0;
                PrintError($"Maximum retry count ({_maxErrorRetry}) reached. Stopping retry attempts.");
            }
        }
    }
}