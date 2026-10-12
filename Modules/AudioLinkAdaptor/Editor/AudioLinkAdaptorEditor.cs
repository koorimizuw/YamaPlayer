using UnityEditor;
using UnityEngine;
using Yamadev.YamaStream.Editor;

#if AUDIOLINK_V1
using AudioLink.Editor;
#endif

namespace Yamadev.YamaStream.Modules.AudioLinkAdaptor.Editor
{
  [CustomEditor(typeof(AudioLinkAdaptor))]
  public class AudioLinkAdaptorEditor : EditorBase
  {
    private SerializedProperty _audioLinkProperty;
    private SerializedProperty _defaultAudioLinkEnabledProperty;

    private void OnEnable()
    {
      Title = EditorLocalization.Get("module.audioLinkAdaptor.name");
      ShowHeader = false;
      _audioLinkProperty = serializedObject.FindProperty("_audioLink");
      _defaultAudioLinkEnabledProperty = serializedObject.FindProperty("_defaultAudioLinkEnabled");
    }

    public override void OnInspectorGUI()
    {
      base.OnInspectorGUI();
      Title = EditorLocalization.Get("module.audioLinkAdaptor.name");

#if AUDIOLINK_V1
      serializedObject.Update();

      EditorGUILayout.PropertyField(_audioLinkProperty);

      if (_audioLinkProperty.objectReferenceValue == null)
      {
        if (GUILayout.Button(EditorLocalization.Get("module.audioLinkAdaptor.addToScene")))
        {
          AudioLinkAssetManager.AddAudioLinkToScene();
          var audioLink = Object.FindFirstObjectByType<AudioLink.AudioLink>();
          if (audioLink != null)
          {
            _audioLinkProperty.objectReferenceValue = audioLink;
            serializedObject.ApplyModifiedProperties();
          }
        }
      }

      EditorGUILayout.PropertyField(_defaultAudioLinkEnabledProperty);

      if (_audioLinkProperty.objectReferenceValue == null)
      {
        EditorGUILayout.HelpBox(EditorLocalization.Get("module.audioLinkAdaptor.hint"), MessageType.Info);
      }
      serializedObject.ApplyModifiedProperties();
#else
      EditorGUILayout.HelpBox(EditorLocalization.Get("module.audioLinkAdaptor.notInstalled"), MessageType.Warning);
#endif
    }
  }
}
