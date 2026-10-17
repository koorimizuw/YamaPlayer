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

#if VRC_LIGHT_VOLUMES_3
        public static LightVolumeManager GetOrAddLVManager()
        {
            LightVolumeManager[] managers = Utils.FindComponentsInHierarthy<LightVolumeManager>();
            if (managers.Length > 0) return managers[0];

            GameObject go = new GameObject("Light Volume Manager");
            Undo.RegisterCreatedObjectUndo(go, "Create Light Volume Manager");
            LightVolumeManager manager = UdonSharpUndo.AddComponent<LightVolumeManager>(go);
            UdonSharpEditorUtility.CopyProxyToUdon(manager);
            return manager;
        }

        static void EnableAutoUpdateVolumes()
        {
            LightVolumeManager manager = GetOrAddLVManager();
            Undo.RecordObject(manager, "Enable Auto Update Volumes");
            manager.AutoUpdateVolumes = true;
            UdonSharpEditorUtility.CopyProxyToUdon(manager);
            EditorUtility.SetDirty(manager);
        }
#else
        public static LightVolumeSetup GetOrAddLVSetup()
        {
            LightVolumeSetup[] setups = Utils.FindComponentsInHierarthy<LightVolumeSetup>();
            if (setups.Length > 0) return setups[0];

            LightVolumeSetup setup = new GameObject("Light Volume Manager").AddComponent<LightVolumeSetup>();
            setup.SyncUdonScript();

            return setup;
        }

        static void EnableAutoUpdateVolumes()
        {
            LightVolumeSetup setup = GetOrAddLVSetup();
            setup.AutoUpdateVolumes = true;
            setup.SyncUdonScript();
            EditorUtility.SetDirty(setup);
        }
#endif

        public static void SetUpVRCLV(this YamaPlayer player)
        {
            EnableAutoUpdateVolumes();

            LightVolumeTVGI tvgi = GetOrAddLVTVGI();
            tvgi.TargetRenderTexture = TVGICRT;

            Controller controller = player.GetComponentInChildren<Controller>();
            controller.AddScreenProperty(ScreenType.Material, TVGICRT.material);

            Selection.activeObject = tvgi;
        }

        public static void RemoveVRCLV()
        {
            foreach (Controller controller in Utils.FindComponentsInHierarthy<Controller>())
                controller.RemoveScreenProperty(TVGICRT.material);
            foreach (LightVolumeTVGI tvgi in Utils.FindComponentsInHierarthy<LightVolumeTVGI>())
            {
                if (tvgi.TargetRenderTexture == TVGICRT)
                {
                    tvgi.TargetRenderTexture = null;
                }
            }
        }
    }
}
#endif
