
using UdonSharp;
using VRC.SDK3.Data;
using System;

namespace Yamadev.YamaStream
{
    public class Localization: UdonSharpBehaviour
    {
        public static Localization Initialize(string translation)
        {
            // 0. DataDictionary: translation data
            // 1. string: current language
            object[] result = new object[] { null, null };
            if (VRCJson.TryDeserializeFromJson(translation, out DataToken data) &&
                data.TokenType == TokenType.DataDictionary) result[0] = data.DataDictionary;
            result[1] = LocalizationExtentions.GetLanguageByTimeZone();
            return (Localization)(object)result;
        }
    }

    public static class LocalizationExtentions
    {
        public static string GetValue(this Localization _i18n, string key)
        {
            DataDictionary data = (DataDictionary)((object[])(object)_i18n)[0];
            string language = (string)((object[])(object)_i18n)[1];
            if (data == null) return string.Empty;
            if (data.TryGetValue(language, out var tr))
                if (tr.DataDictionary.TryGetValue(key, out var value)) return value.String;
            return string.Empty;
        }

        public static void SetLanguage(this Localization _i18n, string language) =>
            ((object[])(object)_i18n)[1] = string.IsNullOrEmpty(language) ? GetLanguageByTimeZone() : language;

        public static string GetLanguageByTimeZone()
        {
            TimeZoneInfo tz = TimeZoneInfo.Local;
            switch (tz.Id)
            {
                case "Tokyo Standard Time":
                    return "ja-JP";
                case "Taipei Standard Time":
                    return "zh-TW";
                case "China Standard Time":
                    return "zh-CN";
                case "Korea Standard Time":
                    return "ko-KR";
                case "North Korea Standard Time":
                    return "ko-KR";
                default:
                    return "en-US";
            }
        }
    }
}