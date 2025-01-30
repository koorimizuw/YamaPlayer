using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Yamadev.YamaStream.Script;

namespace Yamadev.YamaStream.Editor
{
    public class PlaylistEditor : EditorWindow
    {
        YamaPlayer _player;
        List<Playlist> _playlists;
        ReorderableList _playlistsTable;
        ReorderableList _playlistTracksTable;
        Vector2 _leftScrollPos, _rightScrollPos;
        Playlist _selectedPlaylist;
        bool _useYoutubePlaylistName;
        VideoPlayerType _defaultTrackMode = VideoPlayerType.AVProVideoPlayer;
        bool _isDirty;

        public YamaPlayer YamaPlayer
        {
            get => _player;
            set
            {
                if (_player == value) return;
                ConfirmSave();
                _player = value;
                ReadInternalPlaylists();
            }
        }

        [MenuItem("YamaPlayer/Edit Playlist")]
        public static void ShowPlaylistEditorWindow()
        {
            PlaylistEditor window = GetWindow<PlaylistEditor>(title: "YamaPlayer Playlist Editor");
            window.Show();
        }

        public static void ShowPlaylistEditorWindow(YamaPlayer player)
        {
            PlaylistEditor window = GetWindow<PlaylistEditor>(title: "YamaPlayer Playlist Editor");
            window.YamaPlayer = player;
            window.Show();
        }

        void OnEnable()
        {
            if (_player == null) return;
            ReadInternalPlaylists();
        }

        void OnDisable()
        {
            ConfirmSave();
        }

        public void ReadInternalPlaylists()
        {
            PlayListContainer container = _player?.GetComponentInChildren<PlayListContainer>();
            if (container == null) return;
            _playlists = container.ReadPlaylists();
            GeneratePlaylistsView();
        }

