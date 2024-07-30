
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Components;

namespace Yamadev.YamaStream.Script
{
    [CustomEditor(typeof(YamaPlayerController))]
    public class YamaPlayerControllerEditor : EditorBase
    {
        SerializedProperty _yamaPlayer;
        VRCPickup _vrcPickup;
        SerializedObject _vrcPickupSerializedObject;
        SerializedProperty _pickup;
        YamaPlayerController _target;
        UIController _uiController;
        UIEditor _uiEditor;
        bool _disableUI;
        bool _globalSync;

        void OnEnable()
        {
            _target = target as YamaPlayerController;

            _uiController = _target.GetComponentInChildren<UIController>(true);
            if (_uiController != null) _uiEditor = new UIEditor(_uiController);

            _vrcPickup = _target.GetComponentInChildren<VRCPickup>(true);
            if (_vrcPickup != null)
            {
                _vrcPickupSerializedObject = new SerializedObject(_vrcPickup);
                _pickup = _vrcPickupSerializedObject.FindProperty("pickupable");
            }

            _yamaPlayer = serializedObject.FindProperty("YamaPlayer");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();

            EditorGUILayout.LabelField(_target.name, Styles.Title);
            EditorGUILayout.Space(48f);

            EditorGUILayout.PropertyField(_yamaPlayer);
            EditorGUILayout.LabelField("　", "Target video player.");
            Styles.DrawDivider();

            VRCPickup vrcPickup = _target.GetComponentInChildren<VRCPickup>();
            if (vrcPickup != null)
            {
                EditorGUILayout.PropertyField(_pickup);
                EditorGUILayout.LabelField("　", "User could pick up this object in game.");
                if (_pickup.boolValue)
                {
                    VRCObjectSync objectSync = vrcPickup.gameObject.GetComponent<VRCObjectSync>();
                    _globalSync = objectSync != null;
                    _globalSync = EditorGUILayout.Toggle("Global Sync", _globalSync);
                    EditorGUILayout.LabelField("　", "Object position will synced globally.");
                    if (_globalSync && objectSync == null) vrcPickup.gameObject.AddComponent<VRCObjectSync>();
                    if (!_globalSync && objectSync != null) GameObject.DestroyImmediate(objectSync);
                }
                Styles.DrawDivider();
            }

            VRCUiShape uiSharp = _uiController.GetComponentInChildren<VRCUiShape>(true);
            if (uiSharp == null) return;

            _disableUI = !uiSharp.gameObject.activeSelf;
            _disableUI = EditorGUILayout.Toggle("Disable UI", _disableUI);
            uiSharp.gameObject.SetActive(!_disableUI);
            if (_disableUI) return;
            Styles.DrawDivider();

            _uiEditor.DrawUISettings();

            ApplyModifiedProperties();
        }

        internal void ApplyModifiedProperties()
        {
            serializedObject.ApplyModifiedProperties();
            _vrcPickupSerializedObject?.ApplyModifiedProperties();

            Controller controller = (_yamaPlayer.objectReferenceValue as YamaPlayer)?.GetComponentInChildren<Controller>();
            if (controller == null) return;

            if (_uiEditor != null) _uiEditor.SetController(controller);
            YamaPlayerScreen screen = _target.gameObject.GetComponentInChildren<YamaPlayerScreen>();
            if (screen != null)
            {
                SerializedObject serializedObject = new SerializedObject(screen);
                serializedObject.FindProperty("_controller").objectReferenceValue = controller;
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}