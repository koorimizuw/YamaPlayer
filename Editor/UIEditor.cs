using UnityEditor;
using UnityEngine;
using Yamadev.YamaStream.UI;

namespace Yamadev.YamaStream.Script
{
    public class UIEditor
    {
        UIController _uiController;
        SerializedObject _uiControllerSerializedObject;
        SerializedProperty _controller;
        SerializedProperty _primaryColor;
        SerializedProperty _secondaryColor;
        SerializedProperty _idle;
        SerializedProperty _idleImage;
        SerializedProperty _defaultOpen;
        SerializedProperty _disableUIOnPickUp;

        public UIEditor(UIController uiController)
        {
            _uiController = uiController;
            _uiControllerSerializedObject = new SerializedObject(uiController);
            _controller = _uiControllerSerializedObject.FindProperty("_controller");
            _primaryColor = _uiControllerSerializedObject.FindProperty("_primaryColor");
            _primaryColor = _uiControllerSerializedObject.FindProperty("_primaryColor");
            _secondaryColor = _uiControllerSerializedObject.FindProperty("_secondaryColor");
            _idle = _uiControllerSerializedObject.FindProperty("_idle");
            _idleImage = _uiControllerSerializedObject.FindProperty("_idleImage");
            _defaultOpen = _uiControllerSerializedObject.FindProperty("_defaultPlaylistOpen");
            _disableUIOnPickUp = _uiControllerSerializedObject.FindProperty("_disableUIOnPickUp");
        }

        public void DrawUISettings()
        {
            if (_uiControllerSerializedObject == null) return;
            _uiControllerSerializedObject.Update();

            EditorGUILayout.PropertyField(_primaryColor);
            EditorGUILayout.PropertyField(_secondaryColor);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Preview")) SetUIColor();
            }
            Styles.DrawDivider();

            EditorGUILayout.PropertyField(_defaultOpen);
            EditorGUILayout.LabelField("Å@", "Show playlist UI after game started.");
            Styles.DrawDivider();

            if (_idle.objectReferenceValue != null)
            {
                EditorGUILayout.PropertyField(_idleImage);
                EditorGUILayout.LabelField("Å@", "Show image when video not playing.");
                Styles.DrawDivider();
            }

            EditorGUILayout.PropertyField(_disableUIOnPickUp);
            EditorGUILayout.LabelField("Å@", "Disable video player UI when user is picking up something.");

            ApplyModifiedProperties();
        }

        void SetUIColor()
        {
            foreach (UIColor component in _uiController.GetComponentsInChildren<UIColor>(true))
            {
                if (component.GetProgramVariable("_uiController") == null)
                    component.SetProgramVariable("_uiController", _uiController);
                component.Apply();
            }
        }


        public void SetController(Controller controller)
        {
            _controller.objectReferenceValue = controller;
            ApplyModifiedProperties();
        }

        public void ApplyModifiedProperties()
        {
            _uiControllerSerializedObject?.ApplyModifiedProperties();
        }
    }
}