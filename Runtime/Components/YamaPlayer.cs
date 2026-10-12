using UnityEngine;

namespace Yamadev.YamaStream
{
  public sealed class YamaPlayer : MonoBehaviour
  {
    public PlaylistManager PlaylistManager => GetComponentInChildren<PlaylistManager>();
    public ModuleManager ModuleManager => GetComponentInChildren<ModuleManager>();
  }
}