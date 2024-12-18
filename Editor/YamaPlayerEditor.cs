
using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Video.Components.AVPro;
using Yamadev.YamaStream.UI;
using Yamadev.YamaStream.Script;
using System.Collections.Generic;

namespace Yamadev.YamaStream.Editor
{
    [CustomEditor(typeof(YamaPlayer))]
    internal class YamaPlayerEditor : EditorBase
    {
        // controller
        Controller _controller;
        SerializedObject _controllerSerializedObject;
        SerializedProperty _localMode;
        SerializedProperty _useAudioLink;
        SerializedProperty _audioLink;
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
        SerializedProperty _grantInstanceOwner;
        SerializedProperty _grantInstanceMaster;
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
        TabScope _tabScope;
        bool _useLTCGI;

        private void OnEnable()
        {
            Title = $"YamaPlayer v{VersionManager.Version}";

            _target = target as YamaPlayer;

            _controller = _target.GetComponentInChildren<Controller>();
            if (_controller != null )
            {
                _controllerSerializedObject = new SerializedObject(_controller);
                _localMode = _controllerSerializedObject.FindProperty("_isLocal");
                _useAudioLink = _controllerSerializedObject.FindProperty("_useAudioLink");
                _audioLink = _controllerSerializedObject.FindProperty("_audioLink");
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
                _grantInstanceOwner = _permissionSerializedObject.FindProperty("_grantInstanceOwner");
                _grantInstanceMaster = _permissionSerializedObject.FindProperty("_grantInstanceMaster");
            }
            _uiController = _target.GetComponentInChildren<UIController>(true);
            if (_uiController != null)
            {
                _uiEditor = new UIEditor(_uiController);
            }
            _avPro = _target.GetComponentInChildren<VRCAVProVideoPlayer>();
            if (_avPro != null )
            {
                _avProSerializedObject = new SerializedObject(_avPro);
                _useLowLatency = _avProSerializedObject.FindProperty("useLowLatency");
            }

            for (int i = 0; i < _screens.arraySize; i++)
            {
                if (_screens.GetArrayElementAtIndex(i).objectReferenceValue == LTCGIUtils.YamaPlayerCRT.material)
                    _useLTCGI = true;
            }

            GenerateScreenList();

            _tabScope = new TabScope(new List<TabScope.Tab>()
            {
                new TabScope.Tab("UI", _uiEditor.DrawUISettings),
                new TabScope.Tab("Settings", DrawPlayerSettings),
                new TabScope.Tab("Playlist", DrawPlaylistSettings),
                new TabScope.Tab("Permission", DrawPermissionSettings),
                new TabScope.Tab("Version", DrawVersionSettings),
            });
        }

        public void GenerateScreenList()
        {
            _screenList = new ReorderableList(_controllerSerializedObject, _screens)
            {
                drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, Localization.GetLayout("screenTargets"), EditorStyles.boldLabel),
                onAddCallback = (list) =>
                {
                    _screenTypes.arraySize += 1;
                    _screens.arraySize += 1;
                    _textureProperties.arraySize += 1;
                    _screenTypes.GetArrayElementAtIndex(_screenTypes.arraySize - 1).intValue = (int)ScreenType.Renderer;
                    _screens.GetArrayElementAtIndex(_screens.arraySize - 1).objectReferenceValue = null;
                    _textureProperties.GetArrayElementAtIndex(_textureProperties.arraySize - 1).stringValue = "_MainTex";
                },
                onRemoveCallback = (list) =>
                {
                    _screenTypes.DeleteArrayElementAtIndex(list.index);
                    _screens.DeleteArrayElementAtIndex(list.index);
                    _textureProperties.DeleteArrayElementAtIndex(list.index);
                },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    SerializedProperty screenType = _screenTypes.GetArrayElementAtIndex(index);
                    SerializedProperty screen = _screens.GetArrayElementAtIndex(index);
                    SerializedProperty textureProperty = _textureProperties.GetArrayElementAtIndex(index);
                    rect.height = EditorGUIUtility.singleLineHeight;
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        EditorGUI.PropertyField(rect, screen, Localization.GetLayout("screen"));
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
                    EditorGUI.PropertyField(rect, textureProperty, Localization.GetLayout("mainTextureProperty"));
                },
                elementHeight = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2,
            };
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();

