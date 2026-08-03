using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Components;
using Yamadev.YamaStream.UI;

namespace Yamadev.YamaStream.Editor
{
  [CustomEditor(typeof(YamaPlayerSubController))]
  public class YamaPlayerSubControllerEditor : EditorBase
  {
    private YamaPlayerSubController _target;
    private SerializedProperty _yamaPlayer;

    private AppearanceSettings _appearanceSettings;
    private SerializedObject _appearanceSerializedObject;
    private SerializedProperty _defaultColorSet;
    private SerializedProperty _colorSets;

    private LocalizationSettings _localizationSettings;
    private SerializedObject _localizationSerializedObject;
    private SerializedProperty _defaultLanguage;
    private SerializedProperty _languages;

    private UIController _uiController;
    private SerializedObject _uiControllerSerializedObject;
    private SerializedProperty _idleScreenSprite;

    private VRCPickup _vrcPickup;
    private SerializedObject _vrcPickupSerializedObject;
    private SerializedProperty _pickupable;
    private bool _globalSync;

    private void OnEnable()
    {
      _target = target as YamaPlayerSubController;
      _yamaPlayer = serializedObject.FindProperty("YamaPlayer");

      if (Application.isPlaying) return;

      _appearanceSettings = _target.GetComponentInChildren<AppearanceSettings>(true);
      if (_appearanceSettings != null)
      {
        _appearanceSerializedObject = new SerializedObject(_appearanceSettings);
        _defaultColorSet = _appearanceSerializedObject.FindProperty("defaultColorSet");
        _colorSets = _appearanceSerializedObject.FindProperty("colorSets");
      }

      _localizationSettings = _target.GetComponentInChildren<LocalizationSettings>(true);
      if (_localizationSettings != null)
      {
        _localizationSerializedObject = new SerializedObject(_localizationSettings);
        _defaultLanguage = _localizationSerializedObject.FindProperty("defaultLanguage");
        _languages = _localizationSerializedObject.FindProperty("languages");
      }

      _uiController = _target.GetComponentInChildren<UIController>(true);
      if (_uiController != null)
      {
        _uiControllerSerializedObject = new SerializedObject(_uiController);
        _idleScreenSprite = _uiControllerSerializedObject.FindProperty("_idleScreenSprite");
      }

      _vrcPickup = _target.GetComponentInChildren<VRCPickup>(true);
      if (_vrcPickup != null)
      {
        _vrcPickupSerializedObject = new SerializedObject(_vrcPickup);
        _pickupable = _vrcPickupSerializedObject.FindProperty("pickupable");
      }
    }

    public override void OnInspectorGUI()
    {
      base.OnInspectorGUI();
      serializedObject.Update();

      if (Application.isPlaying)
      {
        EditorGUILayout.HelpBox(EditorLocalization.Get("msg.playModeNotAvailable"), MessageType.Info);
        return;
      }

      using (var check = new EditorGUI.ChangeCheckScope())
      {
        EditorGUILayout.PropertyField(_yamaPlayer, EditorLocalization.GetLayout("label.yamaPlayer"));
        if (check.changed)
        {
          serializedObject.ApplyModifiedProperties();
          ProcessYamaPlayerChange();
        }
      }

      EditorGUILayout.Space(SpaceMedium);

      if (_target.YamaPlayer == null)
      {
        EditorGUILayout.HelpBox(EditorLocalization.Get("msg.yamaPlayerRequired"), MessageType.Warning);
      }

      DrawAppearanceSettings();
      DrawLocalizationSettings();
      DrawUISettings();
      DrawPickupSettings();

      ApplyModifiedProperties();
    }

    private void ProcessYamaPlayerChange()
    {
      var yamaPlayer = _yamaPlayer.objectReferenceValue as YamaPlayer;
      if (yamaPlayer == null) return;

      var controller = yamaPlayer.GetComponentInChildren<Controller>(true);
      if (controller == null) return;

      if (_uiController != null)
      {
        _uiControllerSerializedObject.FindProperty("_controller").objectReferenceValue = controller;
        _uiControllerSerializedObject.ApplyModifiedProperties();
      }

      var screens = _target.GetComponentsInChildren<YamaPlayerScreen>(true);
      foreach (var screen in screens)
      {
        var so = new SerializedObject(screen);
        so.FindProperty("controller").objectReferenceValue = controller;
        so.ApplyModifiedProperties();
      }

      var speakers = _target.GetComponentsInChildren<YamaPlayerSpeaker>(true);
      foreach (var speaker in speakers)
      {
        var so = new SerializedObject(speaker);
        so.FindProperty("controller").objectReferenceValue = controller;
        so.ApplyModifiedProperties();
      }
    }

