#if USE_VPM_RESOLVER
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using Cysharp.Threading.Tasks;
using VRC.PackageManagement.Resolver;
using VRC.PackageManagement.Core;
using VRC.PackageManagement.Core.Types.Packages;
using VRC.PackageManagement.Core.Types;
#endif

namespace Yamadev.YamaStream.Editor
{
#if USE_VPM_RESOLVER
  public static class VpmResolver
  {
    public static void MigrateToVCC(string vpmId, string vpmUrl)
    {
      if (Repos.UserRepoExists(vpmId)) return;
      Repos.AddRepo(new Uri(vpmUrl));
    }

    public static async UniTask<List<string>> GetVersions(string packageName)
    {
      var versions = await UniTask.RunOnThreadPool(() => Resolver.GetAllVersionsOf(packageName));
      if (versions == null || versions.Count == 0)
      {
        Debug.LogWarning($"No versions found for package: {packageName}");
        return null;
      }
      
      versions.Sort((a, b) =>
      {
        try
        {
          var versionA = new SemanticVersioning.Version(a);
          var versionB = new SemanticVersioning.Version(b);
          return versionB.CompareTo(versionA);
        }
        catch
        {
          return string.Compare(b, a);
        }
      });

      return versions;
    }

    public static async UniTask<bool> UpdatePackage(string packageName, string version)
    {
      try
      {
        var package = Repos.GetPackageWithVersionMatch(packageName, version);
        if (package == null)
        {
          Debug.LogError($"Package not found: {packageName} version {version}");
          return false;
        }

        var dialogMsg = new StringBuilder();
        var affectedPackages = Resolver.GetAffectedPackageList(package);

        for (int v = 0; v < affectedPackages.Count; v++)
          dialogMsg.Append(affectedPackages[v]);

        if (affectedPackages.Count > 1)
        {
          dialogMsg.Insert(0, "This will update multiple packages:\n\n");
          dialogMsg.AppendLine("\nAre you sure?");
          if (!EditorUtility.DisplayDialog("Update YamaPlayer", dialogMsg.ToString(), "OK", "Cancel"))
            return false;
        }

        await UniTask.Delay(500);
        Resolver.ForceRefresh();

        new UnityProject(Resolver.ProjectDir).UpdateVPMPackage(package);
        Debug.Log($"Update package {packageName} to version: {version}");

        Resolver.ForceRefresh();

        return true;
      }
      catch (Exception ex)
      {
        Debug.LogError($"Update package failed: {ex}");
      }

      return false;
    }
  }
#endif
}