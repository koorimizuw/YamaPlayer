using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Yamadev.YamaStream.Editor
{
#if USE_AUDIOLINK
    using AudioLink = AudioLink.AudioLink;

    public class AudioLinkBuildProcess : IYamaPlayerBuildProcess
    {
        public int callbackOrder => -100;

        private const string AudioLinkGuid = "8c1f201f848804f42aa401d0647f8902";

        public void Process()
        {
            try
            {
                var yamaPlayerInstances = GameObject.FindObjectsOfType<Controller>();
                var audioLinked = yamaPlayerInstances.Where(instance => (bool)instance.GetProgramVariable("_useAudioLink"));
                foreach (var instance in audioLinked)
                {
                    // ProcessAudioLink(instance);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error occurred: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private void ProcessAudioLink(Controller instance)
        {
            try
            {
                var audioLink = SetupAudioLink();
                if (audioLink == null)
                {
                    Debug.LogError("Failed to add AudioLink");
                    return;
                }
                instance.SetProgramVariable("_audioLink", audioLink);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error occurred: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        public static AudioLink SetupAudioLink()
        {
            AudioLink[] audioLinks = GameObject.FindObjectsOfType<AudioLink>();
            if (audioLinks.Length > 0)
            {
                if (audioLinks.Length > 1)
                {
                    Debug.LogWarning("More than one AudioLink in the scene");
                }
                return audioLinks[0];
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(AudioLinkGuid));
            if (prefab != null)
            {
                GameObject obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                return obj.GetComponent<AudioLink>();
            }

            return null;
        }
    }
#endif
}