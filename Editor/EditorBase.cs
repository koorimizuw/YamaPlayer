
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Yamadev.YamaStream.Script
{
    public abstract class EditorBase : Editor
    {
        public string Title;

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space(16f);
            EditorGUILayout.LabelField($"{Title}", Styles.Title);
            EditorGUILayout.Space(16f);
            DrawLanguageSelector();
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
    }
}