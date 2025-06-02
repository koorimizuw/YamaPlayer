using UnityEngine;
using VRC.SDK3.Components;

namespace Yamadev.YamaStream
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(VRCSpatialAudioSource))]
    [DisallowMultipleComponent]
    public class YamaPlayerAudioSource : MonoBehaviour
    {
        public Controller controller;
    }
}