            EditorGUILayout.Space(16f);

            if (EditorApplication.isPlaying) return;

            _tabScope.Draw();

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

        #region Player Settings
        public void DrawPlaylistPopup()
        {
            string[] playlistNames = _playlists.Select(i => i.PlayListName.Replace("/", "|")).ToArray();
            _autoPlayPlaylistIndex.intValue = EditorGUILayout.Popup(
                label: Localization.GetLayout("playlist"),
                selectedIndex: _autoPlayPlaylistIndex.intValue,
                displayedOptions: playlistNames
            );

            List<string> playlistItemNames = _playlists[_autoPlayPlaylistIndex.intValue].Tracks.Select(i => i.Title.Replace("/", "|")).ToList(); 
            playlistItemNames.Insert(0, Localization.Get("random"));
            if (_autoPlayPlaylistTrackIndex.intValue >= playlistItemNames.Count - 1) _autoPlayPlaylistTrackIndex.intValue = playlistItemNames.Count - 2;
            _autoPlayPlaylistTrackIndex.intValue = EditorGUILayout.Popup(
                label: Localization.GetLayout("track"),
                selectedIndex: _autoPlayPlaylistTrackIndex.intValue + 1,
                displayedOptions: playlistItemNames.ToArray()
            ) - 1;
        }

        public void SetUpLTCGI()
        {
#if LTCGI_INCLUDED
            if (_controller == null) return;
            foreach (Controller controller in Utils.FindComponentsInHierarthy<Controller>())
            {
                if (Array.IndexOf(controller.Screens, LTCGIUtils.YamaPlayerCRT.material) >= 0 &&
                    controller != _controller &&
                    !Styles.DisplayConfirmDialog(Localization.Get("ltcgiSetOnOtherPlayer"), Localization.Get("clearLTCGISettings"))) return;
            }
            LTCGIUtils.ClearLTCGISettings();
            _controller.AddScreenProperty(ScreenType.Material, LTCGIUtils.YamaPlayerCRT.material);
            var ltcgiController = LTCGIUtils.GetOrAddLTCGIController();
            if (ltcgiController == null)
            {
                EditorUtility.DisplayDialog(Localization.Get("setUpFailed"), Localization.Get("setUpLTCGIFailed"), "OK");
            }
            ltcgiController.VideoTexture = LTCGIUtils.YamaPlayerCRT;
            bool applyToSubScreens = Styles.DisplayConfirmDialog(Localization.Get("applyToSubScreens"), Localization.Get("applyToSubScreensConfirm"));
            _target.SetUpLTCGIScreen(applyToSubScreens);
            _useLTCGI = true;
            GenerateScreenList();
#endif
        }

        public void RemoveLTCGI()
        {
#if LTCGI_INCLUDED
            if (_controller == null) return;
            if (Styles.DisplayConfirmDialog(Localization.Get("removeLTCGI"), Localization.Get("removeLTCGIConfirm")))
            {
                LTCGIUtils.ClearLTCGISettings();
                _useLTCGI = false;
                GenerateScreenList();
            }
#endif
        }

