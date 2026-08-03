using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDK3.Video.Components.AVPro;

namespace Yamadev.YamaStream
{
  [RequireComponent(typeof(AudioSource))]
  [RequireComponent(typeof(VRCAVProVideoSpeaker))]
  [RequireComponent(typeof(VRCSpatialAudioSource))]
  [DisallowMultipleComponent]
  [AddComponentMenu("YamaStream/YamaPlayer Speaker")]
  public class YamaPlayerSpeaker : MonoBehaviour
  {
    public Controller controller;

    public AudioSource AudioSource => GetComponent<AudioSource>();
  }
}