using System.IO;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine.Networking;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;

using Debug = UnityEngine.Debug;

namespace Yamadev.YamaStream.Editor
{
    public static class YtdlpResolver
    {
        static readonly string _url = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe";
        static readonly string _path = Path.Combine(Application.dataPath, "yt-dlp.exe");

        public static bool Exist => File.Exists(_path);

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