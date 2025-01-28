using UnityEngine;
using UnityEngine.UI;

namespace Yamadev.YamaStream
{
    public partial class Controller
    {
        [SerializeField] int _maxResolution;
        [SerializeField] bool _mirrorInverse = true;
        [SerializeField, Range(0f, 1f)] float _emission = 1f;
        [SerializeField] ScreenType[] _screenTypes;
        [SerializeField] Object[] _screens;
        [SerializeField] string[] _textureProperties;
        MaterialPropertyBlock _propertyBlock;

        private void InitializeScreen()
        {
            _propertyBlock = new MaterialPropertyBlock();
            Handler.MaxResolution = _maxResolution;
            _propertyBlock.SetInt("_MirrorFlip", _mirrorInverse ? 1 : 0);
            _propertyBlock.SetFloat("_Emission", _emission);
        }

        public Texture Texture => Handler.Texture;

        public Object[] Screens => _screens;

        public int MaxResolution
        {
            get => _maxResolution;
            set
            {
                if (value == _maxResolution) return;
                _maxResolution = value;
                Handler.MaxResolution = value;
                if (!Stopped) SendCustomEventDelayedFrames(nameof(Reload), 0);
                foreach (Listener listener in _listeners) listener.OnMaxResolutionChanged();
            }
        }

        public bool MirrorInverse
        {
            get => _mirrorInverse;
            set
            {
                _mirrorInverse = value;
                _propertyBlock.SetInt("_MirrorFlip", value ? 1 : 0);
                foreach (Listener listener in _listeners) listener.OnMirrorInversionChanged();
            }
        }

        public float Emission
        {
            get => _emission;
            set
            {
                _emission = value;
                _propertyBlock.SetFloat("_Emission", value);
                foreach (Listener listener in _listeners) listener.OnEmissionChanged();
            }
        }

        public override void OnTextureUpdated(Texture texture)
        {
            for (int i = 0; i < _screens.Length; i++)
            {
                if (!_screens[i]) continue;

                string textureProperty = _textureProperties[i];
                switch (_screenTypes[i])
                {
                    case ScreenType.Renderer:
                        if (!texture) ((Renderer)_screens[i]).SetPropertyBlock(new MaterialPropertyBlock(), 0);
                        else
                        {
                            _propertyBlock.SetTexture(textureProperty, texture);
                            ((Renderer)_screens[i]).SetPropertyBlock(_propertyBlock, 0);
                        }
                        break;
                    case ScreenType.RawImage:
                        ((RawImage)_screens[i]).texture = texture;
                        break;
                    case ScreenType.Material:
                        ((Material)_screens[i]).SetTexture(textureProperty, texture);
                        break;
                }
            }
        }

        public void AddScreen(ScreenType screenType, Object screen, string textureProperty = "_MainTex")
        {
            foreach (Object obj in _screens)
            {
                if (obj == screen) return;
            }
            _screenTypes = _screenTypes.Add(screenType);
            _screens = _screens.Add(screen);
            _textureProperties = _textureProperties.Add(textureProperty);
        }
    }
}