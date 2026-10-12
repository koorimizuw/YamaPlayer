using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Yamadev.YamaStream.Editor
{
  [CustomEditor(typeof(ModuleManager))]
  public class ModuleManagerEditor : EditorBase
  {
    private const float RowHeight = 36f;

    private ModuleManager _moduleManager;
    private Vector2 _installedScrollPos;
    private Vector2 _availableScrollPos;

    private static Color RowEvenColor => EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.76f, 0.76f, 0.76f);
    private static Color RowOddColor => EditorGUIUtility.isProSkin ? new Color(0.25f, 0.25f, 0.25f) : new Color(0.8f, 0.8f, 0.8f);
    private static readonly Color ActiveColor = new Color(0.4f, 0.8f, 0.4f);
    private static Color SubTextColor => EditorGUIUtility.isProSkin ? new Color(0.55f, 0.55f, 0.55f) : new Color(0.45f, 0.45f, 0.45f);

    private void OnEnable()
    {
      _moduleManager = (ModuleManager)target;
      Title = EditorLocalization.Get("module.manager.title");
      FindYamaPlayerModules();
    }

    public override void OnInspectorGUI()
    {
      base.OnInspectorGUI();

      DrawInstalledModulesSection();
      EditorGUILayout.Space(SpaceLarge);
      DrawAvailableModulesSection();
    }

    private void DrawInstalledModulesSection()
    {
      EditorGUILayout.LabelField(EditorLocalization.Get("module.manager.installedModules"), EditorStyles.boldLabel);
      EditorGUILayout.Space(SpaceSmall);

      var installedModules = GetInstalledModules();

      if (installedModules.Count == 0)
      {
        EditorGUILayout.HelpBox(EditorLocalization.Get("module.manager.noInstalledModules"), MessageType.Info);
        return;
      }

      for (int i = 0; i < installedModules.Count; i++)
      {
        DrawInstalledModuleRow(installedModules[i], i);
      }
    }

    private string GetModuleName(YamaPlayerModuleDefinition module)
    {
      if (!string.IsNullOrEmpty(module.moduleNameTranslationKey))
      {
        var translated = EditorLocalization.Get(module.moduleNameTranslationKey);
        if (!string.IsNullOrEmpty(translated))
          return translated;
      }
      return module.moduleName;
    }

    private string GetModuleDescription(YamaPlayerModuleDefinition module)
    {
      if (!string.IsNullOrEmpty(module.moduleDescriptionTranslationKey))
      {
        var translated = EditorLocalization.Get(module.moduleDescriptionTranslationKey);
        if (!string.IsNullOrEmpty(translated))
          return translated;
      }
      return module.moduleDescription;
    }

    private void DrawInstalledModuleRow(YamaPlayerModuleDefinition module, int index)
    {
      bool isActive = module.gameObject.activeSelf;
      var rowColor = index % 2 == 0 ? RowEvenColor : RowOddColor;
      var rowRect = EditorGUILayout.GetControlRect(false, RowHeight);
      EditorGUI.DrawRect(rowRect, rowColor);

      var statusBarRect = new Rect(rowRect.x, rowRect.y, 3, rowRect.height);
      EditorGUI.DrawRect(statusBarRect, isActive ? ActiveColor : SubTextColor);

      var nameStyle = new GUIStyle(EditorStyles.label)
      {
        fontSize = 11,
        fontStyle = FontStyle.Bold
      };
      var nameRect = new Rect(rowRect.x + 10, rowRect.y + 2, rowRect.width - 180, 16);
      EditorGUI.LabelField(nameRect, GetModuleName(module), nameStyle);

      var subStyle = new GUIStyle(EditorStyles.miniLabel)
      {
        fontSize = 9
      };
      subStyle.normal.textColor = SubTextColor;

      string moduleDescription = GetModuleDescription(module);
      string subText = "";
      if (!string.IsNullOrEmpty(module.version))
        subText = $"v{module.version}";
      if (!string.IsNullOrEmpty(moduleDescription))
        subText = string.IsNullOrEmpty(subText) ? moduleDescription : $"{subText} | {moduleDescription}";

      var subRect = new Rect(rowRect.x + 10, rowRect.y + 18, rowRect.width - 180, 14);
      EditorGUI.LabelField(subRect, subText, subStyle);

      float buttonWidth = 50f;
      float buttonHeight = 18f;
      float buttonY = rowRect.y + (rowRect.height - buttonHeight) / 2;
      float buttonsStartX = rowRect.xMax - 168;

      if (!module.noNeedSetUp)
      {
        var settingsRect = new Rect(buttonsStartX, buttonY, buttonWidth, buttonHeight);
        if (GUI.Button(settingsRect, EditorLocalization.Get("module.manager.button.settings")))
        {
          if (ActiveEditorTracker.sharedTracker.isLocked)
          {
            EditorUtility.DisplayDialog(
              EditorLocalization.Get("msg.inspectorLocked.title"),
              EditorLocalization.Get("msg.inspectorLocked"),
              EditorLocalization.Get("button.ok"));
          }
          else
          {
            Selection.activeGameObject = module.gameObject;
          }
        }
      }

      var toggleRect = new Rect(buttonsStartX + buttonWidth + 4, buttonY, buttonWidth + 6, buttonHeight);
      if (GUI.Button(toggleRect, isActive ? EditorLocalization.Get("module.manager.button.disable") : EditorLocalization.Get("module.manager.button.enable")))
      {
        Undo.RecordObject(module.gameObject, isActive ? "Disable Module" : "Enable Module");
        module.gameObject.SetActive(!isActive);
        EditorUtility.SetDirty(module.gameObject);
      }

      var deleteRect = new Rect(buttonsStartX + buttonWidth * 2 + 14, buttonY, buttonWidth, buttonHeight);
      if (GUI.Button(deleteRect, EditorLocalization.Get("module.manager.button.delete")))
      {
        if (EditorUtility.DisplayDialog(
          EditorLocalization.Get("module.manager.deleteTitle"),
          string.Format(EditorLocalization.Get("module.manager.deleteConfirm"), module.moduleName),
          EditorLocalization.Get("module.manager.button.delete"),
          EditorLocalization.Get("button.cancel")))
        {
          Undo.DestroyObjectImmediate(module.gameObject);
        }
      }
    }

    private void DrawAvailableModulesSection()
    {
      using (new EditorGUILayout.HorizontalScope())
      {
        EditorGUILayout.LabelField(EditorLocalization.Get("module.manager.availableModules"), EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(EditorLocalization.Get("module.manager.button.refresh"), GUILayout.Width(50)))
        {
          FindYamaPlayerModules();
        }
      }

      EditorGUILayout.Space(SpaceSmall);

      var installedModules = GetInstalledModules();
      var definitions = ModuleManager.ModuleDefinitions
        .Where(d => d.Key.allowMultiple || !installedModules.Any(m => m.moduleName == d.Key.moduleName))
        .ToList();

      if (definitions.Count == 0)
      {
        EditorGUILayout.HelpBox(EditorLocalization.Get("module.manager.noAvailableModules"), MessageType.Info);
        return;
      }

      for (int i = 0; i < definitions.Count; i++)
      {
        DrawAvailableModuleRow(definitions[i].Key, definitions[i].Value, installedModules, i);
      }
    }

    private void DrawAvailableModuleRow(YamaPlayerModuleDefinition definition, GameObject prefab, List<YamaPlayerModuleDefinition> installedModules, int index)
    {
      bool isInstalled = installedModules.Any(m => m.moduleName == definition.moduleName);
      bool canAdd = definition.allowMultiple || !isInstalled;

      var rowColor = index % 2 == 0 ? RowEvenColor : RowOddColor;
      var rowRect = EditorGUILayout.GetControlRect(false, RowHeight);
      EditorGUI.DrawRect(rowRect, rowColor);

      var installedBarRect = new Rect(rowRect.x, rowRect.y, 3, rowRect.height);
      EditorGUI.DrawRect(installedBarRect, isInstalled ? ActiveColor : Color.white);

      var nameStyle = new GUIStyle(EditorStyles.label)
      {
        fontSize = 11,
        fontStyle = FontStyle.Bold
      };
      var nameRect = new Rect(rowRect.x + 10, rowRect.y + 2, rowRect.width - 80, 16);
      EditorGUI.LabelField(nameRect, GetModuleName(definition), nameStyle);

      var subStyle = new GUIStyle(EditorStyles.miniLabel)
      {
        fontSize = 9
      };
      subStyle.normal.textColor = SubTextColor;

      string moduleDescription = GetModuleDescription(definition);
      string subText = "";
      if (!string.IsNullOrEmpty(definition.version))
        subText = $"v{definition.version}";
      if (!string.IsNullOrEmpty(moduleDescription))
        subText = string.IsNullOrEmpty(subText) ? moduleDescription : $"{subText} | {moduleDescription}";
      if (isInstalled)
      {
        string installedLabel = EditorLocalization.Get("module.manager.installed");
        subText = string.IsNullOrEmpty(subText) ? installedLabel : $"{subText} {installedLabel}";
      }

      var subRect = new Rect(rowRect.x + 10, rowRect.y + 18, rowRect.width - 80, 14);
      EditorGUI.LabelField(subRect, subText, subStyle);

      float buttonWidth = 50f;
      float buttonHeight = 18f;
      float buttonY = rowRect.y + (rowRect.height - buttonHeight) / 2;
      var addRect = new Rect(rowRect.xMax - buttonWidth - 8, buttonY, buttonWidth, buttonHeight);

      using (new EditorGUI.DisabledScope(!canAdd))
      {
        if (GUI.Button(addRect, EditorLocalization.Get("module.manager.button.add")))
        {
          AddModule(prefab);
        }
      }
    }

    private void AddModule(GameObject prefab)
    {
      GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, _moduleManager.transform);
      Undo.RegisterCreatedObjectUndo(instance, "Add Module");
      instance.name = prefab.name;
      EditorUtility.SetDirty(_moduleManager);
    }

    private List<YamaPlayerModuleDefinition> GetInstalledModules()
    {
      var modules = new List<YamaPlayerModuleDefinition>();
      if (_moduleManager == null) return modules;

      foreach (Transform child in _moduleManager.transform)
      {
        var moduleDef = child.GetComponent<YamaPlayerModuleDefinition>();
        if (moduleDef != null)
        {
          modules.Add(moduleDef);
        }
      }
      return modules;
    }

    public static void FindYamaPlayerModules()
    {
      ModuleManager.ModuleDefinitions.Clear();

      string[] guids = AssetDatabase.FindAssets("t:Prefab");
      foreach (string guid in guids)
      {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) continue;

        YamaPlayerModuleDefinition moduleDefinition = prefab.GetComponent<YamaPlayerModuleDefinition>();
        if (moduleDefinition != null)
        {
          ModuleManager.ModuleDefinitions[moduleDefinition] = prefab;
        }
      }
    }
  }
}
