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

        static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        static void AddAVProHelper()
        {
            if (Utils.FindType("RenderHeads.Media.AVProVideo.MediaPlayer", true) != null) return;
            UnityEditor.PackageManager.PackageInfo packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(AVProPlayerBuildHelper).Assembly);
            if (packageInfo == null) return;

            foreach (Sample sample in Sample.FindByPackage(packageInfo.name, packageInfo.version))
            {
                if (sample.displayName == "AVProHelper")
                {
                    Debug.Log("Add AVPro helper to avoid spped change not work.");
                    CopyFilesRecursively(sample.resolvedPath, Application.dataPath);
                    return;
                }
            }
        }
    }
}