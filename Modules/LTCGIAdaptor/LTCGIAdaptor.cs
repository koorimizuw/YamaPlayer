using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Yamadev.YamaStream.Modules.LTCGIAdaptor
{
  [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
  public class LTCGIAdaptor : YamaPlayerModule
  {
#if LTCGI_INCLUDED
    [SerializeField, HideInInspector] private LTCGI_UdonAdapter _ltcgiAdapter;
    private Texture2D _blackTexture;

    public override void Start()
    {
      base.Start();
      if (!Utilities.IsValid(_ltcgiAdapter)) return;
      
      _blackTexture = new Texture2D(1, 1);
      _blackTexture.SetPixel(0, 0, Color.black);
      _blackTexture.Apply();
      
      _ltcgiAdapter._SetVideoTexture(_blackTexture);
    }

    public override void AfterTextureUpdated(Texture texture)
    {
      if (!Utilities.IsValid(_ltcgiAdapter)) return;
      if (!Utilities.IsValid(texture))
      {
        if (Utilities.IsValid(_blackTexture))
        {
          _ltcgiAdapter._SetVideoTexture(_blackTexture);
        }
        return;
      }
      _ltcgiAdapter._SetVideoTexture(texture);
    }

    public override void AfterVideoStopped()
    {
      if (!Utilities.IsValid(_ltcgiAdapter)) return;
      if (Utilities.IsValid(_blackTexture))
      {
        _ltcgiAdapter._SetVideoTexture(_blackTexture);
      }
    }
#endif
  }
}
