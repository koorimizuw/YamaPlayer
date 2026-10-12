using System;
using UnityEngine;

namespace Yamadev.YamaStream
{
  [Serializable]
  public class LanguageData
  {
    public string languageCode = "en";
    public string displayName = "English";
    public TextAsset translationFile;
    public Font font;
  }
}
