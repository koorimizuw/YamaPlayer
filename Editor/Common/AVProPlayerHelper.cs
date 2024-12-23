using Cysharp.Threading.Tasks;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace Yamadev.YamaStream.Editor
{
    [InitializeOnLoad]
    public static class AVProPlayerHelper
    {
        private static readonly BuildTargetGroup[] _targetGroups =
        {
            BuildTargetGroup.Standalone,
            BuildTargetGroup.Android,
            BuildTargetGroup.iOS,
        };
        private static readonly string _avProDebugSymbol = "AVPRO_DEBUG";
        private static readonly string _avProReleaseUrl = "https://github.com/RenderHeads/UnityPlugin-AVProVideo/releases/download/3.1.3/UnityPlugin-AVProVideo-v3.1.3-Trial.unitypackage";

        private static bool _avProImported => Utils.FindType("RenderHeads.Media.AVProVideo.VideoTrack", true) != null;

        static AVProPlayerHelper()
        {
            if (_avProImported || VersionManager.PackageInfo == null) return;
            foreach (Sample sample in Sample.FindByPackage(VersionManager.PackageInfo.name, VersionManager.PackageInfo.version))
            {
                if (sample.displayName == "AVProHelper")
                {
                    Debug.Log("AVPro not imported, add AVPro helper to project...");
                    CopyFilesRecursively(sample.resolvedPath, Application.dataPath);
                    return;
                }
            }
        }

        [MenuItem("YamaPlayer/Enable AVPro Debug")]
        public static async UniTask AVProDebug()
        {
            if (!_avProImported)
            {
                if (EditorUtility.DisplayDialog("AVPro not imported", "Download unity package?", "Yes", "No")) 
                    await DownloadAVProVideo();
                else return;
            }
            DefineSymbol.AddSymbol(_targetGroups, _avProDebugSymbol);
            EditorUtility.DisplayDialog("Success", "AVPro debug enabled.", "OK");
        }

        public static async UniTask DownloadAVProVideo()
        {
            try
            {
                string path = Path.GetTempFileName();
                using (var request = UnityWebRequest.Get(_avProReleaseUrl))
                {
                    DownloadHandlerFile handler = new DownloadHandlerFile(path) { removeFileOnAbort = true };
                    request.downloadHandler = handler;
                    await request.SendWebRequest();
                    EditorUtility.DisplayProgressBar("Downloading...", "Download AVPro unity package...", 0);
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Debug.Log("Download AVPro unity package failed.");
                        EditorUtility.ClearProgressBar();
                        return;
                    }
                }
                AssetDatabase.ImportPackage(path, false);
                File.Delete(path);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        public static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));

            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
        }
    }
}