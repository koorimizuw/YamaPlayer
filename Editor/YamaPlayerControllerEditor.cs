
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Components;

namespace Yamadev.YamaStream.Script
{
    [CustomEditor(typeof(YamaPlayerController))]
    public class YamaPlayerControllerEditor : EditorBase
    {
        VRCPickup _vrcPickup;
        SerializedObject _vrcPickupSerializedObject;
        SerializedProperty _pickup;
        UIController _uiController;
        SerializedObject _uiControllerSerializedObject;
        SerializedProperty _yamaPlayer;
        SerializedProperty _primaryColor;
        SerializedProperty _secondaryColor;
        SerializedProperty _idleImage;
        SerializedProperty _defaultOpen;
        YamaPlayerController _target;
        YamaPlayer[] _players;
        bool _uiOn;

        private void OnEnable()
        {
            _target = target as YamaPlayerController;
            _vrcPickup = _target.GetComponentInChildren<VRCPickup>(true);
            if (_vrcPickup != null )
            {
                _vrcPickupSerializedObject = new SerializedObject(_vrcPickup);
                _pickup = _vrcPickupSerializedObject.FindProperty("pickupable");
            }
            _uiController = _target.GetComponentInChildren<UIController>(true);
            if (_uiController != null)
            {
                _uiControllerSerializedObject = new SerializedObject(_uiController);
                _primaryColor = _uiControllerSerializedObject.FindProperty("_primaryColor");
                _secondaryColor = _uiControllerSerializedObject.FindProperty("_secondaryColor");
                _idleImage = _uiControllerSerializedObject.FindProperty("_idleImage");
                _defaultOpen = _uiControllerSerializedObject.FindProperty("_defaultPlaylistOpen");
            }
            _yamaPlayer = serializedObject.FindProperty("YamaPlayer");
            _players = Utils.FindComponentsInHierarthy<YamaPlayer>();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();

            EditorGUILayout.LabelField(_target.name, _uiTitle);
            EditorGUILayout.Space();

            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                if (_players.Length == 1) _yamaPlayer.objectReferenceValue = _players[0];
                EditorGUILayout.PropertyField(_yamaPlayer);
                VRCPickup vrcPickup = _target.GetComponentInChildren<VRCPickup>();
                if (vrcPickup != null)
                {
                    EditorGUILayout.PropertyField(_pickup);
                }
            }
            EditorGUILayout.Space();

            if (_uiController != null)
            {
                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    Transform _uiCanvas = _uiController.transform.Find("Canvas");
                    EditorGUILayout.LabelField("UI", _bold);
                    if (_uiCanvas != null )
                    {
                        _uiOn = _uiCanvas.gameObject.activeSelf;
                        _uiOn = EditorGUILayout.Toggle("UI ON", _uiOn);
                        _uiCanvas?.gameObject.SetActive(_uiOn);
                        if (_uiOn)
                        {
                            EditorGUILayout.PropertyField(_primaryColor);
                            EditorGUILayout.PropertyField(_secondaryColor);
                        }
                        if (_defaultOpen != null)
                        {
                            EditorGUILayout.LabelField("Playlist", _bold);
                            EditorGUILayout.PropertyField(_defaultOpen);
                        }
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Idle", _bold);
                    EditorGUILayout.PropertyField(_idleImage);
                }
            }

            if (serializedObject.ApplyModifiedProperties() 
                || _uiControllerSerializedObject.ApplyModifiedProperties()
                || _vrcPickupSerializedObject.ApplyModifiedProperties())
                ApplyModifiedProperties();
        }

        internal void ApplyModifiedProperties()
        {
            /*
            if (_yamaPlayer.objectReferenceValue == null)
            {
                _players = Utils.FindComponentsInHierarthy<YamaPlayer>();
                if (_players.Length > 0) _yamaPlayer.objectReferenceValue = _players[0];
            }
            */
            Controller controller = (_yamaPlayer.objectReferenceValue as YamaPlayer)?.GetComponentInChildren<Controller>();
            if (controller == null) return;
            UIController uiController = _target.GetComponentInChildren<UIController>();
            if (uiController != null)
            {
                SerializedObject serializedObject = new SerializedObject(uiController);
                serializedObject.FindProperty("_controller").objectReferenceValue = controller;
                serializedObject.FindProperty("_i18n").objectReferenceValue = (_yamaPlayer.objectReferenceValue as YamaPlayer).GetComponentInChildren<i18n>();
                serializedObject.ApplyModifiedProperties();
            }
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