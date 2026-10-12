using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Playables;
using Yamadev.YamaStream.Editor;

namespace Yamadev.YamaStream.Modules.TimelineSync.Editor
{
  [CustomEditor(typeof(TimelineSync))]
  public class TimelineSyncEditor : EditorBase
  {
    private SerializedProperty _urlsProperty;
    private SerializedProperty _timelinesProperty;
    private SerializedProperty _hideOnStopProperty;
    private ReorderableList _mappingList;

    private void OnEnable()
    {
      Title = EditorLocalization.Get("module.timelineSync.title");
      ShowHeader = false;

      _urlsProperty = serializedObject.FindProperty("_urls");
      _timelinesProperty = serializedObject.FindProperty("_timelines");
      _hideOnStopProperty = serializedObject.FindProperty("_hideOnStop");

      SetupReorderableList();
    }

    private void SetupReorderableList()
    {
      int maxCount = Mathf.Max(_urlsProperty.arraySize, Mathf.Max(_timelinesProperty.arraySize, _hideOnStopProperty.arraySize));
      _urlsProperty.arraySize = maxCount;
      _timelinesProperty.arraySize = maxCount;
      _hideOnStopProperty.arraySize = maxCount;

      _mappingList = new ReorderableList(serializedObject, _urlsProperty, true, true, true, true);

      _mappingList.drawHeaderCallback = (Rect rect) =>
      {
        float checkboxWidth = 60;
        float fieldWidth = (rect.width - checkboxWidth - 20) / 2;
        EditorGUI.LabelField(new Rect(rect.x, rect.y, fieldWidth, rect.height), EditorLocalization.Get("module.timelineSync.url"));
        EditorGUI.LabelField(new Rect(rect.x + fieldWidth + 10, rect.y, fieldWidth, rect.height), EditorLocalization.Get("module.timelineSync.timeline"));
        EditorGUI.LabelField(new Rect(rect.x + fieldWidth * 2 + 20, rect.y, checkboxWidth, rect.height), EditorLocalization.Get("module.timelineSync.hideOnStop"));
      };

      _mappingList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
      {
        EnsureArraySize(index);

        float checkboxWidth = 60;
        float fieldWidth = (rect.width - checkboxWidth - 20) / 2;
        float y = rect.y + 2;
        float height = EditorGUIUtility.singleLineHeight;

        SerializedProperty urlElement = _urlsProperty.GetArrayElementAtIndex(index);
        SerializedProperty timelineElement = _timelinesProperty.GetArrayElementAtIndex(index);
        SerializedProperty hideOnStopElement = _hideOnStopProperty.GetArrayElementAtIndex(index);

        EditorGUI.PropertyField(
          new Rect(rect.x, y, fieldWidth, height),
          urlElement,
          GUIContent.none
        );

        EditorGUI.PropertyField(
          new Rect(rect.x + fieldWidth + 10, y, fieldWidth, height),
          timelineElement,
          GUIContent.none
        );

        EditorGUI.PropertyField(
          new Rect(rect.x + fieldWidth * 2 + 20, y, checkboxWidth, height),
          hideOnStopElement,
          GUIContent.none
        );
      };

      _mappingList.onAddCallback = (ReorderableList list) =>
      {
        int newIndex = _urlsProperty.arraySize;
        _urlsProperty.arraySize++;
        _timelinesProperty.arraySize++;
        _hideOnStopProperty.arraySize++;

        _urlsProperty.GetArrayElementAtIndex(newIndex).stringValue = "";
        _timelinesProperty.GetArrayElementAtIndex(newIndex).objectReferenceValue = null;
        _hideOnStopProperty.GetArrayElementAtIndex(newIndex).boolValue = false;
      };

      _mappingList.onRemoveCallback = (ReorderableList list) =>
      {
        if (list.index >= 0 && list.index < _urlsProperty.arraySize)
        {
          _urlsProperty.DeleteArrayElementAtIndex(list.index);
          if (list.index < _timelinesProperty.arraySize)
          {
            if (_timelinesProperty.GetArrayElementAtIndex(list.index).objectReferenceValue != null)
            {
              _timelinesProperty.GetArrayElementAtIndex(list.index).objectReferenceValue = null;
            }
            _timelinesProperty.DeleteArrayElementAtIndex(list.index);
          }
          if (list.index < _hideOnStopProperty.arraySize)
          {
            _hideOnStopProperty.DeleteArrayElementAtIndex(list.index);
          }
        }
      };

      _mappingList.onReorderCallbackWithDetails = (ReorderableList list, int oldIndex, int newIndex) =>
      {
        _timelinesProperty.MoveArrayElement(oldIndex, newIndex);
        _hideOnStopProperty.MoveArrayElement(oldIndex, newIndex);
      };

      _mappingList.elementHeight = EditorGUIUtility.singleLineHeight + 6;
    }

    private void EnsureArraySize(int index)
    {
      while (_urlsProperty.arraySize <= index)
      {
        _urlsProperty.arraySize++;
      }
      while (_timelinesProperty.arraySize <= index)
      {
        _timelinesProperty.arraySize++;
      }
      while (_hideOnStopProperty.arraySize <= index)
      {
        _hideOnStopProperty.arraySize++;
      }
    }

    public override void OnInspectorGUI()
    {
      base.OnInspectorGUI();

      Title = EditorLocalization.Get("module.timelineSync.title");

      serializedObject.Update();

      EditorGUILayout.LabelField(
        $"{EditorLocalization.Get("module.timelineSync.mappingList")} ({_urlsProperty.arraySize})",
        EditorStyles.boldLabel
      );
      EditorGUILayout.Space(SpaceSmall);

      _mappingList.DoLayoutList();

      EditorGUILayout.Space(SpaceMedium);
      EditorGUILayout.HelpBox(EditorLocalization.Get("module.timelineSync.hint"), MessageType.Info);

      serializedObject.ApplyModifiedProperties();
    }
  }
}
