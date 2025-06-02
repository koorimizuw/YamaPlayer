using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Yamadev.YamaStream.Editor
{
    internal class FoldableScope : IDisposable
    {
        private readonly string _title;
        private readonly bool _drawDivider;
        private bool _isExpanded;
        private static GUIStyle _foldoutStyle;
        private static GUIStyle _headerStyle;

        private static Dictionary<string, bool> _expandedStates = new Dictionary<string, bool>();

        public bool IsExpanded => _isExpanded;

        private static void InitializeStyles()
        {
            if (_foldoutStyle == null)
            {
                _foldoutStyle = new GUIStyle(EditorStyles.foldout)
                {
                    fontStyle = FontStyle.Bold,
                    fontSize = 12,
                    margin = new RectOffset(4, 4, 4, 4),
                    padding = new RectOffset(16, 4, 4, 4)
                };
                _foldoutStyle.normal.textColor = EditorGUIUtility.isProSkin
                    ? new Color(0.8f, 0.8f, 0.8f)
                    : new Color(0.2f, 0.2f, 0.2f);
            }

            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle()
                {
                    padding = new RectOffset(0, 0, 2, 2),
                    margin = new RectOffset(0, 0, 2, 2)
                };
            }
        }

        public FoldableScope(string title, bool defaultExpanded = false, bool drawDivider = true)
        {
            InitializeStyles();

            _title = title;
            _drawDivider = drawDivider;

            if (_expandedStates.ContainsKey(_title))
            {
                _isExpanded = _expandedStates[_title];
            }
            else
            {
                _isExpanded = defaultExpanded;
                _expandedStates[_title] = _isExpanded;
            }

            DrawHeader();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(2f);

            using (new EditorGUILayout.HorizontalScope(_headerStyle))
            {
                var rect = EditorGUILayout.GetControlRect(GUILayout.Height(20));
                if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
                {
                    _isExpanded = !_isExpanded;
                    _expandedStates[_title] = _isExpanded;
                    Event.current.Use();
                }

                DrawCustomFoldout(rect, _isExpanded, _title);
            }

            if (_isExpanded && _drawDivider)
            {
                DrawSeparator();
            }
        }

        private void DrawCustomFoldout(Rect rect, bool expanded, string title)
        {
            var iconRect = new Rect(rect.x + 3, rect.y + 2, 16, 16);
            var titleRect = new Rect(rect.x + 20, rect.y, rect.width - 20, rect.height);

            var isHover = rect.Contains(Event.current.mousePosition);
            if (isHover && Event.current.type == EventType.Repaint)
            {
                var hoverColor = EditorGUIUtility.isProSkin
                    ? new Color(1f, 1f, 1f, 0.1f)
                    : new Color(0f, 0f, 0f, 0.1f);
                EditorGUI.DrawRect(rect, hoverColor);
            }

            var iconStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleCenter
            };

            var iconText = expanded ? "▼" : "▶";
            var iconColor = isHover
                ? (EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.9f) : new Color(0.2f, 0.2f, 0.2f, 0.9f))
                : (EditorGUIUtility.isProSkin ? new Color(0.7f, 0.7f, 0.7f, 0.8f) : new Color(0.4f, 0.4f, 0.4f, 0.8f));

            var originalColor = GUI.color;
            GUI.color = iconColor;
            GUI.Label(iconRect, iconText, iconStyle);
            GUI.color = originalColor;

            var titleStyle = new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            titleStyle.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(0.9f, 0.9f, 0.9f)
                : new Color(0.1f, 0.1f, 0.1f);

            GUI.Label(titleRect, title, titleStyle);
        }

        private void DrawSeparator()
        {
            EditorGUILayout.Space(2f);
            var rect = EditorGUILayout.GetControlRect(GUILayout.Height(1));
            var color = EditorGUIUtility.isProSkin
                ? new Color(0.3f, 0.3f, 0.3f, 0.8f)
                : new Color(0.6f, 0.6f, 0.6f, 0.8f);
            EditorGUI.DrawRect(rect, color);
            EditorGUILayout.Space(4f);
        }

        public void Dispose()
        {
            if (_isExpanded && _drawDivider)
            {
                EditorGUILayout.Space(2f);
                Styles.DrawDivider();
            }
        }

        /// <summary>
        /// 状態辞書をクリア（テスト用やメモリ節約用）
        /// </summary>
        public static void ClearStates()
        {
            _expandedStates.Clear();
        }
    }
}
