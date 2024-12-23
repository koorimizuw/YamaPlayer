using UdonSharp;
using UnityEditor;

namespace Yamadev.YamaStream.Editor
{
    [InitializeOnLoad]
    public static class UdonSharpHelper
    {
        static UdonSharpHelper()
        {
            string path = VersionManager.PackageInfo.assetPath;
            UdonSharpProgramAsset[] assets = UdonSharpProgramAsset.GetAllUdonSharpPrograms();
            foreach (UdonSharpProgramAsset asset in assets)
            {
                if (AssetDatabase.GetAssetPath(asset).IndexOf(path) != 0) continue;
                MonoScript script = asset.sourceCsScript;
                if (script.GetClass() == null) 
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(script), ImportAssetOptions.ForceUpdate);
            }
        }
    }
}