using UnityEditor;
using UnityEngine;
using System.Linq;
using Yamadev.YamaStream.Script;

namespace Yamadev.YamaStream.Editor
{
    public class AutoPlaySettings
    {
        private readonly AutoPlay _autoPlay;
        private readonly SerializedObject _autoPlaySerializedObject;
        private readonly PlayList[] _playlists;

        private readonly SerializedProperty _autoPlayMode;
        private readonly SerializedProperty _autoPlayVideoPlayerType;
        private readonly SerializedProperty _autoPlayVideoTitle;
        private readonly SerializedProperty _autoPlayVideoUrl;
        private readonly SerializedProperty _autoPlayPlaylistIndex;
        private readonly SerializedProperty _autoPlayPlaylistTrackIndex;
        private readonly SerializedProperty _autoPlayDelay;

        public bool IsValid => _autoPlay != null && _autoPlaySerializedObject != null;
        public bool AutoPlayEnabled => _autoPlayMode != null && (AutoPlayMode)_autoPlayMode.intValue != AutoPlayMode.Off;

        public AutoPlaySettings(AutoPlay autoPlay, PlayList[] playlists)
        {
            _autoPlay = autoPlay;
            _playlists = playlists ?? new PlayList[0];

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
        }

        public void DrawSettings()
        {
            if (!IsValid) return;

            if (_autoPlayMode != null)
            {
                EditorGUILayout.PropertyField(_autoPlayMode, Localization.GetLayout("autoPlaySource"));
            }

            if (AutoPlayEnabled)
            {
                DrawAutoPlayModeSpecificSettings();
                DrawDelaySettings();
            }
        }

        private void DrawAutoPlayModeSpecificSettings()
        {
            var currentMode = (AutoPlayMode)_autoPlayMode.intValue;

            switch (currentMode)
            {
                case AutoPlayMode.FromTrack:
                    DrawTrackSettings();
                    break;
                case AutoPlayMode.FromPlaylist:
                    DrawPlaylistSettings();
                    break;
                default:
                    EditorGUILayout.HelpBox($"Unsupported mode: {currentMode}", MessageType.Warning);
                    break;
            }
        }

        private void DrawTrackSettings()
        {
            if (_autoPlayVideoPlayerType != null)
                EditorGUILayout.PropertyField(_autoPlayVideoPlayerType, Localization.GetLayout("videoPlayerType"));

            if (_autoPlayVideoTitle != null)
                EditorGUILayout.PropertyField(_autoPlayVideoTitle, Localization.GetLayout("title"));

            if (_autoPlayVideoUrl != null)
                EditorGUILayout.PropertyField(_autoPlayVideoUrl);
        }

        private void DrawPlaylistSettings()
        {
            if (_playlists != null && _playlists.Length > 0)
            {
                DrawPlaylistPopup();
            }
            else
            {
                EditorGUILayout.HelpBox(Localization.Get("noPlaylist"), MessageType.Error, false);
            }
        }
        private void DrawPlaylistPopup()
        {
            if (_autoPlayPlaylistIndex == null || _autoPlayPlaylistTrackIndex == null)
                return;

            try
            {
                var playlistNames = _playlists.Select(playlist =>
                {
                    if (playlist == null) return "Unknown Playlist";
                    return string.IsNullOrEmpty(playlist.PlayListName) ? "Playlist" : playlist.PlayListName.Replace("/", "|");
                }).ToArray();

                _autoPlayPlaylistIndex.intValue = EditorGUILayout.Popup(
                    label: Localization.GetLayout("playlist"),
                    selectedIndex: Mathf.Clamp(_autoPlayPlaylistIndex.intValue, 0, playlistNames.Length - 1),
                    displayedOptions: playlistNames
                );

                DrawTrackSelector();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"AutoPlaySettings: Error drawing playlist popup - {ex.Message}");
            }
        }

        private void DrawTrackSelector()
        {
            var playlistIndex = _autoPlayPlaylistIndex.intValue;
            if (playlistIndex < 0 || playlistIndex >= _playlists.Length)
            {
                playlistIndex = 0;
            }

            var selectedPlaylist = _playlists[playlistIndex];
            if (selectedPlaylist?.Tracks == null)
            {
                EditorGUILayout.HelpBox("No tracks found in the selected playlist.", MessageType.Error);
                return;
            }

            var trackNames = selectedPlaylist.Tracks
                .Select(track => string.IsNullOrEmpty(track.Title) ? "Untitled Track" : track.Title.Replace("/", "|"))
                .ToList();
            trackNames.Insert(0, Localization.Get("random"));
            var currentTrackIndex = Mathf.Clamp(_autoPlayPlaylistTrackIndex.intValue + 1, 0, trackNames.Count - 1);

            _autoPlayPlaylistTrackIndex.intValue = EditorGUILayout.Popup(
                label: Localization.GetLayout("track"),
                selectedIndex: currentTrackIndex,
                displayedOptions: trackNames.ToArray()
            ) - 1;
        }

        private void DrawDelaySettings()
        {
            if (_autoPlayDelay == null) return;

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(_autoPlayDelay, Localization.GetLayout("delay"));
                EditorGUILayout.LabelField("秒", GUILayout.Width(20));
            }
        }

        public void ApplyModifiedProperties()
        {
            try
            {
                _autoPlaySerializedObject?.ApplyModifiedProperties();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"AutoPlaySettings: Failed to apply modified properties - {ex.Message}");
            }
        }
    }
}