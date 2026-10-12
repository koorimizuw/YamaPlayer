using System;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDKBase;
using static VRC.SDKBase.VRCPlayerApi;
using Object = UnityEngine.Object;

namespace Yamadev.YamaStream.UI
{
  public partial class UIController
  {
    #region Serialized Fields

    [SerializeField, HideInInspector] private TextAsset _translationJsonFile;
    [SerializeField, HideInInspector] private string[] _languageCodes;
    [SerializeField, HideInInspector] private Object[] _fontAssets;
    [SerializeField, HideInInspector] private string _defaultLanguage;

    [Header("Localization - Toggles")]
    [SerializeField, RegisterEvent(nameof(Toggle.onValueChanged), nameof(SetLanguageToEnglish))] private Toggle _languageEnglishToggle;
    [SerializeField, RegisterEvent(nameof(Toggle.onValueChanged), nameof(SetLanguageToJapanese))] private Toggle _languageJapaneseToggle;
    [SerializeField, RegisterEvent(nameof(Toggle.onValueChanged), nameof(SetLanguageToSimplifiedChinese))] private Toggle _languageSimplifiedChineseToggle;
    [SerializeField, RegisterEvent(nameof(Toggle.onValueChanged), nameof(SetLanguageToTraditionalChinese))] private Toggle _languageTraditionalChineseToggle;
    [SerializeField, RegisterEvent(nameof(Toggle.onValueChanged), nameof(SetLanguageToKorean))] private Toggle _languageKoreanToggle;
    [SerializeField, RegisterEvent(nameof(Toggle.onValueChanged), nameof(SetLanguageToSpanish))] private Toggle _languageSpanishToggle;
    [SerializeField, RegisterEvent(nameof(Toggle.onValueChanged), nameof(SetLanguageToUkranian))] private Toggle _languageUkranianToggle;
    [SerializeField, RegisterEvent(nameof(Toggle.onValueChanged), nameof(SetLanguageToRussian))] private Toggle _languageRussianToggle;
    [SerializeField, RegisterEvent(nameof(Toggle.onValueChanged), nameof(SetLanguageToItalian))] private Toggle _languageItalianToggle;

    [Header("Localization - Left Side Labels")]
    [SerializeField] private Text _inputUrlLabel;
    [SerializeField] private Text _loopLabel;
    [SerializeField] private Text _loopLabel2;
    [SerializeField] private Text _shuffleLabel;
    [SerializeField] private Text _shuffleLabel2;
    [SerializeField] private Text _reloadLabel; // 現在使用してない
    [SerializeField] private Text _settingsLabel;
    [SerializeField] private Text _versionLabel;
    [SerializeField] private Text _lockUiLabel;

    [Header("Localization - Right Side Labels")]
    [SerializeField] private Text _tabQueueLabel;
    [SerializeField] private Text _tabHistoryLabel;
    [SerializeField] private Text _tabPlaylistsLabel;

    [Header("Localization - Page Labels")]
    [SerializeField] private Text _returnToMainLabel;
    [SerializeField] private Text _tabPlaybackLabel;
    [SerializeField] private Text _tabVideoAndAudioLabel;
    [SerializeField] private Text _tabUiLabel;

    [Header("Localization - Playback Settings")]
    [SerializeField] private Text _videoPlayerTitleLabel;
    [SerializeField] private Text _videoPlayerDescLabel;
    [SerializeField] private Text _unityVideoPlayerLabel; // 現在使用してない
    [SerializeField] private Text _avproVideoPlayerLabel; // 現在使用してない
    [SerializeField] private Text _imageViewerLabel;
    [SerializeField] private Text _playbackSpeedTitleLabel;
    [SerializeField] private Text _playbackSpeedDescLabel;
    [SerializeField] private Text _playbackSpeedSlowerButtonLabel;
    [SerializeField] private Text _playbackSpeedFasterButtonLabel;
    [SerializeField] private Text _repeatPlayTitleLabel;
    [SerializeField] private Text _repeatPlayDescLabel;
    [SerializeField] private Text _repeatOffButtonLabel;
    [SerializeField] private Text _repeatOnButtonLabel;
    [SerializeField] private Text _localDelayTitleLabel;
    [SerializeField] private Text _localDelayDescLabel;