        public void DrawPlayerSettings()
        {
            if (_controller == null) return;
            using (new SectionScope(Localization.Get("syncSettings")))
            {
                EditorGUILayout.PropertyField(_defaultPlayerEngine, Localization.GetLayout("videoPlayerType"));
                EditorGUILayout.LabelField("　", Localization.Get("selectDefaultVideoPlayerType"));
            }

            if (_autoPlay != null)
            {
                using (new SectionScope(Localization.Get("autoPlay")))
                {
                    EditorGUILayout.PropertyField(_autoPlayMode, Localization.GetLayout("autoPlaySource"));
                    if ((AutoPlayMode)_autoPlayMode.intValue != AutoPlayMode.Off)
                    {
                        switch ((AutoPlayMode)_autoPlayMode.intValue)
                        {
                            case AutoPlayMode.FromTrack:
                                EditorGUILayout.PropertyField(_autoPlayVideoPlayerType, Localization.GetLayout("videoPlayerType"));
                                EditorGUILayout.PropertyField(_autoPlayVideoTitle, Localization.GetLayout("title"));
                                EditorGUILayout.PropertyField(_autoPlayVideoUrl);
                                break;
                            case AutoPlayMode.FromPlaylist:
                                _playlists = _target.GetComponentsInChildren<PlayList>();
                                if (_playlists.Length > 0) DrawPlaylistPopup();
                                else EditorGUILayout.HelpBox(Localization.Get("noPlaylist"), MessageType.Error, false);
                                break;
                        }
                        EditorGUILayout.PropertyField(_autoPlayDelay, Localization.GetLayout("delay"));
                        EditorGUILayout.LabelField("　", string.Format(Localization.Get("autoPlayAfterSeconds"), _autoPlayDelay.floatValue));
                    }
                }
            }

            using (new SectionScope(Localization.Get("syncSettings")))
            {
                EditorGUILayout.PropertyField(_localMode, Localization.GetLayout("localMode"));
            }

            using (new SectionScope(Localization.Get("audioSettings")))
            {
                EditorGUILayout.PropertyField(_mute, Localization.GetLayout("mute"));
                EditorGUILayout.PropertyField(_volume, Localization.GetLayout("volume"));
            }

            using (new SectionScope(Localization.Get("videoSettings")))
            {
                EditorGUILayout.PropertyField(_mirrorInverse, Localization.GetLayout("mirrorInverse"));
                EditorGUILayout.PropertyField(_emission, Localization.GetLayout("brightness"));
            }

            using (new SectionScope(Localization.Get("playbackSettings")))
            {
                EditorGUILayout.PropertyField(_loop, Localization.GetLayout("loop"));
                if (_useLowLatency != null) EditorGUILayout.PropertyField(_useLowLatency, Localization.GetLayout("useLowLatency"));
            }

            using (new SectionScope(Localization.Get("externalSettings")))
            {
#if AUDIOLINK_V1
                EditorGUILayout.PropertyField(_useAudioLink, Localization.GetLayout("useAudioLink"));
                if (_useAudioLink.boolValue)
                {
                    EditorGUILayout.PropertyField(_audioLink);
                    if (_audioLink.objectReferenceValue == null)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button(Localization.Get("setUpAudioLink")) &&
                                Styles.DisplayConfirmDialog(Localization.Get("setUpAudioLink"), Localization.Get("setUpAudioLinkConfirm"))) 
                                _audioLink.objectReferenceValue = AudioLinkUtils.GetOrAddAudioLink();
                        }
                    }
                }
#else
                EditorGUILayout.LabelField("Audio Link", Localization.Get("audioLinkNotImported"));
#endif
#if LTCGI_INCLUDED
                if (EditorGUILayout.Toggle(Localization.Get("useLTCGI"), _useLTCGI))
                {
                    if (!_useLTCGI) SetUpLTCGI();
                }
                else
                {
                    if (_useLTCGI) RemoveLTCGI();
                }
                // EditorGUILayout.LabelField("　", Localization.Get("useLTCGIDesc"));
    #else
                EditorGUILayout.LabelField("LTCGI", Localization.Get("ltcgiNotImported"));
#endif
            }
            _screenList?.DoLayoutList();

        }
