using UnityEngine;
using UnityEngine.UI;

namespace Yamadev.YamaStream.UI
{
    public class UIColor : MonoBehaviour
    {
        [SerializeField] private UIController _uiController;
        [SerializeField] private ColorType _colorType;

        public UIController UIController
        {
            get => _uiController;
            set
            {
                _uiController = value;
                Apply();
            }
        }

        private Color TargetColor
        {
            get
            {
                if (_uiController == null) return new Color();
                switch (_colorType)
                {
                    case ColorType.Primary:
                        return _uiController.PrimaryColor;
                    case ColorType.Secondary:
                        return _uiController.SecondaryColor;
                    default:
                        return new Color();
                }
            }
        }

        public void Apply()
        {
            var img = GetComponent<Image>();
            if (img != null) img.color = TargetColor;

            var text = GetComponent<Text>();
            if (text != null) text.color = TargetColor;
        }
    }
}