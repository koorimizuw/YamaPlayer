using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Yamadev.YamaStream.Editor
{
  [CustomEditor(typeof(PlaylistManager))]
  public class PlaylistManagerEditor : EditorBase
  {
    private PlaylistManager _playlistManager;
    private List<PlaylistItem> _playlists;

    private void OnEnable()
    {
      Title = EditorLocalization.Get("label.playlists");
      _playlistManager = (PlaylistManager)target;
      RefreshPlaylists();
    }

    private void RefreshPlaylists()
    {
      _playlists = _playlistManager.GetPlaylists() ?? new List<PlaylistItem>();
    }

    public override void OnInspectorGUI()
    {
      base.OnInspectorGUI();

      RefreshPlaylists();
      DrawPlaylistList();
    }

    private void DrawPlaylistList()
    {
      using (new EditorGUILayout.HorizontalScope())
      {
        EditorGUILayout.LabelField(
            $"{EditorLocalization.Get("label.playlists")} ({_playlists.Count})",
            EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(EditorLocalization.Get("button.edit"), GUILayout.ExpandWidth(false)))
        {
          OpenPlaylistEditorWindow();
        }
      }

      if (_playlists.Count == 0)
      {
        EditorGUILayout.HelpBox(EditorLocalization.Get("settings.playlist.noPlaylist"), MessageType.Info);
        return;
      }

      EditorGUILayout.Space(SpaceSmall);

      for (int i = 0; i < _playlists.Count; i++)
      {
        DrawPlaylistRow(i);
      }
    }

    private void DrawPlaylistRow(int index)
    {
      var playlist = _playlists[index];
      if (playlist == null) return;

      var rowBgColor = index % 2 == 0
          ? (EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.76f, 0.76f, 0.76f))
          : (EditorGUIUtility.isProSkin ? new Color(0.25f, 0.25f, 0.25f) : new Color(0.8f, 0.8f, 0.8f));

      var rowRect = EditorGUILayout.GetControlRect(false, 22);
      EditorGUI.DrawRect(rowRect, rowBgColor);

      var nameStyle = new GUIStyle(EditorStyles.label)
      {
        fontSize = 11
      };
      var playlistName = string.IsNullOrEmpty(playlist.playlistName)
          ? EditorLocalization.Get("playlist.unnamed")
          : playlist.playlistName;
      var nameRect = new Rect(rowRect.x + 4, rowRect.y, rowRect.width - 50, rowRect.height);
      EditorGUI.LabelField(nameRect, playlistName, nameStyle);

      var buttonRect = new Rect(rowRect.xMax - 42, rowRect.y + 2, 40, rowRect.height - 4);
      if (GUI.Button(buttonRect, EditorLocalization.Get("button.edit"), EditorStyles.miniButton))
      {
        OpenPlaylistEditor(playlist);
      }
    }

    private void OpenPlaylistEditor(PlaylistItem playlist)
    {
      var yamaPlayer = _playlistManager.GetComponentInParent<YamaPlayer>();
      if (yamaPlayer != null)
      {
        PlaylistEditorWindow.ShowPlaylistEditorWindow(yamaPlayer, playlist);
      }
      else
      {
        PlaylistEditorWindow.ShowPlaylistEditorWindow();
      }
    }

    private void OpenPlaylistEditorWindow()
    {
      var yamaPlayer = _playlistManager.GetComponentInParent<YamaPlayer>();
      if (yamaPlayer != null)
      {
        PlaylistEditorWindow.ShowPlaylistEditorWindow(yamaPlayer);
      }
      else
      {
        PlaylistEditorWindow.ShowPlaylistEditorWindow();
      }
    }
  }
}
