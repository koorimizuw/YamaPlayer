﻿
using UnityEditor;
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

            int isAVPro = VideoPlayerType == VideoPlayerType.AVProVideoPlayer ? 1 : 0;
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
                        SetST(((Renderer)screen).sharedMaterial, textureProperty);
                        break;
                    case ScreenType.RawImage:
                        ((RawImage)screen).texture = Texture;
                        ((RawImage)screen).material.SetInt(avProProperty, isAVPro);
#if UNITY_STANDALONE_WIN
                        ((RawImage)screen).uvRect = isAVPro == 1 ? new Rect(0, 1, 1, -1) : new Rect(0, 0, 1, 1);
#else
                        ((RawImage)screen).uvRect = new Rect(0, 0, 1, 1);
#endif
                        break;
                    case ScreenType.Material:
                        ((Material)screen).SetTexture(textureProperty, Texture);
                        ((Material)screen).SetInt(avProProperty, isAVPro);
                        SetST((Material)screen, textureProperty);
                        break;
                }
            }
        }

        public void SetST(Material material, string textureProperty)
        {
#if UNITY_STANDALONE_WIN
            bool isAVPro = VideoPlayerType == VideoPlayerType.AVProVideoPlayer;
            material.SetTextureScale(textureProperty, isAVPro ?  new Vector2(1, -1) : new Vector2(1, 1));
            material.SetTextureOffset(textureProperty, isAVPro ? new Vector2(0, 1) : new Vector2(0, 0));
#else
            material.SetTextureScale(textureProperty, new Vector2(1, 1));
            material.SetTextureOffset(textureProperty, new Vector2(0, 0));
#endif
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