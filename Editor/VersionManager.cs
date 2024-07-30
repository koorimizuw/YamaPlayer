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

namespace Yamadev.YamaStream.Script
{
    [InitializeOnLoad]
    public static class VersionManager
    {
        public const string VPMID = "net.kwxxw.vpm";
        public const string VPMUrl = "https://vpm.kwxxw.net/index.json";
        public static string PackageName;
        public static string Version;
        public static bool HasNewVersion = false;
        public static string Newest;
        public static bool AutoUpdate = false;

        static VersionManager()
        {
            CheckUpdate();
            if (HasNewVersion && AutoUpdate) UpdatePackage();
        }

        public static void GetVersionInfo()
        {
            UnityEditor.PackageManager.PackageInfo packageInfo = Utils.GetYamaPlayerPackageInfo();
            if (packageInfo == null) return;

            PackageName = packageInfo.name;
            Version = packageInfo.version;
        }

        public static bool CheckUpdate()
        {
#if USE_VPM_RESOLVER
            MigrateToVCC();
            GetVersionInfo();
            List<string> versions = Resolver.GetAllVersionsOf(PackageName);
            foreach (string version in versions)
            {
                if (version.IndexOf("beta") >= 0) continue;
                Newest = version;
                break;
            }
            if (string.IsNullOrEmpty(Newest) ||
                new SemanticVersioning.Version(Version) >= new SemanticVersioning.Version(Newest)) 
                return false;
            HasNewVersion = true;
            return true;
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
            if (!HasNewVersion) return;
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
                if (!EditorUtility.DisplayDialog("Package Has Dependencies", dialogMsg.ToString(), "OK", "Cancel"))
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
                Debug.LogError($"Update YamaPlayer faild: {ex}"); 
            }
            Resolver.ForceRefresh();
#endif
        }
    }
}