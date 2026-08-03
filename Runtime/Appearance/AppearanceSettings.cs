using System.Linq;
using UnityEngine;

namespace Yamadev.YamaStream
{
  [AddComponentMenu("YamaPlayer/Appearance Settings")]
  public class AppearanceSettings : MonoBehaviour
  {
    public string defaultColorSet = "";
    public ColorSetData[] colorSets = new ColorSetData[0];

    public ColorSetData DefaultColorSet => colorSets.FirstOrDefault(cs => cs.colorSetName == defaultColorSet);
  }
}
