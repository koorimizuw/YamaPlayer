
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace Yamadev.YamaStream.UI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class UIColor : UdonSharpBehaviour
    {
        [SerializeField] UIController _uiController;
        [SerializeField] ColorType _colorType;
        Color _color
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
                }
                return new Color();
            }
        }

        void Start() => Apply();

        public void Apply()
        {
            Image img = GetComponent<Image>();
            if (img != null) img.color = _color;
            Text text = GetComponent<Text>();
            if (text != null) text.color = _color;
        }
    }
}