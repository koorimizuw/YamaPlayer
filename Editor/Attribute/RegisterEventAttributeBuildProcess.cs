using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UdonSharp;
using UdonSharpEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Yamadev.YamaStream.Editor
{
  internal class RegisterEventAttributeBuildProcess : IYamaPlayerBuildProcess
  {
    private const BindingFlags AllFields = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    public int callbackOrder => -8000;

    public void Process()
    {
      var udonTypes = UdonSharpProgramAsset.GetAllUdonSharpPrograms().Select(p => p.GetClass());

      foreach (Type udonType in udonTypes)
      {
        if (udonType == null) continue;
        var fieldEvents = GetFieldAttributes<RegisterEventAttribute>(udonType);
        var classEvents = udonType.GetCustomAttributes<RegisterEventAttribute>().ToList();
        var fieldTriggers = GetFieldAttributes<RegisterEventTriggerAttribute>(udonType);

        if (fieldEvents.Count == 0 && classEvents.Count == 0 && fieldTriggers.Count == 0) continue;

        foreach (Component component in UnityEngine.Object.FindObjectsOfType(udonType, true).OfType<Component>())
        {
          var udonBehaviour = UdonSharpEditorUtility.GetBackingUdonBehaviour((UdonSharpBehaviour)component);
          UnityAction<string> sendEvent = udonBehaviour.SendCustomEvent;

          foreach (var (field, attributes) in fieldEvents)
          {
            if (field.GetValue(component) is not Component target) continue;
            foreach (var attr in attributes)
            {
              var resolved = attr.TargetType == null ? target : target.GetComponent(attr.TargetType);
              AddUnityEventListener(resolved, attr.UnityEvent, attr.EventName, sendEvent);
            }
          }

          foreach (var attr in classEvents)
          {
            var target = component.GetComponent(attr.TargetType);
            AddUnityEventListener(target, attr.UnityEvent, attr.EventName, sendEvent);
          }

          foreach (var (field, attributes) in fieldTriggers)
          {
            if (field.GetValue(component) is not Component target) continue;
            if (!target.gameObject.TryGetComponent<EventTrigger>(out var trigger))
              trigger = target.gameObject.AddComponent<EventTrigger>();
            foreach (var attr in attributes)
            {
              var entry = new EventTrigger.Entry
              {
                eventID = attr.EventType,
                callback = new EventTrigger.TriggerEvent()
              };
              UnityEventTools.AddStringPersistentListener(entry.callback, sendEvent, attr.EventName);
              trigger.triggers.Add(entry);
            }
          }
        }
      }
    }

    private static void AddUnityEventListener(Component target, string eventName, string methodName, UnityAction<string> action)
    {
      if (target == null) return;

      var type = target.GetType();
      var value = type.GetProperty(eventName)?.GetValue(target) ?? type.GetField(eventName)?.GetValue(target);

      if (value is not UnityEventBase unityEvent)
      {
        Debug.LogWarning($"[RegisterEvent] \"{eventName}\" in {type.Name} is not a valid UnityEvent, skipping...");
        return;
      }

      UnityEventTools.AddStringPersistentListener(unityEvent, action, methodName);
    }

    private static List<(FieldInfo field, List<T> attributes)> GetFieldAttributes<T>(Type type) where T : Attribute
    {
      return type.GetFields(AllFields)
          .Select(f => (field: f, attributes: f.GetCustomAttributes<T>(true).ToList()))
          .Where(x => x.attributes.Count > 0)
          .ToList();
    }
  }
}
