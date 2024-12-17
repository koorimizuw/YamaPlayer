using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yamadev.YamaStream.UI;
using UdonSharpEditor;
using Yamadev.YamaStream.Modules;
using VRC.SDK3.Components;
using UnityEngine.UI;
using Yamadev.YamaStream.Script;

#if WEB_UNIT_INCLUDED
using Yamadev.YamachanWebUnit;
#endif

namespace Yamadev.YamaStream.Editor
{
    public class YamaPlayerBuildProcess : IProcessSceneWithReport
    {
        readonly static string _managerPrefabGuid = "3488ed8aceb89a146969441cdf1645f0";

        public int callbackOrder => -10;

        public void CreateWebUnitClient()
        {
#if WEB_UNIT_INCLUDED
            GameObject go = new GameObject("WebUnitClient");
            Client client = go.AddUdonSharpComponent<Client>();
            foreach (VideoResolver resolver in Utils.FindComponentsInHierarthy<VideoResolver>())
                resolver.SetProgramVariable("_client", client);
#endif
        }

        public void CreateInputController()
        {
            GameObject go = new GameObject("InputController");
            InputController inputController = go.AddUdonSharpComponent<InputController>();
            foreach (SliderHelper component in Utils.FindComponentsInHierarthy<SliderHelper>())
            {
                if (component.GetProgramVariable("_inputController") == null)
                    component.SetProgramVariable("_inputController", inputController);
            }
        }

        public YamaPlayerManager CreateManager()
        {
            string path = AssetDatabase.GUIDToAssetPath(_managerPrefabGuid);
            GameObject obj = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path), null) as GameObject;
            YamaPlayerManager manager = obj.GetComponent<YamaPlayerManager>();
            manager.SetProgramVariable("Version", VersionManager.Version);
            LatencyManager latencyManager = obj.GetComponentInChildren<LatencyManager>();
            if (latencyManager != null)
            {
                Transform template = latencyManager.transform.GetChild(0);
                for (int i = 0; i < 300; i++)
                {
                    GameObject newRecord = GameObject.Instantiate(template.gameObject, latencyManager.transform, false);
                    GameObjectUtility.EnsureUniqueNameForSibling(newRecord);
                    newRecord.SetActive(true);
                }
            }
            return manager;
        }

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            CreateWebUnitClient();
            CreateInputController();
            YamaPlayerManager manager = CreateManager();

            foreach (YamaPlayer player in Utils.FindComponentsInHierarthy<YamaPlayer>())
            {
                Controller internalController = player.GetComponentInChildren<Controller>();
                if (internalController != null) internalController.SetProgramVariable("_manager", manager);

                Transform internalTransform = player.Internal != null ? player.Internal : player.transform.Find("Internal");
                if (internalTransform != null)
                {
                    internalTransform.SetParent(null, true);
                    GameObjectUtility.EnsureUniqueNameForSibling(internalTransform.gameObject);
                }

                Transform dynamicPlaylistContainer = player.DynamicPlaylistContainer;
                if (dynamicPlaylistContainer != null)
                {
                    Transform template = dynamicPlaylistContainer.GetChild(0);
                    for (int i = 0; i < 200; i++)
                    {
                        GameObject newRecord = GameObject.Instantiate(template.gameObject, dynamicPlaylistContainer, false);
                        GameObjectUtility.EnsureUniqueNameForSibling(newRecord);
                        newRecord.SetActive(true);
                    }
                }
            }

            foreach (UIController uiController in Utils.FindComponentsInHierarthy<UIController>())
            {
                foreach (UIColor component in uiController.GetComponentsInChildren<UIColor>(true))
                    if (component.GetProgramVariable("_uiController") == null)
                        component.SetProgramVariable("_uiController", uiController);

                Font font = (Font)uiController.GetProgramVariable("_font");
                if (font != null) foreach (Text text in uiController.GetComponentsInChildren<Text>(true)) text.font = font;

                uiController.SetProgramVariable("_manager", manager);

                VRCUrlInputField dynamicUrlInputField = uiController.GetProgramVariable("_dynamicPlaylistUrlInput") as VRCUrlInputField;
                if (dynamicUrlInputField != null)
                {
#if WEB_UNIT_INCLUDED
                    dynamicUrlInputField.gameObject.SetActive(true);
#else
                    dynamicUrlInputField.gameObject.SetActive(false);
#endif
                }
            }
        }
    }
}