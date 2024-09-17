using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using Yamadev.YamaStream.UI;

namespace Yamadev.YamaStream.Script
{
    public class ColorPattern
    {
        public string Name;
        public Color PrimaryColor;
        public Color SecondaryColor;
    }

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
        SerializedProperty _disableUIDistance;
        bool _disableUI;

        public UIEditor(UIController uiController)
        {
            _uiController = uiController;
            _uiControllerSerializedObject = new SerializedObject(uiController);
            _controller = _uiControllerSerializedObject.FindProperty("_controller");
            _font = _uiControllerSerializedObject.FindProperty("_font");
            _primaryColor = _uiControllerSerializedObject.FindProperty("_primaryColor");
            _secondaryColor = _uiControllerSerializedObject.FindProperty("_secondaryColor");
            _idle = _uiControllerSerializedObject.FindProperty("_idle");
            _idleImage = _uiControllerSerializedObject.FindProperty("_idleImage");
            _defaultOpen = _uiControllerSerializedObject.FindProperty("_defaultPlaylistOpen");
            _disableUIOnPickUp = _uiControllerSerializedObject.FindProperty("_disableUIOnPickUp");
            _disableUIDistance = _uiControllerSerializedObject.FindProperty("_disableUIDistance");
        }

        public void DrawUISettings()
        {
            if (_uiControllerSerializedObject == null) return;
            _uiControllerSerializedObject.Update();

            VRCUiShape uiSharp = _uiController.GetComponentInChildren<VRCUiShape>(true);
            if (uiSharp == null) return;

            _disableUI = !uiSharp.gameObject.activeSelf;
            _disableUI = EditorGUILayout.Toggle(Localization.Get("disableUI"), _disableUI);
            uiSharp.gameObject.SetActive(!_disableUI);
            if (_disableUI)
            {
                EditorGUILayout.Space(12f);
                EditorGUILayout.HelpBox(Localization.Get("shouldEnableUIFirst"), MessageType.Info);
                return;
            }
            Styles.DrawDivider();

            List<ColorPattern> colorPatterns = new List<ColorPattern>()
            {
                new ColorPattern{ Name = Localization.Get("pinkColor"), PrimaryColor = new Color(0.9372549f, 0.3843137f, 0.5686275f), SecondaryColor = new Color(0.9686275f, 0.7294118f, 0.8117647f, 0.1215686f) },
                new ColorPattern{ Name = Localization.Get("blueColor"), PrimaryColor = new Color(0.01176471f, 0.6627451f, 0.9568627f), SecondaryColor = new Color(0.5058824f, 0.8313726f, 0.9803922f, 0.1215686f) },
                new ColorPattern{ Name = Localization.Get("greenColor"), PrimaryColor = new Color(0.2980392f, 0.6862745f, 0.3137255f), SecondaryColor = new Color(0.6470588f, 0.8392157f, 0.654902f, 0.1215686f) },
                new ColorPattern{ Name = Localization.Get("orangeColor"), PrimaryColor = new Color(1f, 0.5960785f, 0f), SecondaryColor = new Color(1f, 0.8f, 0.5019608f, 0.1215686f) },
                new ColorPattern{ Name = Localization.Get("purpleColor"), PrimaryColor = new Color(0.6117647f, 0.1529412f, 0.6901961f), SecondaryColor = new Color(0.8078431f, 0.5764706f, 0.8470588f, 0.1215686f) },
                new ColorPattern{ Name = Localization.Get("customColor"), PrimaryColor = new Color(1f, 1f, 1f), SecondaryColor = new Color(1f, 1f, 1f, 0.1215686f) },
            };

            int colorPatternIndex = colorPatterns.Count - 1;
            for (int i = 0; i < colorPatterns.Count; i++)
            {
                if (colorPatterns[i].PrimaryColor == _primaryColor.colorValue &&
                    colorPatterns[i].SecondaryColor == _secondaryColor.colorValue)
                {
                    colorPatternIndex = i;
                    break;
                }
            }

            int selectedColorIndex = EditorGUILayout.Popup(
                label: Localization.GetLayout("colorPattern"),
                selectedIndex: colorPatternIndex,
                displayedOptions: colorPatterns.Select(i => i.Name).ToArray()
            );
            if (selectedColorIndex != colorPatternIndex)
            {
                _primaryColor.colorValue = colorPatterns[selectedColorIndex].PrimaryColor;
                _secondaryColor.colorValue = colorPatterns[selectedColorIndex].SecondaryColor;
            }

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
            EditorGUILayout.LabelField("　", Localization.Get("showPlaylistUIAafterStart"));
            Styles.DrawDivider();

            if (_idle.objectReferenceValue != null)
            {
                EditorGUILayout.PropertyField(_idleImage, Localization.GetLayout("idleImage"));
                EditorGUILayout.LabelField("　", Localization.Get("showIdleImage"));
                Styles.DrawDivider();
            }

            EditorGUILayout.PropertyField(_disableUIOnPickUp, Localization.GetLayout("disableUIOnPickUp"));
            EditorGUILayout.LabelField("　", Localization.Get("disableUIOnPickUpDesc"));

            EditorGUILayout.PropertyField(_disableUIDistance, Localization.GetLayout("disableUIDistance"));
            EditorGUILayout.LabelField("　", Localization.Get("disableUIDistanceDesc"));


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