    [Header("Localization - Video / Audio Settings")]
    [SerializeField] private Text _mirrorFlipTitleLabel;
    [SerializeField] private Text _mirrorFlipDescLabel;
    [SerializeField] private Text _mirrorFlipOnButtonLabel;
    [SerializeField] private Text _mirrorFlipOffButtonLabel;
    [SerializeField] private Text _brightnessTitleLabel;
    [SerializeField] private Text _brightnessDescLabel;
    [SerializeField] private Text _maxResolutionTitleLabel;
    [SerializeField] private Text _maxResolutionDescLabel;

    [Header("Localization - Appearance Settings")]
    [SerializeField] private Text _languageSelectTitleLabel;

    [Header("Localization - Messages")]
    [SerializeField] private Text _unlockUiMessageLabel;

    [Header("Localization - Modal Dialog")]
    [SerializeField] private Text _modalUnityVideoPlayerLabel; // 現在使用してない
    [SerializeField] private Text _modalAVProVideoPlayerLabel; // 現在使用してない
    [SerializeField] private Text _modalImageViewerLabel;

    #endregion

    private DataDictionary _translationData;
    private string _currentLanguage;
    private bool _translationInitialized;

    public string CurrentLanguage => _currentLanguage;

    private void InitializeTranslation()
    {
      if (_translationInitialized) return;

      if (Utilities.IsValid(_translationJsonFile) &&
          VRCJson.TryDeserializeFromJson(_translationJsonFile.text, out DataToken data) &&
          data.TokenType == TokenType.DataDictionary)
      {
        _translationData = data.DataDictionary;
      }

      _currentLanguage = string.IsNullOrEmpty(_defaultLanguage) ? DetectUserLanguage() : _defaultLanguage;
      UpdateTranslationToggles();
      UpdateFont(_currentLanguage);

      _translationInitialized = true;
    }

    public string GetTranslation(string key)
    {
      InitializeTranslation();
      if (!Utilities.IsValid(_translationData)) return string.Empty;
      if (_translationData.TryGetValue(_currentLanguage, out var langData) &&
          langData.TokenType == TokenType.DataDictionary &&
          langData.DataDictionary.TryGetValue(key, out var value))
      {
        return value.String;
      }
      return string.Empty;
    }

    private string DetectUserLanguage()
    {
      string userLanguage = GetCurrentLanguage();
      if (Utilities.IsValid(_translationData) && _translationData.ContainsKey(userLanguage))
      {
        return userLanguage;
      }
      return GetLanguageByTimeZone();
    }

    private string GetLanguageByTimeZone()
    {
      TimeZoneInfo tz = TimeZoneInfo.Local;
      switch (tz.Id)
      {
        case "Tokyo Standard Time":
          return "ja";
        case "Taipei Standard Time":
        case "Hong Kong Standard Time":
          return "zh-HK";
        case "China Standard Time":
          return "zh-CN";
        case "Korea Standard Time":
        case "North Korea Standard Time":
          return "ko";
        default:
          return "en";
      }
    }

