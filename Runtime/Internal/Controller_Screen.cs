using System;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using Object = UnityEngine.Object;

namespace Yamadev.YamaStream
{
    public partial class Controller
    {
        [SerializeField] private int _maxResolution;
        [SerializeField] private bool _mirrorFlip = true;
        [SerializeField, Range(0f, 1f)] private float _emission = 1f;
        [SerializeField] private ScreenType[] _screenTypes;
        [SerializeField] private Object[] _screens;
        [SerializeField] private string[] _textureProperties;
        private MaterialPropertyBlock _propertyBlock;

        private void InitializePropertyBlock()
        {
            if (!Utilities.IsValid(_propertyBlock))
            {
                _propertyBlock = new MaterialPropertyBlock();
            }
        }

        private void UpdateScreenMaterial()
        {
            InitializePropertyBlock();

            _propertyBlock.SetInt("_MirrorFlip", _mirrorFlip ? 1 : 0);
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
                foreach (Listener listener in EventListeners) listener.OnMaxResolutionChanged();
            }
        }

        public bool MirrorFlip
        {
            get => _mirrorFlip;
            set
            {
                _mirrorFlip = value;
                UpdateScreenMaterial();
                foreach (Listener listener in EventListeners) listener.OnMirrorInversionChanged();
            }
        }

        public float Emission
        {
            get => _emission;
            set
            {
                _emission = value;
                UpdateScreenMaterial();
                foreach (Listener listener in EventListeners) listener.OnEmissionChanged();
            }
        }

        public override void OnTextureUpdated(Texture texture)
        {
            InitializePropertyBlock();

            for (int i = 0; i < _screens.Length; i++)
            {
                if (!Utilities.IsValid(_screens[i])) continue;

                string textureProperty = _textureProperties[i];
                switch (_screenTypes[i])
                {
                    case ScreenType.Renderer:
                        if (!Utilities.IsValid(texture))
                        {
                            ((Renderer)_screens[i]).SetPropertyBlock(new MaterialPropertyBlock(), 0);
                        }
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
                if (obj == screen)
                {
                    PrintWarning($"Screen {screen.name} already exists");
                    return;
                }
            }

            _screenTypes = _screenTypes.Add(screenType);
            _screens = _screens.Add(screen);
            _textureProperties = _textureProperties.Add(textureProperty);
        }

        [Obsolete("Use MirrorFlip instead")]
        public bool MirrorInverse
        {
            get => _mirrorFlip;
            set => _mirrorFlip = value;
        }
    }
}