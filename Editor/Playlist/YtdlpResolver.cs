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
            var title = IsAvailable ? Localization.Get("updateYtdlp") : Localization.Get("downloadYtdlp");
            var message = IsAvailable ? Localization.Get("updateYtdlpMessage") : Localization.Get("downloadYtdlpMessage");
            return EditorUtility.DisplayDialog(
                title,
                message,
                Localization.Get("yes"),
                Localization.Get("no")
            );
        }

        public static async UniTask<bool> EnsureYtdlpAvailable()
        {
            if (IsAvailable) return true;

            return await DownloadYtdlpExecutable();
        }

        public static async UniTask<List<string>> GetPlaylist(string playlistUrl)
        {
            if (string.IsNullOrEmpty(playlistUrl))
            {
                Debug.LogError(Localization.Get("playlistUrlCannotBeNullOrEmpty"));
                return new List<string>();
            }

            if (!await EnsureYtdlpAvailable())
            {
                Debug.LogError(Localization.Get("ytdlpNotAvailable"));
                return new List<string>();
            }

            return await ExecutePlaylistExtraction(playlistUrl);
        }

        public static async UniTask<bool> DownloadYtdlpExecutable()
        {
            if (!ShowDownloadConfirmationDialog())
            {
                Debug.LogWarning(Localization.Get("ytdlpDownloadCancelledByUser"));
                return false;
            }

            var progressTitle = Localization.Get("downloadingYtdlp");

            try
            {
                EditorUtility.DisplayProgressBar(progressTitle, Localization.Get("downloadingYtdlpExecutable"), 0.5f);

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
                        Debug.LogError($"{Localization.Get("failedToDownloadYtdlp")}: {request.error}");
                        return false;
                    }
                }

                if (!SetExecutablePermissions())
                {
                    Debug.LogWarning(Localization.Get("failedToSetExecutablePermissionsButDownloadCompleted"));
                }

                Debug.Log(Localization.Get("ytdlpDownloadedSuccessfully"));
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{Localization.Get("exceptionOccurredWhileDownloadingYtdlp")}: {ex.Message}");
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
                Debug.LogWarning($"{Localization.Get("failedToSetExecutablePermissions")}: {ex.Message}");
                return false;
            }
#else
            return true;
#endif
        }

        private static async UniTask<List<string>> ExecutePlaylistExtraction(string playlistUrl)
        {
            var progressTitle = Localization.Get("extractingPlaylist");

            try
            {
                EditorUtility.DisplayProgressBar(progressTitle, $"{Localization.Get("extractingPlaylistFrom")}: {playlistUrl}", 0.5f);

                var processInfo = CreatePlaylistExtractionProcess(playlistUrl);

                using (var process = Process.Start(processInfo))
                {
                    if (process == null)
                    {
                        throw new InvalidOperationException(Localization.Get("failedToStartYtdlpProcess"));
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
                Debug.LogError($"{Localization.Get("exceptionOccurredWhileExtractingPlaylist")}: {ex.Message}");
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
                Debug.LogWarning(Localization.Get("ytDlpReturnedEmptyOutput"));
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