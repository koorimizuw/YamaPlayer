
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDKBase;
using VRC.Utility;
using Yamadev.YamaStream.UI;

namespace Yamadev.YamaStream.Script
{
    public class YamaPlayerBuildProcess : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        private void SetAngle(Transform trans)
        {
            Vector3 angle = trans.eulerAngles;
            if (angle.y % 180 != 0 && angle.y % 90 == 0)
            {
                angle.y += 0.01f;
                trans.eulerAngles = angle;
            }
        }

        private void BuildPlaylists()
        {
            PlayListContainer[] ret = Resources.FindObjectsOfTypeAll<PlayListContainer>();
            if (ret.Length == 0) return;

            foreach (PlayListContainer handle in ret)
            {
                if (!AssetDatabase.GetAssetOrScenePath(handle).Contains(".unity")) continue;

                Transform template = handle.TargetContent.transform.GetChild(0);

                int count = handle.TargetContent.childCount;
                for (int i = 1; i < count; i++)
                {
                    GameObject.DestroyImmediate(handle.TargetContent.GetChild(1).gameObject);
                }

                YamaPlayer yamaPlayer = handle.FindComponentInParent<YamaPlayer>();
                PlayList[] children = handle.transform.GetComponentsInChildren<PlayList>();
                foreach (PlayList li in children)
                {
                    GameObject newObject = Object.Instantiate(template.gameObject, handle.TargetContent, false);
                    GameObjectUtility.EnsureUniqueNameForSibling(newObject);
                    // newObject.transform.Find("Input").gameObject.SetActive(false);

                    Playlist udon = newObject.transform.GetComponent<Playlist>();

                    VideoPlayerType[] players = li.Tracks.Select(tr => ((VideoPlayerType)(int)tr.Mode)).ToArray();
                    string[] titles = li.Tracks.Select(tr => tr.Title).ToArray();
                    VRCUrl[] urls = li.Tracks.Select(tr => new VRCUrl(tr.Url)).ToArray();
                    string[] displayUrls = li.Tracks.Select(tr => "").ToArray();

                    udon.SetProgramVariable("_playlistName", li.PlayListName);
                    udon.SetProgramVariable("_videoPlayerTypes", players);
                    udon.SetProgramVariable("_titles", titles);
                    udon.SetProgramVariable("_urls", urls);
                    udon.SetProgramVariable("_originalUrls", displayUrls);
                    newObject.SetActive(true);
                }

                GameObject.DestroyImmediate(template.gameObject);
                Controller playlistUdon = yamaPlayer.GetComponentInChildren<Controller>();
                playlistUdon.SetProgramVariable("_playlists", handle.TargetContent.GetComponentsInChildren<Playlist>());
            }
        }

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

                Controller internalController = player.GetComponentInChildren<Controller>();
                if (internalController != null) internalController.SetProgramVariable("_version", Utils.GetYamaPlayerVersion());

                BuildPlaylists();

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
                SetAngle(player.transform);
            }

            foreach (YamaPlayerController controller in Utils.FindComponentsInHierarthy<YamaPlayerController>())
            {
                SetAngle(controller.transform.transform);
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