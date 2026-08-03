using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Yamadev.YamaStream.Editor
{
  [CustomEditor(typeof(YamaPlayerModuleDefinition))]
  public class YamaPlayerModuleDefinitionEditor : EditorBase
  {
    private SerializedProperty _moduleName;
    private SerializedProperty _moduleDescription;
    private SerializedProperty _version;
    private SerializedProperty _allowMultiple;
    private SerializedProperty _noNeedSetUp;
    private SerializedProperty _uiSlots;
    private SerializedProperty _moduleNameTranslationKey;
    private SerializedProperty _moduleDescriptionTranslationKey;
    private SerializedProperty _editorTranslationFile;
    private SerializedProperty _playerTranslationFile;

    private bool _developerFoldout;
    private ReorderableList _uiSlotsList;

    private static Color SubTextColor => EditorGUIUtility.isProSkin
      ? new Color(0.55f, 0.55f, 0.55f)
      : new Color(0.45f, 0.45f, 0.45f);

    private void OnEnable()
    {
      Title = EditorLocalization.Get("module.definition.title");

      _moduleName = serializedObject.FindProperty("moduleName");
      _moduleDescription = serializedObject.FindProperty("moduleDescription");
      _version = serializedObject.FindProperty("version");
      _allowMultiple = serializedObject.FindProperty("allowMultiple");
      _noNeedSetUp = serializedObject.FindProperty("noNeedSetUp");
      _uiSlots = serializedObject.FindProperty("uiSlots");
      _moduleNameTranslationKey = serializedObject.FindProperty("moduleNameTranslationKey");
      _moduleDescriptionTranslationKey = serializedObject.FindProperty("moduleDescriptionTranslationKey");
      _editorTranslationFile = serializedObject.FindProperty("editorTranslationFile");
      _playerTranslationFile = serializedObject.FindProperty("playerTranslationFile");

      InitializeUISlotsList();
    }

    private void InitializeUISlotsList()
    {
      _uiSlotsList = new ReorderableList(serializedObject, _uiSlots, true, true, true, true);

      _uiSlotsList.drawHeaderCallback = rect =>
      {
        EditorGUI.LabelField(rect, EditorLocalization.Get("module.definition.uiSlots"));
      };

      _uiSlotsList.elementHeightCallback = index =>
      {
        return EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing * 4 + 4;
      };

      _uiSlotsList.drawElementCallback = (rect, index, isActive, isFocused) =>
      {
        var element = _uiSlots.GetArrayElementAtIndex(index);
        var targetPath = element.FindPropertyRelative("targetPath");
        var content = element.FindPropertyRelative("content");
        var siblingIndex = element.FindPropertyRelative("siblingIndex");

        rect.y += 2;
        var lineHeight = EditorGUIUtility.singleLineHeight;
        var spacing = EditorGUIUtility.standardVerticalSpacing;

        var labelWidth = 80f;
        var fieldRect = new Rect(rect.x, rect.y, rect.width, lineHeight);

        var pathLabelRect = new Rect(fieldRect.x, fieldRect.y, labelWidth, lineHeight);
        var pathFieldRect = new Rect(fieldRect.x + labelWidth, fieldRect.y, fieldRect.width - labelWidth, lineHeight);
        EditorGUI.LabelField(pathLabelRect, "Path");
        EditorGUI.PropertyField(pathFieldRect, targetPath, GUIContent.none);

        fieldRect.y += lineHeight + spacing;
        var contentLabelRect = new Rect(fieldRect.x, fieldRect.y, labelWidth, lineHeight);
        var contentFieldRect = new Rect(fieldRect.x + labelWidth, fieldRect.y, fieldRect.width - labelWidth, lineHeight);
        EditorGUI.LabelField(contentLabelRect, "Content");
        EditorGUI.PropertyField(contentFieldRect, content, GUIContent.none);

        fieldRect.y += lineHeight + spacing;
        var indexLabelRect = new Rect(fieldRect.x, fieldRect.y, labelWidth, lineHeight);
        var indexFieldRect = new Rect(fieldRect.x + labelWidth, fieldRect.y, fieldRect.width - labelWidth, lineHeight);
        EditorGUI.LabelField(indexLabelRect, "Index");
        EditorGUI.PropertyField(indexFieldRect, siblingIndex, GUIContent.none);
      };

      _uiSlotsList.drawElementBackgroundCallback = (rect, index, isActive, isFocused) =>
      {
        if (isActive || isFocused)
        {
          EditorGUI.DrawRect(rect, new Color(0.24f, 0.37f, 0.59f, 0.5f));
        }
        else if (index % 2 == 1)
        {
          EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.1f));
        }
      };

      _uiSlotsList.onAddCallback = list =>
      {
        var index = list.serializedProperty.arraySize;
        list.serializedProperty.arraySize++;
        list.index = index;

        var element = list.serializedProperty.GetArrayElementAtIndex(index);
        element.FindPropertyRelative("targetPath").stringValue = "";
        element.FindPropertyRelative("content").objectReferenceValue = null;
        element.FindPropertyRelative("siblingIndex").intValue = 0;
      };
    }

    public override void OnInspectorGUI()
    {
      base.OnInspectorGUI();

      Title = EditorLocalization.Get("module.definition.title");
      serializedObject.Update();

      DrawModuleInfoSection();
      EditorGUILayout.Space(SpaceMedium);

      DrawDeveloperSection();
      EditorGUILayout.Space(SpaceMedium);

      serializedObject.ApplyModifiedProperties();
    }

    private void DrawModuleInfoSection()
    {
      string moduleName = null;
      if (!string.IsNullOrEmpty(_moduleNameTranslationKey.stringValue))
      {
        var translated = EditorLocalization.Get(_moduleNameTranslationKey.stringValue);
        if (!string.IsNullOrEmpty(translated))
          moduleName = translated;
      }
      if (string.IsNullOrEmpty(moduleName))
      {
        moduleName = string.IsNullOrEmpty(_moduleName.stringValue)
          ? EditorLocalization.Get("module.definition.unnamed")
          : _moduleName.stringValue;
      }

      var nameWithVersion = !string.IsNullOrEmpty(_version.stringValue)
        ? $"{moduleName}  v{_version.stringValue}"
        : moduleName;

      var nameStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 12 };
      EditorGUILayout.LabelField(nameWithVersion, nameStyle);

      string moduleDescription = null;
      if (!string.IsNullOrEmpty(_moduleDescriptionTranslationKey.stringValue))
      {
        var translated = EditorLocalization.Get(_moduleDescriptionTranslationKey.stringValue);
        if (!string.IsNullOrEmpty(translated))
          moduleDescription = translated;
      }
      if (string.IsNullOrEmpty(moduleDescription))
        moduleDescription = _moduleDescription.stringValue;

      if (!string.IsNullOrEmpty(moduleDescription))
      {
        var descStyle = new GUIStyle(EditorStyles.miniLabel) { wordWrap = true };
        descStyle.normal.textColor = SubTextColor;
        EditorGUILayout.LabelField(moduleDescription, descStyle);
      }
    }

    private void DrawDeveloperSection()
    {
      _developerFoldout = EditorGUILayout.Foldout(_developerFoldout, EditorLocalization.Get("module.definition.developer"), true);

      if (!_developerFoldout) return;

      EditorGUI.indentLevel++;

      EditorGUILayout.PropertyField(_moduleName, new GUIContent(EditorLocalization.Get("module.definition.name")));
      EditorGUILayout.PropertyField(_moduleDescription, new GUIContent(EditorLocalization.Get("module.definition.description")));
      EditorGUILayout.PropertyField(_version, new GUIContent(EditorLocalization.Get("module.definition.version")));
      EditorGUILayout.PropertyField(_allowMultiple, new GUIContent(EditorLocalization.Get("module.definition.allowMultiple")));
      EditorGUILayout.PropertyField(_noNeedSetUp, new GUIContent(EditorLocalization.Get("module.definition.noNeedSetUp")));

      EditorGUILayout.Space(SpaceSmall);

      EditorGUILayout.PropertyField(_moduleNameTranslationKey, new GUIContent(EditorLocalization.Get("module.definition.moduleNameTranslationKey")));
      EditorGUILayout.PropertyField(_moduleDescriptionTranslationKey, new GUIContent(EditorLocalization.Get("module.definition.moduleDescriptionTranslationKey")));
      EditorGUILayout.PropertyField(_editorTranslationFile, new GUIContent(EditorLocalization.Get("module.definition.editorTranslation")));
      EditorGUILayout.PropertyField(_playerTranslationFile, new GUIContent(EditorLocalization.Get("module.definition.playerTranslation")));

      EditorGUILayout.Space(SpaceSmall);

      EditorGUI.indentLevel--;
      _uiSlotsList.DoLayoutList();
    }
  }
}
