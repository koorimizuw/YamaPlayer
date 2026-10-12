using System;
using UnityEditor;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using Cysharp.Threading.Tasks;

namespace Yamadev.YamaStream.Editor
{
  [InitializeOnLoad]
  public static class PackageManager
  {
    public const string VpmId = "net.kwxxw.vpm";
    public const string VpmUrl = "https://vpm.kwxxw.net/index.json";
    private const string CheckBetaKey = "YamaPlayer_CheckBetaUpdate";

    public static bool HasNewVersion { get; private set; } = false;
    public static string NewestVersion { get; private set; } = string.Empty;

    static PackageManager()
    {
      EditorApplication.delayCall += () =>
      {
        if (!EditorApplication.isCompiling && !EditorApplication.isUpdating)
        {
          CheckUpdate().Forget();
        }
      };
    }

    public static bool CheckBetaVersion
    {
      get => EditorPrefs.GetBool(CheckBetaKey);
      set => EditorPrefs.SetBool(CheckBetaKey, value);
    }

    public static PackageInfo PackageInfo => PackageInfo.FindForAssembly(typeof(PackageManager).Assembly);

    public static string PackageName => PackageInfo?.name ?? string.Empty;

    public static string Version => PackageInfo?.version ?? string.Empty;

    public static async UniTask<bool> CheckUpdate()
    {
#if USE_VPM_RESOLVER
      try
      {
        HasNewVersion = false;
        NewestVersion = string.Empty;

        VpmResolver.MigrateToVCC(VpmId, VpmUrl);

        if (string.IsNullOrEmpty(PackageName) || string.IsNullOrEmpty(Version))
        {
          Debug.LogWarning("Package name or version is empty. Cannot check for updates.");
          return false;
        }

        var versions = await VpmResolver.GetVersions(PackageName);
        if (versions == null || versions.Count == 0) return false;

        foreach (string version in versions)
        {
          if (!CheckBetaVersion && version.Contains("beta")) continue;
          NewestVersion = version;
          break;
        }

        if (string.IsNullOrEmpty(NewestVersion))
        {
          Debug.LogWarning("No suitable version found.");
          return false;
        }

        HasNewVersion = new SemanticVersioning.Version(Version) < new SemanticVersioning.Version(NewestVersion);
        return HasNewVersion;
      }
      catch (Exception ex)
      {
        Debug.LogError($"CheckUpdate failed: {ex.Message}");
        return false;
      }
#else
      return false;
#endif
    }

    public static async UniTask<bool> UpdatePackage()
    {
#if USE_VPM_RESOLVER
      if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return false;

      await CheckUpdate();
      if (!HasNewVersion)
      {
        Debug.LogWarning("No new version found.");
        return false;
      }

      return await VpmResolver.UpdatePackage(PackageName, NewestVersion);
#else
      return false;
#endif
    }
  }
}