using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDK3.Video.Components.AVPro;

namespace Yamadev.YamaStream
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(VRCAVProVideoSpeaker))]
    [RequireComponent(typeof(VRCSpatialAudioSource))]
    [DisallowMultipleComponent]
    public class YamaPlayerAudioSource : MonoBehaviour
    {
        public Controller controller;
        public VRCAVProVideoSpeaker.ChannelMode channelMode;

        public AudioSource AudioSource
        {
            get
            {
                return GetComponent<AudioSource>();
            }
        }

        public VRCAVProVideoSpeaker AVProSpeaker
        {
            get
            {
                return GetComponent<VRCAVProVideoSpeaker>();
            }
        }
    }
}