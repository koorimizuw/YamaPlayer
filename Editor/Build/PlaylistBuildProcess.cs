using System;
using System.Collections.Generic;
using System.Linq;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.Utility;
using Yamadev.YamaStream.Script;

namespace Yamadev.YamaStream.Editor
{
    public class PlaylistBuildProcess : IYamaPlayerBuildProcess
    {
        public int callbackOrder => -1200;

        public void Process()
        {
            try
            {
                var containers = Utils.FindComponentsInHierarthy<PlayListContainer>();
                foreach (var container in containers)
                {
                    ProcessPlaylistContainer(container);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in PlaylistBuildProcess: {ex.Message}");
                throw;
            }
        }

        private static void ProcessPlaylistContainer(PlayListContainer container)
        {
            if (container == null)
            {
                Debug.LogWarning("PlayListContainer is null, skipping...");
                return;
            }

            var playlists = container.ReadPlaylists();
            if (playlists == null || playlists.Count == 0)
            {
                Debug.LogWarning($"No playlists found in container {container.name}");
                return;
            }

            var results = CreatePlaylistComponents(container, playlists);
            AssignPlaylistsToController(container, results);
        }

        private static List<YamaStream.Playlist> CreatePlaylistComponents(PlayListContainer container, List<Playlist> playlists)
        {
            var results = new List<YamaStream.Playlist>();

            foreach (var playlist in playlists)
            {
                if (!playlist.Active)
                {
                    Debug.Log($"Skipping inactive playlist: {playlist.Name}");
                    continue;
                }

                try
                {
                    var playlistComponent = CreatePlaylistComponent(container, playlist);
                    if (playlistComponent != null)
                    {
                        results.Add(playlistComponent);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to create playlist component for '{playlist.Name}': {ex.Message}");
                }
            }

            return results;
        }

        private static YamaStream.Playlist CreatePlaylistComponent(PlayListContainer container, Playlist playlist)
        {
            if (string.IsNullOrEmpty(playlist.Name))
            {
                Debug.LogWarning("Playlist name is null or empty, skipping...");
                return null;
            }

            var gameObject = new GameObject(playlist.Name);
            if (container.TargetContent != null)
            {
                gameObject.transform.SetParent(container.TargetContent);
            }

            var udonPlaylist = gameObject.AddUdonSharpComponent<YamaStream.Playlist>();
            ConfigureUdonPlaylist(udonPlaylist, playlist);

            return udonPlaylist;
        }

        private static void ConfigureUdonPlaylist(YamaStream.Playlist udonPlaylist, Playlist playlist)
        {
            var backingBehaviour = UdonSharpEditorUtility.GetBackingUdonBehaviour(udonPlaylist);
            if (backingBehaviour != null)
            {
                backingBehaviour.SyncMethod = Networking.SyncType.Manual;
            }

            udonPlaylist.SetProgramVariable("_playlistName", playlist.Name);

            if (playlist.Tracks != null && playlist.Tracks.Count > 0)
            {
                var videoPlayerTypes = playlist.Tracks.Select(track => (VideoPlayerType)(int)track.Mode).ToArray();
                var titles = playlist.Tracks.Select(track => track.Title ?? string.Empty).ToArray();
                var urls = playlist.Tracks.Select(track => new VRCUrl(track.Url ?? string.Empty)).ToArray();
                var originalUrls = playlist.Tracks.Select(track => string.Empty).ToArray();
                var timelines = playlist.Tracks.Select(track => track.PlayableDirector).ToArray();

                udonPlaylist.SetProgramVariable("_videoPlayerTypes", videoPlayerTypes);
                udonPlaylist.SetProgramVariable("_titles", titles);
                udonPlaylist.SetProgramVariable("_urls", urls);
                udonPlaylist.SetProgramVariable("_originalUrls", originalUrls);
                udonPlaylist.SetProgramVariable("_timelines", timelines);
            }
        }

        private static void AssignPlaylistsToController(PlayListContainer container, List<YamaStream.Playlist> playlists)
        {
            try
            {
                var yamaPlayer = container.FindComponentInParent<YamaPlayer>();
                if (yamaPlayer == null)
                {
                    Debug.LogError($"YamaPlayer not found in parent of container {container.name}");
                    return;
                }

                var controller = yamaPlayer.GetComponentInChildren<Controller>();
                if (controller == null)
                {
                    Debug.LogError($"Controller not found in children of YamaPlayer {yamaPlayer.name}");
                    return;
                }

                controller.SetProgramVariable("_playlists", playlists.ToArray());
                Debug.Log($"Successfully assigned {playlists.Count} playlists to controller");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to assign playlists to controller: {ex.Message}");
                throw;
            }
        }
    }
}