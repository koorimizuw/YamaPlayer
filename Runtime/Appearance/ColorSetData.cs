using System;
using UnityEngine;

namespace Yamadev.YamaStream
{
  [Serializable]
  public class ColorSetData
  {
    public string colorSetName = "";
    public Color primaryColor;
    public Color secondaryColor;
    public Color infoColor;
    public Color successColor;
    public Color alermColor;
    public Color errorColor;
  }
}
