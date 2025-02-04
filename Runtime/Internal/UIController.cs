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
        [SerializeField] bool _disableUIOnPickUp = true;
        [SerializeField, Range(0f, 10f)] float _disableUIDistance = 0f;
        [SerializeField] Font _font;
        [SerializeField, HideInInspector] string[] _supportedLanguages = new string[0];
        [SerializeField] TextAsset _updateLogFile;
        [SerializeField] TextAsset _translationTextFile;

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
        [SerializeField] Toggle _modalImageViewer;

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
        [SerializeField] Toggle _imageViewer;
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

        [Header("Slide")]
        [SerializeField] Toggle _slideOn;
        [SerializeField] Toggle _slideOff;
        [SerializeField] Toggle _slide1s;
        [SerializeField] Toggle _slide2s;
        [SerializeField] Toggle _slide3s;

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
        [SerializeField] Text _imageViewerModalText;
        [SerializeField] Text _karaokeMember;
        [SerializeField] Text _settingsTitle;
        [SerializeField] Text _playback;
        [SerializeField] Text _videoAndAudio;
        [SerializeField] Text _other;
        [SerializeField] Text _videoPlayer;
        [SerializeField] Text _videoPlayerDesc;
        [SerializeField] Text _imageViewerText;
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
        [SerializeField] Text _slideMode;
        [SerializeField] Text _slideModeDesc;
        [SerializeField] Text _slideOnText;
        [SerializeField] Text _slideOffText;
        [SerializeField] Text _slideSeconds;
        [SerializeField] Text _slideSecondsDesc;
        [SerializeField] Text _slide1sText;
        [SerializeField] Text _slide2sText;
        [SerializeField] Text _slide3sText;
        [SerializeField] Text _permissionTitle;
        [SerializeField] Text _permissionDesc;

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
            if (!_controller) return;
            _controller.AddListener(this);
            SendCustomEventDelayedFrames(nameof(UpdateUI), 3);
            SendCustomEventDelayedFrames(nameof(GeneratePlaylistView), 3);
            SendCustomEventDelayedFrames(nameof(UpdateTranslation), 3);
            if (Utilities.IsValid(_versionText)) _versionText.text = $"YamaPlayer v{_controller.Version}";
            if (Utilities.IsValid(_updateLog) && Utilities.IsValid(_updateLogFile)) _updateLog.text = _updateLogFile.text;
            if (Utilities.IsValid(_idle) && Utilities.IsValid(_idleImage)) _idle.sprite = _idleImage;
            if (Utilities.IsValid(_animator) && _defaultPlaylistOpen) _animator.SetTrigger("TogglePlaylist");
            _uiBoxCollider = GetComponentInChildren<BoxCollider>();
        }

        void Update()
        {
            if (Utilities.IsValid(_volumeHelper) && Utilities.IsValid(_volumeTooltip))
                _volumeTooltip.text = $"{Mathf.Ceil(_volumeHelper.Percent * 100)}%";
            if (!_controller.Stopped) UpdateProgressView();
            if (Utilities.IsValid(_uiBoxCollider))
                _uiBoxCollider.enabled = !OutOfDistance && (!_disableUIOnPickUp || !Networking.LocalPlayer.PickUpInHand());
        }

        private bool OutOfDistance => _disableUIDistance > 0 && 
            Utilities.IsValid(Networking.LocalPlayer) &&
            (Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Origin).position - transform.position).sqrMagnitude > _disableUIDistance;

        Localization i18n
        {
            get
            {
                if (Utilities.IsValid(_i18n)) return _i18n;
                _i18n = Localization.Initialize(Utilities.IsValid(_translationTextFile) ? _translationTextFile.text : string.Empty);
                return _i18n;
            }
        }

        public Color PrimaryColor => _primaryColor;
        public Color SecondaryColor => _secondaryColor;

        public bool CheckPermission()
        {
            if ((int)_controller.PlayerPermission >= (int)PlayerPermission.Editor) return true;
            if (Utilities.IsValid(_modal))
            {
                _modal.Title = i18n.GetValue("noPermission");
                _modal.Message = i18n.GetValue("noPermissionMessage");
                _modal.CancelText = i18n.GetValue("close");
                _modal.CloseEvent = UdonEvent.Empty();
                _modal.Open(0);
            }
            return false;
        }

        public void SetUnityPlayer()
        {
            UpdateUI();
            if (_controller.PlayerType == VideoPlayerType.UnityVideoPlayer || !CheckPermission()) return;
            if (!_modal || (_controller.Stopped && !_controller.IsLoading))
            {
                SetUnityPlayerEvent();
                return;
            }
            _modal.Title = i18n.GetValue("confirmChangePlayer");
            _modal.Message = i18n.GetValue("confirmChangePlayerMessage");
            _modal.CancelText = i18n.GetValue("cancel");
            _modal.ExecuteText = i18n.GetValue("continue");
            _modal.CloseEvent = UdonEvent.Empty();
            _modal.ExecuteEvent = UdonEvent.New(this, nameof(SetUnityPlayerEvent));
            _modal.Open(1);
        }

        public void SetUnityPlayerEvent()
        {
            _controller.PlayerType = VideoPlayerType.UnityVideoPlayer;
            if (Utilities.IsValid(_modal)) _modal.Close();
        }

        public void SetAVProPlayer()
        {
            UpdateUI();
            if (_controller.PlayerType == VideoPlayerType.AVProVideoPlayer || !CheckPermission()) return;
            if (!_modal || (_controller.Stopped && !_controller.IsLoading))
            {
                SetAVProPlayerEvent();
                return;
            }
            _modal.Title = i18n.GetValue("confirmChangePlayer");
            _modal.Message = i18n.GetValue("confirmChangePlayerMessage");
            _modal.CancelText = i18n.GetValue("cancel");
            _modal.ExecuteText = i18n.GetValue("continue");
            _modal.CloseEvent = UdonEvent.Empty();
            _modal.ExecuteEvent = UdonEvent.New(this, nameof(SetAVProPlayerEvent));
            _modal.Open(1);
        }

        public void SetAVProPlayerEvent()
        {
            _controller.PlayerType = VideoPlayerType.AVProVideoPlayer;
            if (Utilities.IsValid(_modal)) _modal.Close();
        }

        public void SetImageViewer()
        {
            UpdateUI();
            if (_controller.PlayerType == VideoPlayerType.ImageViewer || !CheckPermission()) return;
            if (!_modal || (_controller.Stopped && !_controller.IsLoading))
            {
                SetImageViewerEvent();
                return;
            }
            _modal.Title = i18n.GetValue("confirmChangePlayer");
            _modal.Message = i18n.GetValue("confirmChangePlayerMessage");
            _modal.CancelText = i18n.GetValue("cancel");
            _modal.ExecuteText = i18n.GetValue("continue");
            _modal.CloseEvent = UdonEvent.Empty();
            _modal.ExecuteEvent = UdonEvent.New(this, nameof(SetImageViewerEvent));
            _modal.Open(1);
        }

        public void SetImageViewerEvent()
        {
            _controller.PlayerType = VideoPlayerType.ImageViewer;
            if (Utilities.IsValid(_modal)) _modal.Close();
        }

        public void PlayUrl() => PlayUrlBase(_urlInputField);
        public void PlayUrlTop() => PlayUrlBase(_urlInputFieldTop);
        public void PlayUrlBase(VRCUrlInputField urlInputField)
        {
            if (!urlInputField || !urlInputField.GetUrl().Get().IsValidUrl() || !CheckPermission()) 
            { 
                urlInputField.SetUrl(VRCUrl.Empty); 
                return; 
            }
            if (_controller.Stopped && !_controller.IsLoading || !_modal) 
            {
                _controller.TakeOwnership();
                _controller.PlayTrack(Track.New(_controller.PlayerType, "", urlInputField.GetUrl()));
                urlInputField.SetUrl(VRCUrl.Empty);
                return; 
            }
            if (Utilities.IsValid(_modal))
            {
                _modal.Title = i18n.GetValue("playUrl");
                _modal.Message = i18n.GetValue("confirmPlayUrlMessage");
                _modal.CancelText = i18n.GetValue("cancel");
                _modal.ExecuteText = i18n.GetValue("confirmAddQueue");
                _modal.Execute2Text = i18n.GetValue("confirmPlayUrl");
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
            if (Utilities.IsValid(_modalVideoPlayerSelector))
            {
                if (Utilities.IsValid(_modalUnityPlayer)) _modalUnityPlayer.isOn = _controller.PlayerType == VideoPlayerType.UnityVideoPlayer;
                if (Utilities.IsValid(_modalAVProPlayer)) _modalAVProPlayer.isOn = _controller.PlayerType == VideoPlayerType.AVProVideoPlayer;
                if (Utilities.IsValid(_modalImageViewer)) _modalImageViewer.isOn = _controller.PlayerType == VideoPlayerType.ImageViewer;
                _modalVideoPlayerSelector.gameObject.SetActive(true);
            }
        }
        public void HideVideoPlayerSelector()
        {
            if (Utilities.IsValid(_modalVideoPlayerSelector)) _modalVideoPlayerSelector.gameObject.SetActive(false);
        }

        public VideoPlayerType GetVideoPlayerSelectorValue()
        {
            if (Utilities.IsValid(_modalUnityPlayer) && _modalUnityPlayer.isOn) return VideoPlayerType.UnityVideoPlayer;
            if (Utilities.IsValid(_modalAVProPlayer) && _modalAVProPlayer.isOn) return VideoPlayerType.AVProVideoPlayer;
            if (Utilities.IsValid(_modalImageViewer) && _modalImageViewer.isOn) return VideoPlayerType.ImageViewer;
            return _controller.PlayerType;
        }

        public void AddUrlToQueueEvent() => AddUrlToQueueEventBase(_urlInputField);
        public void AddUrlTopToQueueEvent() => AddUrlToQueueEventBase(_urlInputFieldTop);

        public void AddUrlToQueueEventBase(VRCUrlInputField urlInputField)
        {
            if (!urlInputField) return;
            _controller.Queue.TakeOwnership();
            _controller.Queue.AddTrack(Track.New(GetVideoPlayerSelectorValue(), "", urlInputField.GetUrl()));
            urlInputField.SetUrl(VRCUrl.Empty);
            HideVideoPlayerSelector();
            if (Utilities.IsValid(_modal)) _modal.Close();
        }

        public void PlayUrlEvent() => PlayUrlEventBase(_urlInputField);
        public void PlayUrlTopEvent() => PlayUrlEventBase(_urlInputFieldTop);
        public void PlayUrlEventBase(VRCUrlInputField urlInputField)
        {
            if (!urlInputField) return;
            _controller.TakeOwnership();
            _controller.PlayTrack(Track.New(GetVideoPlayerSelectorValue(), "", urlInputField.GetUrl()));
            urlInputField.SetUrl(VRCUrl.Empty);
            HideVideoPlayerSelector();
            if (Utilities.IsValid(_modal)) _modal.Close();
        }

        public void Play()
        {
            if (!CheckPermission()) return;
            _controller.TakeOwnership();
            _controller.Play();
        }

        public void Pause()
        {
            if (!CheckPermission()) return;
            _controller.TakeOwnership();
            _controller.Pause();
        }

        public void Stop()
        {
            if (!CheckPermission()) return;
            _controller.TakeOwnership();
            _controller.Stop();
        }

        public void ProgressDrag() => _progressDrag = true;

        public void SetTime()
        {
            _progressDrag = false;
            if (!_progress || !CheckPermission() || _controller.Stopped) return;
            _controller.TakeOwnership();
            if (_controller.SlideMode) _controller.SetPage((int)_progress.value);
            else _controller.SetTime(_controller.Duration * _progress.value);
        }

        public void SetTimeByHelper()
        {
            if (!_progressHelper || !CheckPermission()) return;
            _controller.TakeOwnership();
            _controller.SetTime(_controller.Duration * _progressHelper.Percent);
        }

        public void SlideOff()
        {
            if (!CheckPermission()) return;
            _controller.TakeOwnership();
            _controller.SlideMode = false;
        }

        public void SlideOn()
        {
            if (!CheckPermission()) return;
            _controller.TakeOwnership();
            _controller.SlideMode = true;
        }

        public void SetSlideSeconds(int seconds)
        {
            if (!CheckPermission()) return;
            _controller.TakeOwnership();
            _controller.SlideSeconds = seconds;
        }

        public void SetSlide1s() => SetSlideSeconds(1);
        public void SetSlide2s() => SetSlideSeconds(2);
        public void SetSlide3s() => SetSlideSeconds(3);

        public void Loop()
        {
            if (!CheckPermission()) return;
            _controller.TakeOwnership();
            _controller.Loop = true;
        }

        public void LoopOff()
        {
            if (!CheckPermission()) return;
            _controller.TakeOwnership();
            _controller.Loop = false;
        }

        public void Reload() => _controller.Reload();

        public void Repeat() => SetRepeat(true);

        public void RepeatOff() => SetRepeat(false);

        public void SetRepeat(bool on)
        {
            if (!CheckPermission()) return;
            RepeatStatus status = _controller.RepeatStatus;
            if (on) status.TurnOn(); 
            else status.TurnOff();
            _controller.TakeOwnership();
            _controller.RepeatStatus = status;
        }

        public void SetRepeatStart()
        {
            if (!_repeatSlider || _controller.Stopped || !CheckPermission()) return;
            RepeatStatus status = _controller.RepeatStatus;
            if (!status.IsOn())
            {
                status.SetStartTime(_controller.IsLive ? 0f : Mathf.Clamp(_controller.Duration * _repeatSlider.SliderLeft.value, 0f, _controller.Duration));
                _controller.TakeOwnership();
                _controller.RepeatStatus = status;
            }
            else _repeatSlider.SliderLeft.SetValueWithoutNotify(status.GetStartTime() / _controller.Duration);
        }

        public void SetRepeatEnd()
        {
            if (!_repeatSlider || _controller.Stopped || !CheckPermission()) return;
            RepeatStatus status = _controller.RepeatStatus;
            if (!status.IsOn())
            {
                status.SetEndTime(_controller.IsLive ? 0f : Mathf.Clamp(_controller.Duration * _repeatSlider.SliderRight.value, 0f, _controller.Duration));
                _controller.TakeOwnership();
                _controller.RepeatStatus = status;
            }
            else _repeatSlider.SliderRight.SetValueWithoutNotify(status.GetEndTime() / _controller.Duration);
        }

        public void SetShuffle()
        {
            if (!CheckPermission()) return;
            _controller.TakeOwnership();
            _controller.ShufflePlay = true;
        }

        public void SetShuffleOff()
        {
            if (!CheckPermission()) return;
            _controller.TakeOwnership();
            _controller.ShufflePlay = false;
        }

        public void Backward()
        {
            if (!CheckPermission()) return;
            _controller.TakeOwnership();
            if (_controller.SlideMode) _controller.SetPage(_controller.SlidePage - 1);
            else _controller.Backward();
        }

        public void Forward()
        {
            if (!CheckPermission()) return;
            _controller.TakeOwnership();
            if (_controller.SlideMode) _controller.SetPage(_controller.SlidePage + 1);
            else _controller.Forward();
        }

        public void SetSpeed()
        {
            if (!CheckPermission()) return;
            _controller.TakeOwnership();
            if (Utilities.IsValid(_speedSlider)) _controller.Speed = _speedSlider.value / 20f;
        }

        public void SetAudioLinkOn()
        {
            if (!_audioLinkOn || !_audioLinkOn.isOn) return;
            _controller.UseAudioLink = true;
        }
        public void SetAudioLinkOff()
        {
            if (!_audioLinkOff || !_audioLinkOff.isOn) return;
            _controller.UseAudioLink = false;
        }

        public void Mute() => _controller.Mute = true;
        public void MuteOff() => _controller.Mute = false;

        public void SetVolume() => _controller.Volume = _volume.value;
        public void SetVolumeByHelper()
        {
            if (Utilities.IsValid(_volumeHelper)) _controller.Volume = _volumeHelper.Percent;
        }

        public void Subtract50ms() => _controller.LocalDelay -= 0.05f;
        public void Subtract100ms() => _controller.LocalDelay -= 0.1f;
        public void Add50ms() => _controller.LocalDelay += 0.05f;
        public void Add100ms() => _controller.LocalDelay += 0.1f;

        public void SetEmission()
        {
            if (Utilities.IsValid(_emissionSlider)) _controller.Emission = _emissionSlider.value;
        }

        public void SetPitch()
        {
            // if (_pitchSlider != null) _controller.Pitch = _pitchSlider.value;
        }

        public void SetMirrorInverse()
        {
            if (!_mirrorInversion || !_mirrorInversion.isOn) return;
            _controller.MirrorInverse = true;
        }
        public void SetMirrorInverseOff()
        {
            if (!_mirrorInversionOff || !_mirrorInversionOff.isOn) return;
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

        public void SetLanguageAuto() => SetLanguage(null);
        public void SetLanguageJapanese() => SetLanguage("ja");
        public void SetLanguageChineseChina() => SetLanguage("zh-CN");
        public void SetLanguageChineseTaiwan() => SetLanguage("zh-TW");
        public void SetLanguageKorean() => SetLanguage("ko");
        public void SetLanguageEnglish() =>SetLanguage("en");

        public void SetLanguage(string language)
        {
            i18n.SetLanguage(language);
            UpdateUI();
            UpdateTranslation();
            GeneratePlaylistView();
        }

        public void SetKaraokeModeOff()
        {
            if (!CheckPermission()) return;
            _controller.TakeOwnership();
            _controller.KaraokeMode = KaraokeMode.None;
        }

        public void SetKaraokeModeKaraoke()
        {
            if (!CheckPermission()) return;
            _controller.TakeOwnership();
            _controller.KaraokeMode = KaraokeMode.Karaoke;
        }

        public void SetKaraokeModeDance()
        {
            if (!CheckPermission()) return;
            _controller.TakeOwnership();
            _controller.KaraokeMode = KaraokeMode.Dance;
        }

        public void JoinKaraokeMembers()
        {
            if (_controller.KaraokeMode == KaraokeMode.None || _controller.IsKaraokeMember) return;
            _controller.TakeOwnership();
            _controller.KaraokeMembers = _controller.KaraokeMembers.Add(Networking.LocalPlayer.displayName);
            if (Utilities.IsValid(_modal) && _modal.IsActive) OpenKaraokeMemberModal();
        }

        public void LeaveKaraokeMembers()
        {
            if (_controller.KaraokeMode == KaraokeMode.None || !_controller.IsKaraokeMember) return;
            _controller.TakeOwnership();
            _controller.KaraokeMembers = _controller.KaraokeMembers.Remove(Networking.LocalPlayer.displayName);
            if (Utilities.IsValid(_modal) && _modal.IsActive) OpenKaraokeMemberModal();
        }

        public void OpenKaraokeMemberModal()
        {
            if (!_modal || _controller.KaraokeMode == KaraokeMode.None) return;
            UdonEvent callback = _controller.IsKaraokeMember ? UdonEvent.New(this, nameof(LeaveKaraokeMembers)) : UdonEvent.New(this, nameof(JoinKaraokeMembers));
            string executeText = _controller.IsKaraokeMember ? i18n.GetValue("leaveMember") : i18n.GetValue("joinMember");
            _modal.Show(i18n.GetValue("karaokeMember"), string.Join("\n", _controller.KaraokeMembers), callback, i18n.GetValue("close"), executeText);
        }

        public void SetPermission()
        {
            if (!_controller.Permission || !_permission || _permissionIndex < 0) return;
            int index = Array.IndexOf(_permission.Indexes, _permissionIndex);
            if (index >= 0 &&
                _permission.GetComponent<ScrollRect>().content.GetChild(index).TryFind("Dropdown", out var dr) && 
                dr.TryGetComponentLocal(out Dropdown dropdown))
            {
                PlayerPermission playerPermission = PlayerPermission.Viewer;
                if (dropdown.value == 1) playerPermission = PlayerPermission.Editor;
                if (dropdown.value == 0) playerPermission = PlayerPermission.Admin;
                _controller.Permission.TakeOwnership();
                _controller.Permission.SetPermission(_permissionIndex, playerPermission);
            }
        }
    
        public void GeneratePlaylistView()
        {
            if (!_playlists) return;
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
                    trackCount.text = playlist.Length > 0 ? $"{i18n.GetValue("total")} {playlist.Length} {i18n.GetValue("tracks")}" : string.Empty;
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
            if (!_playlists || !_playlistTracks) return;
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
            if (!CheckPermission()) return;
            if (!_playlistTracks || _playlistTrackIndex < 0) return;

            _controller.Queue.TakeOwnership();
            if (_playlistTrackIndex < _controller.Queue.Length) _controller.Queue.RemoveTrack(_playlistTrackIndex);
        }

        public void AddPlaylistTrackToQueue()
        {
            if (!CheckPermission()) return;
            if (!_playlistTracks || _playlistTrackIndex < 0) return;

            Playlist playlist = _isHistoryPage ? _controller.History :
                _playlistIndex >= 0 && _playlistIndex < _controller.Playlists.Length ? _controller.Playlists[_playlistIndex] : null;
            _controller.Queue.TakeOwnership();
            _controller.Queue.AddTrack(playlist.GetTrack(_playlistTrackIndex));
        }

        public void MoveUp()
        {
            if (!CheckPermission()) return;
            _controller.Queue.TakeOwnership();
            _controller.Queue.MoveUp(_playlistTrackIndex);
        }

        public void MoveDown()
        {
            if (!CheckPermission()) return;
            _controller.Queue.TakeOwnership();
            _controller.Queue.MoveDown(_playlistTrackIndex);
        }
        public void PlayPlaylistTrack()
        {
            if (!CheckPermission()) return;
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

        public void UpdateUI()
        {
            UpdatePlayerSelector();
            UpdateProgressView();
            UpdatePlaybackView();
            UpdateTrackView();
            UpdateLoadingView();
            UpdateAudioView();
            UpdateSlideView();
            if (Utilities.IsValid(_idle) && Utilities.IsValid(_idleImage)) _idle.gameObject.SetActive(_controller.Stopped);
        }

        private void UpdateSlideView()
        {
            if (Utilities.IsValid(_videoTime)) _videoTime.alignment = _controller.SlideMode ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;
            if (Utilities.IsValid(_duration)) _duration.alignment = _controller.SlideMode ? TextAnchor.MiddleCenter : TextAnchor.MiddleRight;
            if (Utilities.IsValid(_progress))
            {
                _progress.wholeNumbers = _controller.SlideMode;
                _progress.minValue = _controller.SlideMode && !_controller.Stopped ? 1 : 0;
                _progress.maxValue = _controller.SlideMode ? _controller.SlidePageCount : 1;
            }
            if (Utilities.IsValid(_slideOn)) _slideOn.SetIsOnWithoutNotify(_controller.SlideMode);
            if (Utilities.IsValid(_slideOff)) _slideOff.SetIsOnWithoutNotify(!_controller.SlideMode);
            if (Utilities.IsValid(_slide1s)) _slide1s.SetIsOnWithoutNotify(_controller.SlideSeconds == 1);
            if (Utilities.IsValid(_slide2s)) _slide2s.SetIsOnWithoutNotify(_controller.SlideSeconds == 2);
            if (Utilities.IsValid(_slide3s)) _slide3s.SetIsOnWithoutNotify(_controller.SlideSeconds == 3);
        }

        private void UpdateProgressView()
        {
            if (Utilities.IsValid(_videoTime)) _videoTime.text = _controller.SlideMode ? _controller.SlidePage.ToString() : TimeSpan.FromSeconds(_controller.VideoTime).ToString(_timeFormat);
            if (Utilities.IsValid(_duration)) _duration.text = _controller.SlideMode ? _controller.SlidePageCount.ToString() : _controller.IsLive ? "Live" : TimeSpan.FromSeconds(_controller.Duration).ToString(_timeFormat);
            if (Utilities.IsValid(_progress) && !_progressDrag) _progress.SetValueWithoutNotify(_controller.SlideMode ? _controller.SlidePage : _controller.IsLive ? 1f : Mathf.Clamp(_controller.Duration == 0f ? 0f : _controller.VideoTime / _controller.Duration, 0f, 1f));
            if (Utilities.IsValid(_progressHelper) && Utilities.IsValid(_progressTooltip))
            {
                _progressHelper.gameObject.SetActive(!_controller.Stopped && !_controller.IsLive && !_controller.SlideMode);
                if (_controller.IsLive) _progressTooltip.text = "Live";
                else _progressTooltip.text = TimeSpan.FromSeconds(_controller.Duration * _progressHelper.Percent).ToString(_timeFormat);
            }
        }

        private void UpdatePlayerSelector()
        {
            if (Utilities.IsValid(_unityPlayer)) _unityPlayer.SetIsOnWithoutNotify(_controller.PlayerType == VideoPlayerType.UnityVideoPlayer);
            if (Utilities.IsValid(_avProPlayer)) _avProPlayer.SetIsOnWithoutNotify(_controller.PlayerType == VideoPlayerType.AVProVideoPlayer);
            if (Utilities.IsValid(_imageViewer)) _imageViewer.SetIsOnWithoutNotify(_controller.PlayerType == VideoPlayerType.ImageViewer);
        }

        private void UpdatePlaybackView()
        {
            RepeatStatus repeatStatus = _controller.RepeatStatus;
            if (Utilities.IsValid(_play)) _play.gameObject.SetActive(!_controller.IsPlaying);
            if (Utilities.IsValid(_pause)) _pause.gameObject.SetActive(_controller.IsPlaying);
            if (Utilities.IsValid(_loop)) _loop.gameObject.SetActive(!_controller.Loop);
            if (Utilities.IsValid(_loopOff)) _loopOff.gameObject.SetActive(_controller.Loop);
            if (Utilities.IsValid(_speedSlider)) _speedSlider.SetValueWithoutNotify((float)Math.Round(_controller.Speed * 20));
            if (Utilities.IsValid(_speedText)) _speedText.text = $"{_controller.Speed:F2}x";
            if (Utilities.IsValid(_repeatOff)) _repeatOff.SetIsOnWithoutNotify(!repeatStatus.IsOn());
            if (Utilities.IsValid(_repeat)) _repeat.SetIsOnWithoutNotify(repeatStatus.IsOn());
            if (Utilities.IsValid(_repeatSlider) && Utilities.IsValid(_repeatStartTime) && Utilities.IsValid(_repeatEndTime))
            {
                string notSetText = i18n.GetValue("notSet");
                string startText = repeatStatus.GetStartTime() == 0 ? notSetText : TimeSpan.FromSeconds(repeatStatus.GetStartTime()).ToString(_timeFormat);
                string endText = repeatStatus.GetEndTime() >= _controller.Duration || _controller.IsLive ? notSetText : TimeSpan.FromSeconds(repeatStatus.GetEndTime()).ToString(_timeFormat);
                _repeatSlider.SliderLeft.SetValueWithoutNotify(_controller.IsLive || !_controller.IsPlaying ? 0f : Mathf.Clamp(repeatStatus.GetStartTime() / _controller.Duration, 0f, 1f));
                _repeatSlider.SliderRight.SetValueWithoutNotify(_controller.IsLive || !_controller.IsPlaying ? 1f : Mathf.Clamp(repeatStatus.GetEndTime() / _controller.Duration, 0f, 1f));
                _repeatStartTime.text = $"{i18n.GetValue("start")}(A): {startText}";
                _repeatEndTime.text = $"{i18n.GetValue("end")}(B): {endText}";
            }
            if (Utilities.IsValid(_localDelayText)) _localDelayText.text = (Mathf.Round(_controller.LocalDelay * 100) / 100).ToString();
            if (Utilities.IsValid(_inorderPlay)) _inorderPlay.gameObject.SetActive(!_controller.ShufflePlay);
            if (Utilities.IsValid(_shufflePlay)) _shufflePlay.gameObject.SetActive(_controller.ShufflePlay);
        }

        private void UpdateTrackView()
        {
            Track track = _controller.Track;
            if (Utilities.IsValid(_title)) _title.text = track.HasTitle() ? track.GetTitle() : track.GetUrl();
            if (Utilities.IsValid(_url)) _url.text = track.HasTitle() ? track.GetUrl() : string.Empty;
        }

        private void UpdateAudioView()
        {
            if (Utilities.IsValid(_mute)) _mute.gameObject.SetActive(!_controller.Mute);
            if (Utilities.IsValid(_muteOff)) _muteOff.gameObject.SetActive(_controller.Mute);
            if (Utilities.IsValid(_volume)) _volume.SetValueWithoutNotify(_controller.Volume);
            if (Utilities.IsValid(_audioLinkSettings)) _audioLinkSettings.gameObject.SetActive(Utilities.IsValid(_controller.AudioLink));
            if (Utilities.IsValid(_audioLinkOn)) _audioLinkOn.SetIsOnWithoutNotify(_controller.UseAudioLink);
            if (Utilities.IsValid(_audioLinkOff)) _audioLinkOff.SetIsOnWithoutNotify(!_controller.UseAudioLink);
        }

        private void UpdateScreenView()
        {
            if (Utilities.IsValid(_mirrorInversion)) _mirrorInversion.SetIsOnWithoutNotify(_controller.MirrorInverse);
            if (Utilities.IsValid(_mirrorInversionOff)) _mirrorInversionOff.SetIsOnWithoutNotify(!_controller.MirrorInverse);
            if (Utilities.IsValid(_maxResolution144)) _maxResolution144.SetIsOnWithoutNotify(_controller.MaxResolution == 144);
            if (Utilities.IsValid(_maxResolution240)) _maxResolution240.SetIsOnWithoutNotify(_controller.MaxResolution == 240);
            if (Utilities.IsValid(_maxResolution360)) _maxResolution360.SetIsOnWithoutNotify(_controller.MaxResolution == 360);
            if (Utilities.IsValid(_maxResolution480)) _maxResolution480.SetIsOnWithoutNotify(_controller.MaxResolution == 480);
            if (Utilities.IsValid(_maxResolution720)) _maxResolution720.SetIsOnWithoutNotify(_controller.MaxResolution == 720);
            if (Utilities.IsValid(_maxResolution1080)) _maxResolution1080.SetIsOnWithoutNotify(_controller.MaxResolution == 1080);
            if (Utilities.IsValid(_maxResolution2160)) _maxResolution2160.SetIsOnWithoutNotify(_controller.MaxResolution == 2160);
            if (Utilities.IsValid(_maxResolution4320)) _maxResolution4320.SetIsOnWithoutNotify(_controller.MaxResolution == 4320);
            if (Utilities.IsValid(_emissionSlider)) _emissionSlider.SetValueWithoutNotify(_controller.Emission);
            if (Utilities.IsValid(_emissionText)) _emissionText.text = $"{Mathf.Ceil(_controller.Emission * 100)}%";
        }

        private void UpdateKaraokeView()
        {
            if (Utilities.IsValid(_karaokeModeOff)) _karaokeModeOff.SetIsOnWithoutNotify(_controller.KaraokeMode == KaraokeMode.None);
            if (Utilities.IsValid(_karaokeModeKaraoke)) _karaokeModeKaraoke.SetIsOnWithoutNotify(_controller.KaraokeMode == KaraokeMode.Karaoke);
            if (Utilities.IsValid(_karaokeModeDance)) _karaokeModeDance.SetIsOnWithoutNotify(_controller.KaraokeMode == KaraokeMode.Dance);
            if (Utilities.IsValid(_karaokeModal)) _karaokeModal.SetActive(_controller.KaraokeMode != KaraokeMode.None);
            if (_modal.gameObject.activeSelf) OpenKaraokeMemberModal();
        }

        private void UpdateErrorView(VideoError videoError)
        {
            if (Utilities.IsValid(_loading)) _loading.SetActive(true);
            if (Utilities.IsValid(_animator)) _animator.SetBool("Loading", false);
            if (!_message) return;
            switch (videoError)
            {
                case VideoError.Unknown:
                    _message.text = i18n.GetValue("unknownErrorMessage");
                    break;
                case VideoError.InvalidURL:
                    _message.text = i18n.GetValue("invalidUrlMessage");
                    break;
                case VideoError.AccessDenied:
                    _message.text = i18n.GetValue("accessDeniedMessage");
                    break;
                case VideoError.RateLimited:
                    _message.text = i18n.GetValue("rateLimitedMessage");
                    break;
                case VideoError.PlayerError:
                    _message.text = i18n.GetValue("playerErrorMessage");
                    break;
                default:
                    break;
            }
        }

        private void UpdateLoadingView()
        {
            if (Utilities.IsValid(_loading)) _loading.SetActive(_controller.IsLoading);
            if (Utilities.IsValid(_animator)) _animator.SetBool("Loading", _controller.IsLoading);
            if (Utilities.IsValid(_message)) _message.text = i18n.GetValue("videoLoadingMessage");
        }

        public void GeneratePermissionView()
        {
            if (!_controller.Permission || !_permission) return;
            _permission.CallbackEvent = UdonEvent.New(this, nameof(UpdatePermissionView));
            _permission.Length = _controller.Permission.PermissionData.Count;

            bool showPage = _controller.PlayerPermission == PlayerPermission.Owner || _controller.PlayerPermission == PlayerPermission.Admin;
            if (Utilities.IsValid(_permissionEntry)) _permissionEntry.SetActive(showPage);
            if (Utilities.IsValid(_permissionPage) && !showPage && _permissionPage.activeSelf) _permissionPage.SetActive(false);
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
                            if (Utilities.IsValid(dropdown)) dropdown.GetComponent<Dropdown>().SetValueWithoutNotify(0);
                            break;
                        case PlayerPermission.Editor:
                            markImage.color = _editorColor;
                            if (Utilities.IsValid(dropdown)) dropdown.GetComponent<Dropdown>().SetValueWithoutNotify(1);
                            break;
                        case PlayerPermission.Viewer:
                            markImage.color = _viewerColor;
                            if (Utilities.IsValid(dropdown)) dropdown.GetComponent<Dropdown>().SetValueWithoutNotify(2);
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
            if (Utilities.IsValid(_returnToMain)) _returnToMain.text = i18n.GetValue("returnToMain");
            if (Utilities.IsValid(_inputUrl)) _inputUrl.text = i18n.GetValue("inputUrl");
            if (Utilities.IsValid(_loopPlayText)) _loopPlayText.text = i18n.GetValue("loop");
            if (Utilities.IsValid(_loopPlayText2)) _loopPlayText2.text = i18n.GetValue("loop");
            if (Utilities.IsValid(_shufflePlayText)) _shufflePlayText.text = i18n.GetValue("shuffle");
            if (Utilities.IsValid(_shufflePlayText2)) _shufflePlayText2.text = i18n.GetValue("shuffle");
            if (Utilities.IsValid(_options)) _options.text = i18n.GetValue("options");
            if (Utilities.IsValid(_settings)) _settings.text = i18n.GetValue("settings");
            if (Utilities.IsValid(_karaokeMember)) _karaokeMember.text = i18n.GetValue("karaokeMember");
            if (Utilities.IsValid(_imageViewerModalText)) _imageViewerModalText.text = i18n.GetValue("imageViewer");
            if (Utilities.IsValid(_playlist)) _playlist.text = i18n.GetValue("playlist");
            if (Utilities.IsValid(_videoSearch)) _videoSearch.text = i18n.GetValue("videoSearch");
            if (Utilities.IsValid(_version)) _version.text = i18n.GetValue("version");
            if (Utilities.IsValid(_settingsTitle)) _settingsTitle.text = i18n.GetValue("settingsTitle");
            if (Utilities.IsValid(_playback)) _playback.text = i18n.GetValue("playback");
            if (Utilities.IsValid(_videoAndAudio)) _videoAndAudio.text = i18n.GetValue("videoAndAudio");
            if (Utilities.IsValid(_other)) _other.text = i18n.GetValue("other");
            if (Utilities.IsValid(_videoPlayer)) _videoPlayer.text = $"{i18n.GetValue("videoPlayer")}<size=100>(Global)</size>";
            if (Utilities.IsValid(_videoPlayerDesc)) _videoPlayerDesc.text = i18n.GetValue("videoPlayerDesc");
            if (Utilities.IsValid(_imageViewerText)) _imageViewerText.text = i18n.GetValue("imageViewer");
            if (Utilities.IsValid(_playbackSpeed)) _playbackSpeed.text = $"{i18n.GetValue("playbackSpeed")}<size=100>(Global)</size>";
            if (Utilities.IsValid(_playbackSpeedDesc)) _playbackSpeedDesc.text = i18n.GetValue("playbackSpeedDesc");
            if (Utilities.IsValid(_slower)) _slower.text = i18n.GetValue("slower");
            if (Utilities.IsValid(_faster)) _faster.text = i18n.GetValue("faster");
            if (Utilities.IsValid(_repeatPlay)) _repeatPlay.text = $"{i18n.GetValue("repeatPlay")}<size=100>(Global)</size>";
            if (Utilities.IsValid(_repeatPlayDesc)) _repeatPlayDesc.text = i18n.GetValue("repeatPlayDesc");
            if (Utilities.IsValid(_repeatOnText)) _repeatOnText.text = i18n.GetValue("repeatOn");
            if (Utilities.IsValid(_repeatOffText)) _repeatOffText.text = i18n.GetValue("repeatOff");
            if (Utilities.IsValid(_maxResolution)) _maxResolution.text = i18n.GetValue("maxResolution");
            if (Utilities.IsValid(_maxResolutionDesc)) _maxResolutionDesc.text = i18n.GetValue("maxResolutionDesc");
            if (Utilities.IsValid(_mirrorInversionTitle)) _mirrorInversionTitle.text = i18n.GetValue("mirrorInversion");
            if (Utilities.IsValid(_mirrorInversionDesc)) _mirrorInversionDesc.text = i18n.GetValue("mirrorInversionDesc");
            if (Utilities.IsValid(_mirrorInversionOnText)) _mirrorInversionOnText.text = i18n.GetValue("mirrorInversionOn");
            if (Utilities.IsValid(_mirrorInversionOffText)) _mirrorInversionOffText.text = i18n.GetValue("mirrorInversionOff");
            if (Utilities.IsValid(_brightness)) _brightness.text = i18n.GetValue("brightness");
            if (Utilities.IsValid(_brightnessDesc)) _brightnessDesc.text = i18n.GetValue("brightnessDesc");
            if (Utilities.IsValid(_audioLinkDesc)) _audioLinkDesc.text = i18n.GetValue("audioLinkDesc");
            if (Utilities.IsValid(_audioLinkOnText)) _audioLinkOnText.text = i18n.GetValue("audioLinkOn");
            if (Utilities.IsValid(_audioLinkOffText)) _audioLinkOffText.text = i18n.GetValue("audioLinkOff");
            if (Utilities.IsValid(_karaokeModeText)) _karaokeModeText.text = $"{i18n.GetValue("karaokeMode")}<size=100>(Global)</size>";
            if (Utilities.IsValid(_karaokeModeDesc)) _karaokeModeDesc.text = i18n.GetValue("karaokeModeDesc");
            if (Utilities.IsValid(_karaokeModeOnText)) _karaokeModeOnText.text = i18n.GetValue("karaokeModeOn");
            if (Utilities.IsValid(_danceModeOnText)) _danceModeOnText.text = i18n.GetValue("danceModeOn");
            if (Utilities.IsValid(_karaokeModeOffText)) _karaokeModeOffText.text = i18n.GetValue("karaokeModeOff");
            if (Utilities.IsValid(_localDelay)) _localDelay.text = i18n.GetValue("localOffset");
            if (Utilities.IsValid(_localDelayDesc)) _localDelayDesc.text = i18n.GetValue("localOffsetDesc");
            if (Utilities.IsValid(_languageSelect)) _languageSelect.text = i18n.GetValue("languageSelect");
            if (Utilities.IsValid(_slideMode)) _slideMode.text = $"{i18n.GetValue("slideMode")}<size=100>(Global)</size>";
            if (Utilities.IsValid(_slideModeDesc)) _slideModeDesc.text = i18n.GetValue("slideModeDesc");
            if (Utilities.IsValid(_slideOnText)) _slideOnText.text = i18n.GetValue("slideOn");
            if (Utilities.IsValid(_slideOffText)) _slideOffText.text = i18n.GetValue("slideOff");
            if (Utilities.IsValid(_slideSeconds)) _slideSeconds.text = $"{i18n.GetValue("slideSeconds")}<size=100>(Global)</size>";
            if (Utilities.IsValid(_slideSecondsDesc)) _slideSecondsDesc.text = i18n.GetValue("slideSecondsDesc");
            if (Utilities.IsValid(_slide1sText)) _slide1sText.text = i18n.GetValue("slide1s");
            if (Utilities.IsValid(_slide2sText)) _slide2sText.text = i18n.GetValue("slide2s");
            if (Utilities.IsValid(_slide3sText)) _slide3sText.text = i18n.GetValue("slide3s");

            if (Utilities.IsValid(_playlistTitle)) _playlistTitle.text = i18n.GetValue("playlistTitle");
            if (Utilities.IsValid(_playQueue)) _playQueue.text = i18n.GetValue("playQueue");
            if (Utilities.IsValid(_playHistory)) _playHistory.text = i18n.GetValue("playHistory");
            if (Utilities.IsValid(_addVideoLink)) _addVideoLink.text = i18n.GetValue("addVideoLink");
            if (Utilities.IsValid(_addLiveLink)) _addLiveLink.text = i18n.GetValue("addLiveLink");
            if (Utilities.IsValid(_permissionTitle)) _permissionTitle.text = i18n.GetValue("permission");
            if (Utilities.IsValid(_permissionDesc)) _permissionDesc.text = $"<color=#64B5F6>Owner</color>\t\t\t{i18n.GetValue("ownerPermission")}\r\n<color=#BA68C8>Admin</color>\t\t\t{i18n.GetValue("adminPermission")}\r\n<color=#81C784>Editor</color>\t\t\t{i18n.GetValue("editorPermission")}\r\n<color=#FFB74D>Viewer</color>\t\t\t{i18n.GetValue("viewerPermission")}";

            if (Utilities.IsValid(_playlistTracks))
            {
                RectTransform scrollRectTransform = _playlistTracks.GetComponent<ScrollRect>().content;
                for (int i = 0; i < scrollRectTransform.childCount; i++)
                {
                    Transform cell = scrollRectTransform.GetChild(i);
                    if (cell.transform.TryFind("Actions", out var actions))
                    {
                        if (actions.TryFind("Return/Text", out var back) && back.TryGetComponentLocal<Text>(out var backText)) backText.text = i18n.GetValue("back");
                        if (actions.TryFind("Up/Text", out var up) && up.TryGetComponentLocal<Text>(out var upText)) upText.text = i18n.GetValue("moveUp");
                        if (actions.TryFind("Down/Text", out var down) && down.TryGetComponentLocal<Text>(out var downText)) downText.text = i18n.GetValue("moveDown");
                        if (actions.TryFind("Remove/Text", out var remove) && remove.TryGetComponentLocal<Text>(out var removeText)) removeText.text = i18n.GetValue("remove");
                        if (actions.TryFind("Copy/Text", out var copyUrl) && copyUrl.TryGetComponentLocal<Text>(out var copyUrlText)) copyUrlText.text = i18n.GetValue("copyUrl");
                        if (actions.TryFind("Add/Text", out var addQueue) && addQueue.TryGetComponentLocal<Text>(out var addQueueText)) addQueueText.text = i18n.GetValue("addQueue");
                        if (actions.TryFind("Play/Text", out var play) && play.TryGetComponentLocal<Text>(out var playText)) playText.text = i18n.GetValue("playVideo");
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
        public override void OnVideoError(VideoError videoError) => UpdateErrorView(videoError);
        public override void OnPlayerHandlerChanged() => UpdateUI();
        public override void OnSlideModeChanged() => UpdateUI();
        public override void OnLoopChanged() => UpdatePlaybackView();
        public override void OnRepeatChanged() => UpdatePlaybackView();
        public override void OnSpeedChanged() => UpdatePlaybackView();
        public override void OnLocalDelayChanged() => UpdatePlaybackView();
        public override void OnShufflePlayChanged() => UpdatePlaybackView();
        public override void OnTrackUpdated() => UpdateTrackView();
        public override void OnUrlChanged()
        {
            UpdateLoadingView();
            GeneratePlaylistTracks();
        }
        public override void OnVideoRetry() => UpdateLoadingView();
        public override void OnVideoInfoLoaded()
        {
            if (_isQueuePage) GeneratePlaylistTracks();
            UpdateTrackView();
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
        public override void OnVolumeChanged() => UpdateAudioView();
        public override void OnMuteChanged() => UpdateAudioView();
        public override void OnUseAudioLinkChanged() => UpdateAudioView();
        public override void OnMaxResolutionChanged() => UpdateScreenView();
        public override void OnMirrorInversionChanged() => UpdateScreenView();
        public override void OnEmissionChanged() => UpdateScreenView();
        public override void OnKaraokeModeChanged() => UpdateKaraokeView();
        public override void OnKaraokeMemberChanged() => UpdateKaraokeView();
        public override void OnPermissionChanged() => GeneratePermissionView();
    }
}