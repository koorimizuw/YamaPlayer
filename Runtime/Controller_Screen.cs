
using UnityEngine;
using UnityEngine.UI;

namespace Yamadev.YamaStream
{
    public partial class Controller : Listener
    {
        [SerializeField] int _maxResolution;
        [SerializeField] bool _mirrorInverse = true;
        [SerializeField, Range(0f, 1f)] float _emission = 1f;
        [SerializeField] ScreenType[] _screenTypes;
        [SerializeField] Object[] _screens;
        [SerializeField] string[] _textureProperties;
        [SerializeField] string[] _avProProperties;
        MaterialPropertyBlock _properties;

        void initializeScreen()
        {
            MirrorInverse = _mirrorInverse;
            MaxResolution = _maxResolution;
            Emission = _emission;
        }

        public MaterialPropertyBlock MaterialProperty
        {
            get
            {
                if (_properties == null ) _properties = new MaterialPropertyBlock();
                return _properties;
            }
        }

        public Texture Texture => VideoPlayerHandle.Texture;

        public Object[] Screens => _screens;

        public int MaxResolution
        {
            get => _maxResolution;
            set
            {
                bool valueChanged = value != _maxResolution;
                _maxResolution = value;
                _videoPlayerAnimator.SetFloat("Resolution", value / 4320f);
                _videoPlayerAnimator.Update(0f);
                if (!_stopped) SendCustomEventDelayedFrames(nameof(Reload), 1);
                foreach (Listener listener in _listeners) listener.OnMaxResolutionChanged();
            }
        }

        public bool MirrorInverse
        {
            get => _mirrorInverse;
            set
            {
                _mirrorInverse = value;
                MaterialProperty.SetInt("_InversionInMirror", value ? 1 : 0);
                foreach (Listener listener in _listeners) listener.OnMirrorInversionChanged();
            }
        }

        public float Emission
        {
            get => _emission;
            set
            {
                _emission = value;
                MaterialProperty.SetFloat("_Emission", value);
                foreach (Listener listener in _listeners) listener.OnEmissionChanged();
            }
        }

        public override void OnTextureUpdated()
        {
            if (Texture == null) return;

#if UNITY_STANDALONE_WIN
            int isAVPro = VideoPlayerType == VideoPlayerType.AVProVideoPlayer ? 1 : 0;
#else
            int isAVPro = 0;
#endif
            for (int i = 0; i < _screens.Length; i++)
            {
                Object screen = _screens[i];
                if (screen == null) continue;

                string textureProperty = _textureProperties[i];
                string avProProperty = _avProProperties[i];
                switch (_screenTypes[i])
                {
                    case ScreenType.Renderer:
                        MaterialProperty.SetTexture(textureProperty, Texture);
                        MaterialProperty.SetInt(avProProperty, isAVPro);
                        ((Renderer)screen).SetPropertyBlock(MaterialProperty, 0);
                        break;
                    case ScreenType.RawImage:
                        ((RawImage)screen).texture = Texture;
                        ((RawImage)screen).material.SetInt(avProProperty, isAVPro);
                        break;
                    case ScreenType.Material:
                        ((Material)screen).SetTexture(textureProperty, Texture);
                        ((Material)screen).SetInt(avProProperty, isAVPro);
                        break;
                }
            }
        }

        public void AddScreen(ScreenType screenType, Object screen, string textureProperty = "_MainTex", string avProProperty = "_AVPro")
        {
            foreach (Object obj in _screens)
            {
                if (obj == screen) return;
            }
            _screenTypes = _screenTypes.Add(screenType);
            _screens = _screens.Add(screen);
            _textureProperties = _textureProperties.Add(textureProperty);
            _avProProperties = _avProProperties.Add(avProProperty);
        }
    }
}