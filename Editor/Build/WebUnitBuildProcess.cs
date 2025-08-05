#if WEB_UNIT_INCLUDED
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using Yamadev.YamachanWebUnit;
using Yamadev.YamaStream.Modules;
using Yamadev.YamaStream.UI;

namespace Yamadev.YamaStream.Editor
{
    public class WebUnitBuildProcess : IYamaPlayerBuildProcess
    {
        public int callbackOrder => -2000;

        public void Process()
        {
            Client client = new GameObject("WebUnitClient").AddUdonSharpComponent<Client>(Networking.SyncType.None);
            foreach (VideoResolver resolver in Utils.FindComponentsInHierarthy<VideoResolver>())
            {
                resolver.SetProgramVariable("_client", client);
            }

            foreach (UIController uiController in Utils.FindComponentsInHierarthy<UIController>())
            {
                VRCUrlInputField dynamicUrlInputField = uiController.GetProgramVariable("_dynamicPlaylistUrlInput") as VRCUrlInputField;
                dynamicUrlInputField.gameObject.SetActive(dynamicUrlInputField != null);
            }
        }
    }
}
#endif