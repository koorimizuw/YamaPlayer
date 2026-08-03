using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Yamadev.YamaStream.UI;
using UnityEngine.UI;

namespace Yamadev.YamaStream.Editor
{
  public class YamaPlayerModuleBuildProcess : IYamaPlayerBuildProcess
  {
    public int callbackOrder => -3000;

    public void Process()
    {
      var modules = Object.FindObjectsByType<YamaPlayerModule>(FindObjectsInactive.Include, FindObjectsSortMode.None);
      foreach (var module in modules)
      {
        ProcessModule(module);
      }
    }

    private static void ProcessModule(YamaPlayerModule module)
    {
      if (module == null) return;
      if (!module.gameObject.activeSelf)
      {
        module.gameObject.tag = "EditorOnly";
        return;
      }
      var definition = module.GetComponent<YamaPlayerModuleDefinition>();
      var controller = module.GetComponentInParent<Controller>(true);
      if (controller != null)
      {
        module.SetProgramVariable("_controller", controller);
        ProcessModuleUISlots(definition.uiSlots, controller);
      }
    }

    private static void ProcessModuleUISlots(ModuleUISlot[] uiSlots, Controller controller)
    {
      if (uiSlots == null || uiSlots.Length == 0 || controller == null) return;

      var uiControllers = Object.FindObjectsByType<UIController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
      foreach (var uiController in uiControllers)
      {
        if (uiController == null || uiController.GetProgramVariable("_controller") as Controller != controller) continue;

        var objectMapping = new Dictionary<Object, Object>();
        var instantiatedSlots = new List<(ModuleUISlot slot, GameObject copy, Transform target)>();

        foreach (var uiSlot in uiSlots)
        {
          if (uiSlot == null || uiSlot.content == null) continue;
          if (string.IsNullOrEmpty(uiSlot.targetPath)) continue;

          var targetTransform = uiController.transform.Find(uiSlot.targetPath);
          if (targetTransform == null) continue;

          var copy = Object.Instantiate(uiSlot.content, targetTransform);
          copy.name = uiSlot.content.name;
          instantiatedSlots.Add((uiSlot, copy, targetTransform));

          BuildObjectMapping(uiSlot.content, copy, objectMapping);
        }

        foreach (var (_, copy, _) in instantiatedSlots)
        {
          ReplaceReferences(copy, objectMapping);
        }

        foreach (var (uiSlot, copy, targetTransform) in instantiatedSlots)
        {
          var childCount = targetTransform.childCount;
          var siblingIndex = uiSlot.siblingIndex;

          if (siblingIndex >= 0)
          {
            copy.transform.SetSiblingIndex(siblingIndex >= childCount ? childCount - 1 : siblingIndex);
          }
          else
          {
            var calculatedIndex = childCount + siblingIndex;
            copy.transform.SetSiblingIndex(calculatedIndex < 0 ? 0 : calculatedIndex);
          }
        }

        var toggleGroups = uiController.GetComponentsInChildren<ToggleGroup>(true);
        foreach (var toggleGroup in toggleGroups)
        {
          foreach (var toggle in toggleGroup.GetComponentsInChildren<Toggle>(true))
          {
            if (toggle == null || toggle.transform.parent != toggleGroup.transform) continue;
            toggle.group = toggleGroup;
          }
        }
      }

      foreach (var uiSlot in uiSlots)
      {
        if (uiSlot?.content != null)
        {
          Object.DestroyImmediate(uiSlot.content);
        }
      }
    }

    private static void BuildObjectMapping(GameObject original, GameObject copy, Dictionary<Object, Object> mapping)
    {
      mapping[original] = copy;

      var originalComponents = original.GetComponents<Component>();
      var copyComponents = copy.GetComponents<Component>();
      for (int i = 0; i < originalComponents.Length && i < copyComponents.Length; i++)
      {
        if (originalComponents[i] != null && copyComponents[i] != null)
        {
          mapping[originalComponents[i]] = copyComponents[i];
        }
      }

      for (int i = 0; i < original.transform.childCount && i < copy.transform.childCount; i++)
      {
        BuildObjectMapping(original.transform.GetChild(i).gameObject, copy.transform.GetChild(i).gameObject, mapping);
      }
    }

    private static void ReplaceReferences(GameObject target, Dictionary<Object, Object> mapping)
    {
      var components = target.GetComponentsInChildren<Component>(true);
      foreach (var component in components)
      {
        if (component == null) continue;

        var serializedObject = new SerializedObject(component);
        var iterator = serializedObject.GetIterator();
        var modified = false;

        while (iterator.NextVisible(true))
        {
          if (iterator.propertyType == SerializedPropertyType.ObjectReference && iterator.objectReferenceValue != null)
          {
            if (mapping.TryGetValue(iterator.objectReferenceValue, out var replacement))
            {
              iterator.objectReferenceValue = replacement;
              modified = true;
            }
          }
        }

        if (modified)
        {
          serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
      }
    }
  }
}
