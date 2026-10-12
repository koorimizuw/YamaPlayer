using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Yamadev.YamaStream.Editor
{
  internal class YamaPlayerPostProcess : IYamaPlayerBuildProcess
  {
    public int callbackOrder => -1;

    public void Process()
    {
      var yamaPlayers = Object.FindObjectsByType<YamaPlayer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
      foreach (var player in yamaPlayers)
      {
        ProcessYamaPlayer(player);
      }
    }

    private static void ProcessYamaPlayer(YamaPlayer player)
    {
      if (player == null) return;
      var internalController = player.GetComponentInChildren<Controller>();
      if (internalController != null)
      {
        internalController.SetProgramVariable("_version", PackageManager.Version);
      }
      else
      {
        Debug.LogWarning($"Controller not found in YamaPlayer {player.name}");
      }

      var controller = player.GetComponentInChildren<Controller>();
      if (controller != null)
      {
        controller.transform.SetParent(null, true);
        GameObjectUtility.EnsureUniqueNameForSibling(controller.gameObject);
      }
    }
  }
}