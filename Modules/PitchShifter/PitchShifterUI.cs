using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using Yamadev.YamaStream.UI;

namespace Yamadev.YamaStream.Modules.PitchShifter
{
  [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
  public class PitchShifterUI : YamaPlayerListener
  {
    [SerializeField] private PitchShifter _pitchShifter;
    [SerializeField] private Text _titleText;
    [SerializeField] private Text _descriptionText;
    [SerializeField] private Text _semitonesValueText;
    [SerializeField, RegisterEvent(nameof(Slider.onValueChanged), nameof(OnSliderValueChanged))] private Slider _semitonesSlider;
    private UIController _uiController;

    private void Start()
    {
      _uiController = GetComponentInParent<UIController>();
      if (!Utilities.IsValid(_uiController) || !Utilities.IsValid(_pitchShifter)) return;
      _pitchShifter.AddListener(this);
      _uiController.AddListener(this);
      UpdateTranslation();
      UpdateSemitonesDisplay();
    }

    private void UpdateTranslation()
    {
      if (!Utilities.IsValid(_uiController)) return;
      if (Utilities.IsValid(_titleText)) _titleText.text = _uiController.GetTranslation("module.pitchshifter.title");
      if (Utilities.IsValid(_descriptionText)) _descriptionText.text = _uiController.GetTranslation("module.pitchshifter.description");
    }

    private void UpdateSemitonesDisplay()
    {
      if (!Utilities.IsValid(_pitchShifter)) return;
      float semitones = _pitchShifter.Semitones;

      if (Utilities.IsValid(_semitonesValueText))
      {
        string sign = semitones >= 0 ? "+" : "";
        _semitonesValueText.text = $"{sign}{semitones:0}";
      }

      if (Utilities.IsValid(_semitonesSlider))
      {
        _semitonesSlider.SetValueWithoutNotify(semitones);
      }
    }

    public void AfterLanguageChanged() => UpdateTranslation();

    public void AfterSemitonesChanged() => UpdateSemitonesDisplay();

    public void OnSliderValueChanged()
    {
      if (!Utilities.IsValid(_pitchShifter) || !Utilities.IsValid(_semitonesSlider)) return;
      _pitchShifter.TakeOwnership();
      _pitchShifter.Semitones = _semitonesSlider.value;
    }
  }
}
