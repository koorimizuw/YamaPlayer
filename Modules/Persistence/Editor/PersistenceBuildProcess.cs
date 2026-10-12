using System.Text;
using UnityEngine;
using Yamadev.YamaStream.Editor;

namespace Yamadev.YamaStream.Modules.Persistence.Editor
{
  internal class PersistenceBuildProcess : IYamaPlayerBuildProcess
  {
    public int callbackOrder => -100;

    public void Process()
    {
      var modules = Object.FindObjectsByType<Persistence>(FindObjectsInactive.Include, FindObjectsSortMode.None);

      foreach (var module in modules)
      {
        ProcessModule(module);
      }
    }

    private void ProcessModule(Persistence module)
    {
      if (module == null) return;

      if (string.IsNullOrEmpty((string)module.GetProgramVariable("_uniqueId")))
      {
        var pathBasedKey = GetHierarchyPath(module.transform);
        module.SetProgramVariable("_pathBasedKey", pathBasedKey);
      }
    }

    private string GetHierarchyPath(Transform transform)
    {
      var sb = new StringBuilder(transform.name);
      var current = transform.parent;
      while (current != null)
      {
        sb.Insert(0, current.name + "/");
        current = current.parent;
      }
      return sb.ToString();
    }
  }
}
