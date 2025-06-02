using System.Linq;
using UnityEngine;

namespace Yamadev.YamaStream.Editor
{
    public class YamaPlayerScreenBuildProcess : IYamaPlayerBuildProcess
    {
        public int callbackOrder => -200;

        public void Process()
        {
            try
            {
                var screens = GameObject.FindObjectsOfType<YamaPlayerScreen>(true);
                foreach (var screen in screens)
                {
                    SetupScreen(screen);
                }

                AssignLTCGITexture();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error occurred during YamaPlayerScreenBuildProcess: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        public void SetupScreen(YamaPlayerScreen screen)
        {
            if (screen == null || screen.controller == null)
            {
                Debug.LogWarning("YamaPlayerScreen or its controller is null, skipping setup.");
                return;
            }

            screen.controller.AddScreen(screen.Type, screen.Reference);
        }

        public void AssignLTCGITexture()
        {
#if USE_LTCGI
            try
            {
                var controller = FindLTCGIActivedController();
                if (controller != null)
                {
                    controller.AddScreen(ScreenType.Material, LTCGIUtility.CRT.material);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error occurred: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
#endif
        }

        public Controller FindLTCGIActivedController()
        {
#if USE_LTCGI
       var controllers = GameObject.FindObjectsOfType<Controller>();
            var actived = controllers.Where(c => (bool)c.GetProgramVariable("_useLTCGI"));

            if (actived.Count() > 1)
            {
                Debug.LogWarning("More than one LTCGI controller is active, using the first one.");
            }
            return actived.FirstOrDefault();
#else
            return null;
#endif
        }
    }
}