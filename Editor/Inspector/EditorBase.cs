
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Yamadev.YamaStream.Editor
{
    public abstract class EditorBase : UnityEditor.Editor
    {
        public string Title;
        static string _logoGuid = "45177375d4933bc469e82e59e57ce065";
        static float _marginTop = 16f;

        public override void OnInspectorGUI()
        {
            DrawLogoAndVersion(_marginTop);
            EditorGUILayout.Space(12f);
            DrawLanguageSelector();
            EditorGUILayout.Space(8f);
        }

        public void DrawLanguageSelector()
        {
            string[] languages = Localization.AvailableLanguages.Select(i => Localization.GetLanguageName(i)).ToArray();
            int index = Array.IndexOf(Localization.AvailableLanguages, Localization.CurrentLanguage);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.Space();
                int selected = EditorGUILayout.Popup(index, languages, GUILayout.Width(200));
                Localization.CurrentLanguage = Localization.AvailableLanguages[selected];
                EditorGUILayout.Space();
            }
        }

        public void DrawLogoAndVersion(float marginTop)
        {
            Texture2D logo = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(_logoGuid));
            if (logo == null) return;

            Rect rect = new Rect { height = 60f };
            rect.width = rect.height * logo.width / logo.height;
            rect.x = (EditorGUIUtility.currentViewWidth - rect.width) / 2f;
            rect.y = marginTop;
            GUI.DrawTexture(rect, logo);

            GUIContent version = new GUIContent($"v{VersionManager.PackageInfo.version}");
            Vector2 size = Styles.Bold.CalcSize(version);
            GUI.Label(new Rect(rect.xMax, rect.yMax - size.y, size.x, size.y), version, Styles.Bold);

            EditorGUILayout.Space(marginTop + rect.height);
        }
    }
}