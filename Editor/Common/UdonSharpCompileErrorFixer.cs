using System;
using System.Collections.Generic;
using System.Linq;
using UdonSharp;
using UnityEditor;
using UnityEngine;
using UnityEditor.Callbacks;

namespace Yamadev.YamaStream.Editor
{
    public static class UdonSharpCompileErrorFixer
    {
        private const string LastCheckVersionKey = "YamaPlayer_FixUdonCompileErrorLastVersion";
        private const string CheckedKeyBase = "YamaPlayer_FixUdonCompileErrorChecked";
        private static List<MonoScript> _cachedCheckTargets;
        private static string _cachedPackagePath;

        [DidReloadScripts]
        public static void FixUdonSharpCompileError()
        {
            if (ShouldSkipExecution()) return;

            try
            {
                ProcessUdonSharpPrograms();
                SessionState.SetString(LastCheckVersionKey, VersionManager.Version);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to fix compile errors: {ex.Message}");
            }
        }

        private static bool ShouldSkipExecution()
        {
            if (EditorApplication.isPlaying)
            {
                return true;
            }

            string lastCheckVersion = SessionState.GetString(LastCheckVersionKey, string.Empty);
            return lastCheckVersion == VersionManager.Version;
        }

        private static void ProcessUdonSharpPrograms()
        {
            var checkTargets = GetCheckTargets();
            if (checkTargets == null || checkTargets.Count == 0) return;

            foreach (var target in checkTargets)
            {
                if (target == null) continue;
                string targetPath = AssetDatabase.GetAssetPath(target);
                if (string.IsNullOrEmpty(targetPath)) continue;
                string checkedKey = $"{CheckedKeyBase}_{VersionManager.Version}_{targetPath}";
                if (SessionState.GetBool(checkedKey, false)) continue;

                TryFixScript(target, targetPath);
                SessionState.SetBool(checkedKey, true);
            }
        }

        private static List<MonoScript> GetCheckTargets()
        {
            try
            {
                string currentPackagePath = VersionManager.PackageInfo?.assetPath;
                if (string.IsNullOrEmpty(currentPackagePath))
                {
                    Debug.LogWarning("[UdonSharpCompileErrorFixer] Package path is null or empty.");
                    return new List<MonoScript>();
                }

                if (_cachedCheckTargets != null && _cachedPackagePath == currentPackagePath)
                {
                    return _cachedCheckTargets;
                }

                _cachedPackagePath = currentPackagePath;
                _cachedCheckTargets = BuildCheckTargetsList(currentPackagePath);

                return _cachedCheckTargets;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UdonSharpCompileErrorFixer] Failed to get check targets: {ex.Message}");
                return new List<MonoScript>();
            }
        }

        private static List<MonoScript> BuildCheckTargetsList(string packagePath)
        {
            var allPrograms = UdonSharpProgramAsset.GetAllUdonSharpPrograms();
            if (allPrograms == null)
            {
                return new List<MonoScript>();
            }

            return allPrograms
                .Where(asset => IsValidAsset(asset, packagePath))
                .Select(asset => asset.sourceCsScript)
                .ToList();
        }

        private static bool IsValidAsset(UdonSharpProgramAsset asset, string packagePath)
        {
            if (asset == null || asset.sourceCsScript == null)
            {
                return false;
            }

            string assetPath = AssetDatabase.GetAssetPath(asset);
            return !string.IsNullOrEmpty(assetPath) && assetPath.StartsWith(packagePath);
        }

        private static bool TryFixScript(MonoScript target, string targetPath)
        {
            if (target.GetClass() == null)
            {
                Debug.Log($"Fixing UdonSharp script: {target.name}");
                AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
                return true;
            }

            return false;
        }
    }
}