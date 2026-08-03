using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon.Common.Enums;
using Yamadev.YamaStream.UI;

namespace Yamadev.YamaStream.Modules.SlideShower
{
  [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
  public class SlideShowerUI : YamaPlayerListener
  {
    [SerializeField] private SlideShower _slideShower;
    [SerializeField, RegisterEvent(nameof(Toggle.onValueChanged), nameof(SlideModeOn))] private Toggle _slideModeOn;
    [SerializeField, RegisterEvent(nameof(Toggle.onValueChanged), nameof(SlideModeOff))] private Toggle _slideModeOff;
    [SerializeField, RegisterEvent(nameof(Toggle.onValueChanged), nameof(SetSlide1s))] private Toggle _slide1s;
    [SerializeField, RegisterEvent(nameof(Toggle.onValueChanged), nameof(SetSlide2s))] private Toggle _slide2s;
    [SerializeField, RegisterEvent(nameof(Toggle.onValueChanged), nameof(SetSlide3s))] private Toggle _slide3s;
    [SerializeField] private Text _slideModeTitleText;
    [SerializeField] private Text _slideModeDescriptionText;
    [SerializeField] private Text _slideModeOnText;
    [SerializeField] private Text _slideModeOffText;
    [SerializeField] private Text _slideSecondsTitleText;
    [SerializeField] private Text _slideSecondsDescriptionText;
    [SerializeField] private Text _slide1sText;
    [SerializeField] private Text _slide2sText;
    [SerializeField] private Text _slide3sText;
    private UIController _uiController;
    private Text _currentTimeText;
    private Text _totalDurationText;
    private Slider _progressSlider;
    private SliderHelper _progressSliderHelper;

    private void Start()
    {
      _uiController = GetComponentInParent<UIController>();
      if (!Utilities.IsValid(_uiController) || !Utilities.IsValid(_slideShower)) return;
      _slideShower.AddListener(this);
      _uiController.AddListener(this);

      _currentTimeText = (Text)_uiController.GetProgramVariable("_currentTimeText");
      _totalDurationText = (Text)_uiController.GetProgramVariable("_totalDurationText");
      _progressSlider = (Slider)_uiController.GetProgramVariable("_progressSlider");
      _progressSliderHelper = (SliderHelper)_uiController.GetProgramVariable("_progressSliderHelper");

      UpdateTranslation();
      UpdateDisplay();
    }


    private void LateUpdate()
    {
      UpdatePageDisplay();
      UpdateProgressDisplay();
    }

    private void UpdateDisplay()
    {
      UpdateSlideModeDisplay();
      UpdateSlideSecondsDisplay();
      UpdatePageDisplay();
      UpdateProgressDisplay();
    }

    private void UpdateSlideModeDisplay()
    {
      if (!Utilities.IsValid(_slideShower)) return;

      if (Utilities.IsValid(_slideModeOn)) _slideModeOn.SetIsOnWithoutNotify(_slideShower.SlideMode);
      if (Utilities.IsValid(_slideModeOff)) _slideModeOff.SetIsOnWithoutNotify(!_slideShower.SlideMode);
    }

    private void UpdateSlideSecondsDisplay()
    {
      if (!Utilities.IsValid(_slideShower)) return;

      if (Utilities.IsValid(_slide1s)) _slide1s.SetIsOnWithoutNotify(_slideShower.SlideSeconds == 1);
      if (Utilities.IsValid(_slide2s)) _slide2s.SetIsOnWithoutNotify(_slideShower.SlideSeconds == 2);
      if (Utilities.IsValid(_slide3s)) _slide3s.SetIsOnWithoutNotify(_slideShower.SlideSeconds == 3);
    }

    private void UpdatePageDisplay()
    {
      if (!Utilities.IsValid(_slideShower) || !_slideShower.SlideMode) return;
      if (!Utilities.IsValid(_currentTimeText) || !Utilities.IsValid(_totalDurationText) || !Utilities.IsValid(_progressSlider)) return;
      _currentTimeText.text = $"{_slideShower.SlidePage}";
      _totalDurationText.text = $"{_slideShower.SlidePageCount}";

      if (!(bool)_uiController.GetProgramVariable("_progressDrag"))
      {
        _progressSlider.SetValueWithoutNotify(_slideShower.SlidePage);
      }
    }

    private void UpdateProgressDisplay()
    {
      if (!Utilities.IsValid(_slideShower) || !Utilities.IsValid(_progressSlider)) return;
      _progressSlider.wholeNumbers = _slideShower.SlideMode;
      _progressSlider.minValue = _slideShower.SlideMode && _slideShower.SlidePage > 0 ? 1 : 0;
      _progressSlider.maxValue = _slideShower.SlideMode && _slideShower.SlidePageCount > 0 ? _slideShower.SlidePageCount : 1;
      if (Utilities.IsValid(_progressSliderHelper))
      {
        _progressSliderHelper.gameObject.SetActive(!_slideShower.SlideMode);
      }
    }

    private void UpdateTranslation()
    {
      if (!Utilities.IsValid(_uiController)) return;
      if (Utilities.IsValid(_slideModeTitleText)) _slideModeTitleText.text = _uiController.GetTranslation("module.slideshower.slideMode.title");
      if (Utilities.IsValid(_slideModeDescriptionText)) _slideModeDescriptionText.text = _uiController.GetTranslation("module.slideshower.slideMode.description");
      if (Utilities.IsValid(_slideModeOnText)) _slideModeOnText.text = _uiController.GetTranslation("module.slideshower.slideMode.on");
      if (Utilities.IsValid(_slideModeOffText)) _slideModeOffText.text = _uiController.GetTranslation("module.slideshower.slideMode.off");
      if (Utilities.IsValid(_slideSecondsTitleText)) _slideSecondsTitleText.text = _uiController.GetTranslation("module.slideshower.slideSeconds.title");
      if (Utilities.IsValid(_slideSecondsDescriptionText)) _slideSecondsDescriptionText.text = _uiController.GetTranslation("module.slideshower.slideSeconds.description");
      if (Utilities.IsValid(_slide1sText)) _slide1sText.text = _uiController.GetTranslation("module.slideshower.slideSeconds.1s");
      if (Utilities.IsValid(_slide2sText)) _slide2sText.text = _uiController.GetTranslation("module.slideshower.slideSeconds.2s");
      if (Utilities.IsValid(_slide3sText)) _slide3sText.text = _uiController.GetTranslation("module.slideshower.slideSeconds.3s");
    }

    public void SlideModeOn()
    {
      if (!Utilities.IsValid(_slideShower) || !Utilities.IsValid(_slideModeOn) || !_slideModeOn.isOn) return;
      _slideShower.TakeOwnership();
      _slideShower.SlideMode = true;
    }

    public void SlideModeOff()
    {
      if (!Utilities.IsValid(_slideShower) || !Utilities.IsValid(_slideModeOff) || !_slideModeOff.isOn) return;
      _slideShower.TakeOwnership();
      _slideShower.SlideMode = false;
    }

    public void SetSlideSeconds(int seconds)
    {
      if (!Utilities.IsValid(_slideShower)) return;
      _slideShower.TakeOwnership();
      _slideShower.SlideSeconds = seconds;
    }

    public void SetSlide1s()
    {
      if (!Utilities.IsValid(_slide1s) || !_slide1s.isOn) return;
      SetSlideSeconds(1);
    }

    public void SetSlide2s()
    {
      if (!Utilities.IsValid(_slide2s) || !_slide2s.isOn) return;
      SetSlideSeconds(2);
    }

    public void SetSlide3s()
    {
      if (!Utilities.IsValid(_slide3s) || !_slide3s.isOn) return;
      SetSlideSeconds(3);
    }

    public void BeforeUserSetTime()
    {
      if (!Utilities.IsValid(_slideShower) || !_slideShower.SlideMode || !Utilities.IsValid(_uiController)) return;
      _uiController.CancelCurrentAction();

      _slideShower.TakeOwnership();
      _slideShower.SetSlidePage((int)_progressSlider.value);
    }

    public void BeforeUserBackward()
    {
      if (!Utilities.IsValid(_slideShower) || !_slideShower.SlideMode || !Utilities.IsValid(_uiController)) return;
      _uiController.CancelCurrentAction();

      _slideShower.TakeOwnership();
      _slideShower.PreviousSlide();
    }

    public void BeforeUserForward()
    {
      if (!Utilities.IsValid(_slideShower) || !_slideShower.SlideMode || !Utilities.IsValid(_uiController)) return;
      _uiController.CancelCurrentAction();

      _slideShower.TakeOwnership();
      _slideShower.NextSlide();
    }

    public void AfterLanguageChanged() => UpdateTranslation();
    public void AfterSlideModeChanged() => UpdateDisplay();
    public void AfterSlideSecondsChanged() => UpdateDisplay();
    public override void AfterVideoReady() => SendCustomEventDelayedFrames(nameof(UpdateDisplay), 0, EventTiming.LateUpdate);
    public override void AfterVideoStarted() => SendCustomEventDelayedFrames(nameof(UpdateDisplay), 0, EventTiming.LateUpdate);
    public override void AfterVideoEnded() => SendCustomEventDelayedFrames(nameof(UpdateDisplay), 0, EventTiming.LateUpdate);
    public override void AfterVideoPlayed() => SendCustomEventDelayedFrames(nameof(UpdateDisplay), 0, EventTiming.LateUpdate);
    public override void AfterVideoPaused() => SendCustomEventDelayedFrames(nameof(UpdateDisplay), 0, EventTiming.LateUpdate);
    public override void AfterVideoStopped() => SendCustomEventDelayedFrames(nameof(UpdateDisplay), 0, EventTiming.LateUpdate);
    public override void AfterTrackLoaded() => SendCustomEventDelayedFrames(nameof(UpdateDisplay), 0, EventTiming.LateUpdate);
  }
}