    private void DrawAppearanceSettings()
    {
      if (_appearanceSettings == null) return;

      EditorGUILayout.LabelField(EditorLocalization.Get("appearance.title"), EditorStyles.boldLabel);

      using (new EditorGUILayout.HorizontalScope())
      {
        if (_colorSets != null && _colorSets.arraySize > 0)
        {
          var colorSetNames = new string[_colorSets.arraySize];
          int selectedIndex = 0;

          for (int i = 0; i < _colorSets.arraySize; i++)
          {
            var colorSet = _colorSets.GetArrayElementAtIndex(i);
            var nameProperty = colorSet.FindPropertyRelative("colorSetName");
            colorSetNames[i] = nameProperty != null ? nameProperty.stringValue : $"ColorSet {i}";

            if (_defaultColorSet != null && colorSetNames[i] == _defaultColorSet.stringValue)
            {
              selectedIndex = i;
            }
          }

          using (var check = new EditorGUI.ChangeCheckScope())
          {
            int newIndex = EditorGUILayout.Popup(EditorLocalization.Get("appearance.defaultColorSet"), selectedIndex, colorSetNames);
            if (check.changed && _defaultColorSet != null)
            {
              _defaultColorSet.stringValue = colorSetNames[newIndex];
            }
          }
        }
        else
        {
          EditorGUILayout.LabelField(EditorLocalization.Get("appearance.noColorSets"));
        }

        if (GUILayout.Button(EditorLocalization.Get("button.edit"), GUILayout.Width(60)))
        {
          Selection.activeObject = _appearanceSettings;
        }
      }
    }

    private void DrawLocalizationSettings()
    {
      if (_localizationSettings == null) return;

      using (new EditorGUILayout.HorizontalScope())
      {
        if (_languages != null && _languages.arraySize > 0)
        {
          var optionCodes = new List<string> { "" };
          var optionNames = new List<string> { EditorLocalization.Get("localization.defaultLanguage.auto") };

          for (int i = 0; i < _languages.arraySize; i++)
          {
            var language = _languages.GetArrayElementAtIndex(i);
            var displayNameProperty = language.FindPropertyRelative("displayName");
            var codeProperty = language.FindPropertyRelative("languageCode");
            var code = codeProperty != null ? codeProperty.stringValue : "";
            var displayName = displayNameProperty != null ? displayNameProperty.stringValue : $"Language {i}";
            optionCodes.Add(code);
            optionNames.Add($"{code} - {displayName}");
          }

          int selectedIndex = 0;
          if (!string.IsNullOrEmpty(_defaultLanguage?.stringValue))
          {
            selectedIndex = optionCodes.IndexOf(_defaultLanguage.stringValue);
            if (selectedIndex < 0) selectedIndex = 0;
          }

          using (var check = new EditorGUI.ChangeCheckScope())
          {
            int newIndex = EditorGUILayout.Popup(EditorLocalization.Get("localization.defaultLanguage"), selectedIndex, optionNames.ToArray());
            if (check.changed && _defaultLanguage != null && newIndex >= 0 && newIndex < optionCodes.Count)
            {
              _defaultLanguage.stringValue = optionCodes[newIndex];
            }
          }
        }
        else
        {
          EditorGUILayout.LabelField(EditorLocalization.Get("localization.noLanguages"));
        }

        if (GUILayout.Button(EditorLocalization.Get("button.edit"), GUILayout.Width(60)))
        {
          Selection.activeObject = _localizationSettings;
        }
      }
    }

    private void DrawUISettings()
    {
      if (_uiController == null) return;
      if (_idleScreenSprite != null)
      {
        EditorGUILayout.PropertyField(_idleScreenSprite, EditorLocalization.GetLayout("settings.ui.idleScreenSprite"));
      }
    }

    private void DrawPickupSettings()
    {
      if (_vrcPickup == null || _pickupable == null) return;

      EditorGUILayout.Space(SpaceMedium);
      EditorGUILayout.LabelField(EditorLocalization.Get("settings.pickUp.label"), EditorStyles.boldLabel);

      EditorGUILayout.PropertyField(_pickupable, EditorLocalization.GetLayout("settings.pickUp.label", "settings.pickUp.tooltip"));

      if (_pickupable.boolValue)
      {
        VRCObjectSync objectSync = _vrcPickup.gameObject.GetComponent<VRCObjectSync>();
        _globalSync = objectSync != null;

        using (var check = new EditorGUI.ChangeCheckScope())
        {
          _globalSync = EditorGUILayout.Toggle(EditorLocalization.GetLayout("settings.globalSync.label", "settings.globalSync.tooltip"), _globalSync);

          if (check.changed)
          {
            if (_globalSync && objectSync == null) _vrcPickup.gameObject.AddComponent<VRCObjectSync>();
            if (!_globalSync && objectSync != null) GameObject.DestroyImmediate(objectSync);
          }
        }
      }
    }

    private void ApplyModifiedProperties()
    {
      serializedObject.ApplyModifiedProperties();
      _appearanceSerializedObject?.ApplyModifiedProperties();
      _localizationSerializedObject?.ApplyModifiedProperties();
      _uiControllerSerializedObject?.ApplyModifiedProperties();
      _vrcPickupSerializedObject?.ApplyModifiedProperties();
    }
  }
}
