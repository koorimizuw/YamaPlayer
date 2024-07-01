
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Yamadev.YamaStream.Script
{
    public class PlaylistEditor : EditorWindow
    {
        [Serializable]
        class Playlist
        {
            public bool Active;
            public string Name;
            public List<Track> Tracks;
            public string YoutubeListId;
            private bool _edit;
            public bool IsEdit
            {
                get => _edit; 
                set { _edit = value;}
            }
        }

        YamaPlayer _player;
        List<Playlist> _playlists = new List<Playlist>();
        ReorderableList _playlistsTable;
        ReorderableList _playlistTracksTable;
        Vector2 _leftScrollPos, _rightScrollPos;
        Playlist _selectedPlaylist;
        bool _useYoutubePlaylistName;
        TrackMode _defaultTrackMode = TrackMode.AVProPlayer;
        bool _isDirty;

        public YamaPlayer YamaPlayer
        {
            get => _player;
            set
            {
                if (_player == value) return;
                confirmSave();
                _player = value;
                OnEnable();
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
            _playlists = readPlaylists();
            _isDirty = false;
            GeneratePlaylistsView();
        }

        void OnDisable() => confirmSave();

        public void GeneratePlaylistsView()
        {
            _playlistsTable = new ReorderableList(_playlists, typeof(Playlist))
            {
                drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Playlists / プレイリスト", EditorStyles.boldLabel),
                onAddCallback = (list) =>
                {
                    _playlists.Add(new Playlist { Active = true, Name = "New Playlist", Tracks = new List<Track>() });
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
                        if (GUI.Button(btnRect, "Save"))
                        {
                            playlist.IsEdit = false;
                            _isDirty = true;
                        }
                    }
                    else
                    {
                        if (GUI.Button(btnRect, "Edit")) playlist.IsEdit = true;
                    }
                    rect.y += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
                    Rect activeRect = rect;
                    activeRect.xMax = rect.width;
                    EditorGUI.LabelField(
                        activeRect,
                        playlist.Active ? "Active" : "Inactive",
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
                elementHeight = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2,
            };
        }

        public void GeneratePlaylistTracksView(ReorderableList selected)
        {
            if (selected.index >= _playlists.Count)
            {
                _selectedPlaylist = null;
                _playlistTracksTable = null;
                return;
            }
            _selectedPlaylist = _playlists[selected.index];
            _playlistTracksTable = new ReorderableList(_selectedPlaylist.Tracks, typeof(Track))
            {
                drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Playlists Tracks / トラック一覧", EditorStyles.boldLabel),
                onAddCallback = (list) =>
                {
                    ReorderableList.defaultBehaviours.DoAddButton(list);
                    _isDirty = true;
                },
                onRemoveCallback = (list) =>
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(list);
                    _isDirty = true;
                },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    Track track = _selectedPlaylist.Tracks[index];
                    rect.height = EditorGUIUtility.singleLineHeight;
                    float labelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 60;
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        Rect playerRect = rect;
                        playerRect.xMax = 240;
                        TrackMode mode = (TrackMode)EditorGUI.Popup(playerRect, "Player", (int)track.Mode, Enum.GetNames(typeof(TrackMode)));
                        rect.y += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
                        string title = EditorGUI.TextField(rect, "Title", track.Title);
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
                elementHeight = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 3,
            };
        }

        void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                YamaPlayer = EditorGUILayout.ObjectField(YamaPlayer, typeof(YamaPlayer), true) as YamaPlayer;
                if (GUILayout.Button("Import from json", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false))) Import();
                if (GUILayout.Button("Export to json", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false))) Export();
                if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false))) save();
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                using (var vert = new EditorGUILayout.VerticalScope(GUILayout.MaxWidth(380)))
                {
                    handleDragEvent(vert.rect);
                    _leftScrollPos = EditorGUILayout.BeginScrollView(_leftScrollPos, GUI.skin.box);
                    if (_player != null) _playlistsTable?.DoLayoutList();
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndScrollView();
                    EditorGUILayout.HelpBox("Drag iwaSync3 or kinel playlist here to import.", MessageType.Info);
                }
                using (new EditorGUILayout.VerticalScope())
                {
                    _rightScrollPos = EditorGUILayout.BeginScrollView(_rightScrollPos, GUI.skin.box);
                    if (_player != null) _playlistTracksTable?.DoLayoutList();
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndScrollView();
                    if (_selectedPlaylist != null) DrawPlaylistSettings();
                }
            }
        }

        void handleDragEvent(Rect rect)
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
                        handleImportPlaylists(DragAndDrop.objectReferences);
                    }
                    Event.current.Use();
                    break;
            }
        }

        void DrawPlaylistSettings()
        {
            if (_selectedPlaylist == null) return;
            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                using (new EditorGUI.ChangeCheckScope())
                {
                    _defaultTrackMode = (TrackMode)EditorGUILayout.Popup("Video Player Type", (int)_defaultTrackMode, Enum.GetNames(typeof(TrackMode)));
                    _useYoutubePlaylistName = EditorGUILayout.Toggle("Overwrite Playlist Name", _useYoutubePlaylistName);
                }
                EditorGUILayout.Space();
                using (new EditorGUILayout.HorizontalScope())
                {
                    _selectedPlaylist.YoutubeListId = EditorGUILayout.TextField(_selectedPlaylist.YoutubeListId);
                    if (GUILayout.Button("Load Youtube Playlist", GUILayout.ExpandWidth(false)))
                    {
                        _selectedPlaylist.YoutubeListId = getYoutubePlaylistIdFromUrl(_selectedPlaylist.YoutubeListId);
                        getPlayListItem(_selectedPlaylist.YoutubeListId);
                    }
                }
            }
        }

        void confirmSave()
        {
            if (_isDirty && EditorUtility.DisplayDialog("Unsaved Changes", "Do you want to save the changes you made before quitting?", "Save", "Don't Save"))
                save();
        }

        List<Playlist> readPlaylists()
        {
            List<Playlist> results = new List<Playlist>();
            PlayListContainer container = _player?.GetComponentInChildren<PlayListContainer>();
            if (container == null) return results;
            for (int i = 1; i < container.transform.childCount; i++)
            {
                PlayList li = container.transform.GetChild(i).GetComponent<PlayList>();
                results.Add(readPlaylist(li));
            }
            return results;
        }

        Playlist readPlaylist(PlayList li)
        {
            return new Playlist()
            {
                Active = li.gameObject.activeSelf,
                Name = li.PlayListName,
                Tracks = li.Tracks.ToList(),
                YoutubeListId = li.YouTubePlayListID,
            };
        }

        void save()
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

        static string getYoutubePlaylistIdFromUrl(string url)
        {
            if (!url.StartsWith("https://")) return url;
            Uri uri = new Uri(url);
            Dictionary<string, string> queries = uri.Query.Replace("?", "").Split('&').ToDictionary(pair => pair.Split('=').First(), pair => pair.Split('=').Last());
            if (queries.TryGetValue("list", out var list)) return list;
            return string.Empty;
        }

        void getPlayListItem(string playListID)
        {
            string url = $"https://api.yamachan.moe/youtube/playlist?id={playListID}";
            WebRequest request = WebRequest.Create(url);
            request.Method = "Get";
            WebResponse response;
            EditorUtility.DisplayProgressBar("Getting playlist tracks", "Getting playlist tracks", 0);
            response = request.GetResponse();

            if (response != null)
            {
                try
                {
                    Stream st = response.GetResponseStream();
                    StreamReader sr = new StreamReader(st, Encoding.GetEncoding("UTF-8"));
                    string txt = sr.ReadToEnd();
                    sr.Close();
                    st.Close();

                    YoutubePlayList foo = JsonUtility.FromJson<YoutubePlayList>(txt);
                    if (_useYoutubePlaylistName) _selectedPlaylist.Name = foo.title;
                    _selectedPlaylist.Tracks = foo.items.Where((item) => item.title != "Private video").Select((item) => new Track()
                    {
                        Mode = _defaultTrackMode,
                        Title = item.title,
                        Url = $"https://www.youtube.com/watch?v={item.id}",
                    }).ToList();
                    GeneratePlaylistTracksView(_playlistsTable);
                    _isDirty = true;
                } catch (Exception ex)
                {
                    Debug.LogError(ex);
                } finally
                {
                    EditorUtility.ClearProgressBar();
                }
            }
        }

        void handleImportPlaylists(UnityEngine.Object[] objects)
        {
            List<Playlist> results = new List<Playlist>();
            foreach (UnityEngine.Object obj in objects)
            {
                if (obj is GameObject gameObject)
                {
                    foreach (MonoBehaviour script in gameObject.GetComponents<MonoBehaviour>())
                    {
                        switch (script.GetType().ToString())
                        {
                            case "Yamadev.YamaStream.Script.PlayList":
                                results.Add(readPlaylist(script as PlayList));
                                break;
                            case "HoshinoLabs.IwaSync3.Playlist":
                                Playlist iwaSync3Playlist = readPlaylistFromIwaSync3(script);
                                if (iwaSync3Playlist != null) results.Add(iwaSync3Playlist);
                                break;
                            case "HoshinoLabs.IwaSync3.ListTab":
                                results.AddRange(readPlaylistsFromIwaSync3(script));
                                break;
                            case "Kinel.VideoPlayer.Scripts.KinelPlaylistScript":
                                Playlist kinelPlaylist = readPlaylistFromKinelVideoPlayer(script);
                                if (kinelPlaylist != null) results.Add(kinelPlaylist);
                                break;
                            case "Kinel.VideoPlayer.Scripts.KinelPlaylistGroupManagerScript":
                                results.AddRange(readPlaylistsFromKinelVideoPlayer(script));
                                break;
                        }
                    }
                } 
            }
            _playlists.AddRange(results);
            _isDirty = true;
        }

        Playlist readPlaylistFromIwaSync3(dynamic li)
        {
            try
            {
                dynamic[] tracks = ((object)li).GetType().GetField("tracks", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(li);
                dynamic playlistUrl = ((object)li).GetType().GetField("playlistUrl", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(li);
                return new Playlist() {
                    Active = true,
                    Name = "IwaSync3 Playlist",
                    Tracks = tracks.Select((track) => new Track()
                    {
                        Mode = (TrackMode)track.mode,
                        Title = track.title,
                        Url = track.url,
                    }).ToList(),
                    YoutubeListId = getYoutubePlaylistIdFromUrl(playlistUrl),
                };
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            return null;
        }

        List<Playlist> readPlaylistsFromIwaSync3(dynamic listTab, List<dynamic> exclusion = null)
        {
            List<Playlist> results = new List<Playlist>();
            if (exclusion == null) exclusion = new List<dynamic>() { listTab };
            else exclusion.Add(listTab);
            try
            {
                dynamic[] tabs = ((object)listTab).GetType().GetField("tabs", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(listTab);
                foreach (dynamic tab in tabs)
                {
                    switch (((object)tab.list).GetType().ToString())
                    {
                        case "HoshinoLabs.IwaSync3.Playlist":
                            Playlist li = readPlaylistFromIwaSync3(tab.list);
                            if (li != null)
                            {
                                li.Name = tab.title;
                                results.Add(li);
                            }
                            break;
                        case "HoshinoLabs.IwaSync3.ListTab":
                            if (exclusion.IndexOf(tab.list) >= 0) break;
                            results.AddRange(readPlaylistsFromIwaSync3(tab.list, exclusion));
                            break;
                    }
                }
                return results;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            return results;
        }

        Playlist readPlaylistFromKinelVideoPlayer(dynamic li)
        {
            try
            {
                return new Playlist()
                {
                    Active = true,
                    Name = "Kinel Playlist",
                    Tracks = ((dynamic[])li.videoDatas).Select((data) => new Track()
                    {
                        Title = data.title,
                        Url = data.url,
                        Mode = (TrackMode)data.mode,
                    }).ToList(),
                    YoutubeListId = getYoutubePlaylistIdFromUrl(li.playlistUrl),
                };
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            return null;
        }

        List<Playlist> readPlaylistsFromKinelVideoPlayer(dynamic group)
        {
            List<Playlist> results = new List<Playlist>();
            try
            {
                Transform trans = ((MonoBehaviour)group).transform.Find("Playlist");
                for (int i = 0; i < group.playlists.Length; i++)
                {
                    Debug.Log(group.playlists[i]);
                    dynamic script = trans.GetChild(i+1)?.GetComponent(Utils.FindType("Kinel.VideoPlayer.Scripts.KinelPlaylistScript"));
                    Debug.Log(script);
                    if (script == null) continue;
                    Playlist li = readPlaylistFromKinelVideoPlayer(script);
                    if (li != null)
                    {
                        li.Name = group.playlists[i];
                        results.Add(li);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            return results;
        }

        [Serializable] class Playlists { public List<Playlist> playlists;  }

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
    }
}