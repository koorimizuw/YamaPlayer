using Cysharp.Threading.Tasks;
using System;
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
        private static readonly BuildTargetGroup[] TargetGroups =
        {
            BuildTargetGroup.Standalone,
            BuildTargetGroup.Android,
            BuildTargetGroup.iOS,
        };

        private const string AVProDebugSymbol = "AVPRO_DEBUG";
        private const string AVProReleaseUrl = "https://github.com/RenderHeads/UnityPlugin-AVProVideo/releases/download/3.2.6/UnityPlugin-AVProVideo-v3.2.6-Trial.unitypackage";

        private static bool AVProHelperImported => Utils.FindType("RenderHeads.Media.AVProVideo.MediaPlayer", true) != null;
        private static bool AVProImported => Utils.FindType("RenderHeads.Media.AVProVideo.VideoTrack", true) != null;

        static AVProPlayerHelper()
        {
            EditorApplication.delayCall += InitializeAVProHelper;
        }

        private static void InitializeAVProHelper()
        {
            try
            {
                if (AVProHelperImported || VersionManager.PackageInfo == null) return;

                foreach (Sample sample in Sample.FindByPackage(VersionManager.PackageInfo.name, VersionManager.PackageInfo.version))
                {
                    if (sample.displayName == "AVProHelper")
                    {
                        Debug.Log("AVPro not imported, add AVPro helper to project...");
                        if (CopyFilesRecursively(sample.resolvedPath, Application.dataPath))
                        {
                            Debug.Log("AVPro helper files copied successfully.");
                        }
                        else
                        {
                            Debug.LogError("Failed to copy AVPro helper files.");
                        }
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize AVPro helper: {ex.Message}");
            }
        }

        [MenuItem("YamaPlayer/Enable AVPro Debug")]
        public static async UniTask AVProDebug()
        {
            if (AVProImported && DefineSymbol.Contains(BuildTargetGroup.Standalone, AVProDebugSymbol))
            {
                EditorUtility.DisplayDialog("Success", "AVPro debug mode is already enabled.", "OK");
                return;
            }

            try
            {
                if (!AVProImported)
                {
                    if (EditorUtility.DisplayDialog("AVPro not imported", "You should download the AVPro package to use this feature.\nDo you want to download the AVPro package?", "Yes", "No"))
                    {
                        bool downloadSuccess = await DownloadAVProVideo();
                        if (!downloadSuccess)
                        {
                            EditorUtility.DisplayDialog("Error", "Failed to download AVPro package.", "OK");
                            return;
                        }
                    }
                    else return;
                }

                DefineSymbol.AddSymbol(TargetGroups, AVProDebugSymbol);
                EditorUtility.DisplayDialog("Success", "Downloaded AVPro package and enabled debug mode successfully.", "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to enable AVPro debug: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to enable AVPro debug: {ex.Message}", "OK");
            }
        }

        public static async UniTask<bool> DownloadAVProVideo()
        {
            string tempPath = null;
            const string progressTitle = "Downloading AVPro";

            try
            {
                tempPath = Path.GetTempFileName();
                EditorUtility.DisplayProgressBar(progressTitle, "Downloading AVPro unity package...", 0.1f);

                using (var request = UnityWebRequest.Get(AVProReleaseUrl))
                {
                    var downloadHandler = new DownloadHandlerFile(tempPath)
                    {
                        removeFileOnAbort = true
                    };
                    request.downloadHandler = downloadHandler;

                    var downloadOperation = request.SendWebRequest();

                    while (!downloadOperation.isDone)
                    {
                        float progress = Mathf.Clamp01(request.downloadProgress);
                        EditorUtility.DisplayProgressBar(progressTitle,
                            $"Downloading AVPro unity package... {progress * 100:F1}%", progress);
                        await UniTask.Yield();
                    }

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"Failed to download AVPro package: {request.error}");
                        return false;
                    }
                }

                EditorUtility.DisplayProgressBar(progressTitle, "Importing package...", 0.9f);

                try
                {
                    AssetDatabase.ImportPackage(tempPath, false);
                    Debug.Log("AVPro package downloaded and imported successfully.");
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to import AVPro package: {ex.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception occurred while downloading AVPro: {ex.Message}");
                return false;
            }
            finally
            {
                EditorUtility.ClearProgressBar();

                if (!string.IsNullOrEmpty(tempPath) && File.Exists(tempPath))
                {
                    try
                    {
                        File.Delete(tempPath);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Failed to delete temporary file: {ex.Message}");
                    }
                }
            }
        }

        public static bool CopyFilesRecursively(string sourcePath, string targetPath)
        {
            if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(targetPath))
            {
                Debug.LogError("Source path or target path is null or empty.");
                return false;
            }

            if (!Directory.Exists(sourcePath))
            {
                Debug.LogError($"Source directory does not exist: {sourcePath}");
                return false;
            }

            try
            {
                foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                {
                    string targetDirPath = dirPath.Replace(sourcePath, targetPath);
                    Directory.CreateDirectory(targetDirPath);
                }

                foreach (string filePath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                {
                    string targetFilePath = filePath.Replace(sourcePath, targetPath);

                    string targetDirectory = Path.GetDirectoryName(targetFilePath);
                    if (!Directory.Exists(targetDirectory))
                    {
                        Directory.CreateDirectory(targetDirectory);
                    }

                    File.Copy(filePath, targetFilePath, true);
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to copy files recursively: {ex.Message}");
                return false;
            }
        }
    }
}