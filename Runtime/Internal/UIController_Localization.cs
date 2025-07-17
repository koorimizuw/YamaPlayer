using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace Yamadev.YamaStream.UI
{
    public partial class UIController
    {
        [SerializeField] private TextAsset _translationTextFile;

        [SerializeField] private Text _options;
        [SerializeField] private Text _settings;
        [SerializeField] private Text _playlist;
        [SerializeField] private Text _videoSearch;
        [SerializeField] private Text _version;
        [SerializeField] private Text _returnToMain;
        [SerializeField] private Text _inputUrl;
        [SerializeField] private Text _loopPlayText;
        [SerializeField] private Text _loopPlayText2;
        [SerializeField] private Text _shufflePlayText;
        [SerializeField] private Text _shufflePlayText2;
        [SerializeField] private Text _imageViewerModalText;
        [SerializeField] private Text _karaokeMember;
        [SerializeField] private Text _settingsTitle;
        [SerializeField] private Text _playback;
        [SerializeField] private Text _videoAndAudio;
        [SerializeField] private Text _other;
        [SerializeField] private Text _videoPlayer;
        [SerializeField] private Text _videoPlayerDesc;
        [SerializeField] private Text _imageViewerText;
        [SerializeField] private Text _playbackSpeed;
        [SerializeField] private Text _playbackSpeedDesc;
        [SerializeField] private Text _slower;
        [SerializeField] private Text _faster;
        [SerializeField] private Text _repeatPlay;
        [SerializeField] private Text _repeatPlayDesc;
        [SerializeField] private Text _repeatOnText;
        [SerializeField] private Text _repeatOffText;
        [SerializeField] private Text _maxResolution;
        [SerializeField] private Text _maxResolutionDesc;
        [SerializeField] private Text _mirrorInversionTitle;
        [SerializeField] private Text _mirrorInversionDesc;
        [SerializeField] private Text _mirrorInversionOnText;
        [SerializeField] private Text _mirrorInversionOffText;
        [SerializeField] private Text _brightness;
        [SerializeField] private Text _brightnessDesc;
        [SerializeField] private Text _audioLinkDesc;
        [SerializeField] private Text _audioLinkOnText;
        [SerializeField] private Text _audioLinkOffText;
        [SerializeField] private Text _karaokeModeText;
        [SerializeField] private Text _karaokeModeDesc;
        [SerializeField] private Text _karaokeModeOnText;
        [SerializeField] private Text _danceModeOnText;
        [SerializeField] private Text _karaokeModeOffText;
        [SerializeField] private Text _localDelay;
        [SerializeField] private Text _localDelayDesc;
        [SerializeField] private Text _languageSelect;
        [SerializeField] private Text _slideMode;
        [SerializeField] private Text _slideModeDesc;
        [SerializeField] private Text _slideOnText;
        [SerializeField] private Text _slideOffText;
        [SerializeField] private Text _slideSeconds;
        [SerializeField] private Text _slideSecondsDesc;
        [SerializeField] private Text _slide1sText;
        [SerializeField] private Text _slide2sText;
        [SerializeField] private Text _slide3sText;
        [SerializeField] private Text _permissionTitle;
        [SerializeField] private Text _permissionDesc;

        private Localization _i18n;

        private Localization I18n
        {
            get
            {
                if (!Utilities.IsValid(_i18n))
                {
                    string text = Utilities.IsValid(_translationTextFile) ? _translationTextFile.text : string.Empty;
                    _i18n = Localization.Initialize(text);
                }

                return _i18n;
            }
        }

        public void SetLanguageAuto() => SetLanguage(null);
        public void SetLanguageJapanese() => SetLanguage("ja");
        public void SetLanguageChineseChina() => SetLanguage("zh-CN");
        public void SetLanguageChineseTaiwan() => SetLanguage("zh-TW");
        public void SetLanguageKorean() => SetLanguage("ko");
        public void SetLanguageEnglish() => SetLanguage("en");
        public void SetLanguageSpanish() => SetLanguage("es-CL");

        public void SetLanguage(string language)
        {
            I18n.SetLanguage(language);
            UpdateUI();
            UpdateTranslation();
            GeneratePlaylistView();
        }

        public void UpdateTranslation()
        {
            if (Utilities.IsValid(_returnToMain)) _returnToMain.text = I18n.GetValue("returnToMain");
            if (Utilities.IsValid(_inputUrl)) _inputUrl.text = I18n.GetValue("inputUrl");
            if (Utilities.IsValid(_loopPlayText)) _loopPlayText.text = I18n.GetValue("loop");
            if (Utilities.IsValid(_loopPlayText2)) _loopPlayText2.text = I18n.GetValue("loop");
            if (Utilities.IsValid(_shufflePlayText)) _shufflePlayText.text = I18n.GetValue("shuffle");
            if (Utilities.IsValid(_shufflePlayText2)) _shufflePlayText2.text = I18n.GetValue("shuffle");
            if (Utilities.IsValid(_options)) _options.text = I18n.GetValue("options");
            if (Utilities.IsValid(_settings)) _settings.text = I18n.GetValue("settings");
            if (Utilities.IsValid(_karaokeMember)) _karaokeMember.text = I18n.GetValue("karaokeMember");
            if (Utilities.IsValid(_imageViewerModalText)) _imageViewerModalText.text = I18n.GetValue("imageViewer");
            if (Utilities.IsValid(_playlist)) _playlist.text = I18n.GetValue("playlist");
            if (Utilities.IsValid(_videoSearch)) _videoSearch.text = I18n.GetValue("videoSearch");
            if (Utilities.IsValid(_version)) _version.text = I18n.GetValue("version");
            if (Utilities.IsValid(_settingsTitle)) _settingsTitle.text = I18n.GetValue("settingsTitle");
            if (Utilities.IsValid(_playback)) _playback.text = I18n.GetValue("playback");
            if (Utilities.IsValid(_videoAndAudio)) _videoAndAudio.text = I18n.GetValue("videoAndAudio");
            if (Utilities.IsValid(_other)) _other.text = I18n.GetValue("other");
            if (Utilities.IsValid(_videoPlayer)) _videoPlayer.text = $"{I18n.GetValue("videoPlayer")}<size=100>(Global)</size>";
            if (Utilities.IsValid(_videoPlayerDesc)) _videoPlayerDesc.text = I18n.GetValue("videoPlayerDesc");
            if (Utilities.IsValid(_imageViewerText)) _imageViewerText.text = I18n.GetValue("imageViewer");
            if (Utilities.IsValid(_playbackSpeed)) _playbackSpeed.text = $"{I18n.GetValue("playbackSpeed")}<size=100>(Global)</size>";
            if (Utilities.IsValid(_playbackSpeedDesc)) _playbackSpeedDesc.text = I18n.GetValue("playbackSpeedDesc");
            if (Utilities.IsValid(_slower)) _slower.text = I18n.GetValue("slower");
            if (Utilities.IsValid(_faster)) _faster.text = I18n.GetValue("faster");
            if (Utilities.IsValid(_repeatPlay)) _repeatPlay.text = $"{I18n.GetValue("repeatPlay")}<size=100>(Global)</size>";
            if (Utilities.IsValid(_repeatPlayDesc)) _repeatPlayDesc.text = I18n.GetValue("repeatPlayDesc");
            if (Utilities.IsValid(_repeatOnText)) _repeatOnText.text = I18n.GetValue("repeatOn");
            if (Utilities.IsValid(_repeatOffText)) _repeatOffText.text = I18n.GetValue("repeatOff");
            if (Utilities.IsValid(_maxResolution)) _maxResolution.text = I18n.GetValue("maxResolution");
            if (Utilities.IsValid(_maxResolutionDesc)) _maxResolutionDesc.text = I18n.GetValue("maxResolutionDesc");
            if (Utilities.IsValid(_mirrorInversionTitle)) _mirrorInversionTitle.text = I18n.GetValue("mirrorInversion");
            if (Utilities.IsValid(_mirrorInversionDesc)) _mirrorInversionDesc.text = I18n.GetValue("mirrorInversionDesc");
            if (Utilities.IsValid(_mirrorInversionOnText)) _mirrorInversionOnText.text = I18n.GetValue("mirrorInversionOn");
            if (Utilities.IsValid(_mirrorInversionOffText)) _mirrorInversionOffText.text = I18n.GetValue("mirrorInversionOff");
            if (Utilities.IsValid(_brightness)) _brightness.text = I18n.GetValue("brightness");
            if (Utilities.IsValid(_brightnessDesc)) _brightnessDesc.text = I18n.GetValue("brightnessDesc");
            if (Utilities.IsValid(_audioLinkDesc)) _audioLinkDesc.text = I18n.GetValue("audioLinkDesc");
            if (Utilities.IsValid(_audioLinkOnText)) _audioLinkOnText.text = I18n.GetValue("audioLinkOn");
            if (Utilities.IsValid(_audioLinkOffText)) _audioLinkOffText.text = I18n.GetValue("audioLinkOff");
            if (Utilities.IsValid(_karaokeModeText)) _karaokeModeText.text = $"{I18n.GetValue("karaokeMode")}<size=100>(Global)</size>";
            if (Utilities.IsValid(_karaokeModeDesc)) _karaokeModeDesc.text = I18n.GetValue("karaokeModeDesc");
            if (Utilities.IsValid(_karaokeModeOnText)) _karaokeModeOnText.text = I18n.GetValue("karaokeModeOn");
            if (Utilities.IsValid(_danceModeOnText)) _danceModeOnText.text = I18n.GetValue("danceModeOn");
            if (Utilities.IsValid(_karaokeModeOffText)) _karaokeModeOffText.text = I18n.GetValue("karaokeModeOff");
            if (Utilities.IsValid(_localDelay)) _localDelay.text = I18n.GetValue("localOffset");
            if (Utilities.IsValid(_localDelayDesc)) _localDelayDesc.text = I18n.GetValue("localOffsetDesc");
            if (Utilities.IsValid(_languageSelect)) _languageSelect.text = I18n.GetValue("languageSelect");
            if (Utilities.IsValid(_slideMode)) _slideMode.text = $"{I18n.GetValue("slideMode")}<size=100>(Global)</size>";
            if (Utilities.IsValid(_slideModeDesc)) _slideModeDesc.text = I18n.GetValue("slideModeDesc");
            if (Utilities.IsValid(_slideOnText)) _slideOnText.text = I18n.GetValue("slideOn");
            if (Utilities.IsValid(_slideOffText)) _slideOffText.text = I18n.GetValue("slideOff");
            if (Utilities.IsValid(_slideSeconds)) _slideSeconds.text = $"{I18n.GetValue("slideSeconds")}<size=100>(Global)</size>";
            if (Utilities.IsValid(_slideSecondsDesc)) _slideSecondsDesc.text = I18n.GetValue("slideSecondsDesc");
            if (Utilities.IsValid(_slide1sText)) _slide1sText.text = I18n.GetValue("slide1s");
            if (Utilities.IsValid(_slide2sText)) _slide2sText.text = I18n.GetValue("slide2s");
            if (Utilities.IsValid(_slide3sText)) _slide3sText.text = I18n.GetValue("slide3s");

            if (Utilities.IsValid(_playlistTitle)) _playlistTitle.text = I18n.GetValue("playlistTitle");
            if (Utilities.IsValid(_playQueue)) _playQueue.text = I18n.GetValue("playQueue");
            if (Utilities.IsValid(_playHistory)) _playHistory.text = I18n.GetValue("playHistory");
            if (Utilities.IsValid(_addVideoLink)) _addVideoLink.text = I18n.GetValue("addVideoLink");
            if (Utilities.IsValid(_addLiveLink)) _addLiveLink.text = I18n.GetValue("addLiveLink");
            if (Utilities.IsValid(_permissionTitle)) _permissionTitle.text = I18n.GetValue("permission");
            if (Utilities.IsValid(_permissionDesc)) _permissionDesc.text = $"<color=#64B5F6>Owner</color>\t\t\t{I18n.GetValue("ownerPermission")}\r\n<color=#BA68C8>Admin</color>\t\t\t{I18n.GetValue("adminPermission")}\r\n<color=#81C784>Editor</color>\t\t\t{I18n.GetValue("editorPermission")}\r\n<color=#FFB74D>Viewer</color>\t\t\t{I18n.GetValue("viewerPermission")}";

            if (Utilities.IsValid(_playlistTracks))
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
                        if (actions.TryFind("Copy/Text", out var copyUrl) && copyUrl.TryGetComponentLocal<Text>(out var copyUrlText)) copyUrlText.text = I18n.GetValue("copyUrl");
                        if (actions.TryFind("Add/Text", out var addQueue) && addQueue.TryGetComponentLocal<Text>(out var addQueueText)) addQueueText.text = I18n.GetValue("addQueue");
                        if (actions.TryFind("Play/Text", out var play) && play.TryGetComponentLocal<Text>(out var playText)) playText.text = I18n.GetValue("playVideo");
                    }
                }
            }
        }
    }
}