using UnityEditor;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Yamadev.YamaStream.Editor
{
    public class VersionSettings
    {
#if USE_VPM_RESOLVER
        private bool _vpmResolverImported = true;
#else
        private bool _vpmResolverImported = false;
#endif

        public bool IsValid => _vpmResolverImported;

        private void DrawInvalidMessage()
        {
            EditorGUILayout.LabelField(Localization.Get("onlyWorksOnVCCProject"));
        }

        public void DrawAutoUpdateSettings()
        {
            if (!_vpmResolverImported) return;
#if USE_VPM_RESOLVER
            VersionManager.AutoUpdate = EditorGUILayout.Toggle(Localization.Get("autoUpdate"), VersionManager.AutoUpdate);
            EditorGUILayout.LabelField("Å@", Localization.Get("autoUpdateToLatestVersion"));
#endif
        }

        public void DrawVersionSettings()
        {
            if (!_vpmResolverImported) return;
#if USE_VPM_RESOLVER
            VersionManager.CheckBetaVersion = EditorGUILayout.Toggle(Localization.Get("checkBetaVersion"), VersionManager.CheckBetaVersion);
            EditorGUILayout.LabelField(Localization.Get("currentVersion"), VersionManager.Version);
            EditorGUILayout.LabelField(Localization.Get("newestVersion"), VersionManager.Newest);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(Localization.Get("checkUpdate")))
                {
                    if (VersionManager.CheckUpdate())
                    {
                        if (EditorUtility.DisplayDialog(
                            Localization.Get("newVersionFound"),
                            string.Format(Localization.Get("newVersionUpdateConfirm"), VersionManager.Newest),
                            Localization.Get("doUpdate"),
                            Localization.Get("cancel"))
                            )
                            VersionManager.UpdatePackage().Forget();
                    }
                    else EditorUtility.DisplayDialog(Localization.Get("noNewVersionFound"), Localization.Get("youUseNewest"), "OK");
                }

                EditorGUI.BeginDisabledGroup(!VersionManager.HasNewVersion);
                if (GUILayout.Button(Localization.Get("update")))
                    VersionManager.UpdatePackage().Forget();
                EditorGUI.EndDisabledGroup();
            }
#endif
        }
    }
}