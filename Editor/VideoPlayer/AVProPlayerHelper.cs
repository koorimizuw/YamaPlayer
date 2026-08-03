using Cysharp.Threading.Tasks;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace Yamadev.YamaStream.Editor
{
  [InitializeOnLoad]
  public static class AVProPlayerHelper
  {
    private static readonly NamedBuildTarget[] TargetGroups =
    {
      NamedBuildTarget.Standalone,
      NamedBuildTarget.Android,
      NamedBuildTarget.iOS,
    };

    private const string AVProDebugSymbol = "AVPRO_DEBUG";
    private const string AVProReleaseUrl = "https://github.com/RenderHeads/UnityPlugin-AVProVideo/releases/download/3.2.9/UnityPlugin-AVProVideo-v3.2.9-Trial.unitypackage";

    private static bool AVProHelperImported => EditorUtils.FindType("RenderHeads.Media.AVProVideo.MediaPlayer", true) != null;
    private static bool AVProImported => EditorUtils.FindType("RenderHeads.Media.AVProVideo.VideoTrack", true) != null;

    static AVProPlayerHelper()
    {
      EditorApplication.delayCall += InitializeAVProHelper;
    }

    private static void InitializeAVProHelper()
    {
      if (AVProHelperImported || PackageManager.PackageInfo == null) return;

      foreach (Sample sample in Sample.FindByPackage(PackageManager.PackageName, PackageManager.Version))
      {
        if (sample.displayName == "AVProHelper")
        {
          Debug.Log("AVPro not imported, add AVPro helper to project...");
          if (CopyFilesRecursively(sample.resolvedPath, Application.dataPath))
            AssetDatabase.Refresh();
          return;
        }
      }
    }

    [MenuItem("YamaPlayer/Enable AVPro Debug")]
    public static async UniTask AVProDebug()
    {
      PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone, out string[] symbols);
      if (AVProImported && symbols.Contains(AVProDebugSymbol))
      {
        EditorUtility.DisplayDialog("Info", "AVPro debug mode is already enabled.", "OK");
        return;
      }

      if (!AVProImported)
      {
        if (!EditorUtility.DisplayDialog("AVPro not imported", "You should download the AVPro package to use this feature.\nDo you want to download the AVPro package?", "Yes", "No"))
          return;

        if (!await DownloadAVProVideo())
        {
          EditorUtility.DisplayDialog("Error", "Failed to download AVPro package.", "OK");
          return;
        }
      }

      foreach (var target in TargetGroups)
      {
        PlayerSettings.GetScriptingDefineSymbols(target, out string[] current);
        if (!current.Contains(AVProDebugSymbol))
        {
          var list = current.ToList();
          list.Insert(0, AVProDebugSymbol);
          PlayerSettings.SetScriptingDefineSymbols(target, list.ToArray());
        }
      }
      EditorUtility.DisplayDialog("Success", "AVPro debug mode enabled successfully.", "OK");
    }

    public static async UniTask<bool> DownloadAVProVideo()
    {
      string tempPath = Path.GetTempFileName();
      const string progressTitle = "Downloading AVPro";
      bool importScheduled = false;

      try
      {
        EditorUtility.DisplayProgressBar(progressTitle, "Downloading AVPro unity package...", 0.1f);

        using (var request = UnityWebRequest.Get(AVProReleaseUrl))
        {
          request.downloadHandler = new DownloadHandlerFile(tempPath) { removeFileOnAbort = true };
          var downloadOperation = request.SendWebRequest();

          while (!downloadOperation.isDone)
          {
            EditorUtility.DisplayProgressBar(progressTitle,
                $"Downloading AVPro unity package... {request.downloadProgress * 100:F1}%",
                request.downloadProgress);
            await UniTask.Yield();
          }

          if (request.result != UnityWebRequest.Result.Success)
          {
            Debug.LogError($"Failed to download AVPro package: {request.error}");
            return false;
          }
        }

        EditorUtility.DisplayProgressBar(progressTitle, "Importing package...", 0.9f);

        string expectedPackageName = Path.GetFileNameWithoutExtension(tempPath);
        void Cleanup()
        {
          AssetDatabase.importPackageCompleted -= OnImportCompleted;
          AssetDatabase.importPackageFailed -= OnImportFailed;
          if (File.Exists(tempPath)) File.Delete(tempPath);
        }
        void OnImportCompleted(string packageName)
        {
          if (packageName == expectedPackageName) Cleanup();
        }
        void OnImportFailed(string packageName, string errorMessage)
        {
          if (packageName == expectedPackageName) Cleanup();
        }

        AssetDatabase.importPackageCompleted += OnImportCompleted;
        AssetDatabase.importPackageFailed += OnImportFailed;

        try
        {
          AssetDatabase.ImportPackage(tempPath, false);
        }
        catch
        {
          Cleanup();
          throw;
        }
        importScheduled = true;
        return true;
      }
      finally
      {
        EditorUtility.ClearProgressBar();
        if (!importScheduled && File.Exists(tempPath))
        {
          File.Delete(tempPath);
        }
      }
    }

    public static bool CopyFilesRecursively(string sourcePath, string targetPath)
    {
      if (!Directory.Exists(sourcePath))
      {
        Debug.LogError($"Source directory does not exist: {sourcePath}");
        return false;
      }

      foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
        Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));

      foreach (string filePath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
        File.Copy(filePath, filePath.Replace(sourcePath, targetPath), true);

      return true;
    }
  }
}