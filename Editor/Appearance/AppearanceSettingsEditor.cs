using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Yamadev.YamaStream.Editor
{
  [CustomEditor(typeof(AppearanceSettings))]
  public class AppearanceSettingsEditor : EditorBase
  {
    private SerializedProperty _defaultColorSet;
    private SerializedProperty _colorSets;

    private Dictionary<int, bool> _foldouts = new Dictionary<int, bool>();
    private int _pendingDeleteIndex = -1;

    private void OnEnable()
    {
      Title = EditorLocalization.Get("appearance.title");
      _defaultColorSet = serializedObject.FindProperty("defaultColorSet");
      _colorSets = serializedObject.FindProperty("colorSets");
    }

    public override void OnInspectorGUI()
    {
      base.OnInspectorGUI();

      Title = EditorLocalization.Get("appearance.title");
      serializedObject.Update();

      DrawDefaultColorSetSection();
      EditorGUILayout.Space(SpaceLarge);

      DrawColorSetListSection();
      EditorGUILayout.Space(SpaceMedium);

      serializedObject.ApplyModifiedProperties();
    }

    private void DrawDefaultColorSetSection()
    {
      EditorGUILayout.LabelField(EditorLocalization.Get("appearance.defaultColorSet"), EditorStyles.boldLabel);

      var optionNames = new List<string> { "" };
      var optionDisplayNames = new List<string> { EditorLocalization.Get("appearance.defaultColorSet.auto") };

      for (int i = 0; i < _colorSets.arraySize; i++)
      {
        var colorSet = _colorSets.GetArrayElementAtIndex(i);
        var name = colorSet.FindPropertyRelative("colorSetName").stringValue;
        optionNames.Add(name);
        optionDisplayNames.Add(string.IsNullOrEmpty(name) ? $"ColorSet {i + 1}" : name);
      }

      int currentIndex = 0;
      if (!string.IsNullOrEmpty(_defaultColorSet.stringValue))
      {
        currentIndex = optionNames.IndexOf(_defaultColorSet.stringValue);
        if (currentIndex < 0) currentIndex = 0;
      }

      int newIndex = EditorGUILayout.Popup(
        EditorLocalization.Get("appearance.defaultColorSet.label"),
        currentIndex,
        optionDisplayNames.ToArray());
      if (newIndex >= 0 && newIndex < optionNames.Count)
      {
        _defaultColorSet.stringValue = optionNames[newIndex];
      }
    }

    private void DrawColorSetListSection()
    {
      using (new EditorGUILayout.HorizontalScope())
      {
        EditorGUILayout.LabelField(
          $"{EditorLocalization.Get("appearance.colorSetList")} ({_colorSets.arraySize})",
          EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();

        if (GUILayout.Button(EditorLocalization.Get("appearance.add"), GUILayout.Width(60)))
        {
          AddNewColorSet();
        }
      }

      EditorGUILayout.Space(SpaceSmall);

      if (_colorSets.arraySize == 0)
      {
        EditorGUILayout.HelpBox(EditorLocalization.Get("appearance.noColorSets"), MessageType.Info);
      }
      else
      {
        DrawColorSetListBox();
      }
    }

    private void DrawColorSetListBox()
    {
      EditorGUILayout.BeginVertical();

      for (int i = 0; i < _colorSets.arraySize; i++)
      {
        DrawColorSetRow(i);
      }

      EditorGUILayout.EndVertical();

      ExecutePendingDelete();
    }

    private void ExecutePendingDelete()
    {
      if (_pendingDeleteIndex < 0) return;

      if (_pendingDeleteIndex < _colorSets.arraySize)
      {
        _colorSets.DeleteArrayElementAtIndex(_pendingDeleteIndex);
        _foldouts.Clear();
      }
      _pendingDeleteIndex = -1;
    }

    private void DrawColorSetRow(int index)
    {
      var colorSetProp = _colorSets.GetArrayElementAtIndex(index);
      var colorSetName = colorSetProp.FindPropertyRelative("colorSetName");
      var primaryColor = colorSetProp.FindPropertyRelative("primaryColor");
      var secondaryColor = colorSetProp.FindPropertyRelative("secondaryColor");
      // var infoColor = colorSetProp.FindPropertyRelative("infoColor");
      // var successColor = colorSetProp.FindPropertyRelative("successColor");
      // var alermColor = colorSetProp.FindPropertyRelative("alermColor");
      // var errorColor = colorSetProp.FindPropertyRelative("errorColor");

      var rowBgColor = index % 2 == 0
        ? (EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.76f, 0.76f, 0.76f))
        : (EditorGUIUtility.isProSkin ? new Color(0.25f, 0.25f, 0.25f) : new Color(0.8f, 0.8f, 0.8f));

      var rowRect = EditorGUILayout.BeginVertical();
      EditorGUI.DrawRect(rowRect, rowBgColor);

      using (new EditorGUILayout.HorizontalScope(GUILayout.Height(22)))
      {
        GUILayout.Space(4);

        if (!_foldouts.ContainsKey(index)) _foldouts[index] = false;

        var foldoutContent = new GUIContent(_foldouts[index] ? "▼" : "▶");
        if (GUILayout.Button(foldoutContent, EditorStyles.label, GUILayout.Width(16), GUILayout.Height(22)))
        {
          _foldouts[index] = !_foldouts[index];
        }

        var displayName = string.IsNullOrEmpty(colorSetName.stringValue) ? $"ColorSet {index + 1}" : colorSetName.stringValue;
        float headerLabelWidth = EditorGUIUtility.labelWidth - 20;
        EditorGUILayout.LabelField(displayName, LanguageCodeStyle, GUILayout.Width(headerLabelWidth));

        var previewRect = GUILayoutUtility.GetRect(40, 16, GUILayout.Width(40));
        EditorGUI.DrawRect(new Rect(previewRect.x, previewRect.y, 20, 16), primaryColor.colorValue);
        EditorGUI.DrawRect(new Rect(previewRect.x + 20, previewRect.y, 20, 16), secondaryColor.colorValue);

        GUILayout.FlexibleSpace();

        if (_defaultColorSet.stringValue == colorSetName.stringValue && !string.IsNullOrEmpty(colorSetName.stringValue))
        {
          var defaultStyle = new GUIStyle(EditorStyles.miniLabel)
          {
            normal = { textColor = new Color(0.3f, 0.7f, 0.4f, 1f) },
            alignment = TextAnchor.MiddleRight
          };
          GUILayout.Label(EditorLocalization.Get("appearance.default"), defaultStyle, GUILayout.Width(70), GUILayout.Height(22));
        }

        if (GUILayout.Button("✕", EditorStyles.label, GUILayout.Width(20), GUILayout.Height(22)))
        {
          if (EditorUtility.DisplayDialog(
              EditorLocalization.Get("appearance.deleteColorSet"),
              string.Format(EditorLocalization.Get("appearance.deleteConfirm"), displayName),
              EditorLocalization.Get("appearance.delete"),
              EditorLocalization.Get("button.cancel")))
          {
            _pendingDeleteIndex = index;
          }
        }
        GUILayout.Space(4);
      }

      if (_foldouts[index])
      {
        DrawColorSetDetails(colorSetName, primaryColor, secondaryColor/*, infoColor, successColor, alermColor, errorColor*/);
      }

      EditorGUILayout.EndVertical();
    }

    private void DrawColorSetDetails(SerializedProperty colorSetName, SerializedProperty primaryColor, SerializedProperty secondaryColor/*, SerializedProperty infoColor, SerializedProperty successColor, SerializedProperty alermColor, SerializedProperty errorColor*/)
    {
      var detailBgColor = EditorGUIUtility.isProSkin
        ? new Color(0.18f, 0.18f, 0.18f)
        : new Color(0.7f, 0.7f, 0.7f);

      var detailRect = EditorGUILayout.BeginVertical();
      EditorGUI.DrawRect(detailRect, detailBgColor);

      GUILayout.Space(SpaceSmall);
      using (new EditorGUILayout.HorizontalScope())
      {
        GUILayout.Space(20);
        using (new EditorGUILayout.VerticalScope())
        {
          float originalLabelWidth = EditorGUIUtility.labelWidth;
          EditorGUIUtility.labelWidth = originalLabelWidth - 20;

          EditorGUILayout.PropertyField(colorSetName, new GUIContent(EditorLocalization.Get("appearance.colorSetName")));

          GUILayout.Space(SpaceSmall);

          EditorGUILayout.PropertyField(primaryColor, new GUIContent(EditorLocalization.Get("appearance.primaryColor")));
          EditorGUILayout.PropertyField(secondaryColor, new GUIContent(EditorLocalization.Get("appearance.secondaryColor")));
          // EditorGUILayout.PropertyField(infoColor, new GUIContent(EditorLocalization.Get("appearance.infoColor")));
          // EditorGUILayout.PropertyField(successColor, new GUIContent(EditorLocalization.Get("appearance.successColor")));
          // EditorGUILayout.PropertyField(alermColor, new GUIContent(EditorLocalization.Get("appearance.alermColor")));
          // EditorGUILayout.PropertyField(errorColor, new GUIContent(EditorLocalization.Get("appearance.errorColor")));

          EditorGUIUtility.labelWidth = originalLabelWidth;
        }
        GUILayout.Space(8);
      }
      GUILayout.Space(SpaceSmall);

      EditorGUILayout.EndVertical();
    }

    private void AddNewColorSet()
    {
      _colorSets.InsertArrayElementAtIndex(_colorSets.arraySize);
      var newColorSet = _colorSets.GetArrayElementAtIndex(_colorSets.arraySize - 1);

      newColorSet.FindPropertyRelative("colorSetName").stringValue = "";
      newColorSet.FindPropertyRelative("primaryColor").colorValue = new Color(240f / 256f, 98f / 256f, 146f / 256f, 1.0f);
      newColorSet.FindPropertyRelative("secondaryColor").colorValue = new Color(248f / 256f, 187f / 256f, 208f / 256f, 31f / 256f);
      // newColorSet.FindPropertyRelative("infoColor").colorValue = Color.cyan;
      // newColorSet.FindPropertyRelative("successColor").colorValue = Color.green;
      // newColorSet.FindPropertyRelative("alermColor").colorValue = Color.yellow;
      // newColorSet.FindPropertyRelative("errorColor").colorValue = Color.red;

      _foldouts[_colorSets.arraySize - 1] = true;
    }
  }
}
