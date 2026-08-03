using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Yamadev.YamaStream.UI;

using Object = UnityEngine.Object;

namespace Yamadev.YamaStream.Editor
{
  public class LocalizationBuildProcess : IYamaPlayerBuildProcess
  {
    public int callbackOrder => -10000;

    public void Process()
    {
      var localizationSettings = Object.FindObjectsByType<LocalizationSettings>(FindObjectsInactive.Include, FindObjectsSortMode.None);

      foreach (var settings in localizationSettings)
      {
        ProcessLocalizationSettings(settings);
      }
    }

    private void ProcessLocalizationSettings(LocalizationSettings settings)
    {
      var uiController = settings.GetComponent<UIController>();
      if (uiController == null)
      {
        Debug.LogWarning($"[LocalizationBuildProcess] UIController not found on {settings.gameObject.name}");
        return;
      }

      if (settings.languages == null || settings.languages.Length == 0)
      {
        Debug.LogWarning($"[LocalizationBuildProcess] No languages configured on {settings.gameObject.name}");
        return;
      }

      var moduleTranslationFiles = GetModuleTranslationFiles(uiController);
      var mergedTranslationFile = CreateMergedTranslationFile(settings, moduleTranslationFiles);
      if (mergedTranslationFile != null)
      {
        uiController.SetProgramVariable("_translationJsonFile", mergedTranslationFile);
      }

      var defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      var languageCodes = new string[settings.languages.Length];
      var languageFonts = new Object[settings.languages.Length];
      for (int i = 0; i < settings.languages.Length; i++)
      {
        var lang = settings.languages[i];
        languageCodes[i] = lang != null ? lang.languageCode : string.Empty;
        languageFonts[i] = lang != null && lang.font != null ? lang.font : defaultFont;
      }
      uiController.SetProgramVariable("_languageCodes", languageCodes);
      uiController.SetProgramVariable("_fontAssets", languageFonts);

      uiController.SetProgramVariable("_defaultLanguage", settings.defaultLanguage);
    }

    private List<TextAsset> GetModuleTranslationFiles(UIController uiController)
    {
      var translationFiles = new List<TextAsset>();

      var controller = uiController.GetProgramVariable("_controller") as Controller;
      if (controller == null)
      {
        Debug.LogWarning("[LocalizationBuildProcess] Controller not found on UIController");
        return translationFiles;
      }

      var moduleManager = controller.GetComponentInParent<Transform>()?.parent?.GetComponentInChildren<ModuleManager>();
      if (moduleManager == null)
      {
        moduleManager = controller.GetComponentInChildren<ModuleManager>();
      }

      if (moduleManager == null)
      {
        return translationFiles;
      }

      foreach (Transform child in moduleManager.transform)
      {
        if (!child.gameObject.activeSelf) continue;

        var moduleDef = child.GetComponent<YamaPlayerModuleDefinition>();
        if (moduleDef != null && moduleDef.playerTranslationFile != null)
        {
          translationFiles.Add(moduleDef.playerTranslationFile);
        }
      }

      return translationFiles;
    }

    private TextAsset CreateMergedTranslationFile(LocalizationSettings settings, List<TextAsset> moduleTranslationFiles)
    {
      var mergedData = new Dictionary<string, Dictionary<string, string>>();

      foreach (var language in settings.languages)
      {
        if (language == null || language.translationFile == null) continue;

        try
        {
          var translations = JsonConvert.DeserializeObject<Dictionary<string, string>>(language.translationFile.text);
          if (translations != null)
          {
            mergedData[language.languageCode] = translations;
          }
        }
        catch (Exception e)
        {
          Debug.LogWarning($"[LocalizationBuildProcess] Failed to parse translation file for {language.languageCode}: {e.Message}");
        }
      }

      foreach (var moduleFile in moduleTranslationFiles)
      {
        try
        {
          var moduleData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(moduleFile.text);
          if (moduleData == null) continue;

          foreach (var kvp in moduleData)
          {
            var langCode = kvp.Key;
            var translations = kvp.Value;

            if (!mergedData.ContainsKey(langCode))
            {
              mergedData[langCode] = new Dictionary<string, string>();
            }

            foreach (var translation in translations)
            {
              mergedData[langCode][translation.Key] = translation.Value;
            }
          }
        }
        catch (Exception e)
        {
          Debug.LogWarning($"[LocalizationBuildProcess] Failed to parse module translation file {moduleFile.name}: {e.Message}");
        }
      }

      if (mergedData.Count == 0)
      {
        return null;
      }

      var json = JsonConvert.SerializeObject(mergedData, Formatting.None);
      var textAsset = new TextAsset(json);
      textAsset.name = "MergedTranslations";
      return textAsset;
    }
  }
}
