﻿// USharpVideo - EditorURLResolverShim
// https://github.com/MerlinVR/USharpVideo/blob/master/Assets/USharpVideo/Scripts/Editor/EditorURLResolverShim.cs
// MIT LICENSE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Video.Components;
using VRC.SDK3.Video.Components.AVPro;
using VRC.SDK3.Video.Interfaces.AVPro;
using VRC.SDKBase;
using System.IO;

#if AVPRO_DEBUG
using RenderHeads.Media.AVProVideo;
using static RenderHeads.Media.AVProVideo.MediaPlayer;
#endif


namespace Yamadev.YamaStream.Editor
{
    /// <summary>
    /// Allows people to put in links to YouTube videos and other supported video services and have links just work
    /// Hooks into VRC's video player URL resolve callback and uses the VRC installation of YouTubeDL to resolve URLs in the editor.
    /// </summary>
    public static class VideoPlayerResolver
    {
        private static string _youtubeDLPath = "";
        private static HashSet<System.Diagnostics.Process> _runningYtdlProcesses = new HashSet<System.Diagnostics.Process>();
        private static HashSet<MonoBehaviour> _registeredBehaviours = new HashSet<MonoBehaviour>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void SetupURLResolveCallback()
        {

            string[] splitPath = Application.persistentDataPath.Split('/', '\\');
            _youtubeDLPath = string.Join("\\", splitPath.Take(splitPath.Length - 2)) + "\\VRChat\\VRChat\\Tools\\yt-dlp.exe";

            if (!File.Exists(_youtubeDLPath))
            {
                _youtubeDLPath = string.Join("\\", splitPath.Take(splitPath.Length - 2)) + "\\VRChat\\VRChat\\Tools\\youtube-dl.exe";
            }

            if (!File.Exists(_youtubeDLPath))
            {
                Debug.LogWarning("[YamaStream] Unable to find VRC YouTube-DL or YT-DLP installation, URLs will not be resolved in editor. Test your videos in game.");
                return;
            }

#if AVPRO_DEBUG
            VRCAVProVideoPlayer.Initialize = InitilizeVRCAVProVideoPlayer;
            AVProPlayerResolver.StartResolveURLCoroutine = ResolveURLCallback;
#endif
            VRCUnityVideoPlayer.StartResolveURLCoroutine = ResolveURLCallback;
            EditorApplication.playModeStateChanged += PlayModeChanged;
        }

#if AVPRO_DEBUG
        private static IAVProVideoPlayerInternal InitilizeVRCAVProVideoPlayer(VRCAVProVideoPlayer avPro)
        {
            MediaPlayer mediaPlayer = avPro.gameObject.AddComponent<MediaPlayer>();
#if UNITY_EDITOR_WIN
            OptionsWindows options = (OptionsWindows)mediaPlayer.GetCurrentPlatformOptions();
            options._audioMode = Windows.AudioOutput.Unity;
#endif
            VRCAVProVideoScreen[] vrcAVProVideoScreens = Utils.FindComponentsInHierarthy<VRCAVProVideoScreen>();
            foreach (VRCAVProVideoScreen screen in vrcAVProVideoScreens)
            {
                if (screen.VideoPlayer == avPro)
                {
                    ApplyToMesh applyToMesh = screen.gameObject.AddComponent<ApplyToMesh>();
                    applyToMesh.Player = mediaPlayer;
                    MeshRenderer renderer = screen.GetComponent<MeshRenderer>();
                    if (renderer != null) applyToMesh.MeshRenderer = renderer;
                }
            }
            VRCAVProVideoSpeaker[] vrcAVProVideoSpeakers = Utils.FindComponentsInHierarthy<VRCAVProVideoSpeaker>();
            foreach (VRCAVProVideoSpeaker speaker in vrcAVProVideoSpeakers)
            {
                if (speaker.VideoPlayer == avPro)
                {
                    AudioOutput audioOutput = speaker.gameObject.AddComponent<AudioOutput>();
                    audioOutput.Player = mediaPlayer;
                }
            }

            AVProPlayerResolver resolver = new AVProPlayerResolver();
            resolver.BasePlayer = avPro;
            resolver.MediaPlayer = mediaPlayer;
            return resolver;
        }
#endif

        /// <summary>
        /// Cleans up any remaining YTDL processes from this play.
        /// In some cases VRC's YTDL has hung indefinitely eating CPU so this is a precaution against that potentially happening.
        /// </summary>
        /// <param name="change"></param>
        private static void PlayModeChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingPlayMode)
            {
                foreach (var process in _runningYtdlProcesses)
                {
                    if (!process.HasExited)
                    {
                        //Debug.Log("Closing YTDL process");
                        process.Close();
                    }
                }

                _runningYtdlProcesses.Clear();

                // Apparently the URLResolveCoroutine will run after this method in some cases magically. So don't because the process will throw an exception.
                foreach (MonoBehaviour behaviour in _registeredBehaviours)
                    behaviour.StopAllCoroutines();

                _registeredBehaviours.Clear();
            }
        }

        private static void ResolveURLCallback(VRCUrl url, int resolution, UnityEngine.Object videoPlayer, Action<string> urlResolvedCallback, Action<VideoError> errorCallback)
        {
            // Broken for some unknown reason, when multiple rate limits fire off, only fires the first callback.
            //if ((System.DateTime.UtcNow - lastRequestTime).TotalSeconds < 5.0)
            //{
            //    Debug.LogWarning("Rate limited " + videoPlayer, videoPlayer);
            //    errorCallback(VideoError.RateLimited);
            //    return;
            //}

            var ytdlProcess = new System.Diagnostics.Process();

            ytdlProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            ytdlProcess.StartInfo.CreateNoWindow = true;
            ytdlProcess.StartInfo.UseShellExecute = false;
            ytdlProcess.StartInfo.RedirectStandardOutput = true;
            ytdlProcess.StartInfo.FileName = _youtubeDLPath;
            ytdlProcess.StartInfo.Arguments = $"--no-check-certificate --no-cache-dir --rm-cache-dir -f \"(mp4/best)[height<=?{resolution}][height>=?64][protocol^=http]\" --get-url \"{url}\"";

            Debug.Log($"[<color=#ff70ab>YamaStream</color>] Attempting to resolve URL '{url}'");

            ytdlProcess.Start();
            _runningYtdlProcesses.Add(ytdlProcess);

            ((MonoBehaviour)videoPlayer).StartCoroutine(URLResolveCoroutine(url.ToString(), ytdlProcess, videoPlayer, urlResolvedCallback, errorCallback));

            _registeredBehaviours.Add((MonoBehaviour)videoPlayer);
        }

        private static IEnumerator URLResolveCoroutine(string originalUrl, System.Diagnostics.Process ytdlProcess, UnityEngine.Object videoPlayer, Action<string> urlResolvedCallback, Action<VideoError> errorCallback)
        {
            while (!ytdlProcess.HasExited)
                yield return new WaitForSeconds(0.1f);

            _runningYtdlProcesses.Remove(ytdlProcess);

            string resolvedURL = ytdlProcess.StandardOutput.ReadLine();

            // If a URL fails to resolve, YTDL will send error to stderror and nothing will be output to stdout
            if (string.IsNullOrEmpty(resolvedURL))
                errorCallback(VideoError.InvalidURL);
            else
            {
                Debug.Log($"[<color=#ff70ab>YamaStream</color>] Successfully resolved URL '{originalUrl}' to '{resolvedURL}'");
                urlResolvedCallback(resolvedURL);
            }
        }
    }
}