    public void SetLanguageToJapanese()
    {
      if (!Utilities.IsValid(_languageJapaneseToggle) || !_languageJapaneseToggle.isOn) return;
      SetLanguage("ja");
    }
    public void SetLanguageToSimplifiedChinese()
    {
      if (!Utilities.IsValid(_languageSimplifiedChineseToggle) || !_languageSimplifiedChineseToggle.isOn) return;
      SetLanguage("zh-CN");
    }
    public void SetLanguageToTraditionalChinese()
    {
      if (!Utilities.IsValid(_languageTraditionalChineseToggle) || !_languageTraditionalChineseToggle.isOn) return;
      SetLanguage("zh-HK");
    }
    public void SetLanguageToKorean()
    {
      if (!Utilities.IsValid(_languageKoreanToggle) || !_languageKoreanToggle.isOn) return;
      SetLanguage("ko");
    }
    public void SetLanguageToEnglish()
    {
      if (!Utilities.IsValid(_languageEnglishToggle) || !_languageEnglishToggle.isOn) return;
      SetLanguage("en");
    }
    public void SetLanguageToSpanish()
    {
      if (!Utilities.IsValid(_languageSpanishToggle) || !_languageSpanishToggle.isOn) return;
      SetLanguage("es");
    }
    public void SetLanguageToUkranian()
    {
      if (!Utilities.IsValid(_languageUkranianToggle) || !_languageUkranianToggle.isOn) return;
      SetLanguage("uk");
    }
    public void SetLanguageToRussian()
    {
      if (!Utilities.IsValid(_languageRussianToggle) || !_languageRussianToggle.isOn) return;
      SetLanguage("ru");
    }
    public void SetLanguageToItalian()
    {
      if (!Utilities.IsValid(_languageItalianToggle) || !_languageItalianToggle.isOn) return;
      SetLanguage("it");
    }

    private void SetLanguage(string language)
    {
      if (!InvokeBeforeEvent("BeforeUserChangeLanguage")) return;

      InitializeTranslation();
      _currentLanguage = string.IsNullOrEmpty(language) ? DetectUserLanguage() : language;

      if (Utilities.IsValid(_translationData) && !_translationData.ContainsKey(_currentLanguage))
      {
        DataList keys = _translationData.GetKeys();
        _currentLanguage = keys.Count > 0 ? keys[0].String : "en";
      }

      UpdateTranslation();
      UpdateFont(_currentLanguage);
      GeneratePlaylistView();
      UpdateTranslationToggles();

      int len = _listeners.Length;
      for (int i = 0; i < len; i++) _listeners[i].SendCustomEvent("AfterLanguageChanged");
    }

    private void UpdateFont(string languageCode)
    {
      Font font = null;

      if (Utilities.IsValid(_languageCodes) && Utilities.IsValid(_fontAssets))
      {
        for (int i = 0; i < _languageCodes.Length; i++)
        {
          if (_languageCodes[i] == languageCode && i < _fontAssets.Length)
          {
            font = (Font)_fontAssets[i];
            break;
          }
        }
      }

      var texts = GetComponentsInChildren<Text>(true);
      int len = texts.Length;
      for (int i = 0; i < len; i++)
      {
        var text = texts[i];
        if (Utilities.IsValid(text)) text.font = font;
      }
    }

    private void UpdateTranslationToggles()
    {
      if (Utilities.IsValid(_languageEnglishToggle)) _languageEnglishToggle.SetIsOnWithoutNotify(_currentLanguage == "en");
      if (Utilities.IsValid(_languageJapaneseToggle)) _languageJapaneseToggle.SetIsOnWithoutNotify(_currentLanguage == "ja");
      if (Utilities.IsValid(_languageSimplifiedChineseToggle)) _languageSimplifiedChineseToggle.SetIsOnWithoutNotify(_currentLanguage == "zh-CN");
      if (Utilities.IsValid(_languageTraditionalChineseToggle)) _languageTraditionalChineseToggle.SetIsOnWithoutNotify(_currentLanguage == "zh-HK");
      if (Utilities.IsValid(_languageKoreanToggle)) _languageKoreanToggle.SetIsOnWithoutNotify(_currentLanguage == "ko");
      if (Utilities.IsValid(_languageSpanishToggle)) _languageSpanishToggle.SetIsOnWithoutNotify(_currentLanguage == "es");
      if (Utilities.IsValid(_languageUkranianToggle)) _languageUkranianToggle.SetIsOnWithoutNotify(_currentLanguage == "uk");
      if (Utilities.IsValid(_languageRussianToggle)) _languageRussianToggle.SetIsOnWithoutNotify(_currentLanguage == "ru");
      if (Utilities.IsValid(_languageItalianToggle)) _languageItalianToggle.SetIsOnWithoutNotify(_currentLanguage == "it");
    }

