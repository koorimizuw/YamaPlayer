
using UdonSharp;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
    public partial class Controller : Listener
    {
        [UdonSynced] VideoPlayerType _targetPlayer;
        [UdonSynced] string _title = string.Empty;
        [UdonSynced] VRCUrl _url = VRCUrl.Empty;
        [UdonSynced] string _originalUrl = string.Empty;
        Track _track;
        UdonEvent _resolveTrack;

        void initializeTrack()
        {
            _track = Track.New(_videoPlayerType, string.Empty, VRCUrl.Empty);
            _resolveTrack = UdonEvent.New(this, nameof(Resolve));
        }

        public Track Track
        {
            get
            {
                if (!_initialized) initialize();
                return _track;
            }
            set
            {
                _track = value;
                foreach (Listener listener in _listeners) listener.OnTrackUpdated();
            }
        }

        public UdonEvent ResolveTrack
        {
            get
            {
                if (!_initialized) initialize();
                return _resolveTrack;
            }
            set => _resolveTrack = value;
        }

        public void PlayTrack(Track track, bool isReload = false)
        {
            if (!track.GetUrl().IsValidUrl()) return;
            if (isReload) _isReload = true;
            if (Track.GetUrl() != string.Empty) VideoPlayerHandle.Stop();
            VideoPlayerType = track.GetPlayer();
            Track = track;
            _resolveTrack.Invoke();
            _loading = true;
            if (Networking.IsOwner(gameObject) && !_isLocal && !isReload) RequestSerialization();
            foreach (Listener listener in _listeners) listener.OnUrlChanged();
        }
        public void Resolve() => VideoPlayerHandle.PlayUrl(Track.GetVRCUrl());

        public override void OnPreSerialization()
        {
            _targetPlayer = Track.GetPlayer();
            _title = Track.GetTitle();
            _url = Track.GetVRCUrl();
            _originalUrl = Track.GetOriginalUrl();
        }
    }
}