#endregion

        #region Playlist Settings
        public void DrawPlaylistSettings()
        {
            using (new SectionScope(Localization.Get("playlist")))
            {
                if (GUILayout.Button(Localization.Get("editPlaylist"))) 
                    PlaylistEditor.ShowPlaylistEditorWindow(_target);
            }

            using (new SectionScope())
            {
                EditorGUILayout.PropertyField(_shuffle, Localization.GetLayout("shuffle"));
                EditorGUILayout.LabelField("　", Localization.Get("playInRandomOrder"));
            }

            using (new SectionScope(false))
            {
                EditorGUILayout.PropertyField(_forwardInterval, Localization.GetLayout("forwardInterval"));
                EditorGUILayout.LabelField("　", string.Format(Localization.Get("playTrackAfterSeconds"), _forwardInterval.floatValue));
                EditorGUILayout.LabelField("　", Localization.Get("disableSmallerThen0"));
            }
        }
        #endregion

        #region Permission Settings
        public void DrawPermissionSettings()
        {
            EditorGUILayout.LabelField("Owner:\t\t" + Localization.Get("ownerPermission"));
            EditorGUILayout.LabelField("Admin:\t\t" + Localization.Get("adminPermission"));
            EditorGUILayout.LabelField("Editor:\t\t" + Localization.Get("editorPermission"));
            EditorGUILayout.LabelField("Viewer:\t\t" + Localization.Get("viewerPermission"));
            EditorGUILayout.Space(12f);
            EditorGUILayout.PropertyField(_defaultPermission, Localization.GetLayout("defaultPermission"));
            EditorGUILayout.PropertyField(_grantInstanceOwner, Localization.GetLayout("grantInstanceOwner"));
            EditorGUILayout.LabelField("　", Localization.Get("grantInstanceOwnerDesc"));
            EditorGUILayout.PropertyField(_grantInstanceMaster, Localization.GetLayout("grantInstanceMaster"));
            EditorGUILayout.LabelField("　", Localization.Get("grantInstanceMasterDesc"));
            EditorGUILayout.PropertyField(_ownerList, Localization.GetLayout("ownerList"));
        }
        #endregion

        #region Version Settings
        public void DrawVersionSettings()
        {
#if USE_VPM_RESOLVER
            using (new SectionScope())
            {
                VersionManager.AutoUpdate = EditorGUILayout.Toggle(Localization.Get("autoUpdate"), VersionManager.AutoUpdate);
                EditorGUILayout.LabelField("　", Localization.Get("autoUpdateToLatestVersion"));
            }

            using (new SectionScope(false))
            {
                VersionManager.CheckBetaVersion = EditorGUILayout.Toggle(Localization.Get("checkBetaVersion"), VersionManager.CheckBetaVersion);
                EditorGUILayout.LabelField(Localization.Get("currentVersion"), VersionManager.Version);
                EditorGUILayout.LabelField(Localization.Get("newestVersion"), VersionManager.Newest);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(Localization.Get("checkUpdate")))
                    {
                        if (VersionManager.CheckUpdate())
                        {
                            if (EditorUtility.DisplayDialog(
                                Localization.Get("newVersionFound"),
                                string.Format(Localization.Get("newVersionUpdateConfirm"), VersionManager.Newest),
                                Localization.Get("doUpdate"),
                                Localization.Get("cancel"))
                                )
                                VersionManager.UpdatePackage();
                        }
                        else EditorUtility.DisplayDialog(Localization.Get("noNewVersionFound"), Localization.Get("youUseNewest"), "OK");
                    }
                    EditorGUI.BeginDisabledGroup(!VersionManager.HasNewVersion);
                    if (GUILayout.Button(Localization.Get("update"))) VersionManager.UpdatePackage();
                    EditorGUI.EndDisabledGroup();
                }
            }
#else
            EditorGUILayout.LabelField(Localization.Get("onlyWorksOnVCCProject"));
#endif
        }
        #endregion
    }
}