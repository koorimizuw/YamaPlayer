﻿using System.IO;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine.Networking;
using System.Diagnostics;
using System;
using System.Collections.Generic;

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
            if (!EditorUtility.DisplayDialog("Download Yt-dlp", "Download newest Yt-dlp?", "Yes", "No")) return;
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
            List<string> results = new();
            if (!Exist) return results;
            try
            {
                EditorUtility.DisplayProgressBar("Getting Playlist", $"Getting Playlist from ${url}", 0);
                ProcessStartInfo startInfo = new ProcessStartInfo(_path, $"--flat-playlist --no-write-playlist-metafiles --no-exec -sijo - {url}")
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
                foreach (string line in output.Split("\n"))
                {
                    if (!line.StartsWith("{")) continue;
                    results.Add(line);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            return results;
        }
    }
}