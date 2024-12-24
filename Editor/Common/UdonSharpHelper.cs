using System.Collections.Generic;
using System.Linq;
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
            EditorApplication.delayCall += FixUdonSharpCompileError;
        }

        static readonly string _lastCheckVersionKey = "YamaPlayer_FixUdonCompileErrorLastVersion";
        static readonly string _checkedKeyBase = "YamaPlayer_FixUdonCompileErrorChecked";

        static readonly List<MonoScript> _checkTargets = UdonSharpProgramAsset.GetAllUdonSharpPrograms().Where(asset =>
        {
            string packagePath = VersionManager.PackageInfo.assetPath;
            return AssetDatabase.GetAssetPath(asset).StartsWith(packagePath) && asset.sourceCsScript != null;
        }).Select(asset => asset.sourceCsScript).ToList();

        public static void FixUdonSharpCompileError()
        {
            if (EditorApplication.isPlaying) return;
            string lastCheckVersion = SessionState.GetString(_lastCheckVersionKey, string.Empty);
            if (lastCheckVersion == VersionManager.Version) return;
            foreach (MonoScript target in _checkTargets)
            {
                string targetPath = AssetDatabase.GetAssetPath(target);
                string checkedKey = $"{_checkedKeyBase}_{VersionManager.Version}_{targetPath}";
                if (SessionState.GetBool(checkedKey, false)) continue;
                if (target.GetClass() == null)
                {
                    Debug.Log($"Udon script {target.name} has no class, trying to fix...");
                    AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
                }
                SessionState.SetBool(checkedKey, true);
            }
            SessionState.SetString(_lastCheckVersionKey, VersionManager.Version);
        }
    }
}