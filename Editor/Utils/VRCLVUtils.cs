#if VRC_LIGHT_VOLUMES_INCLUDED
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using VRCLightVolumes;
using Yamadev.YamaStream.Script;

namespace Yamadev.YamaStream.Editor
{
    public static class VRCLVUtils
    {
        const string _crtGuid = "eb781ab662c55e5479cf313e5916c575";

        public static CustomRenderTexture TVGICRT =>
            AssetDatabase.LoadAssetAtPath<CustomRenderTexture>(AssetDatabase.GUIDToAssetPath(_crtGuid));

        public static LightVolumeTVGI GetOrAddLVTVGI()
        {
            LightVolumeTVGI[] tvgis = Utils.FindComponentsInHierarthy<LightVolumeTVGI>();
            if (tvgis.Length > 0) return tvgis[0];

            LightVolumeTVGI tvgi = new GameObject("YamaPlayerTVGI").AddUdonSharpComponent<LightVolumeTVGI>();
            tvgi.transform.SetParent(null);
            return tvgi;
        }

        public static void SetUpVRCLV(this YamaPlayer player)
        {
            LightVolumeTVGI tvgi = GetOrAddLVTVGI();
            tvgi.TargetRenderTexture = TVGICRT;

            Controller controller = player.GetComponentInChildren<Controller>();
            controller.AddScreenProperty(ScreenType.Material, TVGICRT.material);
        }

        public static void RemoveVRCLV()
        {
            foreach (Controller controller in Utils.FindComponentsInHierarthy<Controller>())
                controller.RemoveScreenProperty(TVGICRT.material);
            foreach (LightVolumeTVGI tvgi in Utils.FindComponentsInHierarthy<LightVolumeTVGI>())
            {
                if (tvgi.TargetRenderTexture == TVGICRT)
                {
                    Object.DestroyImmediate(tvgi.gameObject);
                }
            }
        }
    }
}
#endif