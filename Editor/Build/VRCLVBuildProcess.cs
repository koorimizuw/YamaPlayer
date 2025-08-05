using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if USE_VRCLV
using VRCLightVolumes;
using Yamadev.YamaStream.Script;
#endif

namespace Yamadev.YamaStream.Editor
{
#if USE_VRCLV

    public class VRCLVBuildProcess : IYamaPlayerBuildProcess
    {
        private const string CRTGuid = "eb781ab662c55e5479cf313e5916c575";
        private Lazy<List<LightVolume>> _lightVolumes = new Lazy<List<LightVolume>>(
            () => GameObject.FindObjectsOfType<LightVolume>().ToList()
        );

        public int callbackOrder => -100;

        public void Process()
        {
            try
            {
                var players = FindTVGIEnabledYamaPlayers();
                foreach (var player in players)
                {
                    SetupTVGI(player);
                }

                EnsureAutoUpdateVolumes();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error occurred during VRCLVBuildProcess: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private YamaPlayer[] FindTVGIEnabledYamaPlayers()
        {
            var players = GameObject.FindObjectsOfType<YamaPlayer>();
            return players.Where(player => player.UseLightVolumes).ToArray();
        }

        public void SetupTVGI(YamaPlayer player)
        {
            try
            {
                var go = new GameObject($"YamaPlayerTVGI");
                GameObjectUtility.EnsureUniqueNameForSibling(go);
                var tvgi = go.AddUdonSharpComponent<LightVolumeTVGI>(typeof(LightVolumeTVGI).GetSyncType());

                // AutoAssignAdditiveLightVolumes(player, tvgi);
                var instances = player.TargetLightVolumes.Select(lv => lv.LightVolumeInstance).ToArray();
                tvgi.TargetLightVolumes = instances;

                var crt = CreateNewCustomRenderTexture();
                tvgi.TargetRenderTexture = crt;

                var controller = player.transform.GetComponentInChildren<Controller>();
                if (controller != null)
                {
                    controller.AddScreen(ScreenType.Material, crt.material);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error occurred while setting up TVGI: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private CustomRenderTexture CreateNewCustomRenderTexture()
        {
            var path = AssetDatabase.GUIDToAssetPath(CRTGuid);
            var texture = AssetDatabase.LoadAssetAtPath<CustomRenderTexture>(path);

            if (texture != null && texture.material != null)
            {
                var newMaterial = new Material(texture.material);
                var newCRT = new CustomRenderTexture(texture.width, texture.height, texture.graphicsFormat);
                newCRT.name = texture.name;
                newCRT.useMipMap = true;
                newCRT.autoGenerateMips = true;
                newCRT.initializationMode = CustomRenderTextureUpdateMode.OnLoad;
                newCRT.updateMode = CustomRenderTextureUpdateMode.Realtime;
                newCRT.material = newMaterial;

                return newCRT;
            }
            return null;
        }

        private void EnsureAutoUpdateVolumes()
        {
            try
            {
                var managers = GameObject.FindObjectsOfType<LightVolumeManager>();
                if (managers.Length == 0)
                {
                    Debug.LogError("No LightVolumeManager found in the scene. LightVolumes will not work.");
                    return;
                }
                managers.FirstOrDefault().AutoUpdateVolumes = true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error occurred while ensuring AutoUpdateVolumes: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private void AutoAssignAdditiveLightVolumes(YamaPlayer player, LightVolumeTVGI tvgi)
        {
            var lightVolumes = _lightVolumes.Value;
            Debug.Log($"Found {lightVolumes.Count} LightVolumes in the scene.");

            var instances = new List<LightVolumeInstance>();
            foreach (var lightVolume in lightVolumes)
            {
                if (!lightVolume.Additive) continue;
                if (ContainsPoint(lightVolume, player.transform.position))
                {
                    instances.Add(lightVolume.LightVolumeInstance);
                }
            }

            tvgi.TargetLightVolumes = instances.ToArray();
        }

        private bool ContainsPoint(LightVolume lightVolume, Vector3 point)
        {
            Vector3 center = lightVolume.GetPosition();
            Vector3 size = lightVolume.GetScale();
            var bounds = new Bounds(center, size);
            return bounds.Contains(point);
        }
    }
#endif
}