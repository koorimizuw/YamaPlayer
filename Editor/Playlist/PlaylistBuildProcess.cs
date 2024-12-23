using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDKBase;
using Yamadev.YamaStream.Script;
using System.Linq;
using VRC.Utility;
using System.Collections.Generic;
using UdonSharpEditor;

namespace Yamadev.YamaStream.Editor
{
    public class PlaylistBuildProcess : IProcessSceneWithReport
    {
        public int callbackOrder => -100;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            foreach (PlayListContainer container in Utils.FindComponentsInHierarthy<PlayListContainer>())
            {
                List<Playlist> playlists = container.ReadPlaylists();
                List<YamaStream.Playlist> results = new();
                foreach (Playlist playlist in playlists)
                {
                    GameObject go = new GameObject(playlist.Name);
                    if (container.TargetContent != null) go.transform.SetParent(container.TargetContent);
                    YamaStream.Playlist udon = go.AddUdonSharpComponent<YamaStream.Playlist>();
                    UdonSharpEditorUtility.GetBackingUdonBehaviour(udon).SyncMethod = Networking.SyncType.Manual;
                    udon.SetProgramVariable("_playlistName", playlist.Name);
                    udon.SetProgramVariable("_videoPlayerTypes", playlist.Tracks.Select(tr => ((VideoPlayerType)(int)tr.Mode)).ToArray());
                    udon.SetProgramVariable("_titles", playlist.Tracks.Select(tr => tr.Title).ToArray());
                    udon.SetProgramVariable("_urls", playlist.Tracks.Select(tr => new VRCUrl(tr.Url)).ToArray());
                    udon.SetProgramVariable("_originalUrls", playlist.Tracks.Select(tr => "").ToArray());
                    results.Add(udon);
                }

                YamaPlayer yamaPlayer = container.FindComponentInParent<YamaPlayer>();
                Controller controller = yamaPlayer.GetComponentInChildren<Controller>();
                controller.SetProgramVariable("_playlists", results.ToArray());
            }
        }
    }
}