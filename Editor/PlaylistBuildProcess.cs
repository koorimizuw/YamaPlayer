
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor;
using VRC.SDKBase;
using System.Linq;
using VRC.Utility;

namespace Yamadev.YamaStream.Script
{
    public class PlaylistBuildProcess : IProcessSceneWithReport
    {
        public int callbackOrder => -1;
        public void OnProcessScene(Scene scene, BuildReport report)
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
                    GameObject newObject = UnityEngine.Object.Instantiate(template.gameObject, handle.TargetContent, false);
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
    }
}