
using UnityEngine;
using UnityEditor;

namespace Yamadev.YamaStream.Script
{
    [CustomEditor(typeof(PlayListContainer))]
    internal class PlayListHandleEditor : EditorBase
    {

        private void OnEnable()
        {
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("Play List", _uiTitle);
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(string.Join("\n",
                    "このObjectの下のTemplateをコピー、編集してください",
                    "ゲーム内に適用するにはPlayListをActiveにする必要があります"
                    ), MessageType.Info
                );

                PlayList[] children = (target as PlayListContainer).transform.GetComponentsInChildren<PlayList>();
                for (int i = 0; i < children.Length; i++)
                {
                    EditorGUILayout.LabelField($"{i}: {children[i].PlayListName}", _bold);
                    EditorGUI.indentLevel++;
                    foreach (Track tr in children[i].Tracks)
                    {
                        EditorGUILayout.LabelField(tr.Title);
                    }
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                }
            }


            if (serializedObject.ApplyModifiedProperties())
                ApplyModifiedProperties();
        }

        internal void ApplyModifiedProperties()
        {
        }
    }
}