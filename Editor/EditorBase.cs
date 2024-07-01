
using System.Linq;
using UnityEditor;
using UnityEngine;
using Yamadev.YamaStream.Script;

namespace Yamadev.YamaStream
{
    public abstract class EditorBase : Editor
    {
        protected GUIStyle _uiTitle;
        protected GUIStyle _bold;
        protected virtual void Initilize()
        {
            _uiTitle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
            };
            _uiTitle.normal.textColor = Color.white;

            _bold = new GUIStyle(GUI.skin.label);
            _bold.fontStyle = FontStyle.Bold;
        }

        public override void OnInspectorGUI()
        {
            Initilize();
        }
    }
}