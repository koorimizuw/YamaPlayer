using UnityEditor;
using UnityEngine;

namespace Yamadev.YamaStream.Editor
{
  [CustomEditor(typeof(PlaylistItem))]
  public class PlaylistItemEditor : EditorBase
  {
    private PlaylistItem _playlistItem;
    private SerializedProperty _playlistNameProp;
    private SerializedProperty _tracksProp;


    private void OnEnable()
    {
      Title = EditorLocalization.Get("label.playlist");
      _playlistItem = (PlaylistItem)target;
      _playlistNameProp = serializedObject.FindProperty("playlistName");
      _tracksProp = serializedObject.FindProperty("tracks");
    }

    public override void OnInspectorGUI()
    {
      base.OnInspectorGUI();
      serializedObject.Update();

      DrawPlaylistHeader();
      EditorGUILayout.Space(SpaceSmall);
      DrawEditButton();
      EditorGUILayout.Space(SpaceLarge);
      DrawTrackList();

      serializedObject.ApplyModifiedProperties();
    }

    private void DrawPlaylistHeader()
    {
      var headerRect = EditorGUILayout.GetControlRect(false, 32);

      var bgColor = EditorGUIUtility.isProSkin
          ? new Color(0.18f, 0.18f, 0.18f)
          : new Color(0.82f, 0.82f, 0.82f);
      EditorGUI.DrawRect(headerRect, bgColor);

      var accentRect = new Rect(headerRect.x, headerRect.y, 3, headerRect.height);
      EditorGUI.DrawRect(accentRect, Color.white);

      var nameStyle = new GUIStyle(EditorStyles.boldLabel)
      {
        fontSize = 12,
        alignment = TextAnchor.MiddleLeft
      };
      var nameRect = new Rect(headerRect.x + 12, headerRect.y, headerRect.width - 80, headerRect.height);
      var playlistName = string.IsNullOrEmpty(_playlistNameProp.stringValue)
          ? EditorLocalization.Get("playlist.unnamed")
          : _playlistNameProp.stringValue;
      EditorGUI.LabelField(nameRect, playlistName, nameStyle);

      var countStyle = new GUIStyle(EditorStyles.miniLabel)
      {
        alignment = TextAnchor.MiddleRight
      };
      var countRect = new Rect(headerRect.xMax - 70, headerRect.y, 66, headerRect.height);
      EditorGUI.LabelField(countRect, $"{_tracksProp.arraySize} {EditorLocalization.Get("label.track")}", countStyle);
    }

    private void DrawEditButton()
    {
      if (GUILayout.Button(EditorLocalization.Get("settings.playlist.edit")))
      {
        OpenPlaylistEditor();
      }
    }

    private void OpenPlaylistEditor()
    {
      var yamaPlayer = _playlistItem.GetComponentInParent<YamaPlayer>();
      if (yamaPlayer != null)
      {
        PlaylistEditorWindow.ShowPlaylistEditorWindow(yamaPlayer, _playlistItem);
      }
      else
      {
        PlaylistEditorWindow.ShowPlaylistEditorWindow();
      }
    }

    private void DrawTrackList()
    {
      EditorGUILayout.LabelField(
          $"{EditorLocalization.Get("label.playlistTracks")} ({_tracksProp.arraySize})",
          EditorStyles.boldLabel);

      if (_tracksProp.arraySize == 0)
      {
        EditorGUILayout.HelpBox(EditorLocalization.Get("playlist.noTracks"), MessageType.Info);
        return;
      }

      EditorGUILayout.Space(SpaceSmall);

      for (int i = 0; i < _tracksProp.arraySize; i++)
      {
        DrawTrackRow(i);
      }
    }

    private void DrawTrackRow(int index)
    {
      var trackProp = _tracksProp.GetArrayElementAtIndex(index);
      var titleProp = trackProp.FindPropertyRelative("title");
      var urlProp = trackProp.FindPropertyRelative("url");

      var rowBgColor = index % 2 == 0
          ? (EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.76f, 0.76f, 0.76f))
          : (EditorGUIUtility.isProSkin ? new Color(0.25f, 0.25f, 0.25f) : new Color(0.8f, 0.8f, 0.8f));

      var rowRect = EditorGUILayout.GetControlRect(false, 36);
      EditorGUI.DrawRect(rowRect, rowBgColor);

      var numberStyle = new GUIStyle(EditorStyles.boldLabel)
      {
        fontSize = 11,
        alignment = TextAnchor.MiddleCenter
      };
      var numberRect = new Rect(rowRect.x, rowRect.y, 28, rowRect.height);
      EditorGUI.LabelField(numberRect, $"{index + 1}", numberStyle);

      var titleStyle = new GUIStyle(EditorStyles.label)
      {
        fontSize = 11,
        fontStyle = FontStyle.Bold
      };
      var title = string.IsNullOrEmpty(titleProp.stringValue)
          ? EditorLocalization.Get("playlist.untitledTrack")
          : titleProp.stringValue;
      var titleRect = new Rect(rowRect.x + 30, rowRect.y + 2, rowRect.width - 34, 16);
      EditorGUI.LabelField(titleRect, title, titleStyle);

      var urlStyle = new GUIStyle(EditorStyles.miniLabel)
      {
        fontSize = 9
      };
      urlStyle.normal.textColor = EditorGUIUtility.isProSkin
          ? new Color(0.55f, 0.55f, 0.55f)
          : new Color(0.45f, 0.45f, 0.45f);
      var url = string.IsNullOrEmpty(urlProp.stringValue)
          ? EditorLocalization.Get("playlist.noUrl")
          : urlProp.stringValue;
      var urlRect = new Rect(rowRect.x + 30, rowRect.y + 18, rowRect.width - 34, 14);
      EditorGUI.LabelField(urlRect, url, urlStyle);
    }
  }
}
