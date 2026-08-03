using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Yamadev.YamaStream.Editor;

namespace Yamadev.YamaStream.Modules.PermissionManagement.Editor
{
  [CustomEditor(typeof(PermissionManagement))]
  public class PermissionManagementEditor : EditorBase
  {
    private SerializedProperty _defaultPermissionProperty;
    private SerializedProperty _ownerListProperty;
    private SerializedProperty _grantPermissionToInstanceOwnerProperty;
    private SerializedProperty _grantPermissionToInstanceMasterProperty;
    private ReorderableList _ownerList;

    private void OnEnable()
    {
      Title = EditorLocalization.Get("module.permissionManagement.title");
      ShowHeader = false;

      _defaultPermissionProperty = serializedObject.FindProperty("_defaultPermission");
      _ownerListProperty = serializedObject.FindProperty("_ownerList");
      _grantPermissionToInstanceOwnerProperty = serializedObject.FindProperty("_grantPermissionToInstanceOwner");
      _grantPermissionToInstanceMasterProperty = serializedObject.FindProperty("_grantPermissionToInstanceMaster");

      SetupOwnerList();
    }

    private void SetupOwnerList()
    {
      _ownerList = new ReorderableList(serializedObject, _ownerListProperty, true, true, true, true);

      _ownerList.drawHeaderCallback = (Rect rect) =>
      {
        EditorGUI.LabelField(rect, EditorLocalization.Get("module.permissionManagement.ownerList"));
      };

      _ownerList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
      {
        SerializedProperty element = _ownerListProperty.GetArrayElementAtIndex(index);
        rect.y += 2;
        rect.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(rect, element, GUIContent.none);
      };

      _ownerList.onAddCallback = (ReorderableList list) =>
      {
        int newIndex = _ownerListProperty.arraySize;
        _ownerListProperty.arraySize++;
        _ownerListProperty.GetArrayElementAtIndex(newIndex).stringValue = "";
      };

      _ownerList.elementHeight = EditorGUIUtility.singleLineHeight + 6;
    }

    public override void OnInspectorGUI()
    {
      base.OnInspectorGUI();

      Title = EditorLocalization.Get("module.permissionManagement.title");

      serializedObject.Update();

      EditorGUILayout.LabelField("Owner:\t\t" + EditorLocalization.Get("module.permissionManagement.desc.owner"));
      EditorGUILayout.LabelField("Admin:\t\t" + EditorLocalization.Get("module.permissionManagement.desc.admin"));
      EditorGUILayout.LabelField("Editor:\t\t" + EditorLocalization.Get("module.permissionManagement.desc.editor"));
      EditorGUILayout.LabelField("Viewer:\t\t" + EditorLocalization.Get("module.permissionManagement.desc.viewer"));
      EditorGUILayout.Space(SpaceSmall);

      EditorGUILayout.PropertyField(
        _defaultPermissionProperty,
        new GUIContent(EditorLocalization.Get("module.permissionManagement.defaultPermission"))
      );

      EditorGUILayout.Space(SpaceSmall);

      EditorGUILayout.PropertyField(
        _grantPermissionToInstanceOwnerProperty,
        new GUIContent(EditorLocalization.Get("module.permissionManagement.grantToInstanceOwner"))
      );

      EditorGUILayout.PropertyField(
        _grantPermissionToInstanceMasterProperty,
        new GUIContent(EditorLocalization.Get("module.permissionManagement.grantToInstanceMaster"))
      );

      EditorGUILayout.Space(SpaceMedium);

      _ownerList.DoLayoutList();

      serializedObject.ApplyModifiedProperties();
    }
  }
}
