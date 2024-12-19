
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Yamadev.YamaStream.Editor
{
    internal class TabScope
    {
        public class Tab
        {
            public string title;
            public Action draw;

            public Tab(string title, Action draw)
            {
                this.title = title;
                this.draw = draw;
            }
        }

        List<Tab> _tabs;
        int _current;
        float _spaceSize;

        public TabScope(List<Tab> tabs, float spaceSize = 16f)
        {
            if (tabs == null || tabs.Count == 0)
            {
                throw new ArgumentException("Tab length must rather then 0.");
            }
            _tabs = tabs;
            _current = 0;
            _spaceSize = spaceSize;
        }

        public void Draw()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.Space();
                _current = GUILayout.Toolbar(
                    _current,
                    _tabs.Select(x => Localization.GetLayout(x.title.ToLower())).ToArray(),
                    "LargeButton",
                    GUI.ToolbarButtonSize.Fixed
                    );
                EditorGUILayout.Space();
            }
            EditorGUILayout.Space(_spaceSize);

            _tabs[_current].draw();
        }
    }
}