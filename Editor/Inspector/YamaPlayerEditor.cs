using UnityEditor;
using UnityEngine;
using VRC.SDK3.Video.Components.AVPro;
using Yamadev.YamaStream.UI;
using Yamadev.YamaStream.Script;
using System.Collections.Generic;

namespace Yamadev.YamaStream.Editor
{
    [CustomEditor(typeof(YamaPlayer))]
    public class YamaPlayerEditor : EditorBase
    {
        private YamaPlayer _target;

        private UIController _uiController;
        private Controller _controller;
        private AutoPlay _autoPlay;
        private Permission _permission;
        private PlayList[] _playlists;

        private UISettings _uiSettings;
        private ControllerSettings _controllerSettings;
        private AutoPlaySettings _autoPlaySettings;
        private PermissionSettings _permissionSettings;
        private VersionSettings _versionSettings;
        private ExternalSettings _externalSettings;

        private VRCAVProVideoPlayer _avPro;
        private SerializedObject _avProSerializedObject;
        private SerializedProperty _useLowLatency;

        private bool _isEasyMode = true;
        private TabScope _tabScope;

        private void OnEnable()
        {
            Title = $"YamaPlayer v{VersionManager.Version}";
            _target = target as YamaPlayer;

            if (Application.isPlaying) return;

            _uiController = _target.GetComponentInChildren<UIController>(true);
            _controller = _target.GetComponentInChildren<Controller>(true);
            _autoPlay = _target.GetComponentInChildren<AutoPlay>(true);
            _permission = _target.GetComponentInChildren<Permission>(true);
            _playlists = _target.GetComponentsInChildren<PlayList>();

            _uiSettings = new UISettings(_uiController);
            _controllerSettings = new ControllerSettings(_controller);
            _autoPlaySettings = new AutoPlaySettings(_autoPlay, _playlists);
            _permissionSettings = new PermissionSettings(_permission);
            _externalSettings = new ExternalSettings(_controller);
            _versionSettings = new VersionSettings();

            _avPro = _target.GetComponentInChildren<VRCAVProVideoPlayer>();
            if (_avPro != null)
            {
                _avProSerializedObject = new SerializedObject(_avPro);
                _useLowLatency = _avProSerializedObject.FindProperty("useLowLatency");
            }

            _tabScope = new TabScope(new List<TabScope.Tab>()
            {
                new TabScope.Tab("UI", DrawUISettings),
                new TabScope.Tab("Settings", DrawPlayerSettings),
                new TabScope.Tab("Playlist", DrawPlaylistSettings),
                new TabScope.Tab("Permission", DrawPermissionSettings),
                new TabScope.Tab("Version", DrawVersionSettings),
            });
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

            DrawModeToggleButton();
            EditorGUILayout.Space(4f);

            if (_isEasyMode)
            {
                DrawEasyModeSettings();
            }
            else
            {
                _tabScope.Draw();
            }

            ApplyModifiedProperties();
        }

        private void DrawEasyModeSettings()
        {
            using (new SectionScope("UI"))
            {
                if (_uiSettings.UIDisabled)
                {
                    _uiSettings.DrawUIDisabledMessage();
                }
                else
                {
                    _uiSettings.DrawColorSettings();
                    _uiSettings.DrawIdleImageSettings();
                }
            }

            using (new SectionScope(Localization.Get("autoPlay")))
            {
                _autoPlaySettings?.DrawSettings();
            }

            using (new SectionScope(Localization.Get("playlist")))
            {
                _controllerSettings.DrawShuffleSettings();
                if (GUILayout.Button(Localization.Get("editPlaylist")))
                    PlaylistEditor.ShowPlaylistEditorWindow(_target);
            }

            using (new SectionScope(Localization.Get("playerSettings"), _versionSettings.IsValid))
            {
                _controllerSettings.DrawSyncSettings();
                DrawAVProSettings();
                _controllerSettings.DrawPlaybackSettings();
                _controllerSettings.DrawAudioSettings();
            }

            if (_versionSettings.IsValid)
            {
                using (new SectionScope(Localization.Get("version"), false))
                {
                    _versionSettings.DrawVersionSettings();
                }
            }
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

            using (new SectionScope(Localization.Get("color")))
            {
                _uiSettings.DrawColorSettings();
            }

            using (new SectionScope(Localization.Get("font")))
            {
                _uiSettings.DrawFontSettings();
            }

            using (new SectionScope(Localization.Get("idleImage")))
            {
                _uiSettings.DrawIdleImageSettings();
            }

            using (new SectionScope(Localization.Get("playlist")))
            {
                _uiSettings.DrawDefaultOpenSettings();
            }

            using (new SectionScope(Localization.Get("pickUp"), false))
            {
                _uiSettings.DrawPickupSettings();
            }
        }

