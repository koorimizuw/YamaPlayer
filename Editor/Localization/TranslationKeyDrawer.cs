using UnityEditor;
using UnityEngine;

namespace Yamadev.YamaStream.Editor
{
    [CustomPropertyDrawer(typeof(TranslationKeyAttribute))]
    public class TranslationKeyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = (TranslationKeyAttribute)attribute;
            var translatedLabel = EditorLocalization.Get(attr.Key);
            EditorGUI.PropertyField(position, property, new GUIContent(translatedLabel, label.tooltip));
        }
    }
}
