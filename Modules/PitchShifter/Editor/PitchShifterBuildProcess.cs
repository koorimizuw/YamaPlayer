using System.Linq;
using UnityEngine;
using VRC.SDK3.Video.Components.AVPro;
using Yamadev.YamaStream.Editor;

namespace Yamadev.YamaStream.Modules.PitchShifter.Editor
{
  public class PitchShifterBuildProcess : IYamaPlayerBuildProcess
  {
    public int callbackOrder => -2800;

    public void Process()
    {
      var pitchShifters = Object.FindObjectsByType<PitchShifter>(FindObjectsSortMode.None);
      if (pitchShifters == null || pitchShifters.Length == 0) return;

      foreach (var pitchShifter in pitchShifters)
      {
        if (pitchShifter == null) continue;
        ProcessPitchShifter(pitchShifter);
      }
    }

    private void ProcessPitchShifter(PitchShifter pitchShifter)
    {
      var controller = pitchShifter.GetComponentInParent<Controller>();
      if (controller == null)
      {
        Debug.LogWarning("[PitchShifter] No Controller found in parent hierarchy.");
        return;
      }

      var allSpeakers = Object.FindObjectsByType<YamaPlayerSpeaker>(FindObjectsInactive.Include, FindObjectsSortMode.None);
      var matchingSpeakers = allSpeakers.Where(speaker => speaker != null && speaker.controller == controller).ToList();

      if (matchingSpeakers.Count == 0) return;

      var clonedAudioSources = new AudioSource[matchingSpeakers.Count];

      for (int i = 0; i < matchingSpeakers.Count; i++)
      {
        var speaker = matchingSpeakers[i];
        var original = speaker.gameObject;

        var clone = Object.Instantiate(original, original.transform.parent);
        clone.name = original.name + "_PitchShifted";
        clone.transform.SetPositionAndRotation(original.transform.position, original.transform.rotation);
        clone.transform.localScale = original.transform.localScale;

        var yamaPlayerSpeaker = clone.GetComponent<YamaPlayerSpeaker>();
        if (yamaPlayerSpeaker != null)
        {
          Object.DestroyImmediate(yamaPlayerSpeaker);
        }

        var avProSpeaker = clone.GetComponent<VRCAVProVideoSpeaker>();
        if (avProSpeaker != null)
        {
          Object.DestroyImmediate(avProSpeaker);
        }

        var clonedAudioSource = clone.GetComponent<AudioSource>();
        clonedAudioSource.clip = null;
        clonedAudioSource.playOnAwake = false;

        clonedAudioSources[i] = clonedAudioSource;
      }

      pitchShifter.SetProgramVariable("_outputSources", clonedAudioSources);
    }
  }
}
