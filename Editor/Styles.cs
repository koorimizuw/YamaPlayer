using UnityEditor;
using UnityEngine;

namespace Yamadev.YamaStream.Script
{
    internal static class Styles
    {
        public static GUIStyle Bold => new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
        public static GUIStyle SectionName => new GUIStyle(GUI.skin.label) { 
            fontStyle = FontStyle.Bold,
            fontSize = 16,
        };

        public static GUIStyle Title
        {
            get
            {
                var style = new GUIStyle()
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 26,
                };
                style.normal.textColor = Color.white;
                return style;
            }
        }

        public static void DrawDivider()
        {
            EditorGUILayout.Space(12f);
            DrawLine(new Color(0.5f, 0.5f, 0.5f, 1));
            EditorGUILayout.Space(12f);
        }

        public static void DrawLine(Color color, float height = 0.5f)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, height);
            rect.height = height;
            EditorGUI.DrawRect(rect, color);
        }
    }
}