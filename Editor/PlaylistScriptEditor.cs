
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using UnityEditor;
using UnityEditorInternal;
using VRC.SDK3.Components;
using System.IO;
using System.Net;
using System.Text;
using System;
using UdonSharp;

namespace Yamadev.YamaStream.Script
{
    [Serializable]
    public class YoutubePlayListItem
    {
        public string title;
        public string id;
        public bool live;
    }

    [Serializable]
    public class YoutubePlayList
    {
        public string title;
        public YoutubePlayListItem[] items;
    }

    [CustomEditor(typeof(PlayList))]
    internal class PlayListEditor : EditorBase
    {
        SerializedProperty _playListName;
        SerializedProperty _tracks;
        ReorderableList _list;
        PlayList _target;

        SerializedProperty _youTubePlayListID;

        private void OnEnable()
        {
            _target = target as PlayList;

            _playListName = serializedObject.FindProperty("playListName");
            _tracks = serializedObject.FindProperty("tracks");

            _list = new ReorderableList(serializedObject, _tracks)
            {
                drawHeaderCallback = (rect) =>
                {
                    EditorGUI.LabelField(rect, _tracks.displayName);
                },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    EditorGUI.PropertyField(rect, _tracks.GetArrayElementAtIndex(index));
                },
                elementHeightCallback = (index) =>
                {
                    return EditorGUI.GetPropertyHeight(_tracks.GetArrayElementAtIndex(index)) + EditorGUIUtility.standardVerticalSpacing;
                },
                onReorderCallback = (list) =>
                {
                    ApplyModifiedProperties();
                }
            };
        }

        public static string GetPlayListItem(string playListID)
        {
            string url = $"http://api.yamachan.moe/youtube/playlist?id={playListID}";
            WebRequest request = WebRequest.Create(url);
            request.Method = "Get";
            WebResponse response;
            response = request.GetResponse();

            if (response != null)
            {
                Stream st = response.GetResponseStream();
                StreamReader sr = new StreamReader(st, Encoding.GetEncoding("UTF-8"));
                string txt = sr.ReadToEnd();
                sr.Close();
                st.Close();
                return txt;
            }
            return null;
        }

        public void DrawYoutubeField()
        {
            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("Get Play List From YouTube", _bold);

                _youTubePlayListID = serializedObject.FindProperty("YouTubePlayListID");
                EditorGUILayout.PropertyField(_youTubePlayListID);

                if (GUILayout.Button("Generate Play List"))
                {
                    string results = GetPlayListItem(_youTubePlayListID.stringValue);
                    if (results != null)
                    {
                        YoutubePlayList foo = JsonUtility.FromJson<YoutubePlayList>(results);
                        _playListName.stringValue = foo.title;
                        YoutubePlayListItem[] list = foo.items;

                        for (int i = 0; i < list.Length; i++)
                        {
                            _tracks.arraySize++;

                            SerializedProperty lastTrack = _tracks.GetArrayElementAtIndex(_tracks.arraySize - 1);
                            lastTrack.FindPropertyRelative("Mode").intValue = list[i].live ? 1 : 0;
                            lastTrack.FindPropertyRelative("Title").stringValue = list[i].title;
                            lastTrack.FindPropertyRelative("Url").stringValue = $"https://www.youtube.com/watch?v={list[i].id}";
                        }
                    }

                    ApplyModifiedProperties();
                }
            }
            EditorGUILayout.Space();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.LabelField("Play List", _uiTitle);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_playListName);
            EditorGUILayout.Space();

            if (_target.YouTubePlayListID != null) DrawYoutubeField();
            if (_list != null) _list.DoLayoutList();

            EditorGUILayout.Space();

            if (serializedObject.ApplyModifiedProperties())
                ApplyModifiedProperties();
        }

        internal void ApplyModifiedProperties()
        {

        }
    }
}