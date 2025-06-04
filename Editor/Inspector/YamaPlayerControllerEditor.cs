using UnityEditor;
using UnityEngine;
using VRC.SDK3.Components;
using Yamadev.YamaStream.UI;
using Yamadev.YamaStream.Script;

namespace Yamadev.YamaStream.Editor
{
    [CustomEditor(typeof(YamaPlayerController))]
    public class YamaPlayerControllerEditor : EditorBase
    {
        private YamaPlayerController _target;

        private UIController _uiController;
        private Controller _controller;
        private VRCPickup _vrcPickup;

        private UISettings _uiSettings;
        private SerializedObject _pickupSerializedObject;
        private SerializedProperty _pickupable;
        private SerializedProperty _yamaPlayer;
        bool _globalSync;

        void OnEnable()
        {
            Title = target.name;
            _target = target as YamaPlayerController;

            if (Application.isPlaying) return;

            _uiController = _target.GetComponentInChildren<UIController>(true);
            _uiSettings = new UISettings(_uiController);

            _vrcPickup = _target.GetComponentInChildren<VRCPickup>(true);
            if (_vrcPickup != null)
            {
                _pickupSerializedObject = new SerializedObject(_vrcPickup);
                _pickupable = _pickupSerializedObject.FindProperty("pickupable");
            }

            _yamaPlayer = serializedObject.FindProperty("YamaPlayer");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("YamaPlayer is not available in the play mode.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space(8f);

            using (new SectionScope())
            {
                EditorGUILayout.PropertyField(_yamaPlayer);
            }

            using (new SectionScope())
            {
                DrawPickupSettings();
            }

            DrawUISettings();

            ApplyModifiedProperties();
        }

        private void DrawUISettings()
        {
            using (new SectionScope(!_uiSettings.UIDisabled))
            {
                _uiSettings.DrawDisableUISettings();
            }

            if (_uiSettings.UIDisabled)
            {
                EditorGUILayout.Space(4f);
                _uiSettings.DrawUIDisabledMessage();
                return;
            }

            using (new SectionScope("Color"))
            {
                _uiSettings.DrawColorSettings();
            }

            using (new SectionScope("Font"))
            {
                _uiSettings.DrawFontSettings();
            }

            using (new SectionScope("Idle Image"))
            {
                _uiSettings.DrawIdleImageSettings();
            }

            using (new SectionScope())
            {
                _uiSettings.DrawDefaultOpenSettings();
            }

            using (new SectionScope(false))
            {
                _uiSettings.DrawPickupSettings();
            }
        }

        private void DrawTargetPlayerSettings()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(_yamaPlayer);
                EditorGUILayout.LabelField("　", Localization.Get("targetPlayer"));
                if (check.changed && _yamaPlayer.objectReferenceValue != null)
                {
                    Controller controller = (_yamaPlayer.objectReferenceValue as YamaPlayer)?.GetComponentInChildren<Controller>();
                    if (controller != null)
                    {
                        _uiSettings.SetController(controller);
                        YamaPlayerScreen screen = _target.gameObject.GetComponentInChildren<YamaPlayerScreen>();
                        if (screen != null)
                        {
                            SerializedObject serializedObject = new SerializedObject(screen);
                            serializedObject.FindProperty("controller").objectReferenceValue = controller;
                            serializedObject.ApplyModifiedProperties();
                        }
                    }
                }
            }
        }

        private void DrawPickupSettings()
        {
            VRCPickup vrcPickup = _target.GetComponentInChildren<VRCPickup>();
            if (vrcPickup != null)
            {
                EditorGUILayout.PropertyField(_pickupable, Localization.GetLayout("pickUp"));
                EditorGUILayout.LabelField("　", Localization.Get("pickUpDesc"));
                if (_pickupable.boolValue)
                {
                    VRCObjectSync objectSync = vrcPickup.gameObject.GetComponent<VRCObjectSync>();
                    _globalSync = objectSync != null;
                    _globalSync = EditorGUILayout.Toggle(Localization.Get("globalSync"), _globalSync);
                    EditorGUILayout.LabelField("　", Localization.Get("globalSyncDesc"));
                    if (_globalSync && objectSync == null) vrcPickup.gameObject.AddComponent<VRCObjectSync>();
                    if (!_globalSync && objectSync != null) GameObject.DestroyImmediate(objectSync);
                }
            }
        }

        private void ApplyModifiedProperties()
        {
            serializedObject.ApplyModifiedProperties();
            _uiSettings?.ApplyModifiedProperties();
            _pickupSerializedObject?.ApplyModifiedProperties();
        }
    }
}