using System.IO;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine.Networking;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using Debug = UnityEngine.Debug;

namespace Yamadev.YamaStream.Editor
{
    public static class YtdlpResolver
    {
#if UNITY_EDITOR_WIN
        static readonly string _filename = "yt-dlp.exe";
#elif UNITY_EDITOR_OSX
        static readonly string _filename = "yt-dlp_macos";
#elif UNITY_EDITOR_LINUX
        static readonly string _filename = "yt-dlp_linux";
#endif
        static readonly string _url = $"https://github.com/yt-dlp/yt-dlp/releases/latest/download/{_filename}";
        static readonly string _path = Path.Combine(Path.GetTempPath(), _filename);

        public static bool Exist => File.Exists(_path);

#if UNITY_EDITOR_OSX
        [DllImport("libc", EntryPoint = "chmod", SetLastError = true)]
        private static extern int sys_chmod(string path, uint mode);

        const int _0755 = 0x100 | 0x40 | 0x80 | 0x20 | 0x8 | 0x4 | 0x1;
#endif

        public static async UniTask DownloadYtdlp()
        {
            if (Exist) return;
            if (!EditorUtility.DisplayDialog("Download Yt-dlp", "Download Yt-dlp?", "Yes", "No")) return;
            try
            {
                using (var request = UnityWebRequest.Get(_url))
                {
                    DownloadHandlerFile handler = new DownloadHandlerFile(_path) { removeFileOnAbort = true };
                    request.downloadHandler = handler;
                    await request.SendWebRequest();
                    EditorUtility.DisplayProgressBar("Downloading...", "Download Yt-dlp...", 0);
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Debug.Log("Download Yt-dlp failed.");
                        EditorUtility.ClearProgressBar();
                        return;
                    }
                }
#if UNITY_EDITOR_OSX
                sys_chmod(_path, _0755);
#endif
                Debug.Log("Download Yt-dlp successed.");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        public static async UniTask<List<string>> GetPlaylist(string url)
        {
            if (!Exist) await DownloadYtdlp();
            if (!Exist) return new();
            try
            {
                EditorUtility.DisplayProgressBar("Getting Playlist", $"Getting Playlist from ${url}", 0);
                ProcessStartInfo startInfo = new ProcessStartInfo(_path, $"--extractor-args \"youtube:lang=ja\" --flat-playlist --no-write-playlist-metafiles --no-exec -sijo - {url}")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                };
                Process process = Process.Start(startInfo);
                if (process == null)
                {
                    throw new ArgumentException("Process cannot start.");
                }
                string output = await process.StandardError.ReadToEndAsync();
                return output.Split("\n").Where(line => line.StartsWith("{")).ToList();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}