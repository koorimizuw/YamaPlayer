using System.IO;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Linq;

namespace Yamadev.YamaStream.Script
{
    public static class YtdlpResolver
    {
        static string _url = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe";
        static bool _exist = false;
        static bool _isOriginal = false;
        static string _ytdlpPath = string.Empty;
        static string _dlPath = Path.Combine(Application.dataPath, "yt-dlp.exe");

        static void checkExist()
        {
            if (File.Exists(_dlPath))
            {
                _ytdlpPath = _dlPath;
                _isOriginal = true;
                _exist = true;
                return;
            }
            
            /*
            string[] splitPath = Application.persistentDataPath.Split('/', '\\');
            string path = string.Join("\\", splitPath.Take(splitPath.Length - 2)) + "\\VRChat\\VRChat\\Tools\\yt-dlp.exe";
            if (!File.Exists(path)) path = string.Join("\\", splitPath.Take(splitPath.Length - 2)) + "\\VRChat\\VRChat\\Tools\\youtube-dl.exe";
            if (File.Exists(path))
            {
                _ytdlpPath = path;
                _exist = true;
            }
            */
        }

        public static bool IsOriginal => _isOriginal;
        public static string YtdlpPath
        {
            get
            {
                if (_exist) return _ytdlpPath;
                checkExist();
                return _exist ? _ytdlpPath : string.Empty;
            }
        }

        [MenuItem("YamaPlayer/Download Yt-dlp")]
        public static async Task DownloadYtdlp()
        {
            checkExist();
            if (_exist)
            {
                EditorUtility.DisplayDialog("", "Yt-dlp exist.", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog("Download yt-dlp", $"Download latest release from github.com?", "Yes", "No")) return;
            if (!Uri.TryCreate(_url, UriKind.Absolute, out Uri uriResult)) return;
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.SendAsync(new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = uriResult
            });
            if (response.StatusCode == HttpStatusCode.OK)
            {
                byte[] contentByteData = await response.Content.ReadAsByteArrayAsync();
                File.WriteAllBytes(_dlPath, contentByteData);
            } else
            {
                if (File.Exists(_dlPath)) File.Delete(_dlPath);
            }
            if (!File.Exists(_dlPath)) EditorUtility.DisplayDialog("Error", "Download failed.", "OK");
            checkExist();
            if (_exist) EditorUtility.DisplayDialog("", "Yt-dlp download success.", "OK");
        }

        public static async Task<string> GetPlaylist(string url)
        {
            if (!_exist || !_isOriginal) await DownloadYtdlp();
            if (!_exist || !_isOriginal) return string.Empty;
            EditorUtility.DisplayProgressBar("Getting Playlist", $"Getting Playlist from ${url}", 0);
            ProcessStartInfo startInfo = new ProcessStartInfo(_ytdlpPath, $"--flat-playlist --no-write-playlist-metafiles --no-exec -sijo - {url}")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            Process process = Process.Start(startInfo);
            process.WaitForExit();
            string results = process.StandardOutput.ReadLine();
            string resultsErr = process.StandardError.ReadLine();
            UnityEngine.Debug.Log(results);
            UnityEngine.Debug.Log(resultsErr);
            EditorUtility.ClearProgressBar();
            return string.Empty;
        }
    }
}