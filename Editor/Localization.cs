
using System;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Data;

namespace Yamadev.YamaStream.Script
{
    internal static class Localization
    {
        const string _tranlationGuid = "2d9e9cf6962ed7041b11321394b0bcac";
        const string _languageKey = "YamaPlayer_EditorLanguage";
        static string _defaultLanguage = "en-US";
        static DataDictionary _translation;

        public static DataDictionary Translation
        {
            get
            {
                if (_translation == null)
                {
                    TextAsset languageAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(_tranlationGuid));
                    if (languageAsset != null &&
                        VRCJson.TryDeserializeFromJson(languageAsset.text, out DataToken data) &&
                        data.TokenType == TokenType.DataDictionary) 
                        _translation = data.DataDictionary;
                    else _translation = new DataDictionary();
                }
                return _translation;
            }
        }

        public static string[] AvailableLanguages => Translation.GetKeys().ToArray().Select(i => i.String).ToArray();

        public static string CurrentLanguage
        {
            get
            {
                if (string.IsNullOrEmpty(EditorPrefs.GetString(_languageKey)))
                {
                    if (Array.IndexOf(AvailableLanguages, CultureInfo.CurrentCulture.Name) >= 0)
                        EditorPrefs.SetString(_languageKey, CultureInfo.CurrentCulture.Name);
                    else EditorPrefs.SetString(_languageKey, _defaultLanguage);
                }
                return EditorPrefs.GetString(_languageKey);
            }
            set => EditorPrefs.SetString(_languageKey, value);
        }

        public static string GetValue(string key, string language)
        {
            if (Translation.TryGetValue(language, out var tr) && 
                tr.DataDictionary.TryGetValue(key, out var value))
                return value.String;
            return null;
        }

        public static string Get(string key) => GetValue(key, CurrentLanguage) ?? GetValue(key, _defaultLanguage) ?? key;
        public static GUIContent GetLayout(string key) => new GUIContent(Get(key));

        public static string GetLanguageName(string language)
        {
            switch (language)
            {
                case "ja-JP":
                    return "日本語";
                case "en-US":
                case "en-GB​":
                case "en-CA":
                case "en-AU":
                case "en-SG":
                case "en-IN":
                    return "English";
                case "ko-KR":
                    return "한국어";
                case "zh-CN":
                    return "简体中文";
                case "zh-TW":
                case "zh-HK":
                    return "繁體中文";
            }
            return string.Empty;
        }
    }
}