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
    public partial class UIController : Listener
    {
        [SerializeField] Controller _controller;
        [SerializeField] bool _disableUIOnPickUp = true;
        [SerializeField, Range(0f, 10f)] float _disableUIDistance = 0f;
        [SerializeField] Font _font;
        [SerializeField] TextAsset _updateLogFile;

        [SerializeField] Color _primaryColor = new Color(240f / 256f, 98f / 256f, 146f / 256f, 1.0f);
        [SerializeField] Color _secondaryColor = new Color(248f / 256f, 187f / 256f, 208f / 256f, 31f / 256f);
        [SerializeField] Color _ownerColor;
        [SerializeField] Color _adminColor;
        [SerializeField] Color _editorColor;
        [SerializeField] Color _viewerColor;

        [SerializeField] Modal _modal;
        [SerializeField] ToggleGroup _modalVideoPlayerSelector;
        [SerializeField] Toggle _modalUnityPlayer;
        [SerializeField] Toggle _modalAVProPlayer;
        [SerializeField] Toggle _modalImageViewer;

        [SerializeField] Animator _animator;

        [SerializeField] VRCUrlInputField _urlInputField;
        [SerializeField] VRCUrlInputField _urlInputFieldTop;
        [SerializeField] Text _title;
        [SerializeField] Text _url;

        [SerializeField] Text _videoTime;
        [SerializeField] Text _duration;
        [SerializeField] Slider _progress;
        [SerializeField] SliderHelper _progressHelper;
        [SerializeField] Text _progressTooltip;

        [SerializeField] Button _play;
        [SerializeField] Button _pause;
        [SerializeField] Button _loop;
        [SerializeField] Button _loopOff;
        [SerializeField] Button _inorderPlay;
        [SerializeField] Button _shufflePlay;

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

        [SerializeField] LoopScroll _permission;
        [SerializeField] GameObject _permissionEntry;
        [SerializeField] GameObject _permissionPage;

        [SerializeField] GameObject _loading;
        [SerializeField] Text _message;

        [SerializeField] Image _idle;
        [SerializeField] Sprite _idleImage;

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

        [SerializeField] Text _versionText;
        [SerializeField] Text _updateLog;

        [SerializeField] Toggle _slideOn;
        [SerializeField] Toggle _slideOff;
        [SerializeField] Toggle _slide1s;
        [SerializeField] Toggle _slide2s;
        [SerializeField] Toggle _slide3s;

        private BoxCollider _uiBoxCollider;
        private string _timeFormat = @"hh\:mm\:ss";
        private bool _progressDrag = false;
        private int _permissionIndex = -1;

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

        public Color PrimaryColor => _primaryColor;
        public Color SecondaryColor => _secondaryColor;

        private bool IsPermissionGranted()
        {
            if ((int)_controller.PlayerPermission >= (int)PlayerPermission.Editor) return true;
            if (Utilities.IsValid(_modal))
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
            if (_controller.PlayerType == VideoPlayerType.UnityVideoPlayer || !IsPermissionGranted()) return;
            if (!_modal || (_controller.Stopped && !_controller.IsLoading))
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
            _controller.PlayerType = VideoPlayerType.UnityVideoPlayer;
            if (Utilities.IsValid(_modal)) _modal.Close();
        }

        public void SetAVProPlayer()
        {
            UpdateUI();
            if (_controller.PlayerType == VideoPlayerType.AVProVideoPlayer || !IsPermissionGranted()) return;
            if (!_modal || (_controller.Stopped && !_controller.IsLoading))
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
            _controller.PlayerType = VideoPlayerType.AVProVideoPlayer;
            if (Utilities.IsValid(_modal)) _modal.Close();
        }

        public void SetImageViewer()
        {
            UpdateUI();
            if (_controller.PlayerType == VideoPlayerType.ImageViewer || !IsPermissionGranted()) return;
            if (!_modal || (_controller.Stopped && !_controller.IsLoading))
            {
                SetImageViewerEvent();
                return;
            }
            _modal.Title = I18n.GetValue("confirmChangePlayer");
            _modal.Message = I18n.GetValue("confirmChangePlayerMessage");
            _modal.CancelText = I18n.GetValue("cancel");
            _modal.ExecuteText = I18n.GetValue("continue");
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
            if (!urlInputField || !urlInputField.GetUrl().Get().IsValidUrl() || !IsPermissionGranted())
            {
                urlInputField.SetUrl(VRCUrl.Empty);
                return;
            }
            if (_controller.Stopped && !_controller.IsLoading || !_modal)
            {
                _controller.TakeOwnership();
                _controller.ClearPlaylistIndexes();
                _controller.PlayTrack(Track.New(_controller.PlayerType, "", urlInputField.GetUrl()));
                urlInputField.SetUrl(VRCUrl.Empty);
                return;
            }
            if (Utilities.IsValid(_modal))
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
            _controller.ClearPlaylistIndexes();
            _controller.PlayTrack(Track.New(GetVideoPlayerSelectorValue(), "", urlInputField.GetUrl()));
            urlInputField.SetUrl(VRCUrl.Empty);
            HideVideoPlayerSelector();
            if (Utilities.IsValid(_modal)) _modal.Close();
        }

        public void Play()
        {
            if (!IsPermissionGranted()) return;
            _controller.TakeOwnership();
            _controller.Play();
        }

        public void Pause()
        {
            if (!IsPermissionGranted()) return;
            _controller.TakeOwnership();
            _controller.Pause();
        }

        public void Stop()
        {
            if (!IsPermissionGranted()) return;
            _controller.TakeOwnership();
            _controller.ClearPlaylistIndexes();
            _controller.Stop();
        }

        public void ProgressDrag() => _progressDrag = true;

        public void SetTime()
        {
            _progressDrag = false;
            if (!_progress || !IsPermissionGranted() || _controller.Stopped) return;
            _controller.TakeOwnership();
            if (_controller.SlideMode) _controller.SetSlidePage((int)_progress.value);
            else _controller.SetTime(_controller.Duration * _progress.value);
        }

        public void SetTimeByHelper()
        {
            if (!_progressHelper || !IsPermissionGranted()) return;
            _controller.TakeOwnership();
            _controller.SetTime(_controller.Duration * _progressHelper.Percent);
        }

        public void SlideOff()
        {
            if (!IsPermissionGranted()) return;
            _controller.TakeOwnership();
            _controller.SlideMode = false;
        }

        public void SlideOn()
        {
            if (!IsPermissionGranted()) return;
            _controller.TakeOwnership();
            _controller.SlideMode = true;
        }

        public void SetSlideSeconds(int seconds)
        {
            if (!IsPermissionGranted()) return;
            _controller.TakeOwnership();
            _controller.SlideSeconds = seconds;
        }

        public void SetSlide1s() => SetSlideSeconds(1);
        public void SetSlide2s() => SetSlideSeconds(2);
        public void SetSlide3s() => SetSlideSeconds(3);

        public void Loop()
        {
            if (!IsPermissionGranted()) return;
            _controller.TakeOwnership();
            _controller.Loop = true;
        }

        public void LoopOff()
        {
            if (!IsPermissionGranted()) return;
            _controller.TakeOwnership();
            _controller.Loop = false;
        }

        public void Reload() => _controller.Reload();

        public void Repeat() => SetRepeat(true);

        public void RepeatOff() => SetRepeat(false);

        public void SetRepeat(bool on)
        {
            if (!IsPermissionGranted()) return;
            RepeatStatus status = _controller.RepeatStatus;
            if (on) status.TurnOn();
            else status.TurnOff();
            _controller.TakeOwnership();
            _controller.RepeatStatus = status;
        }

        public void SetRepeatStart()
        {
            if (!_repeatSlider || _controller.Stopped || !IsPermissionGranted()) return;
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
            if (!_repeatSlider || _controller.Stopped || !IsPermissionGranted()) return;
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
            if (!IsPermissionGranted()) return;
            _controller.TakeOwnership();
            _controller.ShufflePlay = true;
        }

        public void SetShuffleOff()
        {
            if (!IsPermissionGranted()) return;
            _controller.TakeOwnership();
            _controller.ShufflePlay = false;
        }

        public void Backward()
        {
            if (!IsPermissionGranted()) return;
            _controller.TakeOwnership();
            if (_controller.SlideMode) _controller.SetSlidePage(_controller.SlidePage - 1);
            else _controller.Backward();
        }

        public void Forward()
        {
            if (!IsPermissionGranted()) return;
            _controller.TakeOwnership();
            if (_controller.SlideMode) _controller.SetSlidePage(_controller.SlidePage + 1);
            else _controller.Forward();
        }

        public void SetSpeed()
        {
            if (!IsPermissionGranted()) return;
            _controller.TakeOwnership();
            if (Utilities.IsValid(_speedSlider)) _controller.Speed = _speedSlider.value / 20f;
        }

        public void SetAudioLinkOn()
        {
            if (!_audioLinkOn || !_audioLinkOn.isOn) return;
            _controller.AudioLinkEnabled = true;
        }
        public void SetAudioLinkOff()
        {
            if (!_audioLinkOff || !_audioLinkOff.isOn) return;
            _controller.AudioLinkEnabled = false;
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

        public void SetMirrorInverse()
        {
            if (!_mirrorInversion || !_mirrorInversion.isOn) return;
            _controller.MirrorFlip = true;
        }
        public void SetMirrorInverseOff()
        {
            if (!_mirrorInversionOff || !_mirrorInversionOff.isOn) return;
            _controller.MirrorFlip = false;
        }

        public void SetMaxResolution144() => _controller.MaxResolution = 144;
        public void SetMaxResolution240() => _controller.MaxResolution = 240;
        public void SetMaxResolution360() => _controller.MaxResolution = 360;
        public void SetMaxResolution480() => _controller.MaxResolution = 480;
        public void SetMaxResolution720() => _controller.MaxResolution = 720;
        public void SetMaxResolution1080() => _controller.MaxResolution = 1080;
        public void SetMaxResolution2160() => _controller.MaxResolution = 2160;
        public void SetMaxResolution4320() => _controller.MaxResolution = 4320;

        public void SetKaraokeModeOff()
        {
            if (!IsPermissionGranted()) return;
            _controller.TakeOwnership();
            _controller.KaraokeMode = KaraokeMode.None;
        }

        public void SetKaraokeModeKaraoke()
        {
            if (!IsPermissionGranted()) return;
            _controller.TakeOwnership();
            _controller.KaraokeMode = KaraokeMode.Karaoke;
        }

        public void SetKaraokeModeDance()
        {
            if (!IsPermissionGranted()) return;
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
            string executeText = _controller.IsKaraokeMember ? I18n.GetValue("leaveMember") : I18n.GetValue("joinMember");
            _modal.Show(I18n.GetValue("karaokeMember"), string.Join("\n", _controller.KaraokeMembers), callback, I18n.GetValue("close"), executeText);
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
            if (Utilities.IsValid(_duration)) _duration.text = _controller.SlideMode ? _controller.SlidePageCount.ToString() : _controller.PlayerType == VideoPlayerType.ImageViewer ? "Image" : _controller.IsLive ? "Live" : TimeSpan.FromSeconds(_controller.Duration).ToString(_timeFormat);
            if (Utilities.IsValid(_progress) && !_progressDrag) _progress.SetValueWithoutNotify(_controller.SlideMode ? _controller.SlidePage : _controller.IsLive ? 1f : Mathf.Clamp(_controller.Duration == 0f ? 0f : _controller.VideoTime / _controller.Duration, 0f, 1f));
            if (Utilities.IsValid(_progressHelper) && Utilities.IsValid(_progressTooltip))
            {
                _progressHelper.gameObject.SetActive(!_controller.Stopped && !_controller.IsLive && !_controller.SlideMode && _controller.PlayerType != VideoPlayerType.ImageViewer);
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
                string notSetText = I18n.GetValue("notSet");
                string startText = repeatStatus.GetStartTime() == 0 ? notSetText : TimeSpan.FromSeconds(repeatStatus.GetStartTime()).ToString(_timeFormat);
                string endText = repeatStatus.GetEndTime() >= _controller.Duration || _controller.IsLive ? notSetText : TimeSpan.FromSeconds(repeatStatus.GetEndTime()).ToString(_timeFormat);
                _repeatSlider.SliderLeft.SetValueWithoutNotify(_controller.IsLive || !_controller.IsPlaying ? 0f : Mathf.Clamp(repeatStatus.GetStartTime() / _controller.Duration, 0f, 1f));
                _repeatSlider.SliderRight.SetValueWithoutNotify(_controller.IsLive || !_controller.IsPlaying ? 1f : Mathf.Clamp(repeatStatus.GetEndTime() / _controller.Duration, 0f, 1f));
                _repeatStartTime.text = $"{I18n.GetValue("start")}(A): {startText}";
                _repeatEndTime.text = $"{I18n.GetValue("end")}(B): {endText}";
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
            if (Utilities.IsValid(_audioLinkSettings))
            {
                _audioLinkSettings.gameObject.SetActive(Utilities.IsValid(_controller.AudioLinkAssigned));
            }
            if (Utilities.IsValid(_audioLinkOn)) _audioLinkOn.SetIsOnWithoutNotify(_controller.AudioLinkEnabled);
            if (Utilities.IsValid(_audioLinkOff)) _audioLinkOff.SetIsOnWithoutNotify(!_controller.AudioLinkEnabled);
        }

        private void UpdateScreenView()
        {
            if (Utilities.IsValid(_mirrorInversion)) _mirrorInversion.SetIsOnWithoutNotify(_controller.MirrorFlip);
            if (Utilities.IsValid(_mirrorInversionOff)) _mirrorInversionOff.SetIsOnWithoutNotify(!_controller.MirrorFlip);
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

        private void UpdateLoadingView()
        {
            if (Utilities.IsValid(_loading)) _loading.SetActive(_controller.IsLoading);
            if (Utilities.IsValid(_animator)) _animator.SetBool("Loading", _controller.IsLoading);
            if (Utilities.IsValid(_message)) _message.text = I18n.GetValue("videoLoadingMessage");
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