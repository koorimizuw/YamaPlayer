using System;
using VRC.SDK3.Data;
using VRC.SDKBase;
using static VRC.SDKBase.VRCPlayerApi;

namespace Yamadev.YamaStream
{
    public class Localization: ObjectClass
    {
        public static Localization Initialize(string translation)
        {
            // 0. DataDictionary: translation data
            // 1. string: current language
            object[] result = new object[] { null, null };
            if (VRCJson.TryDeserializeFromJson(translation, out DataToken data) &&
                data.TokenType == TokenType.DataDictionary) result[0] = data.DataDictionary;
            result[1] = result.ForceCast<Localization>().GetDefaultLanguage();
            return result.ForceCast<Localization>();
        }
    }

    public static class LocalizationExtentions
    {
        public static string GetValue(this Localization i18n, string key)
        {
            DataDictionary data = (DataDictionary)i18n.UnPack()[0];
            string language = (string)i18n.UnPack()[1];
            if (!Utilities.IsValid(data)) return string.Empty;
            if (data.TryGetValue(language, out var tr))
                if (tr.DataDictionary.TryGetValue(key, out var value)) return value.String;
            return string.Empty;
        }

        public static string GetLanguage(this Localization i18n)
        {
            return (string)i18n.UnPack()[1];
        }

        public static void SetLanguage(this Localization i18n, string language) =>
            ((object[])(object)i18n)[1] = string.IsNullOrEmpty(language) ? GetCurrentLanguage() : language;

        public static string GetDefaultLanguage(this Localization i18n)
        {
            string userLanguage = GetCurrentLanguage();
            DataDictionary data = (DataDictionary)i18n.UnPack()[0];
            if (data.ContainsKey(userLanguage)) return userLanguage;
            return GetLanguageByTimeZone();
        }

        public static string GetLanguageByTimeZone()
        {
            TimeZoneInfo tz = TimeZoneInfo.Local;
            switch (tz.Id)
            {
                case "Tokyo Standard Time":
                    return "ja";
                case "Taipei Standard Time":
                    return "zh-TW";
                case "China Standard Time":
                    return "zh-CN";
                case "Korea Standard Time":
                case "North Korea Standard Time":
                    return "ko";
                default:
                    return "en";
            }
        }
    }
}