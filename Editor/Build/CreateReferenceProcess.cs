using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using Yamadev.YamaStream.Script;
using Yamadev.YamaStream.UI;

namespace Yamadev.YamaStream.Editor
{
    public class CreateReferenceProcess : IYamaPlayerBuildProcess
    {
        public int callbackOrder => -1000;

        public void CreateInputController()
        {
            InputController inputController = new GameObject("InputController").AddUdonSharpComponent<InputController>();
            UdonSharpEditorUtility.GetBackingUdonBehaviour(inputController).SyncMethod = Networking.SyncType.None;
            foreach (SliderHelper component in Utils.FindComponentsInHierarthy<SliderHelper>())
            {
                if (component.GetProgramVariable("_inputController") == null)
                    component.SetProgramVariable("_inputController", inputController);
            }
        }

        public LatencyManager CreateLatencyManager()
        {
            LatencyManager latencyManager = new GameObject("LatencyManager").AddUdonSharpComponent<LatencyManager>();
            UdonSharpEditorUtility.GetBackingUdonBehaviour(latencyManager).SyncMethod = Networking.SyncType.None;
            for (int i = 0; i < 300; i++)
            {
                LatencyRecord record = new GameObject($"LatencyRecord({i})").AddUdonSharpComponent<LatencyRecord>();
                UdonSharpEditorUtility.GetBackingUdonBehaviour(record).SyncMethod = Networking.SyncType.Manual;
                record.transform.SetParent(latencyManager.transform);
            }
            return latencyManager;
        }

        const string _translationFileGuid = "02e2b6ce10f26f94fb504aba7ccd2bfe";
        const string _updateLogFileGuid = "011c1aa791634cf45b66a6811ad47c8a";

        public void Process()
        {
            CreateInputController();
            LatencyManager latencyManager = CreateLatencyManager();

            foreach (YamaPlayer player in Utils.FindComponentsInHierarthy<YamaPlayer>())
            {
                Controller internalController = player.GetComponentInChildren<Controller>();
                if (internalController != null) internalController.SetProgramVariable("_version", VersionManager.Version);
                if (internalController != null) internalController.SetProgramVariable("_latencyManager", latencyManager);

                Transform internalTransform = player.Internal != null ? player.Internal : player.transform.Find("Internal");
                if (internalTransform != null)
                {
                    internalTransform.SetParent(null, true);
                    GameObjectUtility.EnsureUniqueNameForSibling(internalTransform.gameObject);
                }
            }

            foreach (UIController uiController in Utils.FindComponentsInHierarthy<UIController>())
            {
                TextAsset updateLogFile = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(_updateLogFileGuid));
                if (updateLogFile != null) uiController.SetProgramVariable("_updateLogFile", updateLogFile);
                TextAsset translationFile = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(_translationFileGuid));
                if (translationFile != null) uiController.SetProgramVariable("_translationTextFile", translationFile);

                foreach (UIColor component in uiController.GetComponentsInChildren<UIColor>(true))
                    if (component.GetProgramVariable("_uiController") == null)
                        component.SetProgramVariable("_uiController", uiController);

                Font font = (Font)uiController.GetProgramVariable("_font");
                if (font != null) foreach (Text text in uiController.GetComponentsInChildren<Text>(true)) text.font = font;
            }
        }
    }
}