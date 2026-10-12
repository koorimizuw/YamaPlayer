using System.Collections.Generic;
using UnityEngine;

namespace Yamadev.YamaStream
{
  public class ModuleManager : MonoBehaviour
  {
    public static Dictionary<YamaPlayerModuleDefinition, GameObject> ModuleDefinitions = new Dictionary<YamaPlayerModuleDefinition, GameObject>();
  }
}
