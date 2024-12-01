
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDKBase;
using VRC.Utility;
using Yamadev.YamaStream.UI;
using UdonSharpEditor;
using Yamadev.YamaStream.Modules;
using VRC.SDK3.Components;
using UnityEngine.UI;
#if WEB_UNIT_INCLUDED
using Yamadev.YamachanWebUnit;
#endif

namespace Yamadev.YamaStream.Script
{
    public class YamaPlayerBuildProcess : IProcessSceneWithReport
    {
        public int callbackOrder => -2;

        private void GeneratePlaylists()
        {
            foreach (PlayListContainer handle in Utils.FindComponentsInHierarthy<PlayListContainer>())
            {
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
                    GameObject newObject = UnityEngine.Object.Instantiate(template.gameObject, handle.TargetContent, false);
                    GameObjectUtility.EnsureUniqueNameForSibling(newObject);

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

        public void CreateWebUnitClient()
        {
#if WEB_UNIT_INCLUDED
            GameObject go = new GameObject("WebUnitClient");
            Client client = go.AddUdonSharpComponent<Client>();
            foreach (VideoResolver resolver in Utils.FindComponentsInHierarthy<VideoResolver>())
                resolver.SetProgramVariable("_client", client);
#endif
        }

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            GeneratePlaylists();
            CreateWebUnitClient();

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
                if (internalController != null) internalController.SetProgramVariable("_version", Utils.GetYamaPlayerPackageInfo().version);

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

            foreach (InputController inputController in Utils.FindComponentsInHierarthy<InputController>())
            {
                foreach (SliderHelper component in inputController.GetComponentsInChildren<SliderHelper>(true))
                {
                    if (component.GetProgramVariable("_inputController") == null)
                        component.SetProgramVariable("_inputController", inputController);
                }
            }
        }
    }
}