        private void DrawPlayerSettings()
        {
            using (new SectionScope(Localization.Get("videoPlayerSettings")))
            {
                _controllerSettings.DrawVideoPlayerSettings();
            }

            using (new SectionScope(Localization.Get("syncSettings")))
            {
                _controllerSettings.DrawSyncSettings();
            }

            using (new SectionScope(Localization.Get("audioSettings")))
            {
                _controllerSettings.DrawAudioSettings();
            }

            using (new SectionScope(Localization.Get("videoSettings")))
            {
                _controllerSettings.DrawVideoSettings();
            }

            using (new SectionScope(Localization.Get("playbackSettings")))
            {
                _controllerSettings.DrawPlaybackSettings();
                DrawAVProSettings();
            }

            using (new SectionScope(Localization.Get("externalSettings")))
            {
                _externalSettings.DrawAudioLinkSettings();
                _externalSettings.DrawLTCGISettings();
                _externalSettings.DrawLVTVGISettings();
            }

            using (new SectionScope(Localization.Get("screenSettings"), false))
            {
                _controllerSettings.DrawScreenSettings();
            }
        }

        private void DrawPlaylistSettings()
        {
            using (new SectionScope(Localization.Get("autoPlay")))
            {
                _autoPlaySettings?.DrawSettings();
            }

            using (new SectionScope())
            {
                _controllerSettings.DrawShuffleSettings();
                _controllerSettings.DrawPlaylistForwardIntervalSettings();
            }

            using (new SectionScope(Localization.Get("playlist"), false))
            {
                if (GUILayout.Button(Localization.Get("editPlaylist")))
                    PlaylistEditor.ShowPlaylistEditorWindow(_target);
            }

        }

        private void DrawPermissionSettings()
        {
            _permissionSettings?.DrawPermissionDescription();
            _permissionSettings?.DrawPermissionFields();
        }

        private void DrawVersionSettings()
        {
            using (new SectionScope())
            {
                _versionSettings.DrawAutoUpdateSettings();
            }

            using (new SectionScope(false))
            {
                _versionSettings.DrawVersionSettings();
            }
        }

        private void ApplyModifiedProperties()
        {
            serializedObject.ApplyModifiedProperties();
            _uiSettings?.ApplyModifiedProperties();
            _controllerSettings?.ApplyModifiedProperties();
            _autoPlaySettings?.ApplyModifiedProperties();
            _permissionSettings?.ApplyModifiedProperties();
            _avProSerializedObject?.ApplyModifiedProperties();
            _externalSettings?.ApplyModifiedProperties();
        }

        private void DrawAVProSettings()
        {
            if (_useLowLatency != null)
            {
                EditorGUILayout.PropertyField(_useLowLatency, Localization.GetLayout("useLowLatency"));
            }
        }

        private void DrawModeToggleButton()
        {
            EditorGUILayout.Space(2f);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar, GUILayout.Width(200)))
                {
                    var easyModeStyle = _isEasyMode ? EditorStyles.toolbarButton : EditorStyles.toolbarButton;
                    var detailModeStyle = !_isEasyMode ? EditorStyles.toolbarButton : EditorStyles.toolbarButton;

                    var easyContent = new GUIContent(_isEasyMode ? "● 簡単設定" : "○ 簡単設定");
                    if (GUILayout.Button(easyContent, easyModeStyle, GUILayout.Width(95)))
                    {
                        _isEasyMode = true;
                    }

                    var detailContent = new GUIContent(!_isEasyMode ? "● 詳細設定" : "○ 詳細設定");
                    if (GUILayout.Button(detailContent, detailModeStyle, GUILayout.Width(95)))
                    {
                        _isEasyMode = false;
                    }
                }
            }

            EditorGUILayout.Space(2f);
        }
    }
}