
using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Video.Components.AVPro;
using Yamadev.YamaStream.UI;

namespace Yamadev.YamaStream.Script
{
    [CustomEditor(typeof(YamaPlayer))]
    internal class YamaPlayerEditor : EditorBase
    {
        enum Tab
        {
            UI,
            Settings,
            Playlist,
            Permission,
            Other
        }

        // controller
        Controller _controller;
        SerializedObject _controllerSerializedObject;
        SerializedProperty _volume;
        SerializedProperty _mirrorInverse;
        SerializedProperty _emission;
        SerializedProperty _mute;
        SerializedProperty _loop;
        SerializedProperty _shuffle;
        SerializedProperty _defaultPlayerEngine;
        SerializedProperty _forwardInterval;
        SerializedProperty _screenTypes;
        SerializedProperty _screens;
        SerializedProperty _textureProperties;
        SerializedProperty _avProProperties;
        // auto play
        AutoPlay _autoPlay;
        SerializedObject _autoPlaySerializedObject;
        SerializedProperty _autoPlayMode;
        SerializedProperty _autoPlayVideoPlayerType;
        SerializedProperty _autoPlayVideoTitle;
        SerializedProperty _autoPlayVideoUrl;
        SerializedProperty _autoPlayPlaylistIndex;
        SerializedProperty _autoPlayPlaylistTrackIndex;
        SerializedProperty _autoPlayDelay;
        // permission
        Permission _permission;
        SerializedObject _permissionSerializedObject;
        SerializedProperty _defaultPermission;
        SerializedProperty _ownerList;
        // UI
        UIController _uiController;
        UIEditor _uiEditor;
        // avpro
        VRCAVProVideoPlayer _avPro;
        SerializedObject _avProSerializedObject;
        SerializedProperty _useLowLatency;

        YamaPlayer _target;
        ReorderableList _screenList;
        PlayList[] _playlists;
        Tab _tab = Tab.UI;

