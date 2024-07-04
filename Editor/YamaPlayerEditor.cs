
using System;
using System.Linq;
using UnityEditor;
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
            DefaultSettings,
            Playlist,
            Permission,
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
        SerializedObject _uiControllerSerializedObject;
        SerializedProperty _primaryColor;
        SerializedProperty _secondaryColor;
        SerializedProperty _idleImage;
        SerializedProperty _defaultOpen;
        // avpro
        VRCAVProVideoPlayer _avPro;
        SerializedObject _avProSerializedObject;
        SerializedProperty _useLowLatency;

        YamaPlayer _target;
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
            _uiController = _target.GetComponentInChildren<UIController>();
            if (_uiController != null)
            {
                _uiControllerSerializedObject = new SerializedObject(_uiController);
                _primaryColor = _uiControllerSerializedObject.FindProperty("_primaryColor");
                _secondaryColor = _uiControllerSerializedObject.FindProperty("_secondaryColor");
                _idleImage = _uiControllerSerializedObject.FindProperty("_idleImage");
                _defaultOpen = _uiControllerSerializedObject.FindProperty("_defaultPlaylistOpen");
            }
            _avPro = _target.GetComponentInChildren<VRCAVProVideoPlayer>();
            if (_avPro != null )
            {
                _avProSerializedObject = new SerializedObject(_avPro);
                _useLowLatency = _avProSerializedObject.FindProperty("useLowLatency");
            }
        }



        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();

            EditorGUILayout.LabelField($"YamaPlayer v{Utils.GetYamaPlayerVersion()}", _uiTitle);
            EditorGUILayout.Space();

            if (EditorApplication.isPlaying) return;

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.Space();
                _tab = (Tab)GUILayout.Toolbar((int)_tab, Enum.GetNames(typeof(Tab)).Select(x => new GUIContent(x)).ToArray(), "LargeButton", GUI.ToolbarButtonSize.Fixed);
                EditorGUILayout.Space();
            }

            switch (_tab)
            {
                case Tab.UI:
                    drawUISettings();
                    break;
                case Tab.DefaultSettings:
                    drawDefaultSettings();
                    break;
                case Tab.Playlist:
                    drawPlaylistSettings(); 
                    break;
                case Tab.Permission:
                    drawPermissionSettings();
                    break;
            }

            if (serializedObject.ApplyModifiedProperties()
                || (_autoPlaySerializedObject?.ApplyModifiedProperties() ?? false)
                || (_permissionSerializedObject?.ApplyModifiedProperties() ?? false)
                || (_uiControllerSerializedObject?.ApplyModifiedProperties() ?? false)
                || (_controllerSerializedObject?.ApplyModifiedProperties() ?? false)
                || (_avProSerializedObject?.ApplyModifiedProperties() ?? false)
                ) ApplyModifiedProperties();
        }

        internal void ApplyModifiedProperties()
        {
        }

        #region UI Settings
        void setUIColor()
        {
            foreach (UIColor component in _uiController.GetComponentsInChildren<UIColor>(true))
            {
                if (component.GetProgramVariable("_uiController") == null)
                    component.SetProgramVariable("_uiController", _uiController);
                component.Apply();
            }
        }

        void drawUISettings()
        {
            if (_uiController == null) return;
            EditorGUILayout.PropertyField(_primaryColor);
            EditorGUILayout.PropertyField(_secondaryColor);
            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Preview")) setUIColor();
            }
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Idle Image", _bold);
            EditorGUILayout.PropertyField(_idleImage);
            EditorGUILayout.LabelField("　", "Show image when video not playing.");
            EditorGUILayout.Space();

            if (_defaultOpen != null)
            {
                EditorGUILayout.LabelField("Playlist", _bold);
                EditorGUILayout.PropertyField(_defaultOpen);
                EditorGUILayout.LabelField("　", "Open playlist UI after game started.");
            }
        }
        #endregion

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
            EditorGUILayout.LabelField("Default player type", _bold);
            EditorGUILayout.PropertyField(_defaultPlayerEngine);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Auto Play", _bold);
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
                }
            }
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Volume", _bold);
            EditorGUILayout.PropertyField(_mute);
            EditorGUILayout.PropertyField(_volume);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Display", _bold);
            EditorGUILayout.PropertyField(_mirrorInverse);
            EditorGUILayout.PropertyField(_emission);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Playback", _bold);
            EditorGUILayout.PropertyField(_loop);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Playlist", _bold);
            EditorGUILayout.PropertyField(_shuffle);
            if (_useLowLatency != null) EditorGUILayout.PropertyField(_useLowLatency);
            EditorGUILayout.PropertyField(_forwardInterval);
            EditorGUILayout.LabelField("　", "Play next track after seconds.");
            EditorGUILayout.LabelField("　", "Disable when value is smaller then 0.");

        }
        #endregion

        #region Playlist Settings
        void drawPlaylistSettings()
        {
            EditorGUILayout.LabelField("Playlist / プレイリスト", _bold);
            if (GUILayout.Button("Edit Playlist")) PlaylistEditor.ShowPlaylistEditorWindow(_target);
        }
        #endregion

        #region Permission Settings
        void drawPermissionSettings()
        {
            EditorGUILayout.LabelField("Permission / 権限", _bold);
            EditorGUILayout.PropertyField(_defaultPermission);
            EditorGUILayout.PropertyField(_ownerList);
        }
        #endregion
    }
}