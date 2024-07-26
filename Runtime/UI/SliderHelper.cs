
using UdonSharp;
using UnityEngine;

namespace Yamadev.YamaStream
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SliderHelper : UdonSharpBehaviour
    {
        [SerializeField] InputController _inputController;
        [SerializeField] RectTransform _detectArea;
        [SerializeField] GameObject _tooltip;
        RectTransform _trans;
        float _percent = 0f;
        void Start()
        {
            _trans = GetComponent<RectTransform>();
        }

        public float Percent => _percent;

        private void Update()
        {
            Vector3[] corners = new Vector3[4];
            _detectArea.GetWorldCorners(corners);
            Rect detect = corners[2].x - corners[0].x > 0 ? new Rect(corners[0], corners[2] - corners[0]) : new Rect(corners[3], corners[1] - corners[3]);

            if (detect.Contains(_inputController.MousePosition)) _tooltip.SetActive(true);
            else
            {
                _tooltip.SetActive(false);
                return;
            }

            _trans.GetWorldCorners(corners);
            bool normal = corners[2].x - corners[0].x > 0;
            Rect rect = normal ? new Rect(corners[0], corners[2] - corners[0]) : new Rect(corners[3], corners[1] - corners[3]);

            if (!rect.Contains(_inputController.MousePosition)) return;
            RectTransform trans = _tooltip.GetComponent<RectTransform>();
            Vector2 pos = trans.anchoredPosition;
            _percent = (_inputController.MousePosition.x - rect.x) / rect.width;
            _percent = normal ? _percent : 1 - _percent;
            pos.x = _percent * _trans.sizeDelta.x;
            trans.anchoredPosition = pos;
        }
    }
}