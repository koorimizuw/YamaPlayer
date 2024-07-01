
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Video.Components.AVPro;

namespace Yamadev.YamaStream.Script
{
    [CustomEditor(typeof(YamaPlayer))]
    internal class YamaPlayerEditor : EditorBase
    {
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

        internal void DrawPlaylistPopup()
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

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();

            EditorGUILayout.LabelField("YamaPlayer", _uiTitle);
            EditorGUILayout.Space();

            if (_uiController != null)
            {
                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    EditorGUILayout.LabelField("UI", _bold);
                    EditorGUILayout.PropertyField(_primaryColor);
                    EditorGUILayout.PropertyField(_secondaryColor);
                    EditorGUILayout.LabelField("Idle", _bold);
                    EditorGUILayout.PropertyField(_idleImage);
                    if (_defaultOpen != null )
                    {
                        EditorGUILayout.LabelField("Playlist", _bold);
                        EditorGUILayout.PropertyField(_defaultOpen);
                    }
                }
                EditorGUILayout.Space();
            }

            if (_controller != null && _autoPlay != null)
            {
                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    EditorGUILayout.LabelField("Video Player Settings / デフォルト設定", _bold);
                    EditorGUILayout.PropertyField(_defaultPlayerEngine);
                    EditorGUILayout.PropertyField(_mute);
                    EditorGUILayout.PropertyField(_volume);
                    EditorGUILayout.PropertyField(_mirrorInverse);
                    EditorGUILayout.PropertyField(_emission);
                    EditorGUILayout.PropertyField(_loop);
                    EditorGUILayout.PropertyField(_shuffle);
                    if (_useLowLatency != null) EditorGUILayout.PropertyField(_useLowLatency);
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
                                if (_playlists.Length > 0) DrawPlaylistPopup();
                                else EditorGUILayout.HelpBox("No Playlist.", MessageType.Error, false);
                                break;
                        }
                        EditorGUILayout.PropertyField(_autoPlayDelay);
                    }
                }
                EditorGUILayout.Space();
            }

            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("Playlist / プレイリスト", _bold);
                if (GUILayout.Button("Edit Playlist")) PlaylistEditor.ShowPlaylistEditorWindow(_target);
            }
            EditorGUILayout.Space();

            if (_permission != null)
            {
                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    EditorGUILayout.LabelField("Permission / 権限", _bold);
                    EditorGUILayout.PropertyField(_defaultPermission);
                    EditorGUILayout.PropertyField(_ownerList);
                }
            }

            if (EditorApplication.isPlaying) return;

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
    }
}