    private void UpdateTranslation()
    {
      if (Utilities.IsValid(_returnToMainLabel)) _returnToMainLabel.text = GetTranslation("button.returnToMain");
      if (Utilities.IsValid(_tabPlaybackLabel)) _tabPlaybackLabel.text = GetTranslation("tab.playback");
      if (Utilities.IsValid(_tabVideoAndAudioLabel)) _tabVideoAndAudioLabel.text = GetTranslation("tab.videoAndAudio");
      if (Utilities.IsValid(_tabUiLabel)) _tabUiLabel.text = GetTranslation("tab.other");

      if (Utilities.IsValid(_inputUrlLabel)) _inputUrlLabel.text = GetTranslation("label.inputUrl");
      if (Utilities.IsValid(_loopLabel)) _loopLabel.text = GetTranslation("label.loop");
      if (Utilities.IsValid(_loopLabel2)) _loopLabel2.text = GetTranslation("label.loop");
      if (Utilities.IsValid(_shuffleLabel)) _shuffleLabel.text = GetTranslation("label.shuffle");
      if (Utilities.IsValid(_shuffleLabel2)) _shuffleLabel2.text = GetTranslation("label.shuffle");
      if (Utilities.IsValid(_settingsLabel)) _settingsLabel.text = GetTranslation("menu.settings");
      if (Utilities.IsValid(_versionLabel)) _versionLabel.text = GetTranslation("label.version");
      if (Utilities.IsValid(_lockUiLabel)) _lockUiLabel.text = GetTranslation("label.lockUi");

      if (Utilities.IsValid(_tabQueueLabel)) _tabQueueLabel.text = GetTranslation("label.playQueue");
      if (Utilities.IsValid(_tabHistoryLabel)) _tabHistoryLabel.text = GetTranslation("label.playHistory");
      if (Utilities.IsValid(_tabPlaylistsLabel)) _tabPlaylistsLabel.text = GetTranslation("tab.playlist");

      if (Utilities.IsValid(_videoPlayerTitleLabel)) _videoPlayerTitleLabel.text = $"{GetTranslation("label.videoPlayer")}<size=44>(Global)</size>";
      if (Utilities.IsValid(_videoPlayerDescLabel)) _videoPlayerDescLabel.text = GetTranslation("desc.videoPlayer");
      if (Utilities.IsValid(_imageViewerLabel)) _imageViewerLabel.text = GetTranslation("label.imageViewer");
      if (Utilities.IsValid(_playbackSpeedTitleLabel)) _playbackSpeedTitleLabel.text = $"{GetTranslation("label.playbackSpeed")}<size=44>(Global)</size>";
      if (Utilities.IsValid(_playbackSpeedDescLabel)) _playbackSpeedDescLabel.text = GetTranslation("desc.playbackSpeed");
      if (Utilities.IsValid(_playbackSpeedSlowerButtonLabel)) _playbackSpeedSlowerButtonLabel.text = GetTranslation("label.slower");
      if (Utilities.IsValid(_playbackSpeedFasterButtonLabel)) _playbackSpeedFasterButtonLabel.text = GetTranslation("label.faster");
      if (Utilities.IsValid(_repeatPlayTitleLabel)) _repeatPlayTitleLabel.text = $"{GetTranslation("label.repeatPlay")}<size=44>(Global)</size>";
      if (Utilities.IsValid(_repeatPlayDescLabel)) _repeatPlayDescLabel.text = GetTranslation("desc.repeatPlay");
      if (Utilities.IsValid(_repeatOnButtonLabel)) _repeatOnButtonLabel.text = GetTranslation("button.repeatOn");
      if (Utilities.IsValid(_repeatOffButtonLabel)) _repeatOffButtonLabel.text = GetTranslation("button.repeatOff");
      if (Utilities.IsValid(_localDelayTitleLabel)) _localDelayTitleLabel.text = GetTranslation("label.localOffset");
      if (Utilities.IsValid(_localDelayDescLabel)) _localDelayDescLabel.text = GetTranslation("desc.localOffset");

      if (Utilities.IsValid(_mirrorFlipTitleLabel)) _mirrorFlipTitleLabel.text = GetTranslation("label.mirrorInversion");
      if (Utilities.IsValid(_mirrorFlipDescLabel)) _mirrorFlipDescLabel.text = GetTranslation("desc.mirrorInversion");
      if (Utilities.IsValid(_mirrorFlipOnButtonLabel)) _mirrorFlipOnButtonLabel.text = GetTranslation("button.mirrorInversionOn");
      if (Utilities.IsValid(_mirrorFlipOffButtonLabel)) _mirrorFlipOffButtonLabel.text = GetTranslation("button.mirrorInversionOff");
      if (Utilities.IsValid(_brightnessTitleLabel)) _brightnessTitleLabel.text = GetTranslation("label.brightness");
      if (Utilities.IsValid(_brightnessDescLabel)) _brightnessDescLabel.text = GetTranslation("desc.brightness");
      if (Utilities.IsValid(_maxResolutionTitleLabel)) _maxResolutionTitleLabel.text = GetTranslation("label.maxResolution");
      if (Utilities.IsValid(_maxResolutionDescLabel)) _maxResolutionDescLabel.text = GetTranslation("desc.maxResolution");

      if (Utilities.IsValid(_languageSelectTitleLabel)) _languageSelectTitleLabel.text = GetTranslation("label.languageSelect");
      if (Utilities.IsValid(_modalImageViewerLabel)) _modalImageViewerLabel.text = GetTranslation("label.imageViewer");

      if (Utilities.IsValid(_unlockUiMessageLabel))
      {
        if (IsInVR) _unlockUiMessageLabel.text = GetTranslation("msg.unlockUiVr");
        else _unlockUiMessageLabel.text = GetTranslation("msg.unlockUiDesktop");
      }

      if (Utilities.IsValid(_playlistTracksScroll))
      {
        RectTransform scrollRectTransform = _playlistTracksScroll.GetComponent<ScrollRect>().content;
        for (int i = 0; i < scrollRectTransform.childCount; i++)
        {
          Transform cell = scrollRectTransform.GetChild(i);
          var actions = cell.transform.Find("Actions");
          if (Utilities.IsValid(actions))
          {
            var back = actions.Find("Return/Text");
            if (Utilities.IsValid(back))
            {
              var backText = back.GetComponent<Text>();
              if (Utilities.IsValid(backText)) backText.text = GetTranslation("button.back");
            }
            var up = actions.Find("Up/Text");
            if (Utilities.IsValid(up))
            {
              var upText = up.GetComponent<Text>();
              if (Utilities.IsValid(upText)) upText.text = GetTranslation("button.moveUp");
            }
            var down = actions.Find("Down/Text");
            if (Utilities.IsValid(down))
            {
              var downText = down.GetComponent<Text>();
              if (Utilities.IsValid(downText)) downText.text = GetTranslation("button.moveDown");
            }
            var remove = actions.Find("Remove/Text");
            if (Utilities.IsValid(remove))
            {
              var removeText = remove.GetComponent<Text>();
              if (Utilities.IsValid(removeText)) removeText.text = GetTranslation("button.remove");
            }
            var copyUrl = actions.Find("Copy/Text");
            if (Utilities.IsValid(copyUrl))
            {
              var copyUrlText = copyUrl.GetComponent<Text>();
              if (Utilities.IsValid(copyUrlText)) copyUrlText.text = GetTranslation("button.copyUrl");
            }
            var addQueue = actions.Find("Add/Text");
            if (Utilities.IsValid(addQueue))
            {
              var addQueueText = addQueue.GetComponent<Text>();
              if (Utilities.IsValid(addQueueText)) addQueueText.text = GetTranslation("button.addQueue");
            }
            var play = actions.Find("Play/Text");
            if (Utilities.IsValid(play))
            {
              var playText = play.GetComponent<Text>();
              if (Utilities.IsValid(playText)) playText.text = GetTranslation("button.playVideo");
            }
          }
        }
      }
    }
  }
}
