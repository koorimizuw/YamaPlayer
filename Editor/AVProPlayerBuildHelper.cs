using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace Yamadev.YamaStream.Script
{
    [InitializeOnLoad]
    public static class AVProPlayerBuildHelper
    {
        static AVProPlayerBuildHelper()
        {
            AddAVProHelper();
        }

        static void AddAVProHelper()
        {
            if (Utils.FindType("RenderHeads.Media.AVProVideo.MediaPlayer", true) != null) return;
            UnityEditor.PackageManager.PackageInfo packageInfo = Utils.GetYamaPlayerPackageInfo();
            if (packageInfo == null) return;

            foreach (Sample sample in Sample.FindByPackage(packageInfo.name, packageInfo.version))
            {
                if (sample.displayName == "AVProHelper")
                {
                    Debug.Log("Add AVPro helper to avoid spped change not work.");
                    Utils.CopyFilesRecursively(sample.resolvedPath, Application.dataPath);
                    return;
                }
            }
        }
    }
}