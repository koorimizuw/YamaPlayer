using System.IO;
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
#if UNITY_EDITOR_WIN
        private const string FILENAME = "yt-dlp.exe";
#elif UNITY_EDITOR_OSX
        private const string FILENAME = "yt-dlp_macos";
        private const uint EXECUTABLE_PERMISSION = 0x100 | 0x40 | 0x80 | 0x20 | 0x8 | 0x4 | 0x1; // 0755
#elif UNITY_EDITOR_LINUX
        private const string FILENAME = "yt-dlp_linux";
#else
        private const string FILENAME = "yt-dlp";
#endif

        private static readonly string DownloadUrl = $"https://github.com/yt-dlp/yt-dlp/releases/latest/download/{FILENAME}";
        private static readonly string ExecutablePath = Path.Combine(Path.GetTempPath(), FILENAME);


#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
        [DllImport("libc", EntryPoint = "chmod", SetLastError = true)]
        private static extern int SetFilePermissions(string path, uint mode);
#endif

        public static bool IsAvailable => File.Exists(ExecutablePath);

        private static bool ShowDownloadConfirmationDialog()
        {
            return EditorUtility.DisplayDialog(
                "Download yt-dlp",
                "yt-dlp is required to process playlists. Would you like to download it?",
                "Yes",
                "No"
            );
        }

        public static async UniTask<bool> EnsureYtdlpAvailable()
        {
            if (IsAvailable)
            {
                Debug.Log("yt-dlp is already available.");
                return true;
            }

            if (!ShowDownloadConfirmationDialog())
            {
                Debug.LogWarning("yt-dlp download was cancelled by user.");
                return false;
            }

            return await DownloadYtdlpExecutable();
        }

        public static async UniTask<List<string>> GetPlaylist(string playlistUrl)
        {
            if (string.IsNullOrEmpty(playlistUrl))
            {
                Debug.LogError("Playlist URL cannot be null or empty.");
                return new List<string>();
            }

            if (!await EnsureYtdlpAvailable())
            {
                Debug.LogError("yt-dlp is not available. Cannot retrieve playlist.");
                return new List<string>();
            }

            return await ExecutePlaylistExtraction(playlistUrl);
        }

        private static async UniTask<bool> DownloadYtdlpExecutable()
        {
            const string progressTitle = "Downloading yt-dlp";

            try
            {
                EditorUtility.DisplayProgressBar(progressTitle, "Downloading yt-dlp executable...", 0.5f);

                using (var request = UnityWebRequest.Get(DownloadUrl))
                {
                    var downloadHandler = new DownloadHandlerFile(ExecutablePath)
                    {
                        removeFileOnAbort = true
                    };
                    request.downloadHandler = downloadHandler;

                    await request.SendWebRequest();

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"Failed to download yt-dlp: {request.error}");
                        return false;
                    }
                }

                if (!SetExecutablePermissions())
                {
                    Debug.LogWarning("Failed to set executable permissions, but download completed.");
                }

                Debug.Log("yt-dlp downloaded successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception occurred while downloading yt-dlp: {ex.Message}");
                return false;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static bool SetExecutablePermissions()
        {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            try
            {
                int result = SetFilePermissions(ExecutablePath, EXECUTABLE_PERMISSION);
                return result == 0;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to set executable permissions: {ex.Message}");
                return false;
            }
#else
            return true;
#endif
        }

        private static async UniTask<List<string>> ExecutePlaylistExtraction(string playlistUrl)
        {
            const string progressTitle = "Extracting Playlist";

            try
            {
                EditorUtility.DisplayProgressBar(progressTitle, $"Extracting playlist from: {playlistUrl}", 0.5f);

                var processInfo = CreatePlaylistExtractionProcess(playlistUrl);

                using (var process = Process.Start(processInfo))
                {
                    if (process == null)
                    {
                        throw new InvalidOperationException("Failed to start yt-dlp process.");
                    }

                    var outputTask = process.StandardOutput.ReadToEndAsync();
                    var errorTask = process.StandardError.ReadToEndAsync();

                    await UniTask.WhenAll(
                        outputTask.AsUniTask(),
                        errorTask.AsUniTask()
                    );

                    return ParsePlaylistOutput(errorTask.Result);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception occurred while extracting playlist: {ex.Message}");
                return new List<string>();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static ProcessStartInfo CreatePlaylistExtractionProcess(string playlistUrl)
        {
            var arguments = $"--extractor-args \"youtube:lang=ja\" " +
                           $"--flat-playlist " +
                           $"--no-write-playlist-metafiles " +
                           $"--no-exec " +
                           $"-sijo - " +
                           $"\"{playlistUrl}\"";

            return new ProcessStartInfo(ExecutablePath, arguments)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetTempPath()
            };
        }

        private static List<string> ParsePlaylistOutput(string output)
        {
            if (string.IsNullOrEmpty(output))
            {
                Debug.LogWarning("yt-dlp returned empty output.");
                return new List<string>();
            }

            var jsonLines = output
                .Split('\n')
                .Where(line => !string.IsNullOrWhiteSpace(line) && line.Trim().StartsWith("{"))
                .ToList();

            return jsonLines;
        }
    }
}