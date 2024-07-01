
using UnityEngine;
using UnityEditor;

namespace Yamadev.YamaStream.Script
{
    [CustomPropertyDrawer(typeof(Track))]
    public class TrackDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect rect = new Rect(position.x, position.y + 3, position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(rect, property.FindPropertyRelative(nameof(Track.Mode)));
            rect.y += EditorGUIUtility.singleLineHeight + 1;

            EditorGUI.PropertyField(rect, property.FindPropertyRelative(nameof(Track.Title)));
            rect.y += EditorGUIUtility.singleLineHeight + 1;

            EditorGUI.PropertyField(rect, property.FindPropertyRelative(nameof(Track.Url)));
            rect.y += EditorGUIUtility.singleLineHeight + 1;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(Track.Mode)))
                    + EditorGUIUtility.standardVerticalSpacing
                    + EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(Track.Title)))
                    + EditorGUIUtility.standardVerticalSpacing
                    + EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(Track.Url)));
        }
    }
}