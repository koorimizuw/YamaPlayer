using UnityEngine;

namespace Yamadev.YamaStream
{
  [AddComponentMenu("YamaPlayer/Localization Settings")]
  public class LocalizationSettings : MonoBehaviour
  {
    public string defaultLanguage = "";
    public LanguageData[] languages = new LanguageData[0];

    public int GetLanguageIndex(string code)
    {
      if (languages == null) return -1;

      for (int i = 0; i < languages.Length; i++)
      {
        if (languages[i] != null && languages[i].languageCode == code)
        {
          return i;
        }
      }
      return -1;
    }

    public LanguageData GetLanguage(string code)
    {
      int index = GetLanguageIndex(code);
      if (index >= 0 && index < languages.Length)
      {
        return languages[index];
      }
      return null;
    }

    public int LanguageCount => languages != null ? languages.Length : 0;

    public TextAsset GetTranslationFile(string code)
    {
      var lang = GetLanguage(code);
      return lang?.translationFile;
    }

    public Font GetFont(string code)
    {
      var lang = GetLanguage(code);
      return lang?.font;
    }

    public string[] GetLanguageCodes()
    {
      if (languages == null) return new string[0];

      string[] codes = new string[languages.Length];
      for (int i = 0; i < languages.Length; i++)
      {
        codes[i] = languages[i] != null ? languages[i].languageCode : "";
      }
      return codes;
    }

    public string[] GetLanguageDisplayNames()
    {
      if (languages == null) return new string[0];

      string[] names = new string[languages.Length];
      for (int i = 0; i < languages.Length; i++)
      {
        names[i] = languages[i] != null ? languages[i].displayName : "";
      }
      return names;
    }
  }
}
