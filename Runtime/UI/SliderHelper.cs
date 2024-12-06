
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Yamadev.YamaStream.UI
{
    [RequireComponent(typeof(RectTransform))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SliderHelper : UdonSharpBehaviour
    {
        [SerializeField] InputController _inputController;
        [SerializeField] RectTransform _tooltip;
        RectTransform _trans;
        float _percent = 0f;
        void Start()
        {
            _trans = GetComponent<RectTransform>();
        }

        public float Percent => _percent;

        public override void PostLateUpdate()
        {
            Vector3 localPosition = _trans.InverseTransformPoint(_inputController.MousePosition);
            float localX = localPosition.x + (_trans.rect.width * (1 - _trans.pivot.x));
            _percent = localX / _trans.rect.width;

            if (_trans.rect.Contains(localPosition)) _tooltip.gameObject.SetActive(true);
            else
            {
                _tooltip.gameObject.SetActive(false);
                return;
            }

            Vector2 pos = _tooltip.anchoredPosition;
            pos.x = _percent * _trans.sizeDelta.x;
            _tooltip.anchoredPosition = pos;
        }
    }
}