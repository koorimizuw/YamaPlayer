using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Yamadev.YamaStream.UI;

namespace Yamadev.YamaStream.Script
{
    public class UIEditor
    {
        UIController _uiController;
        SerializedObject _uiControllerSerializedObject;
        SerializedProperty _controller;
        SerializedProperty _font;
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
            _font = _uiControllerSerializedObject.FindProperty("_font");
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

            EditorGUILayout.PropertyField(_primaryColor, Localization.GetLayout("primaryColor"));
            EditorGUILayout.PropertyField(_secondaryColor, Localization.GetLayout("secondaryColor"));
            EditorGUILayout.PropertyField(_font, Localization.GetLayout("font"));
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(Localization.Get("preview"))) Preview();
            }
            Styles.DrawDivider();

            EditorGUILayout.PropertyField(_defaultOpen, Localization.GetLayout("defaultPlaylistOpen"));
            EditorGUILayout.LabelField("Å@", Localization.Get("showPlaylistUIAafterStart"));
            Styles.DrawDivider();

            if (_idle.objectReferenceValue != null)
            {
                EditorGUILayout.PropertyField(_idleImage, Localization.GetLayout("idleImage"));
                EditorGUILayout.LabelField("Å@", Localization.Get("showIdleImage"));
                Styles.DrawDivider();
            }

            EditorGUILayout.PropertyField(_disableUIOnPickUp, Localization.GetLayout("disableUIOnPickUp"));
            EditorGUILayout.LabelField("Å@", Localization.Get("disableUIOnPickUpDesc"));

            ApplyModifiedProperties();
        }

        public void Preview()
        {
            foreach (UIColor component in _uiController.GetComponentsInChildren<UIColor>(true))
            {
                if (component.GetProgramVariable("_uiController") == null)
                    component.SetProgramVariable("_uiController", _uiController);
                component.Apply();
            }

            if (_font.objectReferenceValue != null) 
                foreach (Text text in _uiController.GetComponentsInChildren<Text>(true)) text.font = _font.objectReferenceValue as Font;
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