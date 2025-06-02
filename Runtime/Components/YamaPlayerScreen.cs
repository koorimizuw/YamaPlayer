using UnityEngine;
using UnityEngine.UI;

namespace Yamadev.YamaStream
{
    [DisallowMultipleComponent]
    public class YamaPlayerScreen : MonoBehaviour
    {
        public Controller controller;
        public string property = "_MainTex";

        private Object _cachedReference;
        private ScreenType _cachedType = ScreenType.Unknown;
        private bool _hasCached = false;

        public ScreenType Type
        {
            get
            {
                if (!_hasCached) CacheComponents();
                return _cachedType;
            }
        }

        public Object Reference
        {
            get
            {
                if (!_hasCached) CacheComponents();
                return _cachedReference;
            }
        }

        public Shader Shader
        {
            get
            {
                switch (Type)
                {
                    case ScreenType.Renderer:
                        return (Reference as Renderer).sharedMaterial?.shader;
                    case ScreenType.RawImage:
                        return (Reference as RawImage).material?.shader;
                    case ScreenType.Material:
                        return (Reference as Material).shader;
                    default:
                        return null;
                }
            }
        }

        private void CacheComponents()
        {
            _cachedReference = null;
            _cachedType = ScreenType.Unknown;

            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                _cachedReference = renderer;
                _cachedType = ScreenType.Renderer;
            }
            else
            {
                var rawImage = GetComponent<RawImage>();
                if (rawImage != null)
                {
                    _cachedReference = rawImage;
                    _cachedType = ScreenType.RawImage;
                }
            }

            _hasCached = true;
        }
    }
}