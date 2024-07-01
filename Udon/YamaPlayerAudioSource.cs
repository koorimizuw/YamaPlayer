
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;

namespace Yamadev.YamaStream
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(VRCSpatialAudioSource))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class YamaPlayerAudioSource : UdonSharpBehaviour
    {
        [SerializeField] Controller _controller;
        void Start()
        {
            if (_controller == null) return;
            AudioSource audioSource = GetComponent<AudioSource>();
            _controller.AddAudioSource(audioSource);
        }
    }
}