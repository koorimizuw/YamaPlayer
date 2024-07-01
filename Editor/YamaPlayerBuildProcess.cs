
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yamadev.YamaStream.UI;

namespace Yamadev.YamaStream.Script
{
    public class YamaPlayerBuildProcess : IProcessSceneWithReport
    {
        public int callbackOrder => -1;
        public void OnProcessScene(Scene scene, BuildReport report)
        {
            foreach (YamaPlayer player in Utils.FindComponentsInHierarthy<YamaPlayer>())
            {
                LatencyManager latencyManager = player.GetComponentInChildren<LatencyManager>();
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
                Vector3 angle = player.transform.eulerAngles;
                if (angle.y % 180 != 0 && angle.y % 90 == 0)
                {
                    angle.y += 0.01f;
                    player.transform.eulerAngles = angle;
                }
            }

            foreach (YamaPlayerController controller in Utils.FindComponentsInHierarthy<YamaPlayerController>())
            {
                Vector3 angle = controller.transform.eulerAngles;
                if (angle.y % 180 != 0 && angle.y % 90 == 0)
                {
                    angle.y += 0.01f;
                    controller.transform.eulerAngles = angle;
                }
            }

            foreach (UIController uiController in Utils.FindComponentsInHierarthy<UIController>())
            {
                foreach (UIColor component in uiController.GetComponentsInChildren<UIColor>(true))
                    if (component.GetProgramVariable("_uiController") == null)
                        component.SetProgramVariable("_uiController", uiController);
            }

            foreach (InputController inputController in Utils.FindComponentsInHierarthy<InputController>())
            {
                foreach (MouseHover component in inputController.GetComponentsInChildren<MouseHover>(true))
                {
                    if (component.GetProgramVariable("_inputController") == null)
                        component.SetProgramVariable("_inputController", inputController);
                }

                foreach (SliderHelper component in inputController.GetComponentsInChildren<SliderHelper>(true))
                {
                    if (component.GetProgramVariable("_inputController") == null)
                        component.SetProgramVariable("_inputController", inputController);
                }
            }
        }
    }
}