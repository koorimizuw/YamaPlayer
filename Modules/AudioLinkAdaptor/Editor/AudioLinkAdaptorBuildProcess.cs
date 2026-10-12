using UnityEngine;
using Yamadev.YamaStream.Editor;

#if AUDIOLINK_V1
using AudioLink;
#endif

namespace Yamadev.YamaStream.Modules.AudioLinkAdaptor.Editor
{
#if AUDIOLINK_V1
  public class AudioLinkAdaptorBuildProcess : IYamaPlayerBuildProcess
  {
    public int callbackOrder => -2900;

    public void Process()
    {
      var adaptors = Object.FindObjectsByType<AudioLinkAdaptor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
      if (adaptors == null || adaptors.Length == 0) return;

      var audioLinks = Object.FindObjectsByType<AudioLink.AudioLink>(FindObjectsSortMode.None);
      if (audioLinks == null || audioLinks.Length == 0)
      {
        Debug.LogWarning("[AudioLinkAdaptor] No AudioLink found in scene.");
        return;
      }

      var audioLink = audioLinks[0];
      if (audioLinks.Length > 1)
      {
        Debug.LogWarning($"[AudioLinkAdaptor] Multiple AudioLink instances found. Using first one: {audioLink.gameObject.name}");
      }

      foreach (var adaptor in adaptors)
      {
        if (adaptor == null) continue;
        adaptor.SetProgramVariable("_audioLink", audioLink);
      }
    }
  }
#endif
}
