
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace Yamadev.YamaStream.UI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class UIController : Listener
    {
        // [Header("Settings")]
        [SerializeField] Controller _controller;
        [SerializeField] TextAsset _translation;
        [SerializeField] TextAsset _updateLogs;
        [SerializeField] bool _disableUIOnPickUp = true;
        [SerializeField, Range(0f, 10f)] float _disableUIDistance = 0f;
        [SerializeField] Font _font;

        // [Header("Color")]
        [SerializeField] Color _primaryColor = new Color(240f / 256f, 98f / 256f, 146f / 256f, 1.0f);
        [SerializeField] Color _secondaryColor = new Color(248f / 256f, 187f / 256f, 208f / 256f, 31f / 256f);
        [SerializeField] Color _ownerColor;
        [SerializeField] Color _adminColor;
        [SerializeField] Color _editorColor;
        [SerializeField] Color _viewerColor;

        // [Header("Modal")]
        [SerializeField] Modal _modal;
        [SerializeField] ToggleGroup _modalVideoPlayerSelector;
        [SerializeField] Toggle _modalUnityPlayer;
        [SerializeField] Toggle _modalAVProPlayer;

        // [Header("Animation")]
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
        [SerializeField] Slider _pitchSlider;
        [SerializeField] Text _pitchText;
        [SerializeField] GameObject _audioLinkSettings;
        [SerializeField] Toggle _audioLinkOn;
        [SerializeField] Toggle _audioLinkOff;

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
        [SerializeField] LoopScroll _permission;
        [SerializeField] GameObject _permissionEntry;
        [SerializeField] GameObject _permissionPage;

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
        [SerializeField] Text _audioLinkDesc;
        [SerializeField] Text _audioLinkOnText;
        [SerializeField] Text _audioLinkOffText;
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

        Localization _i18n;
        BoxCollider _uiBoxCollider;
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
            if (_idle != null && _idleImage != null) _idle.sprite = _idleImage;
            if (_animator != null && _defaultPlaylistOpen) _animator.SetTrigger("TogglePlaylist");
            _uiBoxCollider = GetComponentInChildren<BoxCollider>();
        }

        void Update()
        {
            if (_volumeHelper != null && _volumeTooltip != null)
                _volumeTooltip.text = $"{Mathf.Ceil(_volumeHelper.Percent * 100)}%";
            if (!_controller.Stopped) updateProgress();
            if (_disableUIOnPickUp && _uiBoxCollider != null)
                _uiBoxCollider.enabled =　!outOfDistance && !Networking.LocalPlayer.PickUpInHand();
            else _uiBoxCollider.enabled = !outOfDistance;
        }

        bool outOfDistance => _disableUIDistance > 0 && 
            Utilities.IsValid(Networking.LocalPlayer) &&
            (Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Origin).position - transform.position).sqrMagnitude > _disableUIDistance;

        Localization I18n
        {
            get
            {
                if (Utilities.IsValid(_i18n)) return _i18n;
                _i18n = Localization.Initialize(_translation.text);
                return _i18n;
            }
        }

        public Color PrimaryColor => _primaryColor;
        public Color SecondaryColor => _secondaryColor;

        public bool CheckPermission()
        {
            if ((int)_controller.PlayerPermission >= (int)PlayerPermission.Editor) return true;
            if (_modal != null)
            {
                _modal.Title = I18n.GetValue("noPermission");
                _modal.Message = I18n.GetValue("noPermissionMessage");
                _modal.CancelText = I18n.GetValue("close");
                _modal.CloseEvent = UdonEvent.Empty();
                _modal.Open(0);
            }
            return false;
        }

        public void SetUnityPlayer()
        {
            UpdateUI();
            if (_controller.VideoPlayerType == VideoPlayerType.UnityVideoPlayer || !CheckPermission()) return;
            if (_modal == null || (_controller.Stopped && !_controller.IsLoading))
            {
                SetUnityPlayerEvent();
                return;
            }
            _modal.Title = I18n.GetValue("confirmChangePlayer");
            _modal.Message = I18n.GetValue("confirmChangePlayerMessage");
            _modal.CancelText = I18n.GetValue("cancel");
            _modal.ExecuteText = I18n.GetValue("continue");
            _modal.CloseEvent = UdonEvent.Empty();
            _modal.ExecuteEvent = UdonEvent.New(this, nameof(SetUnityPlayerEvent));
            _modal.Open(1);
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
            if (_modal == null || (_controller.Stopped && !_controller.IsLoading))
            {
                SetAVProPlayerEvent();
                return;
            }
            _modal.Title = I18n.GetValue("confirmChangePlayer");
            _modal.Message = I18n.GetValue("confirmChangePlayerMessage");
            _modal.CancelText = I18n.GetValue("cancel");
            _modal.ExecuteText = I18n.GetValue("continue");
            _modal.CloseEvent = UdonEvent.Empty();
            _modal.ExecuteEvent = UdonEvent.New(this, nameof(SetAVProPlayerEvent));
            _modal.Open(1);
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
                _modal.Title = I18n.GetValue("playUrl");
                _modal.Message = I18n.GetValue("confirmPlayUrlMessage");
                _modal.CancelText = I18n.GetValue("cancel");
                _modal.ExecuteText = I18n.GetValue("confirmAddQueue");
                _modal.Execute2Text = I18n.GetValue("confirmPlayUrl");
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
            if (on) status.TurnOn(); 
            else status.TurnOff();
            _controller.SetMeOwner();
            _controller.Repeat = status.ToVector3();
        }

        public void SetRepeatStart()
        {
            if (_repeatSlider == null || _controller.Stopped) return;
            if (!_controller.Repeat.ToRepeatStatus().IsOn())
            {
                RepeatStatus status = _controller.Repeat.ToRepeatStatus();
                status.SetStartTime(_controller.IsLive ? 0f : Mathf.Clamp(_controller.Duration * _repeatSlider.SliderLeft.value, 0f, _controller.Duration));
                _controller.SetMeOwner();
                _controller.Repeat = status.ToVector3();
            }
            else _repeatSlider.SliderLeft.SetValueWithoutNotify(_controller.Repeat.ToRepeatStatus().GetStartTime() / _controller.Duration);
        }
        public void SetRepeatEnd()
        {
            if (_repeatSlider == null || _controller.Stopped) return;
            if (!_controller.Repeat.ToRepeatStatus().IsOn())
            {
                RepeatStatus status = _controller.Repeat.ToRepeatStatus();
                status.SetEndTime(_controller.IsLive ? 999999f : Mathf.Clamp(_controller.Duration * _repeatSlider.SliderRight.value, 0f, _controller.Duration));
                _controller.SetMeOwner();
                _controller.Repeat = status.ToVector3();
            }
            else _repeatSlider.SliderRight.SetValueWithoutNotify(_controller.Repeat.ToRepeatStatus().GetEndTime() / _controller.Duration);
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

        public void SetAudioLinkOn()
        {
            if (_audioLinkOn == null || !_audioLinkOn.isOn) return;
            _controller.UseAudioLink = true;
        }
        public void SetAudioLinkOff()
        {
            if (_audioLinkOff == null || !_audioLinkOff.isOn) return;
            _controller.UseAudioLink = false;
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
        public void SetPitch()
        {
            // if (_pitchSlider != null) _controller.Pitch = _pitchSlider.value;
        }
        public void SetMirrorInverse()
        {
            if (_mirrorInversion == null || !_mirrorInversion.isOn) return;
            _controller.MirrorInverse = true;
        }
        public void SetMirrorInverseOff()
        {
            if (_mirrorInversionOff == null || !_mirrorInversionOff.isOn) return;
            _controller.MirrorInverse = false;
        }
        public void SetMaxResolution144() => _controller.MaxResolution = 144;
        public void SetMaxResolution240() => _controller.MaxResolution = 240;
        public void SetMaxResolution360() => _controller.MaxResolution = 360;
        public void SetMaxResolution480() => _controller.MaxResolution = 480;
        public void SetMaxResolution720() => _controller.MaxResolution = 720;
        public void SetMaxResolution1080() => _controller.MaxResolution = 1080;
        public void SetMaxResolution2160() => _controller.MaxResolution = 2160;
        public void SetMaxResolution4320() => _controller.MaxResolution = 4320;
        public void SetLanguageAuto() => SetLanguage(Utils.GetLocalLanguage());
        public void SetLanguageJapanese() => SetLanguage("ja-JP");
        public void SetLanguageChineseChina() => SetLanguage("zh-CN");
        public void SetLanguageChineseTaiwan() => SetLanguage("zh-TW");
        public void SetLanguageKorean() => SetLanguage("ko-KR");
        public void SetLanguageEnglish() =>SetLanguage("en-US");
        public void SetLanguage(string language)
        {
            I18n.SetLanguage(language);
            UpdateUI();
            UpdateTranslation();
            GeneratePlaylistView();
        }
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
            string executeText = _controller.IsKaraokeMember ? I18n.GetValue("leaveMember") : I18n.GetValue("joinMember");
            _modal.Show(I18n.GetValue("karaokeMember"), string.Join("\n", _controller.KaraokeMembers), callback, I18n.GetValue("close"), executeText);
        }

        public void SetPermission()
        {
            if (_controller.Permission == null || _permission == null || _permissionIndex < 0) return;
            int index = Array.IndexOf(_permission.Indexes, _permissionIndex);
            if (index >= 0 &&
                _permission.GetComponent<ScrollRect>().content.GetChild(index).TryFind("Dropdown", out var dr) && 
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
                string notSetText = I18n.GetValue("notSet");
                string startText = _controller.Repeat.ToRepeatStatus().GetStartTime() == 0 ? notSetText : TimeSpan.FromSeconds(_controller.Repeat.ToRepeatStatus().GetStartTime()).ToString(_timeFormat);
                string endText = _controller.Repeat.ToRepeatStatus().GetEndTime() >= _controller.Duration || _controller.IsLive ? notSetText : TimeSpan.FromSeconds(_controller.Repeat.ToRepeatStatus().GetEndTime()).ToString(_timeFormat);
                _repeatSlider.SliderLeft.SetValueWithoutNotify(_controller.IsLive || !_controller.IsPlaying ? 0f : Mathf.Clamp(_controller.Repeat.ToRepeatStatus().GetStartTime() / _controller.Duration, 0f, 1f));
                _repeatSlider.SliderRight.SetValueWithoutNotify(_controller.IsLive || !_controller.IsPlaying ? 1f : Mathf.Clamp(_controller.Repeat.ToRepeatStatus().GetEndTime() / _controller.Duration, 0f, 1f));
                _repeatStartTime.text = $"{I18n.GetValue("start")}(A): {startText}";
                _repeatEndTime.text = $"{I18n.GetValue("end")}(B): {endText}";
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
            if (_audioLinkSettings != null) _audioLinkSettings.gameObject.SetActive(_controller.AudioLink != null);
            if (_audioLinkOn != null) _audioLinkOn.SetIsOnWithoutNotify(_controller.UseAudioLink);
            if (_audioLinkOff != null) _audioLinkOff.SetIsOnWithoutNotify(!_controller.UseAudioLink);
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
                    _message.text = I18n.GetValue("unknownErrorMessage");
                    break;
                case VideoError.InvalidURL:
                    _message.text = I18n.GetValue("invalidUrlMessage");
                    break;
                case VideoError.AccessDenied:
                    _message.text = I18n.GetValue("accessDeniedMessage");
                    break;
                case VideoError.RateLimited:
                    _message.text = I18n.GetValue("rateLimitedMessage");
                    break;
                case VideoError.PlayerError:
                    _message.text = I18n.GetValue("playerErrorMessage");
                    break;
                default:
                    break;
            }
        }

        void updateLoadingView()
        {
            if (_loading != null) _loading.SetActive(_controller.IsLoading);
            if (_animator != null) _animator.SetBool("Loading", _controller.IsLoading);
            if (_message != null) _message.text = I18n.GetValue("videoLoadingMessage");
        }

        public void GeneratePermissionView()
        {
            if (_controller.Permission == null || _permission == null) return;
            _permission.CallbackEvent = UdonEvent.New(this, nameof(UpdatePermissionView));
            _permission.Length = _controller.Permission.PermissionData.Count;

            bool showPage = _controller.PlayerPermission == PlayerPermission.Owner || _controller.PlayerPermission == PlayerPermission.Admin;
            if (_permissionEntry != null) _permissionEntry.SetActive(showPage);
            if (_permissionPage != null && !showPage && _permissionPage.activeSelf) _permissionPage.SetActive(false);
        }

        public void UpdatePermissionView()
        {
            for (int i = 0; i < _permission.LineCount; i++)
            {
                if (_permission.Indexes[i] == _permission.LastIndexes[i] || _permission.Indexes[i] == -1) continue;
                Transform cell = _permission.GetComponent<ScrollRect>().content.GetChild(i);
                DataToken value = _controller.Permission.PermissionData.GetValues()[_permission.Indexes[i]];
                if (value.DataDictionary.TryGetValue("displayName", TokenType.String, out DataToken displayName) &&
                    cell.TryFind("Name", out var name) &&
                    name.TryGetComponentLocal(out Text nameText)) nameText.text = displayName.String;

                PlayerPermission permission = (PlayerPermission)value.DataDictionary["permission"].Int;
                bool couldControl = (int)_controller.PlayerPermission > (int)permission;
                if (cell.TryFind("Label", out var label) &&
                    label.TryGetComponentLocal(out Text labelText)) labelText.text = permission == PlayerPermission.Owner ? "Owner" : "Admin";
                if (cell.TryFind("Dropdown", out var dropdown)) dropdown.gameObject.SetActive(couldControl);

                if (cell.TryFind("Mark", out var mark) && mark.TryGetComponentLocal(out Image markImage))
                {
                    switch (permission)
                    {
                        case PlayerPermission.Owner:
                            markImage.color = _ownerColor;
                            break;
                        case PlayerPermission.Admin:
                            markImage.color = _adminColor;
                            if (dropdown != null) dropdown.GetComponent<Dropdown>().SetValueWithoutNotify(0);
                            break;
                        case PlayerPermission.Editor:
                            markImage.color = _editorColor;
                            if (dropdown != null) dropdown.GetComponent<Dropdown>().SetValueWithoutNotify(1);
                            break;
                        case PlayerPermission.Viewer:
                            markImage.color = _viewerColor;
                            if (dropdown != null) dropdown.GetComponent<Dropdown>().SetValueWithoutNotify(2);
                            break;
                        default:
                            break;
                    }
                }

                if (cell.TryGetComponentLocal<IndexTrigger>(out var trigger)) trigger.SetProgramVariable("_varibaleObject", _permission.Indexes[i]);
                cell.gameObject.SetActive(true);
            }
        }

        public void UpdateTranslation()
        {
            if (_returnToMain != null) _returnToMain.text = I18n.GetValue("returnToMain");
            if (_inputUrl != null) _inputUrl.text = I18n.GetValue("inputUrl");
            if (_loopPlayText != null) _loopPlayText.text = I18n.GetValue("loop");
            if (_loopPlayText2 != null) _loopPlayText2.text = I18n.GetValue("loop");
            if (_shufflePlayText != null) _shufflePlayText.text = I18n.GetValue("shuffle");
            if (_shufflePlayText2 != null) _shufflePlayText2.text = I18n.GetValue("shuffle");
            if (_options != null) _options.text = I18n.GetValue("options");
            if (_settings != null) _settings.text = I18n.GetValue("settings");
            if (_karaokeMember != null) _karaokeMember.text = I18n.GetValue("karaokeMember");
            if (_playlist != null) _playlist.text = I18n.GetValue("playlist");
            if (_videoSearch != null) _videoSearch.text = I18n.GetValue("videoSearch");
            if (_version != null) _version.text = I18n.GetValue("version");
            if (_settingsTitle != null) _settingsTitle.text = I18n.GetValue("settingsTitle");
            if (_playback != null) _playback.text = I18n.GetValue("playback");
            if (_videoAndAudio != null) _videoAndAudio.text = I18n.GetValue("videoAndAudio");
            if (_other != null) _other.text = I18n.GetValue("other");
            if (_videoPlayer != null) _videoPlayer.text = $"{I18n.GetValue("videoPlayer")}<size=100>(Global)</size>";
            if (_videoPlayerDesc != null) _videoPlayerDesc.text = I18n.GetValue("videoPlayerDesc");
            if (_playbackSpeed != null) _playbackSpeed.text = $"{I18n.GetValue("playbackSpeed")}<size=100>(Global)</size>";
            if (_playbackSpeedDesc != null) _playbackSpeedDesc.text = I18n.GetValue("playbackSpeedDesc");
            if (_slower != null) _slower.text = I18n.GetValue("slower");
            if (_faster != null) _faster.text = I18n.GetValue("faster");
            if (_repeatPlay != null) _repeatPlay.text = $"{I18n.GetValue("repeatPlay")}<size=100>(Global)</size>";
            if (_repeatPlayDesc != null) _repeatPlayDesc.text = I18n.GetValue("repeatPlayDesc");
            if (_repeatOnText != null) _repeatOnText.text = I18n.GetValue("repeatOn");
            if (_repeatOffText != null) _repeatOffText.text = I18n.GetValue("repeatOff");
            if (_maxResolution != null) _maxResolution.text = I18n.GetValue("maxResolution");
            if (_maxResolutionDesc != null) _maxResolutionDesc.text = I18n.GetValue("maxResolutionDesc");
            if (_mirrorInversionTitle != null) _mirrorInversionTitle.text = I18n.GetValue("mirrorInversion");
            if (_mirrorInversionDesc != null) _mirrorInversionDesc.text = I18n.GetValue("mirrorInversionDesc");
            if (_mirrorInversionOnText != null) _mirrorInversionOnText.text = I18n.GetValue("mirrorInversionOn");
            if (_mirrorInversionOffText != null) _mirrorInversionOffText.text = I18n.GetValue("mirrorInversionOff");
            if (_brightness != null) _brightness.text = I18n.GetValue("brightness");
            if (_brightnessDesc != null) _brightnessDesc.text = I18n.GetValue("brightnessDesc");
            if (_audioLinkDesc != null) _audioLinkDesc.text = I18n.GetValue("audioLinkDesc");
            if (_audioLinkOnText != null) _audioLinkOnText.text = I18n.GetValue("audioLinkOn");
            if (_audioLinkOffText != null) _audioLinkOffText.text = I18n.GetValue("audioLinkOff");
            if (_karaokeModeText != null) _karaokeModeText.text = $"{I18n.GetValue("karaokeMode")}<size=100>(Global)</size>";
            if (_karaokeModeDesc != null) _karaokeModeDesc.text = I18n.GetValue("karaokeModeDesc");
            if (_karaokeModeOnText != null) _karaokeModeOnText.text = I18n.GetValue("karaokeModeOn");
            if (_danceModeOnText != null) _danceModeOnText.text = I18n.GetValue("danceModeOn");
            if (_karaokeModeOffText != null) _karaokeModeOffText.text = I18n.GetValue("karaokeModeOff");
            if (_localDelay != null) _localDelay.text = I18n.GetValue("localOffset");
            if (_localDelayDesc != null) _localDelayDesc.text = I18n.GetValue("localOffsetDesc");
            if (_languageSelect != null) _languageSelect.text = I18n.GetValue("languageSelect");

            if (_videoSearchTitle != null) _videoSearchTitle.text = I18n.GetValue("videoSearchTitle");
            if (_inputKeyword != null) _inputKeyword.text = I18n.GetValue("inputKeyword");
            if (_inLoading != null) _inLoading.text = I18n.GetValue("inLoading");

            if (_playlistTitle != null) _playlistTitle.text = I18n.GetValue("playlistTitle");
            if (_playQueue != null) _playQueue.text = I18n.GetValue("playQueue");
            if (_playHistory != null) _playHistory.text = I18n.GetValue("playHistory");
            if (_addVideoLink != null) _addVideoLink.text = I18n.GetValue("addVideoLink");
            if (_addLiveLink != null) _addLiveLink.text = I18n.GetValue("addLiveLink");
            if (_permissionTitle != null) _permissionTitle.text = I18n.GetValue("permission");
            if (_permissionDesc != null) _permissionDesc.text = $"<color=#64B5F6>Owner</color>\t\t\t{I18n.GetValue("ownerPermission")}\r\n<color=#BA68C8>Admin</color>\t\t\t{I18n.GetValue("adminPermission")}\r\n<color=#81C784>Editor</color>\t\t\t{I18n.GetValue("editorPermission")}\r\n<color=#FFB74D>Viewer</color>\t\t\t{I18n.GetValue("viewerPermission")}";

            if (_playlistTracks != null)
            {
                RectTransform scrollRectTransform = _playlistTracks.GetComponent<ScrollRect>().content;
                for (int i = 0; i < scrollRectTransform.childCount; i++)
                {
                    Transform cell = scrollRectTransform.GetChild(i);
                    if (cell.transform.TryFind("Actions", out var actions))
                    {
                        if (actions.TryFind("Return/Text", out var back) && back.TryGetComponentLocal<Text>(out var backText)) backText.text = I18n.GetValue("back");
                        if (actions.TryFind("Up/Text", out var up) && up.TryGetComponentLocal<Text>(out var upText)) upText.text = I18n.GetValue("moveUp");
                        if (actions.TryFind("Down/Text", out var down) && down.TryGetComponentLocal<Text>(out var downText)) downText.text = I18n.GetValue("moveDown");
                        if (actions.TryFind("Remove/Text", out var remove) && remove.TryGetComponentLocal<Text>(out var removeText)) removeText.text = I18n.GetValue("remove");
                        if (actions.TryFind("Add/Text", out var addQueue) && addQueue.TryGetComponentLocal<Text>(out var addQueueText)) addQueueText.text = I18n.GetValue("addQueue");
                        if (actions.TryFind("Play/Text", out var play) && play.TryGetComponentLocal<Text>(out var playText)) playText.text = I18n.GetValue("playVideo");
                    }
                }
            }
        }

        public override void OnPlayerJoined(VRCPlayerApi player) => GeneratePermissionView();
        public override void OnPlayerLeft(VRCPlayerApi player) => GeneratePermissionView();
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
        public override void OnVideoRetry() => updateLoadingView();
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
        public override void OnUseAudioLinkChanged() => updateAudioView();
        public override void OnMaxResolutionChanged() => updateScreenView();
        public override void OnMirrorInversionChanged() => updateScreenView();
        public override void OnEmissionChanged() => updateScreenView();
        public override void OnKaraokeModeChanged() => updateKaraokeView();
        public override void OnPermissionChanged() => GeneratePermissionView();
    }
}