using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;

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
        public static string PackageName;
        public static string Version;
        public static bool HasNewVersion;
        public static string Newest;
        const string _autoUpdateKey = "YamaPlayer_AutoUpdate";
        const string _checkBetaKey = "YamaPlayer_CheckBetaUpdate";

        static VersionManager()
        {
            CheckUpdate();
            if (HasNewVersion && AutoUpdate) UpdatePackage();
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

        public static UnityEditor.PackageManager.PackageInfo PackageInfo =>
            UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(VersionManager).Assembly);

        public static void GetVersionInfo()
        {
            if (PackageInfo == null) return;

            PackageName = PackageInfo.name;
            Version = PackageInfo.version;
        }

        public static bool CheckUpdate()
        {
#if USE_VPM_RESOLVER
            MigrateToVCC();
            GetVersionInfo();
            List<string> versions = Resolver.GetAllVersionsOf(PackageName);
            foreach (string version in versions)
            {
                if (!CheckBetaVersion && version.IndexOf("beta") >= 0) continue;
                Newest = version;
                break;
            }
            if (string.IsNullOrEmpty(Newest)) return false;
            HasNewVersion = new SemanticVersioning.Version(Version) < new SemanticVersioning.Version(Newest);
            return HasNewVersion;
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

        public static async void UpdatePackage()
        {
#if USE_VPM_RESOLVER
            if (!HasNewVersion || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            IVRCPackage package = Repos.GetPackageWithVersionMatch(PackageName, Newest);
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

            await Task.Delay(500);
            Resolver.ForceRefresh();
            try
            {
                new UnityProject(Resolver.ProjectDir).UpdateVPMPackage(package);
                Debug.Log($"Update YamaPlayer to version: {Newest}");
            }
            catch (Exception ex) { 
                Debug.LogError($"Update YamaPlayer failed: {ex}"); 
            }
            Resolver.ForceRefresh();
#endif
        }
    }
}