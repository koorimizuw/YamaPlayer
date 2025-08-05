using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace Yamadev.YamaStream.UI
{
    public partial class UIController
    {
        [SerializeField] private bool _defaultPlaylistOpen;

        [SerializeField] private Transform _playlistSelector;
        [SerializeField] private LoopScroll _playlists;
        [SerializeField] private Text _playlistName;
        [SerializeField] private LoopScroll _playlistTracks;
        [SerializeField] private VRCUrlInputField _dynamicPlaylistUrlInput;

        [SerializeField] private Text _playlistTitle;
        [SerializeField] private Text _playQueue;
        [SerializeField] private Text _playHistory;
        [SerializeField] private Text _addVideoLink;
        [SerializeField] private Text _addLiveLink;

        private int _playlistIndex = -1;
        private int _playlistTrackIndex = -1;

        public void GeneratePlaylistView()
        {
            if (!Utilities.IsValid(_playlists)) return;
            _playlists.CallbackEvent = UdonEvent.New(this, nameof(UpdatePlaylistsContent));
            _playlists.Length = _controller.Playlists.Length;
        }

        public void UpdatePlaylistsContent()
        {
            for (int i = 0; i < _playlists.LineCount; i++)
            {
                if (_playlists.Indexes[i] == _playlists.LastIndexes[i] || _playlists.Indexes[i] == -1) continue;
                Transform cell = _playlists.GetComponent<ScrollRect>().content.GetChild(i);
                Playlist playlist = _controller.Playlists[_playlists.Indexes[i]];
                if (cell.TryFind("FolderMark", out var folderMark)) folderMark.gameObject.SetActive(!playlist.IsLoading);
                if (cell.TryFind("Loading", out var loading)) loading.gameObject.SetActive(playlist.IsLoading);
                if (cell.TryFind("Text", out var n) && n.TryGetComponentLocal(out Text name))
                    name.text = _controller.Playlists[_playlists.Indexes[i]].PlaylistName;
                if (cell.TryFind("TrackCount", out var tr) && tr.TryGetComponentLocal(out Text trackCount))
                    trackCount.text = playlist.Length > 0 ? $"{I18n.GetValue("total")} {playlist.Length} {I18n.GetValue("tracks")}" : string.Empty;
                if (cell.TryGetComponentLocal<IndexTrigger>(out var trigger)) trigger.SetProgramVariable("_varibaleObject", _playlists.Indexes[i]);
            }
        }

        bool _isQueuePage
        {
            get
            {
                if (!_playlistSelector) return false;
                Toggle[] toggles = _playlistSelector.GetComponentsInChildren<Toggle>();
                return toggles[0].isOn;
            }
        }
        bool _isHistoryPage
        {
            get
            {
                if (!_playlistSelector) return false;
                Toggle[] toggles = _playlistSelector.GetComponentsInChildren<Toggle>();
                return toggles[1].isOn;
            }
        }

        public void GeneratePlaylistTracks()
        {
            if (!Utilities.IsValid(_playlists) || !Utilities.IsValid(_playlistTracks)) return;
            if (!_isQueuePage && !_isHistoryPage && _playlistIndex < 0) return;

            Playlist playlist = _isQueuePage ? _controller.Queue : _isHistoryPage ? _controller.History : _controller.Playlists[_playlistIndex];
            if (Utilities.IsValid(_playlistName)) _playlistName.text = playlist.PlaylistName;

            _playlistTracks.CallbackEvent = UdonEvent.New(this, nameof(UpdatePlaylistTracksContent));
            _playlistTracks.Length = playlist.Length;
        }

        public void UpdatePlaylistTracksContent()
        {
            for (int i = 0; i < _playlistTracks.LineCount; i++)
            {
                if (_playlistTracks.Indexes[i] == _playlistTracks.LastIndexes[i] || _playlistTracks.Indexes[i] == -1) continue;
                Transform cell = _playlistTracks.GetComponent<ScrollRect>().content.GetChild(i);

                Playlist playlist = _isQueuePage ? _controller.Queue : _isHistoryPage ? _controller.History : _controller.Playlists[_playlistIndex];
                Track track = playlist.GetTrack(_playlistTracks.Indexes[i]);
                bool isPlaying = playlist == _controller.ActivePlaylist && _controller.PlayingTrackIndex == _playlistTracks.Indexes[i];

                if (cell.TryFind("Info", out var info))
                {
                    if (info.TryFind("Title", out var ti) && ti.TryGetComponentLocal(out Text title))
                    {
                        title.text = track.HasTitle() ? track.GetTitle() : track.GetUrl();
                        title.color = isPlaying ? _primaryColor : Color.white;
                    }
                    if (info.TryFind("Url", out var u) && u.TryGetComponentLocal(out Text url))
                        url.text = track.HasTitle() ? track.GetUrl() : string.Empty;
                    if (info.TryFind("No", out var no) && no.TryGetComponentLocal(out Text numberText))
                    {
                        numberText.text = $"{_playlistTracks.Indexes[i] + 1}";
                        numberText.gameObject.SetActive(!isPlaying);
                    }
                    if (info.TryFind("PlayingMark", out var playingMark)) playingMark.gameObject.SetActive(isPlaying);
                }
                if (cell.TryFind("Actions", out var actions))
                {
                    if (actions.TryFind("Up", out var upMark)) upMark.gameObject.SetActive(_isQueuePage);
                    if (actions.TryFind("Down", out var downMark)) downMark.gameObject.SetActive(_isQueuePage);
                    if (actions.TryFind("Remove", out var removeMark)) removeMark.gameObject.SetActive(_isQueuePage);
                    if (actions.TryFind("Copy", out var copyUrl) && copyUrl.TryFind("URL", out var trackUrl) && trackUrl.TryGetComponentLocal<InputField>(out var trackUrlText))
                    {
                        copyUrl.gameObject.SetActive(!_isQueuePage);
                        trackUrlText.text = track.GetUrl();
                    }
                    if (actions.TryFind("Add", out var addMark)) addMark.gameObject.SetActive(!_isQueuePage);
                    if (actions.TryFind("Play", out var PlayMark)) PlayMark.gameObject.SetActive(!_isQueuePage);
                }
                if (cell.TryGetComponentLocal<Animator>(out var ani)) ani.SetTrigger("Reset");
                if (cell.TryGetComponentLocal<IndexTrigger>(out var trigger)) trigger.SetProgramVariable("_varibaleObject", _playlistTracks.Indexes[i]);
            }
        }

        public void RemoveFromQueue()
        {
            if (!IsPermissionGranted()) return;
            if (!_playlistTracks || _playlistTrackIndex < 0) return;

            _controller.Queue.TakeOwnership();
            if (_playlistTrackIndex < _controller.Queue.Length) _controller.Queue.RemoveTrack(_playlistTrackIndex);
        }

        public void AddPlaylistTrackToQueue()
        {
            if (!IsPermissionGranted()) return;
            if (!_playlistTracks || _playlistTrackIndex < 0) return;

            Playlist playlist = _isHistoryPage ? _controller.History :
                _playlistIndex >= 0 && _playlistIndex < _controller.Playlists.Length ? _controller.Playlists[_playlistIndex] : null;
            _controller.Queue.TakeOwnership();
            _controller.Queue.AddTrack(playlist.GetTrack(_playlistTrackIndex));
        }

        public void MoveUp()
        {
            if (!IsPermissionGranted()) return;
            _controller.Queue.TakeOwnership();
            _controller.Queue.MoveUp(_playlistTrackIndex);
        }

        public void MoveDown()
        {
            if (!IsPermissionGranted()) return;
            _controller.Queue.TakeOwnership();
            _controller.Queue.MoveDown(_playlistTrackIndex);
        }
        public void PlayPlaylistTrack()
        {
            if (!IsPermissionGranted()) return;
            if (!_playlistTracks || _playlistTrackIndex < 0) return;

            Playlist playlist = _isHistoryPage ? _controller.History :
                _playlistIndex >= 0 && _playlistIndex < _controller.Playlists.Length ? _controller.Playlists[_playlistIndex] : null;
            _controller.TakeOwnership();
            if (Utilities.IsValid(playlist)) _controller.PlayTrack(playlist, _playlistTrackIndex);
        }

        public void AddDynamicPlaylist()
        {
            if (!_dynamicPlaylistUrlInput || !_dynamicPlaylistUrlInput.GetUrl().IsValid()) return;
            _controller.TakeOwnership();
            _controller.AddDynamicPlaylist(_dynamicPlaylistUrlInput.GetUrl());
            _dynamicPlaylistUrlInput.SetUrl(VRCUrl.Empty);
        }
    }
}