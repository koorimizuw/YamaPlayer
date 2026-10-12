using UnityEditor;
using Yamadev.YamaStream.Editor;

namespace Yamadev.YamaStream.Modules.LightVolumeAdaptor.Editor
{
  [CustomEditor(typeof(LightVolumeAdaptor))]
  public class LightVolumeAdaptorEditor : EditorBase
  {
    private SerializedProperty _lightVolumes;
    private SerializedProperty _pointLightVolumes;

    private void OnEnable()
    {
      Title = EditorLocalization.Get("module.lightVolumeAdaptor.name");
      ShowHeader = false;
      _lightVolumes = serializedObject.FindProperty("_lightVolumes");
      _pointLightVolumes = serializedObject.FindProperty("_pointLightVolumes");
    }

    public override void OnInspectorGUI()
    {
      base.OnInspectorGUI();
      Title = EditorLocalization.Get("module.lightVolumeAdaptor.name");

#if VRC_LIGHT_VOLUMES
      serializedObject.Update();
      EditorGUILayout.PropertyField(_lightVolumes);
      EditorGUILayout.PropertyField(_pointLightVolumes);
      serializedObject.ApplyModifiedProperties();
#else
      EditorGUILayout.HelpBox(EditorLocalization.Get("module.lightVolumeAdaptor.notInstalled"), MessageType.Warning);
#endif
    }
  }
}
