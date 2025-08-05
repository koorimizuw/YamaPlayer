using UnityEditor;
using UnityEngine;
using System.Linq;
using Yamadev.YamaStream.Script;

#if USE_AUDIOLINK
using AudioLink.Editor;
#endif

namespace Yamadev.YamaStream.Editor
{
    public class ExternalSettings
    {
        private readonly YamaPlayer _yamaPlayer;
        private readonly Controller _controller;

        private readonly SerializedObject _yamaPlayerSerializedObject;
        private readonly SerializedObject _controllerSerializedObject;

        private readonly SerializedProperty _useAudioLink;
        private readonly SerializedProperty _audioLink;
        private readonly SerializedProperty _useLTCGI;
        private readonly SerializedProperty _useLightVolumes;
        private readonly SerializedProperty _targetLightVolumes;

        public bool IsValid => _controller != null && _controllerSerializedObject != null;

        public ExternalSettings(YamaPlayer yamaPlayer, Controller controller)
        {
            _yamaPlayer = yamaPlayer;
            if (_yamaPlayer != null)
            {
                _yamaPlayerSerializedObject = new SerializedObject(_yamaPlayer);

                _useLTCGI = _yamaPlayerSerializedObject.FindProperty("UseLTCGI");
                _useLightVolumes = _yamaPlayerSerializedObject.FindProperty("UseLightVolumes");
                _targetLightVolumes = _yamaPlayerSerializedObject.FindProperty("TargetLightVolumes");
            }

            _controller = controller;
            if (_controller != null)
            {
                _controllerSerializedObject = new SerializedObject(_controller);

                _useAudioLink = _controllerSerializedObject.FindProperty("_useAudioLink");
                _audioLink = _controllerSerializedObject.FindProperty("_audioLink");
            }
        }

        public void DrawAudioLinkSettings()
        {
#if USE_AUDIOLINK
            using (var changeCheck = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(_useAudioLink, Localization.GetLayout("useAudioLink"));
                if (changeCheck.changed && _useAudioLink.boolValue)
                {
                    AudioLinkAssetManager.AddAudioLinkToScene();
                    var audioLink = FindAudioLinkInScene();
                    _audioLink.objectReferenceValue = audioLink;
                }
            }
            if (_useAudioLink.boolValue)
            {
                EditorGUILayout.PropertyField(_audioLink);
            }
            else
            {
                _audioLink.objectReferenceValue = null;
            }
#else
            EditorGUILayout.LabelField("Audio Link", Localization.Get("audioLinkNotImported"));
#endif
        }


#if USE_AUDIOLINK
        private AudioLink.AudioLink FindAudioLinkInScene()
#else
        private UdonSharpBehaviour FindAudioLinkInScene()
#endif
        {
#if USE_AUDIOLINK
            return GameObject.FindObjectsOfType<AudioLink.AudioLink>().FirstOrDefault();
#else
            return null;
#endif
        }

        public void DrawLTCGISettings()
        {
#if USE_LTCGI
            using (var changeCheck = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(_useLTCGI, Localization.GetLayout("useLTCGI"));
                if (changeCheck.changed)
                {
                    if (_useLTCGI.boolValue)
                    {
                        if (!CheckLTCGIEnabledOnOtherPlayers(_yamaPlayer))
                        {
                            var applyToAllScreens = ShowApplyToAllScreensMessage();
                            LTCGIUtility.ProcessLTCGI(_controller, applyToAllScreens);
                        }
                    }
                    else
                    {
                        LTCGIUtility.ClearLTCGISettings();
                    }
                }
            }
#else
            EditorGUILayout.LabelField("LTCGI", Localization.Get("ltcgiNotImported"));
#endif
        }

        private bool CheckLTCGIEnabledOnOtherPlayers(YamaPlayer current)
        {
#if USE_LTCGI
            var yamaPlayers = GameObject.FindObjectsOfType<YamaPlayer>();
            var enabled = yamaPlayers.Where(player => player.UseLTCGI).ToArray();

            if (enabled.Length == 0 || enabled.FirstOrDefault() == current)
            {
                return false;
            }

            if (Styles.DisplayConfirmDialog(Localization.Get("ltcgiSetOnOtherPlayer"), Localization.Get("clearLTCGISettings")))
            {
                LTCGIUtility.ClearLTCGISettings();
                foreach (var yamaPlayer in yamaPlayers)
                {
                    if (yamaPlayer != current)
                    {
                        var serializedObject = new SerializedObject(yamaPlayer);
                        serializedObject.FindProperty("UseLTCGI").boolValue = false;
                        serializedObject.ApplyModifiedProperties();
                    }
                }
                return false;
            }

            return true;
#else
            return false;
#endif
        }

        private bool ShowApplyToAllScreensMessage()
        {
            return Styles.DisplayConfirmDialog(Localization.Get("applyToSubScreens"), Localization.Get("applyToSubScreensConfirm"));
        }

        public void DrawLVTVGISettings()
        {
#if USE_VRCLV
            using (var changeCheck = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(_useLightVolumes, Localization.GetLayout("useLightVolumes"));
                if (_useLightVolumes.boolValue)
                {
                    EditorGUILayout.PropertyField(_targetLightVolumes, Localization.GetLayout("targetLightVolumes"));
                }
            }
#else
            EditorGUILayout.LabelField("Light Volumes", Localization.Get("vrclvNotImported"));
#endif
        }

        public void ApplyModifiedProperties()
        {
            try
            {
                _yamaPlayerSerializedObject?.ApplyModifiedProperties();
                _controllerSerializedObject?.ApplyModifiedProperties();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"ExternalSettings: Failed to apply modified properties - {ex.Message}");
            }
        }
    }
}