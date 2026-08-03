using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

#if VRC_ENABLE_PLAYER_PERSISTENCE
using VRC.SDK3.Persistence;
#endif

namespace Yamadev.YamaStream.Modules.Persistence
{
  [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
  public class Persistence : YamaPlayerModule
  {
    public const string KEY_PREFIX = "YamaPlayer";

    [SerializeField] internal string _uniqueId;
    [SerializeField, HideInInspector] internal string _pathBasedKey;

    public string UniqueId => _uniqueId;

    private string EffectiveKey => !string.IsNullOrEmpty(_uniqueId) ? _uniqueId : _pathBasedKey;
    public string VolumeKey => string.IsNullOrEmpty(EffectiveKey) ? null : string.Join(".", KEY_PREFIX, EffectiveKey, "Volume");
    public string MuteKey => string.IsNullOrEmpty(EffectiveKey) ? null : string.Join(".", KEY_PREFIX, EffectiveKey, "Mute");
    public string MirrorFlipKey => string.IsNullOrEmpty(EffectiveKey) ? null : string.Join(".", KEY_PREFIX, EffectiveKey, "MirrorFlip");
    public string BrightnessKey => string.IsNullOrEmpty(EffectiveKey) ? null : string.Join(".", KEY_PREFIX, EffectiveKey, "Brightness");

#if VRC_ENABLE_PLAYER_PERSISTENCE
    public override void OnPlayerRestored(VRCPlayerApi player)
    {
      if (!player.isLocal || !Utilities.IsValid(_controller) || string.IsNullOrEmpty(EffectiveKey)) return;
      RestoreSettings(player);
    }

    private void RestoreSettings(VRCPlayerApi player)
    {
      bool updated = false;
      string volumeKey = VolumeKey;
      string muteKey = MuteKey;
      string mirrorFlipKey = MirrorFlipKey;
      string brightnessKey = BrightnessKey;

      if (!string.IsNullOrEmpty(volumeKey) && PlayerData.TryGetFloat(player, volumeKey, out float volume))
      {
        _controller.Volume = volume;
        updated = true;
      }

      if (!string.IsNullOrEmpty(muteKey) && PlayerData.TryGetBool(player, muteKey, out bool mute))
      {
        _controller.Mute = mute;
        updated = true;
      }

      if (!string.IsNullOrEmpty(mirrorFlipKey) && PlayerData.TryGetBool(player, mirrorFlipKey, out bool mirrorFlip))
      {
        _controller.MirrorFlip = mirrorFlip;
        updated = true;
      }

      if (!string.IsNullOrEmpty(brightnessKey) && PlayerData.TryGetFloat(player, brightnessKey, out float brightness))
      {
        _controller.Brightness = brightness;
        updated = true;
      }

      if (updated)
      {
        PrintLog("Player settings restored from persistence.");
      }
    }

    public override void AfterVolumeChanged(float volume)
    {
      string volumeKey = VolumeKey;
      if (string.IsNullOrEmpty(volumeKey)) return;

      PlayerData.SetFloat(volumeKey, volume);
    }

    public override void AfterMuteChanged(bool mute)
    {
      string muteKey = MuteKey;
      if (string.IsNullOrEmpty(muteKey)) return;

      PlayerData.SetBool(muteKey, mute);
    }

    public override void AfterMirrorFlipChanged(bool mirrorFlip)
    {
      string mirrorFlipKey = MirrorFlipKey;
      if (string.IsNullOrEmpty(mirrorFlipKey)) return;

      PlayerData.SetBool(mirrorFlipKey, mirrorFlip);
    }

    public override void AfterBrightnessChanged(float brightness)
    {
      string brightnessKey = BrightnessKey;
      if (string.IsNullOrEmpty(brightnessKey)) return;

      PlayerData.SetFloat(brightnessKey, brightness);
    }
#endif
  }
}
