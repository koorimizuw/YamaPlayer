using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Text;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using Cysharp.Threading.Tasks;

#if USE_VPM_RESOLVER
using VRC.PackageManagement.Resolver;
using VRC.PackageManagement.Core;
using VRC.PackageManagement.Core.Types.Packages;
using VRC.PackageManagement.Core.Types;
#endif

namespace Yamadev.YamaStream.Editor
{
    [InitializeOnLoad]
    public static class VersionManager
    {
        public const string VPMID = "net.kwxxw.vpm";
        public const string VPMUrl = "https://vpm.kwxxw.net/index.json";
        public static bool HasNewVersion;
        public static string Newest = string.Empty;
        private const string _autoUpdateKey = "YamaPlayer_AutoUpdate";
        private const string _checkBetaKey = "YamaPlayer_CheckBetaUpdate";

        static VersionManager()
        {
            EditorApplication.delayCall += () =>
            {
                if (!EditorApplication.isCompiling && !EditorApplication.isUpdating)
                {
                    CheckUpdate();
                }
            };
        }

        public static bool AutoUpdate
        {
            get => EditorPrefs.GetBool(_autoUpdateKey);
            set => EditorPrefs.SetBool(_autoUpdateKey, value);
        }

        public static bool CheckBetaVersion
        {
            get => EditorPrefs.GetBool(_checkBetaKey);
            set => EditorPrefs.SetBool(_checkBetaKey, value);
        }

        public static PackageInfo PackageInfo => PackageInfo.FindForAssembly(typeof(VersionManager).Assembly);

        public static string PackageName => PackageInfo?.name ?? string.Empty;

        public static string Version => PackageInfo?.version ?? string.Empty;

        public static bool CheckUpdate()
        {
#if USE_VPM_RESOLVER
            try
            {
                MigrateToVCC();

                if (string.IsNullOrEmpty(PackageName) || string.IsNullOrEmpty(Version))
                {
                    Debug.LogWarning("Package name or version is empty. Cannot check for updates.");
                    return false;
                }

                List<string> versions = Resolver.GetAllVersionsOf(PackageName);
                if (versions == null || versions.Count == 0)
                {
                    Debug.LogWarning($"No versions found for package: {PackageName}");
                    return false;
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

                foreach (string version in versions)
                {
                    if (!CheckBetaVersion && version.IndexOf("beta") >= 0) continue;
                    Newest = version;
                    break;
                }

                if (string.IsNullOrEmpty(Newest))
                {
                    Debug.LogWarning("No suitable version found.");
                    return false;
                }

                try
                {
                    HasNewVersion = new SemanticVersioning.Version(Version) < new SemanticVersioning.Version(Newest);
                    return HasNewVersion;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to compare versions: {ex.Message}");
                    return false;
                }
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

        public static void MigrateToVCC()
        {
#if USE_VPM_RESOLVER
            if (Repos.UserRepoExists(VPMID)) return;
            Repos.AddRepo(new Uri(VPMUrl));
#endif
        }

        public static async UniTask UpdatePackage()
        {
#if USE_VPM_RESOLVER
            if (!HasNewVersion || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            {
                Debug.LogWarning("Cannot update package: conditions not met.");
                return;
            }

            try
            {
                IVRCPackage package = Repos.GetPackageWithVersionMatch(PackageName, Newest);
                if (package == null)
                {
                    Debug.LogError($"Package not found: {PackageName} version {Newest}");
                    return;
                }

                Dictionary<string, string> dependencies = new Dictionary<string, string>();
                StringBuilder dialogMsg = new StringBuilder();
                List<string> affectedPackages = Resolver.GetAffectedPackageList(package);

                for (int v = 0; v < affectedPackages.Count; v++)
                    dialogMsg.Append(affectedPackages[v]);

                if (affectedPackages.Count > 1)
                {
                    dialogMsg.Insert(0, "This will update multiple packages:\n\n");
                    dialogMsg.AppendLine("\nAre you sure?");
                    if (!EditorUtility.DisplayDialog("Update YamaPlayer", dialogMsg.ToString(), "OK", "Cancel"))
                        return;
                }

                await UniTask.Delay(500);
                Resolver.ForceRefresh();

                new UnityProject(Resolver.ProjectDir).UpdateVPMPackage(package);
                Debug.Log($"Update YamaPlayer to version: {Newest}");

                Resolver.ForceRefresh();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Update YamaPlayer failed: {ex}");
            }
#endif
        }
    }
}