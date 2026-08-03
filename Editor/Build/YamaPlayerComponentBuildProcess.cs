using UnityEngine;
using System;
using Yamadev.YamaStream.UI;

using Object = UnityEngine.Object;
using UnityEngine.UI;

namespace Yamadev.YamaStream.Editor
{
  internal class YamaPlayerComponentBuildProcess : IYamaPlayerBuildProcess
  {
    public int callbackOrder => -2000;

    public void Process()
    {
      try
      {
        ProcessYamaPlayerSpeaker();
        ProcessYamaPlayerScreen();
      }
      catch (Exception ex)
      {
        Debug.LogError($"Error processing YamaPlayer components: {ex.Message}");
      }
    }

    private static void ProcessYamaPlayerSpeaker()
    {
      var speakers = Object.FindObjectsByType<YamaPlayerSpeaker>(FindObjectsInactive.Include, FindObjectsSortMode.None);
      foreach (var speaker in speakers)
      {
        if (speaker == null || speaker.controller == null) continue;
        speaker.controller.AddAudioSource(speaker.AudioSource);
      }
    }

    private static void ProcessYamaPlayerScreen()
    {
      var screens = Object.FindObjectsByType<YamaPlayerScreen>(FindObjectsInactive.Include, FindObjectsSortMode.None);
      foreach (var screen in screens)
      {
        if (screen == null || screen.controller == null) continue;
        screen.controller.AddScreen(screen.Type, screen.Reference, screen.property);
      }
    }
  }
}