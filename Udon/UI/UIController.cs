
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Data;
using VRC.SDKBase;
using Yamadev.YamaStream.UI;

namespace Yamadev.YamaStream
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class UIController : Listener
    {
        [Header("YamaPlayer Scripts")]
        [SerializeField] Controller _controller;
        [SerializeField] i18n _i18n;
        [SerializeField] TextAsset _updateLogs;

        [Header("Color")]
        [SerializeField] Color _primaryColor = new Color(240f / 256f, 98f / 256f, 146f / 256f, 1.0f);
        [SerializeField] Color _secondaryColor = new Color(248f / 256f, 187f / 256f, 208f / 256f, 31f / 256f);
        [SerializeField] Color _ownerColor;
        [SerializeField] Color _adminColor;
        [SerializeField] Color _editorColor;
        [SerializeField] Color _viewerColor;

        [Header("Modal")]
        [SerializeField] Modal _modal;
        [SerializeField] ToggleGroup _modalVideoPlayerSelector;
        [SerializeField] Toggle _modalUnityPlayer;
        [SerializeField] Toggle _modalAVProPlayer;

        [Header("Animation")]
        [SerializeField] Animator _animator;
        [SerializeField] bool _defaultPlaylistOpen;

        [Header("Main UI - Track Info")]
        [SerializeField] VRCUrlInputField _urlInputField;
        [SerializeField] VRCUrlInputField _urlInputFieldTop;
        [SerializeField] Text _title;
        [SerializeField] Text _url;

        [Header("Main UI - Progress")]
        [SerializeField] Text _videoTime;
        [SerializeField] Text _duration;
        [SerializeField] Slider _progress;
        [SerializeField] SliderHelper _progressHelper;
        [SerializeField] Text _progressTooltip;

        [Header("Main UI - Playback")]
        [SerializeField] Button _play;
        [SerializeField] Button _pause;
        [SerializeField] Button _loop;
        [SerializeField] Button _loopOff;
        [SerializeField] Button _inorderPlay;
        [SerializeField] Button _shufflePlay;

        [Header("Main UI - Audio")]
        [SerializeField] Button _mute;
        [SerializeField] Button _muteOff;
        [SerializeField] Slider _volume;
        [SerializeField] SliderHelper _volumeHelper;
        [SerializeField] Text _volumeTooltip;

        [Header("Main UI - Playlist")]
        [SerializeField] Transform _playlistSelector;
        [SerializeField] LoopScroll _playlists;
        [SerializeField] Text _playlistName;
        [SerializeField] LoopScroll _playlistTracks;
        [SerializeField] VRCUrlInputField _dynamicPlaylistUrlInput;

        [Header("Settings - Playback")]
        [SerializeField] Toggle _unityPlayer;
        [SerializeField] Toggle _avProPlayer;
        [SerializeField] Slider _speedSlider;
        [SerializeField] Text _speedText;
        [SerializeField] RangeSlider _repeatSlider;
        [SerializeField] Toggle _repeatOff;
        [SerializeField] Toggle _repeat;
        [SerializeField] Text _repeatStartTime;
        [SerializeField] Text _repeatEndTime;
        [SerializeField] Text _localDelayText;
        [SerializeField] Toggle _karaokeModeOff;
        [SerializeField] Toggle _karaokeModeKaraoke;
        [SerializeField] Toggle _karaokeModeDance;
        [SerializeField] GameObject _karaokeModal;

        [Header("Settings - Permission")]
        [SerializeField] GameObject _permissionEntry;
        [SerializeField] GameObject _permissionPage;
        [SerializeField] Transform _permissionContent;

        [Header("Loading")]
        [SerializeField] GameObject _loading;
        [SerializeField] Text _message;

        [Header("Idle")]
        [SerializeField] Image _idle;
        [SerializeField] Sprite _idleImage;

        [Header("Screen Renderer")]
        [SerializeField] Slider _emissionSlider;
        [SerializeField] Text _emissionText;
        [SerializeField] Toggle _maxResolution144;
        [SerializeField] Toggle _maxResolution240;
        [SerializeField] Toggle _maxResolution360;
        [SerializeField] Toggle _maxResolution480;
        [SerializeField] Toggle _maxResolution720;
        [SerializeField] Toggle _maxResolution1080;
        [SerializeField] Toggle _maxResolution2160;
        [SerializeField] Toggle _maxResolution4320;
        [SerializeField] Toggle _mirrorInversion;
        [SerializeField] Toggle _mirrorInversionOff;
        [SerializeField] RawImage _preview;

        [Header("Version")]
        [SerializeField] Text _versionText;
        [SerializeField] Text _updateLog;

        [Header("Debug")]
        [SerializeField, HideInInspector] Text _trackTitle;
        [SerializeField, HideInInspector] Text _trackUrl;
        [SerializeField, HideInInspector] Text _trackDisplayUrl;
        [SerializeField, HideInInspector] Text _networkDelay;
        [SerializeField, HideInInspector] Text _videoOffset;

        [Header("Translation - SideBar")]
        [SerializeField] Text _options;
        [SerializeField] Text _settings;
        [SerializeField] Text _playlist;
        [SerializeField] Text _videoSearch;
        [SerializeField] Text _version;

        [Header("Translation - General")]
        [SerializeField] Text _returnToMain;
        [SerializeField] Text _inputUrl;
        [SerializeField] Text _loopPlayText;
        [SerializeField] Text _loopPlayText2;
        [SerializeField] Text _shufflePlayText;
        [SerializeField] Text _shufflePlayText2;
        [SerializeField] Text _karaokeMember;
        [SerializeField] Text _settingsTitle;
        [SerializeField] Text _playback;
        [SerializeField] Text _videoAndAudio;
        [SerializeField] Text _other;
        [SerializeField] Text _videoPlayer;
        [SerializeField] Text _videoPlayerDesc;
        [SerializeField] Text _playbackSpeed;
        [SerializeField] Text _playbackSpeedDesc;
        [SerializeField] Text _slower;
        [SerializeField] Text _faster;
        [SerializeField] Text _repeatPlay;
        [SerializeField] Text _repeatPlayDesc;
        [SerializeField] Text _repeatOnText;
        [SerializeField] Text _repeatOffText;
        [SerializeField] Text _maxResolution;
        [SerializeField] Text _maxResolutionDesc;
        [SerializeField] Text _mirrorInversionTitle;
        [SerializeField] Text _mirrorInversionDesc;
        [SerializeField] Text _mirrorInversionOnText;
        [SerializeField] Text _mirrorInversionOffText;
        [SerializeField] Text _brightness;
        [SerializeField] Text _brightnessDesc;
        [SerializeField] Text _karaokeModeText;
        [SerializeField] Text _karaokeModeDesc;
        [SerializeField] Text _karaokeModeOnText;
        [SerializeField] Text _danceModeOnText;
        [SerializeField] Text _karaokeModeOffText;
        [SerializeField] Text _localDelay;
        [SerializeField] Text _localDelayDesc;
        [SerializeField] Text _languageSelect;
        [SerializeField] Text _permissionTitle;
        [SerializeField] Text _permissionDesc;

        [Header("Translation - Video Search")]
        [SerializeField] Text _videoSearchTitle;
        [SerializeField] Text _inputKeyword;
        [SerializeField] Text _inLoading;

        [Header("Translation - Playlist")]
        [SerializeField] Text _playlistTitle;
        [SerializeField] Text _playQueue;
        [SerializeField] Text _playHistory;
        [SerializeField] Text _addVideoLink;
        [SerializeField] Text _addLiveLink;

        string _timeFormat = @"hh\:mm\:ss";
        bool _progressDrag = false;
        int _permissionIndex = -1;
        int _playlistIndex = -1;
        int _playlistTrackIndex = -1;

        void Start()
        {
            if (_controller == null) return;
            _controller.AddListener(this);
            SendCustomEventDelayedFrames(nameof(UpdateUI), 3);
            SendCustomEventDelayedFrames(nameof(GeneratePlaylistView), 3);
            SendCustomEventDelayedFrames(nameof(UpdateTranslation), 3);
            if (_versionText != null) _versionText.text = $"YamaPlayer v{_controller.Version}";
            if (_updateLog != null && _updateLogs != null) _updateLog.text = _updateLogs.text;
            if (_preview != null) _controller.RawImageScreens = _controller.RawImageScreens.Add(_preview);
            if (_idle != null && _idleImage != null) _idle.sprite = _idleImage;
            if (_animator != null && _defaultPlaylistOpen) _animator.SetTrigger("TogglePlaylist");
        }
        void Update()
        {
            if (_volumeHelper != null && _volumeTooltip != null)
                _volumeTooltip.text = $"{Mathf.Ceil(_volumeHelper.Percent * 100)}%";
            if (_controller.Stopped) return;
            updateProgress();
        }

        public Color PrimaryColor => _primaryColor;
        public Color SecondaryColor => _secondaryColor;

        public bool CheckPermission()
        {
            if ((int)_controller.PlayerPermission >= (int)PlayerPermission.Editor) return true;
            if (_modal != null)
            {
                _modal.Title = _i18n.GetValue("noPermission");
                _modal.Message = _i18n.GetValue("noPermissionMessage");
                _modal.CancelText = _i18n.GetValue("close");
                _modal.CloseEvent = UdonEvent.Empty();
                _modal.Open(0);
            }
            return false;
        }

        public void SetUnityPlayer()
        {
            UpdateUI();
            if (_controller.VideoPlayerType == VideoPlayerType.UnityVideoPlayer || !CheckPermission()) return;
            if (_modal == null || _controller.Stopped)
            {
                SetUnityPlayerEvent();
                return;
            }
            if (_modal != null)
            {
                _modal.Title = _i18n.GetValue("confirmChangePlayer");
                _modal.Message = _i18n.GetValue("confirmChangePlayerMessage");
                _modal.CancelText = _i18n.GetValue("cancel");
                _modal.ExecuteText = _i18n.GetValue("continue");
                _modal.CloseEvent = UdonEvent.Empty();
                _modal.ExecuteEvent = UdonEvent.New(this, nameof(SetUnityPlayerEvent));
                _modal.Open(1);
            }
        }
        public void SetUnityPlayerEvent()
        {
            _controller.VideoPlayerType = VideoPlayerType.UnityVideoPlayer;
            if (_modal != null) _modal.Close();
        }

        public void SetAVProPlayer()
        {
            UpdateUI();
            if (_controller.VideoPlayerType == VideoPlayerType.AVProVideoPlayer || !CheckPermission()) return;
            if (_modal == null || _controller.Stopped)
            {
                SetAVProPlayerEvent();
                return;
            }
            if (_modal != null)
            {
                _modal.Title = _i18n.GetValue("confirmChangePlayer");
                _modal.Message = _i18n.GetValue("confirmChangePlayerMessage");
                _modal.CancelText = _i18n.GetValue("cancel");
                _modal.ExecuteText = _i18n.GetValue("continue");
                _modal.CloseEvent = UdonEvent.Empty();
                _modal.ExecuteEvent = UdonEvent.New(this, nameof(SetAVProPlayerEvent));
                _modal.Open(1);
            }
        }
        public void SetAVProPlayerEvent()
        {
            _controller.VideoPlayerType = VideoPlayerType.AVProVideoPlayer;
            if (_modal != null) _modal.Close();
        }

        public void PlayUrl() => PlayUrlBase(_urlInputField);
        public void PlayUrlTop() => PlayUrlBase(_urlInputFieldTop);
        public void PlayUrlBase(VRCUrlInputField urlInputField)
        {
            if (urlInputField == null || !urlInputField.GetUrl().Get().IsValidUrl() || !CheckPermission()) 
            { 
                urlInputField.SetUrl(VRCUrl.Empty); 
                return; 
            }
            if (_controller.Stopped && !_controller.IsLoading || _modal == null) 
            {
                _controller.SetMeOwner();
                _controller.PlayTrack(Track.New(_controller.VideoPlayerType, "", urlInputField.GetUrl()));
                urlInputField.SetUrl(VRCUrl.Empty);
                return; 
            }
            if (_modal != null)
            {
                _modal.Title = _i18n.GetValue("playUrl");
                _modal.Message = _i18n.GetValue("confirmPlayUrlMessage");
                _modal.CancelText = _i18n.GetValue("cancel");
                _modal.ExecuteText = _i18n.GetValue("confirmAddQueue");
                _modal.Execute2Text = _i18n.GetValue("confirmPlayUrl");
                _modal.CloseEvent = UdonEvent.New(this, nameof(HideVideoPlayerSelector));
                if (urlInputField == _urlInputField) _modal.ExecuteEvent = UdonEvent.New(this, nameof(AddUrlToQueueEvent));
                else if (urlInputField == _urlInputFieldTop) _modal.ExecuteEvent = UdonEvent.New(this, nameof(AddUrlTopToQueueEvent));
                if (urlInputField == _urlInputField) _modal.Execute2Event = UdonEvent.New(this, nameof(PlayUrlEvent));
                else if (urlInputField == _urlInputFieldTop) _modal.Execute2Event = UdonEvent.New(this, nameof(PlayUrlTopEvent));
                ShowVideoPlayerSelector();
                _modal.Open(2);
            }
        }

        public void ShowVideoPlayerSelector()
        {
            if (_modalVideoPlayerSelector != null)
            {
                if (_modalUnityPlayer != null) _modalUnityPlayer.isOn = _controller.VideoPlayerType == VideoPlayerType.UnityVideoPlayer;
                if (_modalAVProPlayer != null) _modalAVProPlayer.isOn = _controller.VideoPlayerType == VideoPlayerType.AVProVideoPlayer;
                _modalVideoPlayerSelector.gameObject.SetActive(true);
            }
        }
        public void HideVideoPlayerSelector()
        {
            if (_modalVideoPlayerSelector != null) _modalVideoPlayerSelector.gameObject.SetActive(false);
        }

        public VideoPlayerType GetVideoPlayerSelectorValue()
        {
            if (_modalUnityPlayer != null && _modalUnityPlayer.isOn) return VideoPlayerType.UnityVideoPlayer;
            if (_modalAVProPlayer != null && _modalAVProPlayer.isOn) return VideoPlayerType.AVProVideoPlayer;
            return _controller.VideoPlayerType;
        }

        public void AddUrlToQueueEvent() => AddUrlToQueueEventBase(_urlInputField);
        public void AddUrlTopToQueueEvent() => AddUrlToQueueEventBase(_urlInputFieldTop);

        public void AddUrlToQueueEventBase(VRCUrlInputField urlInputField)
        {
            if (urlInputField == null) return;
            _controller.SetMeOwner();
            _controller.Queue.AddTrack(Track.New(GetVideoPlayerSelectorValue(), "", urlInputField.GetUrl()));
            urlInputField.SetUrl(VRCUrl.Empty);
            HideVideoPlayerSelector();
            if (_modal != null) _modal.Close();
        }

        public void PlayUrlEvent() => PlayUrlEventBase(_urlInputField);
        public void PlayUrlTopEvent() => PlayUrlEventBase(_urlInputFieldTop);
        public void PlayUrlEventBase(VRCUrlInputField urlInputField)
        {
            if (urlInputField == null) return;
            _controller.SetMeOwner();
            _controller.PlayTrack(Track.New(GetVideoPlayerSelectorValue(), "", urlInputField.GetUrl()));
            urlInputField.SetUrl(VRCUrl.Empty);
            HideVideoPlayerSelector();
            if (_modal != null) _modal.Close();
        }

        public void Play()
        {
            if (!CheckPermission()) return;
            _controller.SetMeOwner();
            _controller.Paused = false;
        }
        public void Pause()
        {
            if (!CheckPermission()) return;
            _controller.SetMeOwner();
            _controller.Paused = true;
        }
        public void Stop()
        {
            if (!CheckPermission()) return;
            _controller.SetMeOwner();
            _controller.Stopped = true;
        }
        public void ProgressDrag() => _progressDrag = true;
        public void SetTime()
        {
            _progressDrag = false;
            if (_progress == null || !CheckPermission()) return;
            _controller.SetMeOwner();
            _controller.SetTime(_controller.Duration * _progress.value);
        }
        public void SetTimeByHelper()
        {
            if (_progressHelper == null || !CheckPermission()) return;
            _controller.SetMeOwner();
            _controller.SetTime(_controller.Duration * _progressHelper.Percent);
        }
        public void Loop()
        {
            if (!CheckPermission()) return;
            _controller.SetMeOwner();
            _controller.Loop = true;
        }
        public void LoopOff()
        {
            if (!CheckPermission()) return;
            _controller.SetMeOwner();
            _controller.Loop = false;
        }

        public void Reload() => _controller.Reload();

        public void Repeat() => SetRepeat(true);

        public void RepeatOff() => SetRepeat(false);

        public void SetRepeat(bool on)
        {
            if (!CheckPermission()) return;
            RepeatStatus status = _controller.Repeat.ToRepeatStatus();
            if (on && !status.IsOn()) status.TurnOn(); else return;
            if (!on && status.IsOn()) status.TurnOff(); else return;
            _controller.SetMeOwner();
            _controller.Repeat = status.ToVector3();
        }

        public void SetRepeatStart()
        {
            if (_repeatSlider != null && _controller.IsPlaying)
            {
                if (!_controller.Repeat.ToRepeatStatus().IsOn())
                {
                    RepeatStatus status = _controller.Repeat.ToRepeatStatus();
                    status.SetStartTime(_controller.IsLive ? 0f : Mathf.Clamp(_controller.Duration * _repeatSlider.SliderLeft.value, 0f, _controller.Duration));
                    _controller.SetMeOwner();
                    _controller.Repeat = status.ToVector3();
                }
                else _repeatSlider.SliderLeft.SetValueWithoutNotify(_controller.Repeat.ToRepeatStatus().GetStartTime() / _controller.Duration);
            }
        }
        public void SetRepeatEnd()
        {
            if (_repeatSlider != null && _controller.IsPlaying)
            {
                if (!_controller.Repeat.ToRepeatStatus().IsOn())
                {
                    RepeatStatus status = _controller.Repeat.ToRepeatStatus();
                    status.SetEndTime(_controller.IsLive ? 999999f : Mathf.Clamp(_controller.Duration * _repeatSlider.SliderRight.value, 0f, _controller.Duration));
                    _controller.SetMeOwner();
                    _controller.Repeat = status.ToVector3();
                }
                else _repeatSlider.SliderRight.SetValueWithoutNotify(_controller.Repeat.ToRepeatStatus().GetEndTime() / _controller.Duration);
            }
        }
        public void SetShuffle()
        {
            if (!CheckPermission()) return;
            _controller.SetMeOwner();
            _controller.ShufflePlay = true;
        }
        public void SetShuffleOff()
        {
            if (!CheckPermission()) return;
            _controller.SetMeOwner();
            _controller.ShufflePlay = false;
        }

        public void Backward()
        {
            if (!CheckPermission()) return;
            _controller.SetMeOwner();
            _controller.Backward();
        }
        public void Forward()
        {
            if (!CheckPermission()) return;
            _controller.SetMeOwner();
            _controller.Forward();
        }
        public void SetSpeed()
        {
            if (!CheckPermission()) return;
            _controller.SetMeOwner();
            if (_speedSlider != null) _controller.Speed = _speedSlider.value / 20f;
        }
        public void Mute() => _controller.Mute = true;
        public void MuteOff() => _controller.Mute = false;
        public void SetVolume() => _controller.Volume = _volume.value;
        public void SetVolumeByHelper()
        {
            if (_volumeHelper != null) _controller.Volume = _volumeHelper.Percent;
        }
        public void Subtract50ms() => _controller.LocalDelay -= 0.05f;
        public void Subtract100ms() => _controller.LocalDelay -= 0.1f;
        public void Add50ms() => _controller.LocalDelay += 0.05f;
        public void Add100ms() => _controller.LocalDelay += 0.1f;
        public void SetEmission()
        {
            if (_emissionSlider != null) _controller.Emission = _emissionSlider.value;
        }
        public void SetMirrorInverse() => _controller.MirrorInverse = true;
        public void SetMirrorInverseOff() => _controller.MirrorInverse = false;
        public void SetMaxResolution144() => _controller.MaxResolution = 144;
        public void SetMaxResolution240() => _controller.MaxResolution = 240;
        public void SetMaxResolution360() => _controller.MaxResolution = 360;
        public void SetMaxResolution480() => _controller.MaxResolution = 480;
        public void SetMaxResolution720() => _controller.MaxResolution = 720;
        public void SetMaxResolution1080() => _controller.MaxResolution = 1080;
        public void SetMaxResolution2160() => _controller.MaxResolution = 2160;
        public void SetMaxResolution4320() => _controller.MaxResolution = 4320;
        public void SetLanguageAuto() => _i18n.Language = null;
        public void SetLanguageJapanese() => _i18n.Language = "ja";
        public void SetLanguageChineseChina() => _i18n.Language = "zh-cn";
        public void SetLanguageChineseTaiwan() => _i18n.Language = "zh-tw";
        public void SetLanguageKorean() => _i18n.Language = "ko";
        public void SetLanguageEnglish() => _i18n.Language = "en";
        public void SetKaraokeModeOff()
        {
            if (!CheckPermission()) return;
            _controller.SetMeOwner();
            _controller.KaraokeMode = KaraokeMode.None;
        }
        public void SetKaraokeModeKaraoke()
        {
            if (!CheckPermission()) return;
            _controller.SetMeOwner();
            _controller.KaraokeMode = KaraokeMode.Karaoke;
        }
        public void SetKaraokeModeDance()
        {
            if (!CheckPermission()) return;
            _controller.SetMeOwner();
            _controller.KaraokeMode = KaraokeMode.Dance;
        }
        public void JoinKaraokeMembers()
        {
            if (_controller.IsKaraokeMember) return;
            _controller.KaraokeMembers = _controller.KaraokeMembers.Add(Networking.LocalPlayer.displayName);
            _controller.SetMeOwner();
            if (_modal != null && _modal.IsActive) OpenKaraokeMemberModal();
        }
        public void LeaveKaraokeMembers()
        {
            if (!_controller.IsKaraokeMember) return;
            _controller.KaraokeMembers = _controller.KaraokeMembers.Remove(Networking.LocalPlayer.displayName);
            _controller.SetMeOwner();
            if (_modal != null && _modal.IsActive) OpenKaraokeMemberModal();
        }
        public void OpenKaraokeMemberModal()
        {
            if (_modal == null || _controller.KaraokeMode == KaraokeMode.None) return;
            UdonEvent callback = _controller.IsKaraokeMember ? UdonEvent.New(this, nameof(LeaveKaraokeMembers)) : UdonEvent.New(this, nameof(JoinKaraokeMembers));
            string executeText = _controller.IsKaraokeMember ? _i18n.GetValue("leaveMember") : _i18n.GetValue("joinMember");
            _modal.Show(_i18n.GetValue("karaokeMember"), string.Join("\n", _controller.KaraokeMembers), callback, _i18n.GetValue("close"), executeText);
        }

        public void SetPermission()
        {
            if (_controller.Permission == null || _permissionContent == null || _permissionIndex < 0) return;
            IndexTrigger[] triggers = _permissionContent.GetComponentsInChildren<IndexTrigger>();
            if (_permissionIndex < triggers.Length && 
                triggers[_permissionIndex].transform.TryFind("Dropdown", out var dr) && 
                dr.TryGetComponentLocal(out Dropdown dropdown))
            {
                PlayerPermission playerPermission = PlayerPermission.Viewer;
                if (dropdown.value == 1) playerPermission = PlayerPermission.Editor;
                if (dropdown.value == 0) playerPermission = PlayerPermission.Admin;
                _controller.Permission.SetMeOwner();
                _controller.Permission.SetPermission(_permissionIndex, playerPermission);
            }
        }
    
        public void GeneratePlaylistView()
        {
            if (_playlists == null) return;
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
                if (cell.transform.TryFind("FolderMark", out var folderMark)) folderMark.gameObject.SetActive(!playlist.IsLoading);
                if (cell.transform.TryFind("Loading", out var loading)) loading.gameObject.SetActive(playlist.IsLoading);
                if (cell.transform.TryFind("Text", out var n) && n.TryGetComponentLocal(out Text name))
                    name.text = _controller.Playlists[_playlists.Indexes[i]].PlaylistName;
                if (cell.transform.TryFind("TrackCount", out var tr) && tr.TryGetComponentLocal(out Text trackCount))
                    trackCount.text = playlist.Length > 0 ? $"{_i18n.GetValue("total")} {playlist.Length} {_i18n.GetValue("tracks")}" : string.Empty;
                if (cell.TryGetComponentLocal<IndexTrigger>(out var trigger)) trigger.SetProgramVariable("_varibaleObject", _playlists.Indexes[i]);
            }
        }

        bool _isQueuePage
        {
            get
            {
                if (_playlistSelector == null) return false;
                Toggle[] toggles = _playlistSelector.GetComponentsInChildren<Toggle>();
                return toggles[0].isOn;
            }
        }
        bool _isHistoryPage
        {
            get
            {
                if (_playlistSelector == null) return false;
                Toggle[] toggles = _playlistSelector.GetComponentsInChildren<Toggle>();
                return toggles[1].isOn;
            }
        }

        public void GeneratePlaylistTracks()
        {
            if (_playlists == null || _playlistTracks == null) return;
            if (!_isQueuePage && !_isHistoryPage && _playlistIndex < 0) return;

            Playlist playlist = _isQueuePage ? _controller.Queue : _isHistoryPage ? _controller.History : _controller.Playlists[_playlistIndex];
            if (_playlistName != null) _playlistName.text = playlist.PlaylistName;

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

                if (cell.transform.TryFind("Info", out var info))
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
                if (cell.transform.TryFind("Actions", out var actions))
                {
                    if (actions.TryFind("Up", out var upMark)) upMark.gameObject.SetActive(_isQueuePage);
                    if (actions.TryFind("Down", out var downMark)) downMark.gameObject.SetActive(_isQueuePage);
                    if (actions.TryFind("Remove", out var removeMark)) removeMark.gameObject.SetActive(_isQueuePage);
                    if (actions.TryFind("Add", out var addMark)) addMark.gameObject.SetActive(!_isQueuePage);
                    if (actions.TryFind("Play", out var PlayMark)) PlayMark.gameObject.SetActive(!_isQueuePage);
                }
                if (cell.TryGetComponentLocal<Animator>(out var ani)) ani.SetTrigger("Reset");
                if (cell.TryGetComponentLocal<IndexTrigger>(out var trigger)) trigger.SetProgramVariable("_varibaleObject", _playlistTracks.Indexes[i]);
            }
        }

        public void RemoveFromQueue()
        {
            if (!CheckPermission()) return;
            if (_playlistTracks == null || _playlistTrackIndex < 0) return;

            _controller.SetMeOwner();
            if (_playlistTrackIndex < _controller.Queue.Length) _controller.Queue.Remove(_playlistTrackIndex);
        }

        public void AddPlaylistTrackToQueue()
        {
            if (!CheckPermission()) return;
            if (_playlistTracks == null || _playlistTrackIndex < 0) return;

            Playlist playlist = _isHistoryPage ? _controller.History :
                _playlistIndex >= 0 && _playlistIndex < _controller.Playlists.Length ? _controller.Playlists[_playlistIndex] : null;
            _controller.SetMeOwner();
            _controller.Queue.AddTrack(playlist.GetTrack(_playlistTrackIndex));
        }

        public void MoveUp()
        {
            if (!CheckPermission()) return;
            _controller.SetMeOwner();
            _controller.Queue.MoveUp(_playlistTrackIndex);
        }

        public void MoveDown()
        {
            if (!CheckPermission()) return;
            _controller.SetMeOwner();
            _controller.Queue.MoveDown(_playlistTrackIndex);
        }
        public void PlayPlaylistTrack()
        {
            if (!CheckPermission()) return;
            if (_playlistTracks == null || _playlistTrackIndex < 0) return;

            Playlist playlist = _isHistoryPage ? _controller.History : 
                _playlistIndex >= 0 && _playlistIndex < _controller.Playlists.Length ? _controller.Playlists[_playlistIndex] : null;
            _controller.SetMeOwner();
            if (playlist != null) _controller.PlayTrack(playlist, _playlistTrackIndex);
        }

        public void AddDynamicPlaylist()
        {
            if (_dynamicPlaylistUrlInput == null || !_dynamicPlaylistUrlInput.GetUrl().IsValid()) return;
            _controller.AddDynamicPlaylist(_dynamicPlaylistUrlInput.GetUrl());
            _dynamicPlaylistUrlInput.SetUrl(VRCUrl.Empty);
        }

        public void UpdateUI()
        {
            updatePlayerView();
            updateProgress();
            updatePlaybackView();
            updateTrackView();
            updateLoadingView();
            updateAudioView();
            if (_idle != null && _idleImage != null) _idle.gameObject.SetActive(_controller.Stopped);
            if (_preview != null) _preview.enabled = _controller.IsPlaying;
        }

        void updateProgress()
        {
            if (_videoTime != null) _videoTime.text = TimeSpan.FromSeconds(_controller.VideoTime).ToString(_timeFormat);
            if (_duration != null) _duration.text = _controller.IsLive ? "Live" : TimeSpan.FromSeconds(_controller.Duration).ToString(_timeFormat);
            if (_progress != null && !_progressDrag) _progress.SetValueWithoutNotify(_controller.IsLive ? 1f : Mathf.Clamp(_controller.Duration == 0f ? 0f : _controller.VideoTime / _controller.Duration, 0f, 1f));
            if (_progressHelper != null && _progressTooltip != null)
            {
                _progressHelper.gameObject.SetActive(!_controller.Stopped && !_controller.IsLive);
                if (_controller.IsLive) _progressTooltip.text = "Live";
                else _progressTooltip.text = TimeSpan.FromSeconds(_controller.Duration * _progressHelper.Percent).ToString(_timeFormat);
            }
        }

        void updatePlayerView()
        {
            if (_unityPlayer != null) _unityPlayer.SetIsOnWithoutNotify(_controller.VideoPlayerType == VideoPlayerType.UnityVideoPlayer);
            if (_avProPlayer != null) _avProPlayer.SetIsOnWithoutNotify(_controller.VideoPlayerType == VideoPlayerType.AVProVideoPlayer);
        }

        void updatePlaybackView()
        {
            if (_play != null) _play.gameObject.SetActive(!_controller.IsPlaying);
            if (_pause != null) _pause.gameObject.SetActive(_controller.IsPlaying);
            if (_loop != null) _loop.gameObject.SetActive(!_controller.Loop);
            if (_loopOff != null) _loopOff.gameObject.SetActive(_controller.Loop);
            if (_speedSlider != null) _speedSlider.SetValueWithoutNotify((float)Math.Round(_controller.Speed * 20));
            if (_speedText != null) _speedText.text = $"{_controller.Speed:F2}x";
            if (_repeatOff != null) _repeatOff.SetIsOnWithoutNotify(!_controller.Repeat.ToRepeatStatus().IsOn());
            if (_repeat != null) _repeat.SetIsOnWithoutNotify(_controller.Repeat.ToRepeatStatus().IsOn());
            if (_repeatSlider != null && _repeatStartTime != null && _repeatEndTime != null)
            {
                string notSetText = _i18n.GetValue("notSet");
                string startText = _controller.Repeat.ToRepeatStatus().GetStartTime() == 0 ? notSetText : TimeSpan.FromSeconds(_controller.Repeat.ToRepeatStatus().GetStartTime()).ToString(_timeFormat);
                string endText = _controller.Repeat.ToRepeatStatus().GetEndTime() >= _controller.Duration || _controller.IsLive ? notSetText : TimeSpan.FromSeconds(_controller.Repeat.ToRepeatStatus().GetEndTime()).ToString(_timeFormat);
                _repeatSlider.SliderLeft.SetValueWithoutNotify(_controller.IsLive || !_controller.IsPlaying ? 0f : Mathf.Clamp(_controller.Repeat.ToRepeatStatus().GetStartTime() / _controller.Duration, 0f, 1f));
                _repeatSlider.SliderRight.SetValueWithoutNotify(_controller.IsLive || !_controller.IsPlaying ? 1f : Mathf.Clamp(_controller.Repeat.ToRepeatStatus().GetEndTime() / _controller.Duration, 0f, 1f));
                _repeatStartTime.text = $"{_i18n.GetValue("start")}(A): {startText}";
                _repeatEndTime.text = $"{_i18n.GetValue("end")}(B): {endText}";
            }
            if (_localDelayText != null) _localDelayText.text = (Mathf.Round(_controller.LocalDelay * 100) / 100).ToString();
            if (_inorderPlay != null) _inorderPlay.gameObject.SetActive(!_controller.ShufflePlay);
            if (_shufflePlay != null) _shufflePlay.gameObject.SetActive(_controller.ShufflePlay);
        }

        void updateTrackView()
        {
            Track track = _controller.Track;
            if (_title != null) _title.text = track.HasTitle() ? track.GetTitle() : track.GetUrl();
            if (_url != null) _url.text = track.HasTitle() ? track.GetUrl() : string.Empty;
        }

        void updateAudioView()
        {
            if (_mute != null) _mute.gameObject.SetActive(!_controller.Mute);
            if (_muteOff != null) _muteOff.gameObject.SetActive(_controller.Mute);
            if (_volume != null) _volume.SetValueWithoutNotify(_controller.Volume);
        }

        void updateScreenView()
        {
            if (_mirrorInversion != null) _mirrorInversion.SetIsOnWithoutNotify(_controller.MirrorInverse);
            if (_mirrorInversionOff != null) _mirrorInversionOff.SetIsOnWithoutNotify(!_controller.MirrorInverse);
            if (_maxResolution144 != null) _maxResolution144.SetIsOnWithoutNotify(_controller.MaxResolution == 144);
            if (_maxResolution240 != null) _maxResolution240.SetIsOnWithoutNotify(_controller.MaxResolution == 240);
            if (_maxResolution360 != null) _maxResolution360.SetIsOnWithoutNotify(_controller.MaxResolution == 360);
            if (_maxResolution480 != null) _maxResolution480.SetIsOnWithoutNotify(_controller.MaxResolution == 480);
            if (_maxResolution720 != null) _maxResolution720.SetIsOnWithoutNotify(_controller.MaxResolution == 720);
            if (_maxResolution1080 != null) _maxResolution1080.SetIsOnWithoutNotify(_controller.MaxResolution == 1080);
            if (_maxResolution2160 != null) _maxResolution2160.SetIsOnWithoutNotify(_controller.MaxResolution == 2160);
            if (_maxResolution4320 != null) _maxResolution4320.SetIsOnWithoutNotify(_controller.MaxResolution == 4320);
            if (_emissionSlider != null) _emissionSlider.SetValueWithoutNotify(_controller.Emission);
            if (_emissionText != null) _emissionText.text = $"{Mathf.Ceil(_controller.Emission * 100)}%";
        }

        void updateKaraokeView()
        {
            if (_karaokeModeOff != null) _karaokeModeOff.SetIsOnWithoutNotify(_controller.KaraokeMode == KaraokeMode.None);
            if (_karaokeModeKaraoke != null) _karaokeModeKaraoke.SetIsOnWithoutNotify(_controller.KaraokeMode == KaraokeMode.Karaoke);
            if (_karaokeModeDance != null) _karaokeModeDance.SetIsOnWithoutNotify(_controller.KaraokeMode == KaraokeMode.Dance);
            if (_karaokeModal != null) _karaokeModal.SetActive(_controller.KaraokeMode != KaraokeMode.None);
        }

        void updateErrorView(VideoError videoError)
        {
            if (_loading != null) _loading.SetActive(true);
            if (_animator != null) _animator.SetBool("Loading", false);
            if (_message == null) return;
            switch (videoError)
            {
                case VideoError.Unknown:
                    _message.text = _i18n.GetValue("unknownErrorMessage");
                    break;
                case VideoError.InvalidURL:
                    _message.text = _i18n.GetValue("invalidUrlMessage");
                    break;
                case VideoError.AccessDenied:
                    _message.text = _i18n.GetValue("accessDeniedMessage");
                    break;
                case VideoError.RateLimited:
                    _message.text = _i18n.GetValue("rateLimitedMessage");
                    break;
                case VideoError.PlayerError:
                    _message.text = _i18n.GetValue("playerErrorMessage");
                    break;
                default:
                    break;
            }
        }

        void updateLoadingView()
        {
            if (_loading != null) _loading.SetActive(_controller.IsLoading);
            if (_animator != null) _animator.SetBool("Loading", _controller.IsLoading);
            if (_message != null) _message.text = _i18n.GetValue("videoLoadingMessage");
        }

        void updatePermissionView()
        {
            if (_controller.Permission == null) return;
            bool showPage = _controller.PlayerPermission == PlayerPermission.Owner || _controller.PlayerPermission == PlayerPermission.Admin;
            if (_permissionEntry != null) _permissionEntry.SetActive(showPage);
            if (_permissionPage != null && !showPage && _permissionPage.activeSelf) _permissionPage.SetActive(false);
            if (_permissionContent == null) return;
            for (int i = 0; i < _permissionContent.childCount; i++)
            {
                Transform item = _permissionContent.GetChild(i);
                item.gameObject.SetActive(false);
                if (i >= _controller.Permission.PermissionData.Count) continue;
                DataToken value = _controller.Permission.PermissionData.GetValues()[i];
                value.DataDictionary.TryGetValue("displayName", TokenType.String, out DataToken displayName);
                item.Find("Name").GetComponent<Text>().text = displayName.String;

                PlayerPermission permission = (PlayerPermission)value.DataDictionary["permission"].Int;
                bool noEdit = (int)_controller.PlayerPermission <= (int)permission;
                if (noEdit)
                {
                    item.Find("Label").GetComponent<Text>().text = permission == PlayerPermission.Owner ? "Owner" : "Admin";
                    item.Find("Label").gameObject.SetActive(noEdit);
                    item.Find("Dropdown").gameObject.SetActive(!noEdit);
                }

                switch (permission)
                {
                    case PlayerPermission.Owner:
                        item.Find("Mark").GetComponent<Image>().color = _ownerColor;
                        break;
                    case PlayerPermission.Admin:
                        item.Find("Mark").GetComponent<Image>().color = _adminColor;
                        item.Find("Dropdown").GetComponent<Dropdown>().SetValueWithoutNotify(0);
                        break;
                    case PlayerPermission.Editor:
                        item.Find("Mark").GetComponent<Image>().color = _editorColor;
                        item.Find("Dropdown").GetComponent<Dropdown>().SetValueWithoutNotify(1);
                        break;
                    case PlayerPermission.Viewer:
                        item.Find("Mark").GetComponent<Image>().color = _viewerColor;
                        item.Find("Dropdown").GetComponent<Dropdown>().SetValueWithoutNotify(2);
                        break;
                    default:
                        break;
                }
                item.gameObject.SetActive(true);
            }
        }

        public void UpdateTranslation()
        {
            if (_i18n == null) return;
            if (_returnToMain != null) _returnToMain.text = _i18n.GetValue("returnToMain");
            if (_inputUrl != null) _inputUrl.text = _i18n.GetValue("inputUrl");
            if (_loopPlayText != null) _loopPlayText.text = _i18n.GetValue("loop");
            if (_loopPlayText2 != null) _loopPlayText2.text = _i18n.GetValue("loop");
            if (_shufflePlayText != null) _shufflePlayText.text = _i18n.GetValue("shuffle");
            if (_shufflePlayText2 != null) _shufflePlayText2.text = _i18n.GetValue("shuffle");
            if (_options != null) _options.text = _i18n.GetValue("options");
            if (_settings != null) _settings.text = _i18n.GetValue("settings");
            if (_karaokeMember != null) _karaokeMember.text = _i18n.GetValue("karaokeMember");
            if (_playlist != null) _playlist.text = _i18n.GetValue("playlist");
            if (_videoSearch != null) _videoSearch.text = _i18n.GetValue("videoSearch");
            if (_version != null) _version.text = _i18n.GetValue("version");
            if (_settingsTitle != null) _settingsTitle.text = _i18n.GetValue("settingsTitle");
            if (_playback != null) _playback.text = _i18n.GetValue("playback");
            if (_videoAndAudio != null) _videoAndAudio.text = _i18n.GetValue("videoAndAudio");
            if (_other != null) _other.text = _i18n.GetValue("other");
            if (_videoPlayer != null) _videoPlayer.text = $"{_i18n.GetValue("videoPlayer")}<size=100>(Global)</size>";
            if (_videoPlayerDesc != null) _videoPlayerDesc.text = _i18n.GetValue("videoPlayerDesc");
            if (_playbackSpeed != null) _playbackSpeed.text = $"{_i18n.GetValue("playbackSpeed")}<size=100>(Global)</size>";
            if (_playbackSpeedDesc != null) _playbackSpeedDesc.text = _i18n.GetValue("playbackSpeedDesc");
            if (_slower != null) _slower.text = _i18n.GetValue("slower");
            if (_faster != null) _faster.text = _i18n.GetValue("faster");
            if (_repeatPlay != null) _repeatPlay.text = $"{_i18n.GetValue("repeatPlay")}<size=100>(Global)</size>";
            if (_repeatPlayDesc != null) _repeatPlayDesc.text = _i18n.GetValue("repeatPlayDesc");
            if (_repeatOnText != null) _repeatOnText.text = _i18n.GetValue("repeatOn");
            if (_repeatOffText != null) _repeatOffText.text = _i18n.GetValue("repeatOff");
            if (_maxResolution != null) _maxResolution.text = _i18n.GetValue("maxResolution");
            if (_maxResolutionDesc != null) _maxResolutionDesc.text = _i18n.GetValue("maxResolutionDesc");
            if (_mirrorInversionTitle != null) _mirrorInversionTitle.text = _i18n.GetValue("mirrorInversion");
            if (_mirrorInversionDesc != null) _mirrorInversionDesc.text = _i18n.GetValue("mirrorInversionDesc");
            if (_mirrorInversionOnText != null) _mirrorInversionOnText.text = _i18n.GetValue("mirrorInversionOn");
            if (_mirrorInversionOffText != null) _mirrorInversionOffText.text = _i18n.GetValue("mirrorInversionOff");
            if (_brightness != null) _brightness.text = _i18n.GetValue("brightness");
            if (_brightnessDesc != null) _brightnessDesc.text = _i18n.GetValue("brightnessDesc");
            if (_karaokeModeText != null) _karaokeModeText.text = $"{_i18n.GetValue("karaokeMode")}<size=100>(Global)</size>";
            if (_karaokeModeDesc != null) _karaokeModeDesc.text = _i18n.GetValue("karaokeModeDesc");
            if (_karaokeModeOnText != null) _karaokeModeOnText.text = _i18n.GetValue("karaokeModeOn");
            if (_danceModeOnText != null) _danceModeOnText.text = _i18n.GetValue("danceModeOn");
            if (_karaokeModeOffText != null) _karaokeModeOffText.text = _i18n.GetValue("karaokeModeOff");
            if (_localDelay != null) _localDelay.text = _i18n.GetValue("localOffset");
            if (_localDelayDesc != null) _localDelayDesc.text = _i18n.GetValue("localOffsetDesc");
            if (_languageSelect != null) _languageSelect.text = _i18n.GetValue("languageSelect");

            if (_videoSearchTitle != null) _videoSearchTitle.text = _i18n.GetValue("videoSearchTitle");
            if (_inputKeyword != null) _inputKeyword.text = _i18n.GetValue("inputKeyword");
            if (_inLoading != null) _inLoading.text = _i18n.GetValue("inLoading");

            if (_playlistTitle != null) _playlistTitle.text = _i18n.GetValue("playlistTitle");
            if (_playQueue != null) _playQueue.text = _i18n.GetValue("playQueue");
            if (_playHistory != null) _playHistory.text = _i18n.GetValue("playHistory");
            if (_addVideoLink != null) _addVideoLink.text = _i18n.GetValue("addVideoLink");
            if (_addLiveLink != null) _addLiveLink.text = _i18n.GetValue("addLiveLink");
            if (_permissionTitle != null) _permissionTitle.text = _i18n.GetValue("permission");
            if (_permissionDesc != null) _permissionDesc.text = $"<color=#64B5F6>Owner</color>\t\t\t{_i18n.GetValue("ownerPermission")}\r\n<color=#BA68C8>Admin</color>\t\t\t{_i18n.GetValue("adminPermission")}\r\n<color=#81C784>Editor</color>\t\t\t{_i18n.GetValue("editorPermission")}\r\n<color=#FFB74D>Viewer</color>\t\t\t{_i18n.GetValue("viewerPermission")}";

            if (_playlistTracks != null)
            {
                RectTransform scrollRectTransform = _playlistTracks.GetComponent<ScrollRect>().content;
                for (int i = 0; i < scrollRectTransform.childCount; i++)
                {
                    Transform cell = scrollRectTransform.GetChild(i);
                    if (cell.transform.TryFind("Actions", out var actions))
                    {
                        if (actions.TryFind("Return/Text", out var back) && back.TryGetComponentLocal<Text>(out var backText)) backText.text = _i18n.GetValue("back");
                        if (actions.TryFind("Up/Text", out var up) && up.TryGetComponentLocal<Text>(out var upText)) upText.text = _i18n.GetValue("moveUp");
                        if (actions.TryFind("Down/Text", out var down) && down.TryGetComponentLocal<Text>(out var downText)) downText.text = _i18n.GetValue("moveDown");
                        if (actions.TryFind("Remove/Text", out var remove) && remove.TryGetComponentLocal<Text>(out var removeText)) removeText.text = _i18n.GetValue("remove");
                        if (actions.TryFind("Add/Text", out var addQueue) && addQueue.TryGetComponentLocal<Text>(out var addQueueText)) addQueueText.text = _i18n.GetValue("addQueue");
                        if (actions.TryFind("Play/Text", out var play) && play.TryGetComponentLocal<Text>(out var playText)) playText.text = _i18n.GetValue("playVideo");
                    }
                }
            }
        }

        public override void OnPlayerJoined(VRCPlayerApi player) => updatePermissionView();
        public override void OnPlayerLeft(VRCPlayerApi player) => updatePermissionView();
        public override void OnVideoReady() => UpdateUI();
        public override void OnVideoStart() => UpdateUI();
        public override void OnVideoEnd() => UpdateUI();
        public override void OnVideoPlay() => UpdateUI();
        public override void OnVideoPause() => UpdateUI();
        public override void OnVideoStop()
        {
            UpdateUI();
            GeneratePlaylistTracks();
        }
        public override void OnVideoError(VideoError videoError) => updateErrorView(videoError);
        public override void OnPlayerChanged() => UpdateUI();
        public override void OnLoopChanged() => updatePlaybackView();
        public override void OnRepeatChanged() => updatePlaybackView();
        public override void OnSpeedChanged() => updatePlaybackView();
        public override void OnLocalDelayChanged() => updatePlaybackView();
        public override void OnShufflePlayChanged() => updatePlaybackView();
        public override void OnTrackUpdated() => updateTrackView();
        public override void OnUrlChanged()
        {
            updateLoadingView();
            GeneratePlaylistTracks();
        }
        public override void OnVideoInfoLoaded()
        {
            if (_isQueuePage) GeneratePlaylistTracks();
        }
        public override void OnQueueUpdated()
        {
            if (_isQueuePage) GeneratePlaylistTracks();
        }
        public override void OnHistoryUpdated()
        {
            if (_isHistoryPage) GeneratePlaylistTracks();
        }
        public override void OnPlaylistsUpdated() => GeneratePlaylistView();
        public override void OnVolumeChanged() => updateAudioView();
        public override void OnMuteChanged() => updateAudioView();
        public override void OnMaxResolutionChanged() => updateScreenView();
        public override void OnMirrorInversionChanged() => updateScreenView();
        public override void OnEmissionChanged() => updateScreenView();
        public override void OnKaraokeModeChanged() => updateKaraokeView();
        public override void OnLanguageChanged()
        {
            UpdateUI();
            UpdateTranslation();
            GeneratePlaylistView();
        }
        public override void OnPermissionChanged() => updatePermissionView();
    }
}