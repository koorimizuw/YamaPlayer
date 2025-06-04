using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using Yamadev.YamaStream.UI;

namespace Yamadev.YamaStream.Editor
{
    public class UISettings
    {
        private readonly UIController _uiController;
        private readonly SerializedObject _uiControllerSerializedObject;

        private readonly SerializedProperty _controller;
        private readonly SerializedProperty _font;
        private readonly SerializedProperty _primaryColor;
        private readonly SerializedProperty _secondaryColor;
        private readonly SerializedProperty _idle;
        private readonly SerializedProperty _idleImage;
        private readonly SerializedProperty _defaultOpen;
        private readonly SerializedProperty _disableUIOnPickUp;
        private readonly SerializedProperty _disableUIDistance;

        private readonly VRCUiShape _uiShape;
        private readonly List<ColorPattern> _colorPatterns;

        public bool IsValid => _uiController != null && _uiControllerSerializedObject != null;

        public bool UIDisabled
        {
            get
            {
                if (_uiShape == null) return true;
                return !_uiShape.gameObject.activeSelf;
            }
            set => _uiShape.gameObject.SetActive(!value);
        }

        public UISettings(UIController uiController)
        {
            _uiController = uiController;

            if (_uiController != null)
            {
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

            _uiShape = _uiController.GetComponentInChildren<VRCUiShape>(true);
            _colorPatterns = ColorPatternPresets.GetAllPatterns();
        }

        public void DrawDisableUISettings()
        {
            if (EditorGUILayout.Toggle(Localization.Get("disableUI"), UIDisabled))
            {
                _uiShape.gameObject.SetActive(false);
            }
            else
            {
                _uiShape.gameObject.SetActive(true);
            }
        }

        public void DrawUIDisabledMessage()
        {
            if (!UIDisabled) return;
            EditorGUILayout.HelpBox(Localization.Get("shouldEnableUIFirst"), MessageType.Info);
        }

        public void DrawColorSettings()
        {
            var currentPattern = FindCurrentColorPattern();
            var currentIndex = _colorPatterns.IndexOf(currentPattern);

            if (currentIndex == -1)
            {
                currentIndex = _colorPatterns.Count - 1;
            }

            var patternNames = _colorPatterns.Select(pattern => pattern.Name).ToArray();
            var selectedIndex = EditorGUILayout.Popup(
                label: Localization.GetLayout("colorPattern"),
                selectedIndex: currentIndex,
                displayedOptions: patternNames
            );

            if (selectedIndex != currentIndex && selectedIndex >= 0 && selectedIndex < _colorPatterns.Count)
            {
                ApplyColorPattern(_colorPatterns[selectedIndex]);
            }

            EditorGUILayout.PropertyField(_primaryColor, Localization.GetLayout("primaryColor"));
            EditorGUILayout.PropertyField(_secondaryColor, Localization.GetLayout("secondaryColor"));

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(Localization.Get("preview"), GUILayout.Width(100)))
                {
                    ApplyPreview();
                }
            }
        }

        private ColorPattern FindCurrentColorPattern()
        {
            if (_primaryColor?.colorValue == null || _secondaryColor?.colorValue == null)
                return _colorPatterns.LastOrDefault();

            return ColorPatternPresets.FindBestMatch(_primaryColor.colorValue, _secondaryColor.colorValue);
        }

        private void ApplyColorPattern(ColorPattern pattern)
        {
            if (_primaryColor != null)
                _primaryColor.colorValue = pattern.PrimaryColor;

            if (_secondaryColor != null)
                _secondaryColor.colorValue = pattern.SecondaryColor;
        }

        public void DrawDefaultOpenSettings()
        {
            EditorGUILayout.PropertyField(_defaultOpen, Localization.GetLayout("defaultPlaylistOpen"));
            EditorGUILayout.LabelField("　", Localization.Get("showPlaylistUIAafterStart"));
        }

        public void DrawFontSettings()
        {
            EditorGUILayout.PropertyField(_font, Localization.GetLayout("font"));
        }

        public void DrawIdleImageSettings()
        {
            if (_idle.objectReferenceValue != null)
            {
                EditorGUILayout.PropertyField(_idleImage, Localization.GetLayout("idleImage"));
                EditorGUILayout.LabelField("　", Localization.Get("showIdleImage"));
            }
        }

        public void DrawPickupSettings()
        {
            EditorGUILayout.PropertyField(_disableUIOnPickUp, Localization.GetLayout("disableUIOnPickUp"));
            EditorGUILayout.LabelField("　", Localization.Get("disableUIOnPickUpDesc"));
            EditorGUILayout.PropertyField(_disableUIDistance, Localization.GetLayout("disableUIDistance"));
            EditorGUILayout.LabelField("　", Localization.Get("disableUIDistanceDesc"));
        }

        public void ApplyPreview()
        {
            foreach (UIColor component in _uiController.GetComponentsInChildren<UIColor>(true))
            {
                if (component.GetProgramVariable("_uiController") == null)
                    component.SetProgramVariable("_uiController", _uiController);
                component.Apply();
            }

            if (_font.objectReferenceValue != null)
                foreach (Text text in _uiController.GetComponentsInChildren<Text>(true))
                    text.font = _font.objectReferenceValue as Font;
        }

        public void SetController(Controller controller)
        {
            _controller.objectReferenceValue = controller;
            ApplyModifiedProperties();
        }

        public void ApplyModifiedProperties()
        {
            try
            {
                _uiControllerSerializedObject?.ApplyModifiedProperties();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"UISettings: Failed to apply modified properties - {ex.Message}");
            }
        }
    }
}