        public void GeneratePlaylistsView()
        {
            _playlistsTable = new ReorderableList(_playlists, typeof(Playlist), true, false, true, true)
            {
                onAddCallback = (list) =>
                {
                    _playlists.Add(new Playlist 
                    {  
                        Active = true, 
                        Name = Localization.Get("newPlaylist"), 
                        Tracks = new List<PlaylistTrack>() 
                    });
                    _isDirty = true;
                },
                onRemoveCallback = (list) =>
                {
                    _playlists.RemoveAt(list.index);
                    list.index = _playlists.Count > 0 ? _playlists.Count - 1 : 0;
                    GeneratePlaylistTracksView(list);
                    _isDirty = true;
                },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    Playlist playlist = _playlists[index];
                    rect.height = EditorGUIUtility.singleLineHeight;
                    Rect nameRect = rect;
                    nameRect.xMax = rect.width - 24;
                    if (playlist.IsEdit) playlist.Name = EditorGUI.TextField(nameRect, playlist.Name);
                    else EditorGUI.LabelField(nameRect, $"{playlist.Name} ({playlist.Tracks.Count})");
                    Rect btnRect = rect;
                    btnRect.xMin = nameRect.xMax;
                    if (playlist.IsEdit)
                    {
                        if (GUI.Button(btnRect, Localization.Get("save")))
                        {
                            playlist.IsEdit = false;
                            _isDirty = true;
                        }
                    }
                    else
                    {
                        if (GUI.Button(btnRect, Localization.Get("edit"))) playlist.IsEdit = true;
                    }
                    rect.y += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
                    Rect activeRect = rect;
                    activeRect.xMax = rect.width;
                    EditorGUI.LabelField(
                        activeRect,
                        playlist.Active ? Localization.Get("active") : Localization.Get("inactive"),
                        new GUIStyle() { normal = new GUIStyleState() { textColor = playlist.Active ? Color.green : Color.red } }
                    );
                    Rect toggleRect = rect;
                    toggleRect.xMin = activeRect.xMax;
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        playlist.Active = EditorGUI.Toggle(toggleRect, playlist.Active);
                        if (check.changed) _isDirty = true;
                    }
                },
                onSelectCallback = GeneratePlaylistTracksView,
                onReorderCallback = (ReorderableList list) => _isDirty = true,
                elementHeight = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2,
                showDefaultBackground = false,
            };
        }

        public void GeneratePlaylistTracksView(ReorderableList selected)
        {
            if (selected.index >= _playlists.Count)
            {
                _selectedPlaylist.Tracks = null;
                _playlistTracksTable = null;
                return;
            }
            _selectedPlaylist = _playlists[selected.index];
            _playlistTracksTable = new ReorderableList(_selectedPlaylist.Tracks, typeof(PlaylistTrack), true, false, true, true)
            {
                onAddCallback = (list) =>
                {
                    ReorderableList.defaultBehaviours.DoAddButton(list);
                    _selectedPlaylist.Tracks[_selectedPlaylist.Tracks.Count - 1].Mode = VideoPlayerType.AVProVideoPlayer;
                    _isDirty = true;
                },
                onRemoveCallback = (list) =>
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(list);
                    _isDirty = true;
                },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    PlaylistTrack track = _selectedPlaylist.Tracks[index];
                    rect.height = EditorGUIUtility.singleLineHeight;
                    float labelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 80;
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        Rect numberRect = rect;
                        string number = $"#{index + 1}";
                        numberRect.xMin = rect.width - number.Length * 8f + 20f;
                        EditorGUI.LabelField(numberRect, number);
                        Rect playerRect = rect;
                        playerRect.xMax = 240;
                        VideoPlayerType mode = (VideoPlayerType)EditorGUI.Popup(playerRect, Localization.Get("videoPlayerType"), (int)track.Mode, Enum.GetNames(typeof(VideoPlayerType)));
                        rect.y += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
                        string title = EditorGUI.TextField(rect, Localization.Get("title"), track.Title);
                        rect.y += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
                        string url = EditorGUI.TextField(rect, "Url", track.Url);
                        if (check.changed)
                        {
                            track.Mode = mode;
                            track.Title = title;
                            track.Url = url;
                            _isDirty = true;
                        }
                    }
                    EditorGUIUtility.labelWidth = labelWidth;
                },
                onReorderCallback = (ReorderableList list) => _isDirty = true,
                elementHeight = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 3,
                showDefaultBackground = false,
            };
        }

        void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                YamaPlayer = EditorGUILayout.ObjectField(YamaPlayer, typeof(YamaPlayer), true) as YamaPlayer;
                if (GUILayout.Button(Localization.Get("importFromJson"), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false))) Import();
                if (GUILayout.Button(Localization.Get("exportToJson"), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false))) Export();
                using (new EditorGUI.DisabledGroupScope(!_isDirty))
                    if (GUILayout.Button(Localization.Get("save"), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false))) Save();
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                using (var vert = new EditorGUILayout.VerticalScope(GUILayout.MaxWidth(380)))
                {
                    HandleDragEvent(vert.rect);
                    EditorGUILayout.LabelField(Localization.Get("playlists"), Styles.Bold);
                    _leftScrollPos = EditorGUILayout.BeginScrollView(_leftScrollPos, GUI.skin.box);
                    if (_player != null) _playlistsTable?.DoLayoutList();
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.HelpBox(Localization.Get("importFromPlayer"), MessageType.Info);
                    EditorGUILayout.EndScrollView();
                    DrawPlaylistSettings();
                }
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField(Localization.Get("playlistTracks"), Styles.Bold);
                    _rightScrollPos = EditorGUILayout.BeginScrollView(_rightScrollPos, GUI.skin.box);
                    if (_player != null) _playlistTracksTable?.DoLayoutList();
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndScrollView();
                }
            }
        }

        void HandleDragEvent(Rect rect)
        {
            switch (Event.current.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!rect.Contains(Event.current.mousePosition)) break;
                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                    if (Event.current.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        _playlists.AddRange(PlaylistImporter.ReadPlaylists(DragAndDrop.objectReferences));
                    }
                    Event.current.Use();
                    break;
            }
        }

        void DrawPlaylistSettings()
        {
            if (_selectedPlaylist == null) return;
            EditorGUILayout.LabelField("プレイリスト設定", Styles.Bold);
            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    _defaultTrackMode = (VideoPlayerType)EditorGUILayout.Popup(Localization.Get("videoPlayerType"), (int)_defaultTrackMode, Enum.GetNames(typeof(VideoPlayerType)));
                    if (GUILayout.Button(Localization.Get("applyForAll"), GUILayout.ExpandWidth(false)))
                    {
                        for (int i = 0; i < _selectedPlaylist.Tracks.Count; i++)
                            _selectedPlaylist.Tracks[i].Mode = _defaultTrackMode;
                        _isDirty = true;
                    }
                }
                _useYoutubePlaylistName = EditorGUILayout.Toggle(Localization.Get("overwritePlaylistName"), _useYoutubePlaylistName);
                EditorGUILayout.Space();
                using (new EditorGUILayout.HorizontalScope())
                {
                    _selectedPlaylist.YoutubeListId = EditorGUILayout.TextField(_selectedPlaylist.YoutubeListId);
                    if (GUILayout.Button(Localization.Get("loadYoutubePlaylist"), GUILayout.ExpandWidth(false)))
                    {
                        ReadYouTubePlaylist().Forget();
                    }
                }
            }
        }

        public async UniTask ReadYouTubePlaylist()
        {
            _selectedPlaylist.YoutubeListId = PlaylistUtils.GetPlaylistIdFromUrl(_selectedPlaylist.YoutubeListId);
            Playlist pl = await PlaylistImporter.GetYouTubePlaylist(_selectedPlaylist.YoutubeListId);
            if (_useYoutubePlaylistName) _selectedPlaylist.Name = pl.Name;
            _selectedPlaylist.Tracks = pl.Tracks;
            _isDirty = true;
            GeneratePlaylistTracksView(_playlistsTable);
        }

        void ConfirmSave()
        {
            if (_isDirty && 
                EditorUtility.DisplayDialog(Localization.Get("notSaved"), Localization.Get("confirmSave"), Localization.Get("save"), Localization.Get("notSave")))
                Save();
        }

        void Save()
        {
            PlayListContainer container = _player.GetComponentInChildren<PlayListContainer>();
            if (container == null) return;
            while (container.transform.childCount > 1) GameObject.DestroyImmediate(container.transform.GetChild(1).gameObject);
            for (int i = 0; i < _playlists.Count; i++)
            {
                Playlist playlist = _playlists[i];
                GameObject obj = new GameObject(playlist.Name);
                SerializedObject serializedObject = new SerializedObject(obj.AddComponent<PlayList>());
                serializedObject.FindProperty("playListName").stringValue = playlist.Name;
                serializedObject.FindProperty("YouTubePlayListID").stringValue = playlist.YoutubeListId;
                SerializedProperty tracks = serializedObject.FindProperty("tracks");
                tracks.arraySize = playlist.Tracks.Count;
                for (int j = 0; j < playlist.Tracks.Count; j++)
                {
                    tracks.GetArrayElementAtIndex(j).FindPropertyRelative("Mode").intValue = (int)playlist.Tracks[j].Mode;
                    tracks.GetArrayElementAtIndex(j).FindPropertyRelative("Title").stringValue = playlist.Tracks[j].Title;
                    tracks.GetArrayElementAtIndex(j).FindPropertyRelative("Url").stringValue = playlist.Tracks[j].Url;
                }
                serializedObject.ApplyModifiedProperties();
                obj.SetActive(playlist.Active);
                obj.transform.SetParent(container.transform);
                GameObjectUtility.EnsureUniqueNameForSibling(obj);
            }
            _isDirty = false;
        }

        #region Exporter
        [Serializable] class Playlists 
        { 
            public List<Playlist> playlists;  
        }

        public void Export()
        {
            string jsonStr = JsonUtility.ToJson(new Playlists { playlists = _playlists }, true);
            string filePath = EditorUtility.SaveFilePanel("Export playlists", Application.dataPath, "playlists", "json");
            if (!string.IsNullOrEmpty(filePath)) File.WriteAllText(filePath, jsonStr);
        }

        public void Import()
        {
            string filePath = EditorUtility.OpenFilePanel("Import playlists", Application.dataPath, "json");
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return;
            string jsonStr = File.ReadAllText(filePath);
            Playlists imported = JsonUtility.FromJson<Playlists>(jsonStr);
            _playlists.AddRange(imported.playlists);
            _isDirty = true;
            GeneratePlaylistsView();
        }
        #endregion

    }
}