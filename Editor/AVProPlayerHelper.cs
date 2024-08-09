using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace Yamadev.YamaStream.Script
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
        private static readonly string _avProReleaseUrl = "https://github.com/RenderHeads/UnityPlugin-AVProVideo/releases/latest";

        static AVProPlayerHelper() => AddAVProHelper();

        public static void AddAVProHelper()
        {
            if (Utils.FindType("RenderHeads.Media.AVProVideo.MediaPlayer", true) != null) return;
            UnityEditor.PackageManager.PackageInfo packageInfo = Utils.GetYamaPlayerPackageInfo();
            if (packageInfo == null) return;

            foreach (Sample sample in Sample.FindByPackage(packageInfo.name, packageInfo.version))
            {
                if (sample.displayName == "AVProHelper")
                {
                    Debug.Log("Add AVPro helper to project.");
                    Utils.CopyFilesRecursively(sample.resolvedPath, Application.dataPath);
                    return;
                }
            }
        }

        [MenuItem("YamaPlayer/Enable AVPro Debug")]
        public static void AVProDebug()
        {
            if (Utils.FindType("RenderHeads.Media.AVProVideo.MediaPlayer", true) == null)
            {
                if (EditorUtility.DisplayDialog("AVPro not included", "Open download AVPro site?", "Yes", "No")) DownloadAVPro();
                return;
            }
            foreach (var group in _targetGroups)
            {
                List<string> symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').Select(s => s.Trim()).ToList();
                if (!symbols.Contains(_avProDebugSymbol))
                {
                    symbols.Insert(0, _avProDebugSymbol);
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", symbols.ToArray()));
                }
            }
            EditorUtility.DisplayDialog("Success", "AVPro debug on.", "OK");
        }

        public static void DownloadAVPro()
        {
            Application.OpenURL(_avProReleaseUrl);
        }
    }
}