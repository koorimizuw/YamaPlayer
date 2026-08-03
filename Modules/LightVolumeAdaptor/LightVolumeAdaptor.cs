using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Rendering;
using VRC.Udon.Common.Interfaces;

#if VRC_LIGHT_VOLUMES
using VRCLightVolumes;
#endif

namespace Yamadev.YamaStream.Modules.LightVolumeAdaptor
{
  [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
  public class LightVolumeAdaptor : YamaPlayerModule
  {
#if VRC_LIGHT_VOLUMES
    [SerializeField] private LightVolumeInstance[] _lightVolumes = new LightVolumeInstance[0];
    [SerializeField] private PointLightVolumeInstance[] _pointLightVolumes = new PointLightVolumeInstance[0];
    private Color32[] _pixels;
    private RenderTexture _downsampledTexture;

    public override void Start()
    {
      base.Start();
      _downsampledTexture = new RenderTexture(64, 32, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
      _downsampledTexture.useMipMap = true;
      _downsampledTexture.autoGenerateMips = true;
      _downsampledTexture.Create();
      _pixels = new Color32[1];
      EnableAutoUpdateVolumes();
    }

    private void EnableAutoUpdateVolumes()
    {
      for (int i = 0; i < _lightVolumes.Length; i++)
      {
        if (Utilities.IsValid(_lightVolumes[i]) &&
            Utilities.IsValid(_lightVolumes[i].LightVolumeManager))
        {
          _lightVolumes[i].LightVolumeManager.AutoUpdateVolumes = true;
          return;
        }
      }

      for (int i = 0; i < _pointLightVolumes.Length; i++)
      {
        if (Utilities.IsValid(_pointLightVolumes[i]) &&
            Utilities.IsValid(_pointLightVolumes[i].LightVolumeManager))
        {
          _pointLightVolumes[i].LightVolumeManager.AutoUpdateVolumes = true;
          return;
        }
      }
    }

    public override void AfterTextureUpdated(Texture texture)
    {
      if (!Utilities.IsValid(texture))
      {
        SetColor(Color.black);
        return;
      }

      VRCGraphics.Blit(texture, _downsampledTexture);
      VRCAsyncGPUReadback.Request(_downsampledTexture, _downsampledTexture.mipmapCount - 1, (IUdonEventReceiver)this);
    }

    public override void OnAsyncGpuReadbackComplete(VRCAsyncGPUReadbackRequest request)
    {
      if (request.TryGetData(_pixels))
      {
        SetColor(_pixels[0]);
      }
    }

    private void SetColor(Color color)
    {
      for (int i = 0; i < _lightVolumes.Length; i++)
      {
        if (Utilities.IsValid(_lightVolumes[i]))
        {
          _lightVolumes[i].Color = color;
        }
      }

      for (int i = 0; i < _pointLightVolumes.Length; i++)
      {
        if (Utilities.IsValid(_pointLightVolumes[i]))
        {
          _pointLightVolumes[i].Color = color;
          _pointLightVolumes[i].IsRangeDirty = true;
        }
      }
    }

    public override void AfterVideoStopped()
    {
      SetColor(Color.black);
    }
#endif
  }
}
