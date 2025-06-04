using System;
using UnityEditor;

namespace Yamadev.YamaStream.Editor
{
    internal class SectionScope : IDisposable
    {
        public string title;
        public bool drawDivider;

        public SectionScope(string title, bool drawDivider = true)
        {
            this.title = title;
            this.drawDivider = drawDivider;
            Draw();
        }

        public SectionScope(bool drawDivider = true)
        {
            title = string.Empty;
            this.drawDivider = drawDivider;
            Draw();
        }

        public void Dispose()
        {
            if (drawDivider) Styles.DrawDivider();
        }

        public void Draw()
        {
            if (!string.IsNullOrEmpty(title)) EditorGUILayout.LabelField(title, Styles.Bold);
        }
    }
}