        private void OnEnable()
        {
            _target = target as YamaPlayer;
            _controller = _target.GetComponentInChildren<Controller>();
            if (_controller != null )
            {
                _controllerSerializedObject = new SerializedObject(_controller);
                _volume = _controllerSerializedObject.FindProperty("_volume");
                _mirrorInverse = _controllerSerializedObject.FindProperty("_mirrorInverse");
                _emission = _controllerSerializedObject.FindProperty("_emission");
                _mute = _controllerSerializedObject.FindProperty("_mute");
                _loop = _controllerSerializedObject.FindProperty("_loop");
                _shuffle = _controllerSerializedObject.FindProperty("_shuffle");
                _defaultPlayerEngine = _controllerSerializedObject.FindProperty("_videoPlayerType");
                _forwardInterval = _controllerSerializedObject.FindProperty("_forwardInterval");
                _screenTypes = _controllerSerializedObject.FindProperty("_screenTypes");
                _screens = _controllerSerializedObject.FindProperty("_screens");
                _textureProperties = _controllerSerializedObject.FindProperty("_textureProperties");
                _avProProperties = _controllerSerializedObject.FindProperty("_avProProperties");
            }
            _autoPlay = _target.GetComponentInChildren<AutoPlay>();
            if (_autoPlay != null)
            {
                _autoPlaySerializedObject = new SerializedObject(_autoPlay);
                _autoPlayMode = _autoPlaySerializedObject.FindProperty("_autoPlayMode");
                _autoPlayVideoPlayerType = _autoPlaySerializedObject.FindProperty("_videoPlayerType");
                _autoPlayVideoTitle = _autoPlaySerializedObject.FindProperty("_title");
                _autoPlayVideoUrl = _autoPlaySerializedObject.FindProperty("_url");
                _autoPlayPlaylistIndex = _autoPlaySerializedObject.FindProperty("_playlistIndex");
                _autoPlayPlaylistTrackIndex = _autoPlaySerializedObject.FindProperty("_playlistTrackIndex");
                _autoPlayDelay = _autoPlaySerializedObject.FindProperty("_delay");
            }
            _permission = _target.GetComponentInChildren<Permission>();
            if (_permission != null)
            {
                _permissionSerializedObject = new SerializedObject(_permission);
                _defaultPermission = _permissionSerializedObject.FindProperty("_defaultPermission");
                _ownerList = _permissionSerializedObject.FindProperty("_ownerList");
            }
            _uiController = _target.GetComponentInChildren<UIController>(true);
            if (_uiController != null) _uiEditor = new UIEditor(_uiController);
            _avPro = _target.GetComponentInChildren<VRCAVProVideoPlayer>();
            if (_avPro != null )
            {
                _avProSerializedObject = new SerializedObject(_avPro);
                _useLowLatency = _avProSerializedObject.FindProperty("useLowLatency");
            }
            _screenList = new ReorderableList(_controllerSerializedObject, _screens)
            {
                drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Screen targets", EditorStyles.boldLabel),
                onAddCallback = (list) =>
                {
                    _screenTypes.arraySize += 1;
                    _screens.arraySize += 1;
                    _textureProperties.arraySize += 1;
                    _avProProperties.arraySize += 1;
                    _screenTypes.GetArrayElementAtIndex(_screenTypes.arraySize - 1).intValue = (int)ScreenType.Renderer;
                    _screens.GetArrayElementAtIndex(_screens.arraySize - 1).objectReferenceValue = null;
                    _textureProperties.GetArrayElementAtIndex(_textureProperties.arraySize - 1).stringValue = "_MainTex";
                    _avProProperties.GetArrayElementAtIndex(_avProProperties.arraySize - 1).stringValue = "_AVPro";
                },
                onRemoveCallback = (list) =>
                {
                    _screenTypes.DeleteArrayElementAtIndex(list.index);
                    _screens.DeleteArrayElementAtIndex(list.index);
                    _textureProperties.DeleteArrayElementAtIndex(list.index);
                    _avProProperties.DeleteArrayElementAtIndex(list.index);
                },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    SerializedProperty screenType = _screenTypes.GetArrayElementAtIndex(index);
                    SerializedProperty screen = _screens.GetArrayElementAtIndex(index);
                    SerializedProperty textureProperty = _textureProperties.GetArrayElementAtIndex(index);
                    SerializedProperty avProProperty = _avProProperties.GetArrayElementAtIndex(index);
                    rect.height = EditorGUIUtility.singleLineHeight;
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        EditorGUI.PropertyField(rect, screen, new GUIContent("Screen target"));
                        if (check.changed)
                        {
                            if (screen.objectReferenceValue is Material) screenType.intValue = (int)ScreenType.Material;
                            else if (screen.objectReferenceValue is CustomRenderTexture crt)
                            {
                                screenType.intValue = (int)ScreenType.Material;
                                screen.objectReferenceValue = crt.material;
                            }
                            else if (screen.objectReferenceValue is GameObject)
                            {
                                if (((GameObject)screen.objectReferenceValue).TryGetComponent(out Renderer renderer))
                                {
                                    screenType.intValue = (int)ScreenType.Renderer;
                                    screen.objectReferenceValue = renderer;
                                }
                                else if (((GameObject)screen.objectReferenceValue).TryGetComponent(out RawImage rawImage))
                                {
                                    screenType.intValue = (int)ScreenType.RawImage;
                                    screen.objectReferenceValue = rawImage;
                                }
                                else if (((GameObject)screen.objectReferenceValue).TryGetComponent(out YamaPlayerController controller) &&
                                controller.GetComponentInChildren<Renderer>() != null)
                                {
                                    screenType.intValue = (int)ScreenType.Renderer;
                                    screen.objectReferenceValue = controller.GetComponentInChildren<Renderer>();
                                }
                                else screen.objectReferenceValue = null;
                            }
                            else if (screen.objectReferenceValue is Renderer) screenType.intValue = (int)ScreenType.Renderer;
                            else if (screen.objectReferenceValue is RawImage) screenType.intValue = (int)ScreenType.RawImage;
                            else if (screen.objectReferenceValue is Material) screenType.intValue = (int)ScreenType.Material;
                            else screen.objectReferenceValue = null;

                            if (screen.objectReferenceValue != null)
                            {
                                Shader shader = null;
                                if (screen.objectReferenceValue is Renderer renderer) shader = renderer.sharedMaterial.shader;
                                else if (screen.objectReferenceValue is RawImage rawImage) shader = rawImage.material.shader;
                                else if (screen.objectReferenceValue is Material material) shader = material.shader;
                                int count = ShaderUtil.GetPropertyCount(shader);
                                if (count > 0)
                                {
                                    int[] tex = Array.FindAll(
                                    Enumerable.Range(0, count).ToArray(),
                                    x => ShaderUtil.GetPropertyType(shader, x) == ShaderUtil.ShaderPropertyType.TexEnv);
                                    if (tex.Length > 0) textureProperty.stringValue = ShaderUtil.GetPropertyName(shader, tex[0]);
                                }
                            }
                        }
                    }
                    rect.y += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(rect, textureProperty, new GUIContent("Main texture property"));
                    rect.y += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(rect, avProProperty, new GUIContent("AVPro flag property"));
                },
                elementHeight = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 3,
            };
        }



        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();

            EditorGUILayout.LabelField($"YamaPlayer v{Utils.GetYamaPlayerPackageInfo().version}", Styles.Title);
            EditorGUILayout.Space(32f);

            if (EditorApplication.isPlaying) return;

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.Space();
                _tab = (Tab)GUILayout.Toolbar((int)_tab, Enum.GetNames(typeof(Tab)).Select(x => new GUIContent(x)).ToArray(), "LargeButton", GUI.ToolbarButtonSize.Fixed);
                EditorGUILayout.Space();
            }
            EditorGUILayout.Space(16f);

            switch (_tab)
            {
                case Tab.UI:
                    _uiEditor.DrawUISettings();
                    break;
                case Tab.Settings:
                    drawDefaultSettings();
                    break;
                case Tab.Playlist:
                    drawPlaylistSettings(); 
                    break;
                case Tab.Permission:
                    drawPermissionSettings();
                    break;
                case Tab.Other:
                    drawOtherView();
                    break;
            }

            ApplyModifiedProperties();
        }

        internal void ApplyModifiedProperties()
        {
            serializedObject.ApplyModifiedProperties();
            _controllerSerializedObject?.ApplyModifiedProperties();
            _autoPlaySerializedObject?.ApplyModifiedProperties();
            _permissionSerializedObject?.ApplyModifiedProperties();
            _avProSerializedObject?.ApplyModifiedProperties();
        }

        #region Default Settings
        void drawPlaylistPopup()
        {
            string[] playlistNames = _playlists.Select(i => i.PlayListName.Replace("/", "|")).ToArray();
            _autoPlayPlaylistIndex.intValue = EditorGUILayout.Popup(
                label: new GUIContent("Playlist"),
                selectedIndex: _autoPlayPlaylistIndex.intValue,
                displayedOptions: playlistNames
            );

            string[] playlistItemNames = _playlists[_autoPlayPlaylistIndex.intValue].Tracks.Select(i => i.Title.Replace("/", "|")).ToArray();
            if (_autoPlayPlaylistTrackIndex.intValue >= playlistItemNames.Length) _autoPlayPlaylistTrackIndex.intValue = playlistItemNames.Length - 1;
            _autoPlayPlaylistTrackIndex.intValue = EditorGUILayout.Popup(
                label: new GUIContent("Track"),
                selectedIndex: _autoPlayPlaylistTrackIndex.intValue,
                displayedOptions: playlistItemNames
            );
        }

        void drawDefaultSettings()
        {
            if (_controller == null) return;
            EditorGUILayout.PropertyField(_defaultPlayerEngine);
            EditorGUILayout.LabelField("　", "Select the default player engine to play video.");
            Styles.DrawDivider();

            if (_autoPlay != null)
            {
                EditorGUILayout.PropertyField(_autoPlayMode);
                if ((AutoPlayMode)_autoPlayMode.intValue != AutoPlayMode.Off)
                {
                    switch ((AutoPlayMode)_autoPlayMode.intValue)
                    {
                        case AutoPlayMode.FromTrack:
                            EditorGUILayout.PropertyField(_autoPlayVideoPlayerType);
                            EditorGUILayout.PropertyField(_autoPlayVideoTitle);
                            EditorGUILayout.PropertyField(_autoPlayVideoUrl);
                            break;
                        case AutoPlayMode.FromPlaylist:
                            _playlists = _target.GetComponentsInChildren<PlayList>();
                            if (_playlists.Length > 0) drawPlaylistPopup();
                            else EditorGUILayout.HelpBox("No Playlist.", MessageType.Error, false);
                            break;
                    }
                    EditorGUILayout.PropertyField(_autoPlayDelay);
                    EditorGUILayout.LabelField("　", "Auto play video after seconds.");
                }
            }
            Styles.DrawDivider();

            EditorGUILayout.LabelField("Volume", Styles.Bold);
            EditorGUILayout.PropertyField(_mute);
            EditorGUILayout.PropertyField(_volume);
            Styles.DrawDivider();

            EditorGUILayout.LabelField("Display", Styles.Bold);
            EditorGUILayout.PropertyField(_mirrorInverse);
            EditorGUILayout.PropertyField(_emission);
            Styles.DrawDivider();

            EditorGUILayout.LabelField("Playback", Styles.Bold);
            EditorGUILayout.PropertyField(_loop);
            if (_useLowLatency != null) EditorGUILayout.PropertyField(_useLowLatency);
            Styles.DrawDivider();

            EditorGUILayout.LabelField("Playlist", Styles.Bold);
            EditorGUILayout.PropertyField(_shuffle);
            EditorGUILayout.PropertyField(_forwardInterval);
            EditorGUILayout.LabelField("　", "Play next track after seconds.");
            EditorGUILayout.LabelField("　", "Disable when value is smaller then 0.");
            Styles.DrawDivider();

            if (_screenList != null) _screenList.DoLayoutList();

        }
        #endregion

        #region Playlist Settings
        void drawPlaylistSettings()
        {
            EditorGUILayout.LabelField("Playlist / プレイリスト", Styles.Bold);
            if (GUILayout.Button("Edit Playlist")) PlaylistEditor.ShowPlaylistEditorWindow(_target);
        }
        #endregion

        #region Permission Settings
        void drawPermissionSettings()
        {
            EditorGUILayout.LabelField("Permission / 権限", Styles.Bold);
            EditorGUILayout.PropertyField(_defaultPermission);
            EditorGUILayout.PropertyField(_ownerList);
        }
        #endregion

        #region Other Settings
        void drawOtherView()
        {
            VersionManager.AutoUpdate = EditorGUILayout.Toggle("Auto Update", VersionManager.AutoUpdate);
            EditorGUILayout.LabelField("　", "Update to latest version automatically.");
            Styles.DrawDivider();

            EditorGUILayout.LabelField("Current Version", Utils.GetYamaPlayerPackageInfo().version);
            EditorGUILayout.LabelField("Newest Version", VersionManager.Newest);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Check Update"))
                {
                    if (VersionManager.CheckUpdate())
                    {
                        if (EditorUtility.DisplayDialog($"New version found", $"New version found: {VersionManager.Newest}.", "Update", "Cancel")) 
                            VersionManager.UpdatePackage();
                    }
                    else EditorUtility.DisplayDialog($"No new version found", $"No new version found.", "OK");
                }
                EditorGUI.BeginDisabledGroup(!VersionManager.HasNewVersion);
                if (GUILayout.Button("Update")) VersionManager.UpdatePackage();
                EditorGUI.EndDisabledGroup();
            }

        }
        #endregion
    }
}