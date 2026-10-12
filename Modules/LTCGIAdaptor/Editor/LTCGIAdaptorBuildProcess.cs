using UnityEngine;
using Yamadev.YamaStream.Editor;

namespace Yamadev.YamaStream.Modules.LTCGIAdaptor.Editor
{
  public class LTCGIAdaptorBuildProcess : IYamaPlayerBuildProcess
  {
    public int callbackOrder => -2900;

    public void Process()
    {
#if LTCGI_INCLUDED
      var adaptors = Object.FindObjectsByType<LTCGIAdaptor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
      if (adaptors == null || adaptors.Length == 0) return;

      var ltcgiAdapters = Object.FindObjectsByType<LTCGI_UdonAdapter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
      if (ltcgiAdapters == null || ltcgiAdapters.Length == 0)
      {
        Debug.LogWarning("[LTCGIAdaptor] No LTCGI_UdonAdapter found in scene. Please add LTCGI Controller to your scene.");
        return;
      }

      var ltcgiAdapter = ltcgiAdapters[0];
      if (ltcgiAdapters.Length > 1)
      {
        Debug.LogWarning($"[LTCGIAdaptor] Multiple LTCGI_UdonAdapter instances found. Using first one: {ltcgiAdapter.gameObject.name}");
      }

      foreach (var adaptor in adaptors)
      {
        if (adaptor == null) continue;
        adaptor.SetProgramVariable("_ltcgiAdapter", ltcgiAdapter);
      }
#endif
    }
  }
}
