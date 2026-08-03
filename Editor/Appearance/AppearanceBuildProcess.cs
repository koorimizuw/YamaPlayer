using UnityEngine;
using UnityEngine.UI;
using Yamadev.YamaStream.UI;

namespace Yamadev.YamaStream.Editor

{
  public class AppearanceBuildProcess : IYamaPlayerBuildProcess
  {
    public int callbackOrder => -2000;

    public void Process()
    {
      var appearanceSettings = Object.FindObjectsByType<AppearanceSettings>(FindObjectsInactive.Include, FindObjectsSortMode.None);
      foreach (var appearanceSetting in appearanceSettings)
      {
        ProcessAppearanceSetting(appearanceSetting);
      }
    }

    private void ProcessAppearanceSetting(AppearanceSettings appearanceSetting)
    {
      if (appearanceSetting == null) return;
      var uiController = appearanceSetting.GetComponent<UIController>();
      if (uiController == null) return;
      var defaultColorSet = appearanceSetting.DefaultColorSet;
      if (defaultColorSet == null) return;

      uiController.SetProgramVariable("_primaryColor", defaultColorSet.primaryColor);
      uiController.SetProgramVariable("_secondaryColor", defaultColorSet.secondaryColor);

      var colorDefinitions = appearanceSetting.GetComponentsInChildren<ColorDefinition>(true);
      foreach (var colorDefinition in colorDefinitions)
      {
        if (colorDefinition == null) continue;
        var color = colorDefinition.colorType == ColorType.Primary ? defaultColorSet.primaryColor : defaultColorSet.secondaryColor;
        if (colorDefinition.TryGetComponent<Image>(out var image)) image.color = color;
        else if (colorDefinition.TryGetComponent<Text>(out var text)) text.color = color;
      }
    }
  }
}