using UdonSharp;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
    public partial class Controller
    {
        [UdonSynced] VideoPlayerType _targetPlayer;
        [UdonSynced] string _title = string.Empty;
        [UdonSynced] VRCUrl _url = VRCUrl.Empty;
        [UdonSynced] string _originalUrl = string.Empty;
        Track _track;
        UdonEvent _resolveTrack;

        public Track Track
        {
            get
            {
                if (!Utilities.IsValid(_track))
                    _track = Track.Empty();
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
                if (!Utilities.IsValid(_resolveTrack)) 
                    _resolveTrack = UdonEvent.New(this, nameof(Resolve));
                return _resolveTrack;
            }
            set => _resolveTrack = value;
        }

        public void PlayTrack(Track track)
        {
            if (!track.GetUrl().IsValidUrl()) return;
            if (Track.GetUrl() != string.Empty && (Networking.IsOwner(gameObject) || _isLocal))
                Stop();
            LoadTrack(track);
            _state = (byte)PlayerState.Playing;
        }

        private void LoadTrack(Track track, bool isReload = false)
        {
            _reloading = isReload;
            Handler.Stop();
            if (!isReload) ChangePlayerTyper(track.GetPlayerType());
            Track = track;
            ResolveTrack.Invoke();
            if (Networking.IsOwner(gameObject) && !_isLocal && !isReload) RequestSerialization();
            foreach (Listener listener in _listeners) listener.OnUrlChanged();
            PrintLog($"Load url: {track.GetUrl()}.");
        }

        public void Resolve() => Handler.LoadUrl(Track.GetVRCUrl());

        public void Reload()
        {
            if (!Stopped && !IsLoading) LoadTrack(Track, true);
        }

        public override void OnPreSerialization()
        {
            _targetPlayer = Track.GetPlayerType();
            _title = Track.GetTitle();
            _url = Track.GetVRCUrl();
            _originalUrl = Track.GetOriginalUrl();
        }
    }
}