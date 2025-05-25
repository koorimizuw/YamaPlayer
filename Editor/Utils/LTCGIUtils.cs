#if LTCGI_INCLUDED
using pi.LTCGI;
using UnityEditor;
using UnityEngine;
using Yamadev.YamaStream.Script;

namespace Yamadev.YamaStream.Editor
{
    public static class LTCGIUtils
    {
        const string _crtGuid = "a9024879323f03444be1a5332baee58e";
        const string _ltcgiControllerGuid = "4b1aac09caa0ea54ba902102643bb545";

        public static CustomRenderTexture LTCGICRT =>
            AssetDatabase.LoadAssetAtPath<CustomRenderTexture>(AssetDatabase.GUIDToAssetPath(_crtGuid));

        public static LTCGI_Controller GetOrAddLTCGIController()
        {
            var ltcgiControllers = Utils.FindComponentsInHierarthy<LTCGI_Controller>();
            if (ltcgiControllers.Length > 0) return ltcgiControllers[0];

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(_ltcgiControllerGuid));
            if (prefab != null)
            {
                GameObject obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                return obj.GetComponent<LTCGI_Controller>();
            }

            return null;
        }

        public static void ClearLTCGISettings()
        {
            foreach (Controller controller in Utils.FindComponentsInHierarthy<Controller>())
                controller.RemoveScreenProperty(LTCGICRT.material);
            foreach (LTCGI_Screen ltcgiScreen in Utils.FindComponentsInHierarthy<LTCGI_Screen>())
                Object.DestroyImmediate(ltcgiScreen);
        }

        public static void SetUpLTCGIScreen(this YamaPlayer player, bool applyToSubScreens)
        {
            Controller controller = player.GetComponentInChildren<Controller>();
            YamaPlayerScreen mainScreen = player.GetComponentInChildren<YamaPlayerScreen>();
            foreach (YamaPlayerScreen screen in Utils.FindComponentsInHierarthy<YamaPlayerScreen>())
            {
                if (screen.GetProgramVariable("_controller") != (object)controller) continue;
                if (applyToSubScreens || screen == mainScreen)
                {
                    LTCGI_Screen ltcgiScreen = screen.gameObject.GetComponent<LTCGI_Screen>();
                    if (ltcgiScreen == null) ltcgiScreen = screen.gameObject.AddComponent<LTCGI_Screen>();
                    ltcgiScreen.ColorMode = ColorMode.Texture;
                }
            }
        }
    }
}
#endif