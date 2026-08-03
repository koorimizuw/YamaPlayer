using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace Yamadev.YamaStream.UI
{
  [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
  public class RangeSlider : UdonSharpBehaviour
  {
    [SerializeField, RegisterEvent(nameof(Slider.onValueChanged), nameof(OnLeftSliderValueChanged))] private Slider _sliderLeft;
    [SerializeField, RegisterEvent(nameof(Slider.onValueChanged), nameof(OnRightSliderValueChanged))] private Slider _sliderRight;
    [SerializeField] private RectTransform _fill;
    [SerializeField] private float _fillPaddingY = -7f;
    private RectTransform _rectTransform;
    private UdonSharpBehaviour _callbackUdon;
    private string _callbackEventNameLeft, _callbackEventNameRight;
    private bool _initialized = false;

    private void Start()
    {
      Initialize();
    }

    public bool IsReady => Utilities.IsValid(_sliderLeft) && Utilities.IsValid(_sliderRight);

    public float LeftValue
    {
      get => Utilities.IsValid(_sliderLeft) ? _sliderLeft.value : 0f;
      set { if (Utilities.IsValid(_sliderLeft)) _sliderLeft.value = value; }
    }

    public float RightValue
    {
      get => Utilities.IsValid(_sliderRight) ? _sliderRight.value : 0f;
      set { if (Utilities.IsValid(_sliderRight)) _sliderRight.value = value; }
    }

    private void Initialize()
    {
      if (_initialized) return;

      _rectTransform = GetComponent<RectTransform>();
      if (!Utilities.IsValid(_rectTransform)) return;

      _initialized = true;
    }

    public void SetUp(UdonSharpBehaviour callbackUdon, string callbackEventNameLeft, string callbackEventNameRight, float minValue = 0f, float maxValue = 1f)
    {
      if (!Utilities.IsValid(_sliderLeft) || !Utilities.IsValid(_sliderRight) || minValue >= maxValue) return;

      if (!_initialized) Initialize();
      _callbackUdon = callbackUdon;
      _callbackEventNameLeft = callbackEventNameLeft;
      _callbackEventNameRight = callbackEventNameRight;
      _sliderLeft.minValue = minValue;
      _sliderLeft.maxValue = maxValue;
      _sliderRight.minValue = minValue;
      _sliderRight.maxValue = maxValue;
    }

    public void SetLeftValueWithoutNotify(float value)
    {
      if (!Utilities.IsValid(_sliderLeft) || !Utilities.IsValid(_sliderRight)) return;
      _sliderLeft.SetValueWithoutNotify(value);
      FitFillArea();
      UpdateSliderOrder();
    }

    public void SetRightValueWithoutNotify(float value)
    {
      if (!Utilities.IsValid(_sliderLeft) || !Utilities.IsValid(_sliderRight)) return;
      _sliderRight.SetValueWithoutNotify(value);
      FitFillArea();
      UpdateSliderOrder();
    }

    public void OnLeftSliderValueChanged()
    {
      if (!Utilities.IsValid(_sliderLeft) || !Utilities.IsValid(_sliderRight) || !Utilities.IsValid(_fill)) return;

      if (_sliderLeft.value > _sliderRight.value)
      {
        _sliderLeft.value = _sliderRight.value;
        return;
      }

      FitFillArea();
      UpdateSliderOrder();
      InvokeCallback(_callbackEventNameLeft);
    }

    public void OnRightSliderValueChanged()
    {
      if (!Utilities.IsValid(_sliderLeft) || !Utilities.IsValid(_sliderRight) || !Utilities.IsValid(_fill)) return;

      if (_sliderRight.value < _sliderLeft.value)
      {
        _sliderRight.value = _sliderLeft.value;
        return;
      }

      FitFillArea();
      UpdateSliderOrder();
      InvokeCallback(_callbackEventNameRight);
    }

    private void UpdateSliderOrder()
    {
      float normalizedLeft = NormalizeSliderValue(_sliderLeft);

      if (normalizedLeft < 0.5f)
      {
        _sliderRight.transform.SetAsLastSibling();
      }
      else
      {
        _sliderLeft.transform.SetAsLastSibling();
      }
    }

    public void FitFillArea()
    {
      if (!Utilities.IsValid(_sliderLeft) || !Utilities.IsValid(_sliderRight) || !Utilities.IsValid(_fill)) return;
      if (!_initialized) Initialize();
      if (!Utilities.IsValid(_rectTransform)) return;

      float width = _rectTransform.rect.width;

      float left = width * NormalizeSliderValue(_sliderLeft);
      float right = width * (1f - NormalizeSliderValue(_sliderRight));
      _fill.offsetMin = new Vector2(left, _fillPaddingY);
      _fill.offsetMax = new Vector2(-right, _fillPaddingY);
    }

    private float NormalizeSliderValue(Slider slider)
    {
      float range = slider.maxValue - slider.minValue;
      if (range <= 0f) return 0f;
      return Mathf.Clamp01((slider.value - slider.minValue) / range);
    }

    private void InvokeCallback(string eventName)
    {
      if (Utilities.IsValid(_callbackUdon) && !string.IsNullOrEmpty(eventName))
      {
        _callbackUdon.SendCustomEvent(eventName);
      }
    }
  }
}