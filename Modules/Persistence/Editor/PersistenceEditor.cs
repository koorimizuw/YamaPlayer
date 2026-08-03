using UnityEditor;
using UnityEngine;
using Yamadev.YamaStream.Editor;

namespace Yamadev.YamaStream.Modules.Persistence.Editor
{
  [CustomEditor(typeof(Persistence))]
  public class PersistenceEditor : EditorBase
  {
    private SerializedProperty _uniqueId;

    private void OnEnable()
    {
      ShowHeader = false;
      Title = EditorLocalization.Get("module.persistence.title");

      _uniqueId = serializedObject.FindProperty("_uniqueId");
    }

    public override void OnInspectorGUI()
    {
      base.OnInspectorGUI();

      Title = EditorLocalization.Get("module.persistence.title");
      serializedObject.Update();

#if !VRC_ENABLE_PLAYER_PERSISTENCE
      EditorGUILayout.HelpBox(EditorLocalization.Get("module.persistence.apiNotAvailable"), MessageType.Warning);
      EditorGUILayout.Space(SpaceMedium);
#endif

      DrawKeySection();

      serializedObject.ApplyModifiedProperties();
    }

    private void DrawKeySection()
    {
      EditorGUILayout.LabelField(EditorLocalization.Get("module.persistence.keySettings"), EditorStyles.boldLabel);

      EditorGUILayout.PropertyField(_uniqueId, new GUIContent(EditorLocalization.Get("module.persistence.uniqueId")));

      if (!string.IsNullOrEmpty(_uniqueId.stringValue))
      {
        EditorGUI.BeginDisabledGroup(true);
        string keyBase = $"{Persistence.KEY_PREFIX}.{_uniqueId.stringValue}";
        EditorGUILayout.TextField(EditorLocalization.Get("module.persistence.volumeKey"), $"{keyBase}.Volume");
        EditorGUILayout.TextField(EditorLocalization.Get("module.persistence.muteKey"), $"{keyBase}.Mute");
        EditorGUILayout.TextField(EditorLocalization.Get("module.persistence.mirrorFlipKey"), $"{keyBase}.MirrorFlip");
        EditorGUILayout.TextField(EditorLocalization.Get("module.persistence.brightnessKey"), $"{keyBase}.Brightness");
        EditorGUI.EndDisabledGroup();
      }
      else
      {
        EditorGUILayout.HelpBox(EditorLocalization.Get("module.persistence.pathBasedWarning"), MessageType.Warning);
      }

      EditorGUILayout.Space(SpaceSmall);

      EditorGUILayout.BeginHorizontal();

      if (GUILayout.Button(EditorLocalization.Get("module.persistence.generateKeys")))
        GenerateUniqueId();

      EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(_uniqueId.stringValue));
      if (GUILayout.Button(EditorLocalization.Get("module.persistence.clearKeys")))
      {
        bool confirmed = EditorUtility.DisplayDialog(
          EditorLocalization.Get("module.persistence.clearKeysDialogTitle"),
          EditorLocalization.Get("module.persistence.clearKeysDialogMessage"),
          EditorLocalization.Get("module.persistence.clearKeysDialogOk"),
          EditorLocalization.Get("module.persistence.clearKeysDialogCancel"));
        if (confirmed) ClearUniqueId();
      }
      EditorGUI.EndDisabledGroup();

      EditorGUILayout.EndHorizontal();
    }

    private void GenerateUniqueId()
    {
      var module = target as Persistence;
      if (module == null) return;

      if (!string.IsNullOrEmpty(_uniqueId.stringValue))
      {
        bool confirmed = EditorUtility.DisplayDialog(
          EditorLocalization.Get("module.persistence.regenerateDialogTitle"),
          EditorLocalization.Get("module.persistence.regenerateDialogMessage"),
          EditorLocalization.Get("module.persistence.regenerateDialogOk"),
          EditorLocalization.Get("module.persistence.regenerateDialogCancel"));
        if (!confirmed) return;
      }

      string guid = System.Guid.NewGuid().ToString("N").Substring(0, 8);
      string objectName = module.gameObject.name.Replace(" ", "_");

      _uniqueId.stringValue = $"{objectName}_{guid}";

      EditorUtility.SetDirty(module);
    }

    private void ClearUniqueId()
    {
      _uniqueId.stringValue = "";
    }
  }
}
