using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Yamadev.YamaStream.Editor
{
  public static class EditorLocalization
  {
    private const string TranslationFolder = "Packages/net.kwxxw.yama-stream/Assets/Localization/Editor";
    private const string LanguageKey = "YamaPlayer_EditorLanguage";
    private const string DefaultLanguage = "en";

    private static readonly (string code, string name)[] SupportedLanguages =
    {
      ("ja", "日本語"),
      ("en", "English"),
      ("es-CL", "Español"),
      ("ko", "한국어"),
      ("ru", "Русский"),
      ("uk", "Українська"),
      ("it", "Italiano"),
      ("zh-CN", "简体中文"),
      ("zh-TW", "繁體中文")
    };

    private static Dictionary<string, Dictionary<string, string>> _translations;

    public static string[] AvailableLanguages => SupportedLanguages.Select(l => l.code).ToArray();

    public static string CurrentLanguage
    {
      get
      {
        var saved = EditorPrefs.GetString(LanguageKey);
        if (!string.IsNullOrEmpty(saved) && AvailableLanguages.Contains(saved))
          return saved;

        var systemLanguage = CultureInfo.CurrentCulture.Name;
        var matched = AvailableLanguages.FirstOrDefault(l =>
            systemLanguage.StartsWith(l, StringComparison.OrdinalIgnoreCase));

        var language = matched ?? DefaultLanguage;
        EditorPrefs.SetString(LanguageKey, language);
        return language;
      }
      set => EditorPrefs.SetString(LanguageKey, value);
    }

    private static Dictionary<string, Dictionary<string, string>> Translations
    {
      get
      {
        if (_translations == null)
        {
          _translations = new Dictionary<string, Dictionary<string, string>>();
          foreach (var (code, _) in SupportedLanguages)
          {
            var path = $"{TranslationFolder}/{code}.json";
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (asset == null) continue;
            try
            {
              var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(asset.text);
              if (dict != null)
              {
                _translations[code] = dict;
              }
            }
            catch (Exception e)
            {
              Debug.LogWarning($"[EditorLocalization] Failed to parse {path}: {e.Message}");
            }
          }

          LoadModuleTranslations();
        }
        return _translations;
      }
    }

    private static void LoadModuleTranslations()
    {
      ModuleManagerEditor.FindYamaPlayerModules();

      foreach (var (moduleDefinition, _) in ModuleManager.ModuleDefinitions)
      {
        if (moduleDefinition.editorTranslationFile == null) continue;

        try
        {
          var nested = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(
            moduleDefinition.editorTranslationFile.text);
          if (nested == null) continue;

          foreach (var (langCode, translations) in nested)
          {
            if (!_translations.ContainsKey(langCode))
              _translations[langCode] = new Dictionary<string, string>();

            foreach (var (key, value) in translations)
            {
              _translations[langCode][key] = value;
            }
          }
        }
        catch (Exception e)
        {
          Debug.LogWarning($"[EditorLocalization] Failed to parse module translation ({moduleDefinition.moduleName}): {e.Message}");
        }
      }
    }

    public static string GetValue(string key, string language)
    {
      if (Translations.TryGetValue(language, out var dict) && dict.TryGetValue(key, out var value))
        return value;
      return null;
    }

    public static string Get(string key) =>
        GetValue(key, CurrentLanguage) ?? GetValue(key, DefaultLanguage) ?? key;

    public static GUIContent GetLayout(string key) => new GUIContent(Get(key));

    public static GUIContent GetLayout(string labelKey, string tooltipKey) =>
        new GUIContent(Get(labelKey), Get(tooltipKey));

    public static string GetLanguageName(string languageCode)
    {
      var found = SupportedLanguages.FirstOrDefault(l => l.code == languageCode);
      return found.name ?? languageCode;
    }

    public static void ReloadTranslations()
    {
      _translations = null;
    }
  }
}
