using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Yamadev.YamaStream.Editor;

namespace Yamadev.YamaStream.Modules.AutoPlay.Editor
{
  [CustomEditor(typeof(AutoPlay))]
  public class AutoPlayEditor : EditorBase
  {
    private SerializedProperty _autoPlayMode;
    private SerializedProperty _delay;
    private SerializedProperty _videoPlayerType;
    private SerializedProperty _title;
    private SerializedProperty _url;
    private SerializedProperty _playlistIndex;
    private SerializedProperty _playlistTrackIndex;

    private List<PlaylistItem> _playlists;
    private string[] _playlistNames;
    private string[] _trackNames;

    private static string EscapePopupName(string name) => name?.Replace("/", "／");

    private void OnEnable()
    {
      ShowHeader = false;
      Title = EditorLocalization.Get("module.autoPlay.title");

      _autoPlayMode = serializedObject.FindProperty("_autoPlayMode");
      _delay = serializedObject.FindProperty("_delay");
      _videoPlayerType = serializedObject.FindProperty("_videoPlayerType");
      _title = serializedObject.FindProperty("_title");
      _url = serializedObject.FindProperty("_url");
      _playlistIndex = serializedObject.FindProperty("_playlistIndex");
      _playlistTrackIndex = serializedObject.FindProperty("_playlistTrackIndex");

      RefreshPlaylists();
    }

    public override void OnInspectorGUI()
    {
      base.OnInspectorGUI();

      Title = EditorLocalization.Get("module.autoPlay.title");
      serializedObject.Update();

      DrawModeSection();
      EditorGUILayout.Space(SpaceMedium);

      DrawDelaySection();
      EditorGUILayout.Space(SpaceMedium);

      var mode = (AutoPlayMode)_autoPlayMode.enumValueIndex;
      switch (mode)
      {
        case AutoPlayMode.FromTrack:
          DrawFromTrackSection();
          break;
        case AutoPlayMode.FromPlaylist:
          DrawFromPlaylistSection();
          break;
        case AutoPlayMode.Off:
        default:
          break;
      }

      serializedObject.ApplyModifiedProperties();
    }

    private void DrawModeSection()
    {
      EditorGUILayout.LabelField(EditorLocalization.Get("module.autoPlay.settings"), EditorStyles.boldLabel);
      EditorGUILayout.PropertyField(_autoPlayMode, new GUIContent(EditorLocalization.Get("module.autoPlay.mode")));
    }

    private void DrawDelaySection()
    {
      EditorGUILayout.PropertyField(_delay, new GUIContent(EditorLocalization.Get("module.autoPlay.delay")));
    }

    private void DrawFromTrackSection()
    {
      EditorGUILayout.LabelField(EditorLocalization.Get("module.autoPlay.trackSettings"), EditorStyles.boldLabel);

      EditorGUILayout.PropertyField(_videoPlayerType, new GUIContent(EditorLocalization.Get("module.autoPlay.playerType")));
      EditorGUILayout.PropertyField(_title, new GUIContent(EditorLocalization.Get("module.autoPlay.trackTitle")));
      EditorGUILayout.PropertyField(_url, new GUIContent(EditorLocalization.Get("module.autoPlay.url")));
    }

    private void DrawFromPlaylistSection()
    {
      EditorGUILayout.LabelField(EditorLocalization.Get("module.autoPlay.playlistSettings"), EditorStyles.boldLabel);

      if (_playlists == null || _playlists.Count == 0)
      {
        EditorGUILayout.HelpBox(EditorLocalization.Get("module.autoPlay.noPlaylists"), MessageType.Warning);

        if (GUILayout.Button(EditorLocalization.Get("module.autoPlay.refresh")))
        {
          RefreshPlaylists();
        }
        return;
      }

      int currentPlaylistIndex = _playlistIndex.intValue;
      if (currentPlaylistIndex < 0 || currentPlaylistIndex >= _playlists.Count)
      {
        currentPlaylistIndex = 0;
        _playlistIndex.intValue = 0;
      }

      int newPlaylistIndex = EditorGUILayout.Popup(
        EditorLocalization.Get("module.autoPlay.playlist"),
        currentPlaylistIndex,
        _playlistNames);

      if (newPlaylistIndex != currentPlaylistIndex)
      {
        _playlistIndex.intValue = newPlaylistIndex;
        _playlistTrackIndex.intValue = 0;
        UpdateTrackNames(newPlaylistIndex);
      }

      DrawTrackSelector(newPlaylistIndex);

      EditorGUILayout.Space(SpaceSmall);

      if (GUILayout.Button(EditorLocalization.Get("module.autoPlay.refresh")))
      {
        RefreshPlaylists();
      }
    }

    private void DrawTrackSelector(int playlistIndex)
    {
      if (playlistIndex < 0 || playlistIndex >= _playlists.Count) return;

      UpdateTrackNames(playlistIndex);

      if (_trackNames == null || _trackNames.Length == 0)
      {
        EditorGUILayout.HelpBox(EditorLocalization.Get("module.autoPlay.noTracks"), MessageType.Info);
        return;
      }

      var trackOptions = new List<string> { EditorLocalization.Get("module.autoPlay.random") };
      trackOptions.AddRange(_trackNames);

      int displayIndex = _playlistTrackIndex.intValue + 1;
      if (displayIndex < 0) displayIndex = 0;
      if (displayIndex >= trackOptions.Count) displayIndex = trackOptions.Count - 1;

      int newDisplayIndex = EditorGUILayout.Popup(
        EditorLocalization.Get("module.autoPlay.track"),
        displayIndex,
        trackOptions.ToArray());

      _playlistTrackIndex.intValue = newDisplayIndex - 1;
    }

    private void RefreshPlaylists()
    {
      _playlists = new List<PlaylistItem>();
      _playlistNames = new string[0];
      _trackNames = new string[0];

      var autoPlay = target as AutoPlay;
      if (autoPlay == null) return;

      var yamaPlayer = autoPlay.GetComponentInParent<YamaPlayer>();
      if (yamaPlayer == null) return;

      var playlistManager = yamaPlayer.PlaylistManager;
      if (playlistManager == null) return;

      _playlists = playlistManager.GetPlaylists();

      var names = new List<string>();
      for (int i = 0; i < _playlists.Count; i++)
      {
        var playlist = _playlists[i];
        var name = string.IsNullOrEmpty(playlist.playlistName) ? $"Playlist {i + 1}" : playlist.playlistName;
        names.Add(EscapePopupName(name));
      }
      _playlistNames = names.ToArray();

      if (_playlistIndex.intValue >= 0 && _playlistIndex.intValue < _playlists.Count)
      {
        UpdateTrackNames(_playlistIndex.intValue);
      }
    }

    private void UpdateTrackNames(int playlistIndex)
    {
      if (_playlists == null || playlistIndex < 0 || playlistIndex >= _playlists.Count)
      {
        _trackNames = new string[0];
        return;
      }

      var playlist = _playlists[playlistIndex];
      if (playlist.tracks == null || playlist.tracks.Length == 0)
      {
        _trackNames = new string[0];
        return;
      }

      var names = new List<string>();
      for (int i = 0; i < playlist.tracks.Length; i++)
      {
        var track = playlist.tracks[i];
        var title = string.IsNullOrEmpty(track.title) ? $"Track {i + 1}" : track.title;
        names.Add(EscapePopupName($"{i + 1}. {title}"));
      }
      _trackNames = names.ToArray();
    }
  }
}
