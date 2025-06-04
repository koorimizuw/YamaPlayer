using UnityEditor;
using UnityEngine;

namespace Yamadev.YamaStream.Editor
{
    public class PermissionSettings
    {
        private readonly Permission _permission;
        private readonly SerializedObject _permissionSerializedObject;

        private readonly SerializedProperty _defaultPermission;
        private readonly SerializedProperty _grantPermissionToInstanceOwner;
        private readonly SerializedProperty _grantPermissionToInstanceMaster;
        private readonly SerializedProperty _ownerList;

        public bool IsValid => _permission != null && _permissionSerializedObject != null;

        public PermissionSettings(Permission permission)
        {
            _permission = permission;

            if (_permission != null)
            {
                _permissionSerializedObject = new SerializedObject(_permission);

                _defaultPermission = _permissionSerializedObject.FindProperty("_defaultPermission");
                _ownerList = _permissionSerializedObject.FindProperty("_ownerList");
                _grantPermissionToInstanceOwner = _permissionSerializedObject.FindProperty("_grantPermissionToInstanceOwner");
                _grantPermissionToInstanceMaster = _permissionSerializedObject.FindProperty("_grantPermissionToInstanceMaster");
            }
        }

        public void DrawPermissionDescription()
        {
            EditorGUILayout.LabelField("Owner:\t\t" + Localization.Get("ownerPermission"));
            EditorGUILayout.LabelField("Admin:\t\t" + Localization.Get("adminPermission"));
            EditorGUILayout.LabelField("Editor:\t\t" + Localization.Get("editorPermission"));
            EditorGUILayout.LabelField("Viewer:\t\t" + Localization.Get("viewerPermission"));
            EditorGUILayout.Space(8f);
        }

        public void DrawPermissionFields()
        {
            if (!IsValid)
            {
                EditorGUILayout.HelpBox("Permission settings is invalid.", MessageType.Error);
                return;
            }
            EditorGUILayout.PropertyField(_defaultPermission, Localization.GetLayout("defaultPermission"));

            EditorGUILayout.PropertyField(_grantPermissionToInstanceOwner, Localization.GetLayout("grantInstanceOwner"));
            EditorGUILayout.LabelField("　", Localization.Get("grantInstanceOwnerDesc"));

            EditorGUILayout.PropertyField(_grantPermissionToInstanceMaster, Localization.GetLayout("grantInstanceMaster"));
            EditorGUILayout.LabelField("　", Localization.Get("grantInstanceMasterDesc"));

            EditorGUILayout.PropertyField(_ownerList, Localization.GetLayout("ownerList"));
        }

        public void ApplyModifiedProperties()
        {
            try
            {
                _permissionSerializedObject?.ApplyModifiedProperties();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"PermissionSettings: Failed to apply modified properties - {ex.Message}");
            }
        }
    }
}