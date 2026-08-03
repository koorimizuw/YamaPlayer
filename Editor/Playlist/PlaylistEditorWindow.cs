using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Yamadev.YamaStream.Editor
{
  public class PlaylistEditorWindow : EditorWindow
  {
    private YamaPlayer _player;
    private List<PlaylistData> _playlists;
    private ReorderableList _playlistsTable;
    private ReorderableList _playlistTracksTable;
    private Vector2 _leftScrollPos, _rightScrollPos;
    private PlaylistData _selectedPlaylist;
    private bool _useYoutubePlaylistName;
    private VideoPlayerType _defaultTrackMode = VideoPlayerType.AVProVideoPlayer;
    private bool _isDirty;
    private bool IsDirty
    {
      get => _isDirty;
      set
      {
        _isDirty = value;
        hasUnsavedChanges = value;
        if (value)
        {
          saveChangesMessage = EditorLocalization.Get("msg.confirmSave");
        }
      }
    }

    public YamaPlayer YamaPlayer
    {
      get => _player;
      set
      {
        if (_player == value) return;
        if (!ConfirmSave()) return;
        _player = value;
        ReadPlaylists();
      }
    }

    [MenuItem("YamaPlayer/Edit Playlist")]
    public static void ShowPlaylistEditorWindow()
    {
      var window = GetWindow<PlaylistEditorWindow>(title: "YamaPlayer Playlist Editor");
      window.Show();
    }

    public static void ShowPlaylistEditorWindow(YamaPlayer player)
    {
      var window = GetWindow<PlaylistEditorWindow>(title: "YamaPlayer Playlist Editor");
      window.YamaPlayer = player;
      window.Show();
    }

    public static void ShowPlaylistEditorWindow(YamaPlayer player, PlaylistItem targetPlaylist)
    {
      var window = GetWindow<PlaylistEditorWindow>(title: "YamaPlayer Playlist Editor");
      window.YamaPlayer = player;
      window.SelectPlaylist(targetPlaylist);
      window.Show();
    }

    public void SelectPlaylist(PlaylistItem targetPlaylist)
    {
      if (_playlists == null || targetPlaylist == null) return;
      int index = _playlists.FindIndex(p => p.originalItem == targetPlaylist);
      if (index >= 0 && _playlistsTable != null)
      {
        _playlistsTable.index = index;
        GeneratePlaylistTracksView(_playlistsTable);
      }
    }

    private void OnEnable()
    {
      if (_player == null) return;
      ReadPlaylists();
    }

    public override void SaveChanges()
    {
      Save();
      base.SaveChanges();
    }

    public override void DiscardChanges()
    {
      RevertChanges();
      base.DiscardChanges();
    }

    private void ReadPlaylists()
    {
      PlaylistManager container = _player?.GetComponentInChildren<PlaylistManager>();
      if (container == null) return;

      var originalPlaylists = container.GetPlaylists();
      _playlists = originalPlaylists.Select(item => new PlaylistData(item)).ToList();
      _selectedPlaylist = null;
      _playlistTracksTable = null;
      IsDirty = false;
      GeneratePlaylistsView();
    }

    private void RevertChanges()
    {
      IsDirty = false;
      ReadPlaylists();
    }

    private void GeneratePlaylistsView()
    {
      _playlistsTable = new ReorderableList(_playlists, typeof(PlaylistData), true, false, true, true)
      {
        onAddCallback = (list) =>
        {
          _playlists.Add(new PlaylistData
          {
            originalItem = null,
            name = EditorLocalization.Get("playlist.new"),
            active = true,
            youtubeListId = "",
            tracks = new List<PlaylistTrack>(),
            isNameEditing = false
          });
          IsDirty = true;
        },
        onRemoveCallback = (list) =>
        {
          if (list.index < 0 || list.index >= _playlists.Count) return;
          _playlists.RemoveAt(list.index);
          list.index = _playlists.Count > 0 ? _playlists.Count - 1 : 0;
          GeneratePlaylistTracksView(list);
          IsDirty = true;
        },
        drawElementCallback = (rect, index, isActive, isFocused) =>
        {
          if (index >= _playlists.Count) return;
          var playlist = _playlists[index];

          rect.height = EditorGUIUtility.singleLineHeight;
          Rect nameRect = rect;
          nameRect.xMax = rect.width - 24;

          if (playlist.isNameEditing)
          {
            playlist.name = EditorGUI.TextField(nameRect, playlist.name);
          }
          else
          {
            EditorGUI.LabelField(nameRect, $"{playlist.name} ({playlist.tracks?.Count ?? 0})");
          }

          Rect btnRect = rect;
          btnRect.xMin = nameRect.xMax;
          if (playlist.isNameEditing)
          {
            if (GUI.Button(btnRect, EditorLocalization.Get("button.save")))
            {
              playlist.isNameEditing = false;
              IsDirty = true;
            }
          }
          else
          {
            if (GUI.Button(btnRect, EditorLocalization.Get("button.edit"))) playlist.isNameEditing = true;
          }

          rect.y += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
          Rect activeRect = rect;
          activeRect.xMax = rect.width;
          EditorGUI.LabelField(
            activeRect,
            playlist.active ? EditorLocalization.Get("button.active") : EditorLocalization.Get("button.inactive"),
            new GUIStyle() { normal = new GUIStyleState() { textColor = playlist.active ? Color.green : Color.red } }
          );

          Rect toggleRect = rect;
          toggleRect.xMin = activeRect.xMax;
          using (var check = new EditorGUI.ChangeCheckScope())
          {
            bool newActive = EditorGUI.Toggle(toggleRect, playlist.active);
            if (check.changed)
            {
              playlist.active = newActive;
              IsDirty = true;
            }
          }
        },
        onSelectCallback = GeneratePlaylistTracksView,
        onReorderCallback = (ReorderableList list) =>
        {
          IsDirty = true;
        },
        elementHeight = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2,
        showDefaultBackground = false,
      };
    }

    private void GeneratePlaylistTracksView(ReorderableList selected)
    {
      if (selected.index < 0 || selected.index >= _playlists.Count)
      {
        _selectedPlaylist = null;
        _playlistTracksTable = null;
        return;
      }
      _selectedPlaylist = _playlists[selected.index];
      if (_selectedPlaylist.tracks == null) _selectedPlaylist.tracks = new List<PlaylistTrack>();

      _playlistTracksTable = new ReorderableList(_selectedPlaylist.tracks, typeof(PlaylistTrack), true, false, true, true)
      {
        onAddCallback = (list) =>
        {
          _selectedPlaylist.tracks.Add(new PlaylistTrack
          {
            playerType = _defaultTrackMode,
            title = "",
            url = ""
          });
          IsDirty = true;
        },
        onRemoveCallback = (list) =>
        {
          if (list.index >= 0 && list.index < _selectedPlaylist.tracks.Count)
          {
            _selectedPlaylist.tracks.RemoveAt(list.index);
          }
          IsDirty = true;
        },
        drawElementCallback = (rect, index, isActive, isFocused) =>
        {
          if (index >= _selectedPlaylist.tracks.Count) return;
          var track = _selectedPlaylist.tracks[index];

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
            track.playerType = (VideoPlayerType)EditorGUI.EnumPopup(playerRect, new GUIContent(EditorLocalization.Get("settings.videoPlayerType.label")), track.playerType);

            rect.y += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
            track.title = EditorGUI.TextField(rect, new GUIContent(EditorLocalization.Get("label.title")), track.title);

            rect.y += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
            track.url = EditorGUI.TextField(rect, new GUIContent("Url"), track.url);

            if (check.changed)
            {
              IsDirty = true;
            }
          }
          EditorGUIUtility.labelWidth = labelWidth;
        },
        onReorderCallback = (ReorderableList list) =>
        {
          IsDirty = true;
        },
        elementHeightCallback = (index => (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 3),
        showDefaultBackground = false,
      };
    }

    private void OnGUI()
    {
      using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
      {
        YamaPlayer = EditorGUILayout.ObjectField(YamaPlayer, typeof(YamaPlayer), true) as YamaPlayer;

        var ytdlpButtonText = YtdlpResolver.IsAvailable ? EditorLocalization.Get("ytdlp.update") : EditorLocalization.Get("ytdlp.download");
        if (GUILayout.Button(ytdlpButtonText, EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
        {
          YtdlpResolver.DownloadYtdlpExecutable().Forget();
        }

        if (GUILayout.Button(EditorLocalization.Get("playlist.importFromJson"), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false))) Import();
        if (GUILayout.Button(EditorLocalization.Get("playlist.exportToJson"), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false))) Export();
        using (new EditorGUI.DisabledGroupScope(!IsDirty))
        {
          var saveStyle = new GUIStyle(EditorStyles.toolbarButton);
          if (IsDirty)
          {
            saveStyle.normal.textColor = new Color(1f, 0.6f, 0.2f);
            saveStyle.hover.textColor = new Color(1f, 0.7f, 0.4f);
          }
          if (GUILayout.Button(EditorLocalization.Get("button.save"), saveStyle, GUILayout.ExpandWidth(false))) Save();
        }
      }
      using (new EditorGUILayout.HorizontalScope())
      {
        using (var vert = new EditorGUILayout.VerticalScope(GUILayout.MaxWidth(380)))
        {
          HandleDragEvent(vert.rect);
          EditorGUILayout.LabelField(EditorLocalization.Get("label.playlists"), Styles.Bold);
          _leftScrollPos = EditorGUILayout.BeginScrollView(_leftScrollPos, GUI.skin.box);
          if (_player != null) _playlistsTable?.DoLayoutList();
          GUILayout.FlexibleSpace();
          EditorGUILayout.HelpBox(EditorLocalization.Get("playlist.importFromPlayer"), MessageType.Info);
          EditorGUILayout.EndScrollView();
          DrawPlaylistSettings();
        }
        using (new EditorGUILayout.VerticalScope())
        {
          using (new EditorGUILayout.HorizontalScope())
          {
            using (new EditorGUI.DisabledScope(_selectedPlaylist == null))
            {
              EditorGUILayout.LabelField(EditorLocalization.Get("label.playlistTracks"), Styles.Bold);
              GUILayout.FlexibleSpace();
              if (GUILayout.Button(EditorLocalization.Get("playlist.reverse"), GUILayout.ExpandWidth(false)))
              {
                ReverseTracks();
              }
            }
          }
          _rightScrollPos = EditorGUILayout.BeginScrollView(_rightScrollPos, GUI.skin.box);
          if (_player != null) _playlistTracksTable?.DoLayoutList();
          GUILayout.FlexibleSpace();
          EditorGUILayout.EndScrollView();
        }
      }
    }

    private void ReverseTracks()
    {
      if (_selectedPlaylist == null) return;
      _selectedPlaylist.tracks?.Reverse();
      GeneratePlaylistTracksView(_playlistsTable);
      IsDirty = true;
    }

    private void HandleDragEvent(Rect rect)
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
            if (_playlists == null)
            {
              EditorUtility.DisplayDialog("Import playlists", "Assign a YamaPlayer to the editor before importing.", "OK");
            }
            else
            {
              var imported = PlaylistImporter.ImportPlaylists(DragAndDrop.objectReferences);
              _playlists.AddRange(imported);
              if (imported.Count > 0) IsDirty = true;
            }
          }
          Event.current.Use();
          break;
      }
    }

    private void DrawPlaylistSettings()
    {
      EditorGUILayout.LabelField(EditorLocalization.Get("playlist.settings"), Styles.Bold);
      using (new GUILayout.VerticalScope(GUI.skin.box))
      {
        using (new EditorGUILayout.HorizontalScope())
        {
          _defaultTrackMode = (VideoPlayerType)EditorGUILayout.Popup(EditorLocalization.Get("settings.videoPlayerType.label"), (int)_defaultTrackMode, Enum.GetNames(typeof(VideoPlayerType)));
          using (new EditorGUI.DisabledScope(_selectedPlaylist == null))
          {
            if (GUILayout.Button(EditorLocalization.Get("playlist.applyForAll"), GUILayout.ExpandWidth(false)))
            {
              ApplyModeForAllTracks();
            }
          }
        }
        _useYoutubePlaylistName = EditorGUILayout.Toggle(EditorLocalization.Get("playlist.overwriteName"), _useYoutubePlaylistName);
        using (new EditorGUILayout.HorizontalScope())
        {
          using (new EditorGUI.DisabledScope(_selectedPlaylist == null))
          {
            string currentYoutubeId = _selectedPlaylist?.youtubeListId ?? "";

            var youtubeListId = EditorGUILayout.TextField(currentYoutubeId);
            if (_selectedPlaylist != null && youtubeListId != currentYoutubeId)
            {
              _selectedPlaylist.youtubeListId = youtubeListId;
              IsDirty = true;
            }

            if (GUILayout.Button(EditorLocalization.Get("playlist.loadYoutube"), GUILayout.ExpandWidth(false)))
            {
              ReadYouTubePlaylist().Forget();
            }
          }
        }
      }
    }

    private void ApplyModeForAllTracks()
    {
      if (_selectedPlaylist?.tracks == null) return;

      foreach (var track in _selectedPlaylist.tracks)
      {
        track.playerType = _defaultTrackMode;
      }
      IsDirty = true;
    }

    public async UniTask ReadYouTubePlaylist()
    {
      if (_selectedPlaylist == null) return;

      _selectedPlaylist.youtubeListId = YtdlpResolver.GetYoutubePlaylistIdFromUrl(_selectedPlaylist.youtubeListId);

      var data = await PlaylistImporter.GetYouTubePlaylistData(_selectedPlaylist.youtubeListId);
      if (data == null) return;

      if (_useYoutubePlaylistName && !string.IsNullOrEmpty(data.name))
      {
        _selectedPlaylist.name = data.name;
      }

      _selectedPlaylist.tracks = data.tracks ?? new List<PlaylistTrack>();

      IsDirty = true;
      GeneratePlaylistTracksView(_playlistsTable);
    }

    private bool ConfirmSave()
    {
      if (!IsDirty) return true;

      int result = EditorUtility.DisplayDialogComplex(
        EditorLocalization.Get("msg.notSaved"),
        EditorLocalization.Get("msg.confirmSave"),
        EditorLocalization.Get("button.save"),
        EditorLocalization.Get("button.cancel"),
        EditorLocalization.Get("button.notSave")
      );

      switch (result)
      {
        case 0:
          Save();
          return true;
        case 1:
          return false;
        case 2:
          RevertChanges();
          return true;
        default:
          return true;
      }
    }

    private void Save()
    {
      PlaylistManager container = _player?.GetComponentInChildren<PlaylistManager>();
      if (container == null) return;

      var existingPlaylists = container.GetPlaylists();
      var processedOriginals = new HashSet<PlaylistItem>();

      for (int i = 0; i < _playlists.Count; i++)
      {
        var playlist = _playlists[i];

        PlaylistItem item;
        if (playlist.originalItem != null)
        {
          item = playlist.originalItem;
          processedOriginals.Add(item);
        }
        else
        {
          GameObject obj = new GameObject(playlist.name);
          obj.transform.SetParent(container.transform);
          item = obj.AddComponent<PlaylistItem>();
          playlist.originalItem = item;
        }

        var so = new SerializedObject(item);
        so.FindProperty("playlistName").stringValue = playlist.name;
        so.FindProperty("youtubePlaylistId").stringValue = playlist.youtubeListId;

        var tracksProp = so.FindProperty("tracks");
        tracksProp.arraySize = playlist.tracks?.Count ?? 0;
        if (playlist.tracks != null)
        {
          for (int j = 0; j < playlist.tracks.Count; j++)
          {
            var track = playlist.tracks[j];
            var trackProp = tracksProp.GetArrayElementAtIndex(j);
            trackProp.FindPropertyRelative("playerType").intValue = (int)track.playerType;
            trackProp.FindPropertyRelative("title").stringValue = track.title;
            trackProp.FindPropertyRelative("url").stringValue = track.url;
          }
        }
        so.ApplyModifiedProperties();

        item.gameObject.name = playlist.name;
        item.gameObject.SetActive(playlist.active);
        item.transform.SetSiblingIndex(i + 1);

        EditorUtility.SetDirty(item);
        EditorUtility.SetDirty(item.gameObject);
      }

      foreach (var existingItem in existingPlaylists)
      {
        if (existingItem != null && !processedOriginals.Contains(existingItem))
        {
          GameObject.DestroyImmediate(existingItem.gameObject);
        }
      }

      if (_player != null)
      {
        EditorUtility.SetDirty(_player.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(_player.gameObject.scene);
      }

      IsDirty = false;
    }

    private void Export()
    {
      var items = _playlists
        .Where(p => p.originalItem != null)
        .Select(p => p.originalItem)
        .ToList();
      PlaylistExporter.Export(items);
    }

    private void Import()
    {
      if (_playlists == null)
      {
        EditorUtility.DisplayDialog("Import playlists", "Assign a YamaPlayer to the editor before importing.", "OK");
        return;
      }

      var imported = PlaylistExporter.Import();
      _playlists.AddRange(imported);

      if (imported.Count > 0)
      {
        IsDirty = true;
        GeneratePlaylistsView();
      }
    }

  }
}
