using System;
using UnityEngine;

namespace Yamadev.YamaStream
{
  public class YamaPlayerModuleDefinition : MonoBehaviour
  {
    public string moduleName;
    public string moduleDescription;
    public string version;
    public bool allowMultiple;
    public bool noNeedSetUp;

    public ModuleUISlot[] uiSlots;

    public string moduleNameTranslationKey;
    public string moduleDescriptionTranslationKey;

    public TextAsset editorTranslationFile;
    public TextAsset playerTranslationFile;
  }

  [Serializable]
  public class ModuleUISlot
  {
    public string targetPath;
    public GameObject content;
    public int siblingIndex;
  }
}
