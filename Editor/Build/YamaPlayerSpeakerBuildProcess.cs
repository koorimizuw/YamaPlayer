using System.Linq;
using System.Reflection;
using UnityEngine;
using VRC.SDK3.Video.Components;
using VRC.SDK3.Video.Components.AVPro;

namespace Yamadev.YamaStream.Editor
{
    public class YamaPlayerSpeakerBuildProcess : IYamaPlayerBuildProcess
    {
        public int callbackOrder => -200;

        public void Process()
        {
            try
            {
                var speakers = GameObject.FindObjectsOfType<YamaPlayerAudioSource>(true);
                foreach (var speaker in speakers)
                {
                    SetupSpeaker(speaker);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error occurred during YamaPlayerSpeakerBuildProcess: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private void SetupSpeaker(YamaPlayerAudioSource speaker)
        {
            if (speaker == null || speaker.controller == null)
            {
                Debug.LogWarning("YamaPlayerAudioSource or its controller is null, skipping setup.");
                return;
            }

            speaker.controller.AddAudioSource(speaker.AudioSource);

            var unityVideo = speaker.controller.transform.parent.GetComponentInChildren<VRCUnityVideoPlayer>();
            if (unityVideo != null)
            {
                var field = unityVideo.GetType().GetField("targetAudioSources", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                var targetAudioSources = (field.GetValue(unityVideo) as AudioSource[]).ToList();
                if (!targetAudioSources.Contains(speaker.AudioSource))
                {
                    targetAudioSources.Add(speaker.AudioSource);
                }
                field.SetValue(unityVideo, targetAudioSources.ToArray());
            }

            var avProVideo = speaker.controller.transform.parent.GetComponentInChildren<VRCAVProVideoPlayer>();
            if (avProVideo != null)
            {
                var field = speaker.AVProSpeaker.GetType().GetField("videoPlayer", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                var targetPlayer = field.GetValue(speaker.AVProSpeaker) as VRCAVProVideoPlayer;
                if (targetPlayer != avProVideo) field.SetValue(speaker.AVProSpeaker, avProVideo);
                speaker.AVProSpeaker.GetType().GetField("mode", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).SetValue(speaker.AVProSpeaker, speaker.channelMode);
            }
        }
    }
}