using UdonSharp;
using UnityEditor;
using UnityEngine;

namespace Yamadev.YamaStream.Editor
{
    [InitializeOnLoad]
    public static class UdonSharpHelper
    {
        static UdonSharpHelper()
        {
            FixUdonSharpCompileError();
        }

        public static void FixUdonSharpCompileError()
        {
            string path = VersionManager.PackageInfo.assetPath;
            UdonSharpProgramAsset[] assets = UdonSharpProgramAsset.GetAllUdonSharpPrograms();
            foreach (UdonSharpProgramAsset asset in assets)
            {
                if (AssetDatabase.GetAssetPath(asset).IndexOf(path) != 0) continue;
                MonoScript script = asset.sourceCsScript;
                if (script.GetClass() == null)
                {
                    Debug.Log($"Udon script {script.name} has no class, trying to fix...");
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(script), ImportAssetOptions.ForceUpdate);
                }
            }
        }
    }
}