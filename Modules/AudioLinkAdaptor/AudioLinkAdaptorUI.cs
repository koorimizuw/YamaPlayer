using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using Yamadev.YamaStream.UI;

namespace Yamadev.YamaStream.Modules.AudioLinkAdaptor
{
  [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
  public class AudioLinkAdaptorUI : YamaPlayerListener
  {
#if AUDIOLINK_V1
    [SerializeField] private AudioLinkAdaptor _audioLinkAdaptor;
#else
    [SerializeField] private UdonSharpBehaviour _audioLinkAdaptor;
#endif
    [SerializeField, RegisterEvent(nameof(Toggle.onValueChanged), nameof(DisableAudioLink))] private Toggle _disabledToggle;
    [SerializeField, RegisterEvent(nameof(Toggle.onValueChanged), nameof(EnableAudioLink))] private Toggle _enabledToggle;
    [SerializeField] private Text _titleText;
    [SerializeField] private Text _descriptionText;
    [SerializeField] private Text _disabledText;
    [SerializeField] private Text _enabledText;
    private UIController _uiController;

#if AUDIOLINK_V1
    private void Start()
    {
      _uiController = GetComponentInParent<UIController>();
      if (!Utilities.IsValid(_uiController) || !Utilities.IsValid(_audioLinkAdaptor)) return;
      _audioLinkAdaptor.AddListener(this);
      _uiController.AddListener(this);
      UpdateTranslation();
      UpdateAudioLinkState();
    }

    private void UpdateTranslation()
    {
      if (!Utilities.IsValid(_uiController)) return;
      if (Utilities.IsValid(_titleText)) _titleText.text = _uiController.GetTranslation("module.audioLinkAdaptor.title");
      if (Utilities.IsValid(_descriptionText)) _descriptionText.text = _uiController.GetTranslation("module.audioLinkAdaptor.description");
      if (Utilities.IsValid(_disabledText)) _disabledText.text = _uiController.GetTranslation("module.audioLinkAdaptor.disabled");
      if (Utilities.IsValid(_enabledText)) _enabledText.text = _uiController.GetTranslation("module.audioLinkAdaptor.enabled");
    }

    private void UpdateAudioLinkState()
    {
      if (!Utilities.IsValid(_uiController)) return;
      if (Utilities.IsValid(_enabledToggle)) _enabledToggle.SetIsOnWithoutNotify(_audioLinkAdaptor.AudioLinkEnabled);
      if (Utilities.IsValid(_disabledToggle)) _disabledToggle.SetIsOnWithoutNotify(!_audioLinkAdaptor.AudioLinkEnabled);
    }

    public void AfterLanguageChanged() => UpdateTranslation();
    public void AfterAudioLinkStateChanged() => UpdateAudioLinkState();
#endif

    public void EnableAudioLink()
    {
      if (!Utilities.IsValid(_audioLinkAdaptor) || !Utilities.IsValid(_enabledToggle) || !_enabledToggle.isOn) return;
#if AUDIOLINK_V1
      _audioLinkAdaptor.AudioLinkEnabled = true;
#else
      _audioLinkAdaptor.SendCustomEvent("EnableAudioLink");
#endif
    }

    public void DisableAudioLink()
    {
      if (!Utilities.IsValid(_audioLinkAdaptor) || !Utilities.IsValid(_disabledToggle) || !_disabledToggle.isOn) return;
#if AUDIOLINK_V1
      _audioLinkAdaptor.AudioLinkEnabled = false;
#else
      _audioLinkAdaptor.SendCustomEvent("DisableAudioLink");
#endif
    }
  }
}