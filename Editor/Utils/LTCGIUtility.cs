using UnityEditor;
using UnityEngine;

#if USE_LTCGI
using pi.LTCGI;
#endif

namespace Yamadev.YamaStream.Editor
{
#if USE_LTCGI
    public static class LTCGIUtility
    {
        private const string CRTGuid = "a9024879323f03444be1a5332baee58e";
        private const string LTCGIControllerGuid = "4b1aac09caa0ea54ba902102643bb545";

        public static CustomRenderTexture CRT
        {
            get
            {
                var path = AssetDatabase.GUIDToAssetPath(CRTGuid);
                return AssetDatabase.LoadAssetAtPath<CustomRenderTexture>(path);
            }
        }

        public static void ProcessLTCGI(Controller yamaPlayer, bool applyToAllScreens)
        {
            try
            {
                var controller = SetupLTCGIController();
                if (controller == null)
                {
                    Debug.LogError("Failed to get or add LTCGI_Controller");
                    return;
                }
                controller.VideoTexture = CRT;

                SetupScreens(yamaPlayer, applyToAllScreens);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error occurred: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        public static LTCGI_Controller SetupLTCGIController()
        {
            var ltcgiControllers = GameObject.FindObjectsOfType<LTCGI_Controller>();
            if (ltcgiControllers.Length > 0)
            {
                if (ltcgiControllers.Length > 1)
                {
                    Debug.LogWarning("More than one LTCGI_Controller in the scene, using the first one");
                }
                return ltcgiControllers[0];
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(LTCGIControllerGuid));
            if (prefab != null)
            {
                GameObject obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                return obj.GetComponent<LTCGI_Controller>();
            }

            Debug.LogError("Failed to get or add LTCGI_Controller");
            return null;
        }

        public static void SetupScreens(Controller controller, bool applyToAllScreens)
        {
            if (controller == null)
            {
                Debug.LogError("Controller is null, cannot set up LTCGI screens.");
                return;
            }

            var screens = GameObject.FindObjectsOfType<YamaPlayerScreen>();
            foreach (var screen in screens)
            {
                if (screen.controller == controller && screen.Type == ScreenType.Renderer && screen.GetComponent<Renderer>() != null)
                {
                    var ltcgiScreen = screen.GetComponent<LTCGI_Screen>() ?? screen.gameObject.AddComponent<LTCGI_Screen>();
                    ltcgiScreen.ColorMode = ColorMode.Texture;
                }
            }
        }

        public static void ClearLTCGISettings()
        {
            var ltcgiScreens = GameObject.FindObjectsOfType<LTCGI_Screen>();
            foreach (LTCGI_Screen ltcgiScreen in ltcgiScreens)
                Object.DestroyImmediate(ltcgiScreen);
        }
    }
#endif
}
