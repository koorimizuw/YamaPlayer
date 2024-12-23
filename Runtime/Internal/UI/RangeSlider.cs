
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;


namespace Yamadev.YamaStream.UI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RangeSlider : UdonSharpBehaviour
    {
        public Slider SliderLeft;
        public Slider SliderRight;
        public RectTransform Fill;

        bool _initialized = false;
        float _width;

        void Start() => Initialize();

        public void Initialize()
        {
            if (_initialized) return;
            _width = GetComponent<RectTransform>().rect.width;
            _initialized = true;
        }

        public void OnSliderValueChanged()
        {
            if (SliderLeft.value > SliderRight.value) SliderRight.value = SliderLeft.value;
            if (SliderRight.value < SliderLeft.value) SliderLeft.value = SliderRight.value;

            FitFillArea();
        }

        public void FitFillArea()
        {
            if (!_initialized) Initialize();

            float left = _width * SliderLeft.value;
            float right = _width * (1 - SliderRight.value);
            Fill.offsetMin = new Vector2(left, -7);
            Fill.offsetMax = new Vector2(-right, -7);
        }
    }
}