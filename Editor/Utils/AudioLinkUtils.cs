#if AUDIOLINK_V1
using UnityEditor;
using UnityEngine;

namespace Yamadev.YamaStream.Editor
{
    using AudioLink = AudioLink.AudioLink;

    public static class AudioLinkUtils
    {
        const string _audioLinkGuid = "8c1f201f848804f42aa401d0647f8902";

        public static AudioLink GetOrAddAudioLink()
        {
            AudioLink[] audioLinks = Utils.FindComponentsInHierarthy<AudioLink>();
            if (audioLinks.Length > 0) return audioLinks[0];

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(_audioLinkGuid));
            if (prefab != null)
            {
                GameObject obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                return obj.GetComponent<AudioLink>();
            }

            return null;
        }
    }
}
#endif