
using System;
using UnityEditor;
using UnityEngine;

namespace Yamadev.YamaStream.Script
{
    public class LTCGIEditor : EditorWindow
    {
        static string _url = "https://github.com/PiMaker/ltcgi/releases/latest";
        static string _crtPath = "Assets/Yamadev/YamaStream/Assets/Textures/YamaPlayerCRT.asset";
        static string _crtGuid = "a9024879323f03444be1a5332baee58e";
        static string _ltcgiControllerPath = "Packages/at.pimaker.ltcgi/LTCGI Controller.prefab";
        static string _ltcgiControllerGuid = "4b1aac09caa0ea54ba902102643bb545";
        static bool _applyToAllScreens = false;
        static YamaPlayer _player;

        [MenuItem("YamaPlayer/LTCGI")]
        public static void ShowLTCGIEditorWindow()
        {
            LTCGIEditor window = GetWindow<LTCGIEditor>(title: "LTCGI Editor");
            window.Show();
        }

        static void clearCurrentSettings()
        {
#if LTCGI_INCLUDED
            Type ltcgiScreen = Utils.FindType("pi.LTCGI.LTCGI_Screen", true);
            UnityEngine.Object[] screens = Utils.FindComponentsInHierarthy(ltcgiScreen);
            foreach (UnityEngine.Object screen in screens) DestroyImmediate(screen);
            YamaPlayer[] players = Utils.FindComponentsInHierarthy<YamaPlayer>();
            foreach (YamaPlayer player in players)
            {
                Controller yamaplayerController = _player.GetComponentInChildren<Controller>();
                if (yamaplayerController != null) yamaplayerController.SetProgramVariable("_lod", null);
            }
#endif
        }

        static void autoApply()
        {
#if LTCGI_INCLUDED
            clearCurrentSettings();
            CustomRenderTexture crt = AssetDatabase.LoadAssetAtPath<CustomRenderTexture>(_crtPath);
            if (crt == null)
            {
                string crtPath = AssetDatabase.GUIDToAssetPath(_crtGuid);
                crt = AssetDatabase.LoadAssetAtPath<CustomRenderTexture>(crtPath);
            }
            if (crt == null) return;
            Controller yamaplayerController = _player.GetComponentInChildren<Controller>();
            if (yamaplayerController != null)
            {
                SerializedObject serializedObject = new SerializedObject(yamaplayerController);
                serializedObject.FindProperty("_lod").objectReferenceValue = crt.material;
                serializedObject.ApplyModifiedProperties();
            }
            Renderer[] renderers = new Renderer[] { };
            if (!_applyToAllScreens)
            {
                Renderer mainScreen = _player.MainScreen;
                if (mainScreen == null) mainScreen = _player.transform.GetComponentInChildren<YamaPlayerScreen>()?.GetComponent<Renderer>();
                if (mainScreen != null) renderers = renderers.Add(mainScreen);
            }
            else
            {
                YamaPlayerScreen[] screens = Utils.FindComponentsInHierarthy<YamaPlayerScreen>();
                foreach (YamaPlayerScreen screen in screens)
                {
                    if ((UnityEngine.Object)(screen.GetProgramVariable("_controller")) == yamaplayerController && screen.TryGetComponent<Renderer>(out var renderer))
                        renderers = renderers.Add(renderer);
                }
            }
            foreach (Renderer renderer in renderers)
            {
                Type ltcgiScreen = Utils.FindType("pi.LTCGI.LTCGI_Screen", true);
                Component screen = renderer.gameObject.GetComponent(ltcgiScreen);
                if (screen == null) screen = renderer.gameObject.AddComponent(ltcgiScreen);
                ((dynamic)screen).ColorMode = Enum.Parse(((dynamic)screen).ColorMode.GetType(), "Texture");
            }
            Type ltcgiController = Utils.FindType("pi.LTCGI.LTCGI_Controller", true);
            UnityEngine.Object[] controllers = Utils.FindComponentsInHierarthy(ltcgiController);
            UnityEngine.Object controller = controllers.Length > 0 ? controllers[0] : null;
            if (controller == null)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(_ltcgiControllerPath);
                if (prefab == null)
                {
                    string prefabPath = AssetDatabase.GUIDToAssetPath(_ltcgiControllerGuid);
                    prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                }
                if (prefab != null)
                {
                    GameObject obj = Instantiate(prefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
                    obj.transform.SetParent(null, true);
                    controller = obj.GetComponent(ltcgiController);
                }
            }
            if (controller != null) ((dynamic)controller).VideoTexture = crt;
#endif
        }

        void OnGUI()
        {
#if !LTCGI_INCLUDED
                GUILayout.Label("LTCGI not imported.", EditorStyles.boldLabel);
                GUILayout.Label("LTCGIを先にインポートしてください。");
                if (GUILayout.Button("Open Github Page")) Application.OpenURL(_url);
                return;
#else
            GUILayout.Label("Target YamaPlayer");
            _player = (YamaPlayer)EditorGUILayout.ObjectField(_player, typeof(YamaPlayer), true);
            if (_player == null) return;
            _applyToAllScreens = EditorGUILayout.Toggle("Apply to all screens", _applyToAllScreens);
            if (GUILayout.Button("LTCGI自動設定") && EditorUtility.DisplayDialog("LTCGI自動設定", "現在のLTCGI設定が上書きされます?", "Yes", "No"))
                autoApply();
#endif
        }
    }
}