using System;
using System.Collections.Generic;
using System.Linq;
using UdonSharpEditor;
using UnityEngine;
using VRC.SDKBase;

using Object = UnityEngine.Object;

namespace Yamadev.YamaStream.Editor
{
  public class PlaylistBuildProcess : IYamaPlayerBuildProcess
  {
    public int callbackOrder => -9000;

    public void Process()
    {
      var managers = Object.FindObjectsByType<PlaylistManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
      foreach (var manager in managers)
      {
        if (manager == null) continue;
        var playlistItems = manager.GetPlaylists();
        if (playlistItems == null || playlistItems.Count == 0)
        {
          Debug.LogWarning($"No playlists found in manager {manager.name}");
          continue;
        }

        var results = CreatePlaylistComponents(playlistItems);
      }
    }

    private static List<Playlist> CreatePlaylistComponents(List<PlaylistItem> playlistItems)
    {
      var results = new List<Playlist>();
      foreach (var item in playlistItems)
      {
        if (item == null || !item.gameObject.activeSelf || item.tracks == null || item.tracks.Length == 0) continue;

        try
        {
          var udonPlaylist = item.gameObject.AddUdonSharpComponent<Playlist>();

          udonPlaylist.SetProgramVariable("_playlistName", item.playlistName);

          var videoPlayerTypes = item.tracks.Select(track => track.playerType).ToArray();
          var titles = item.tracks.Select(track => track.title ?? string.Empty).ToArray();
          var urls = item.tracks.Select(track => new VRCUrl(track.url ?? string.Empty)).ToArray();

          udonPlaylist.SetProgramVariable("_videoPlayerTypes", videoPlayerTypes);
          udonPlaylist.SetProgramVariable("_titles", titles);
          udonPlaylist.SetProgramVariable("_urls", urls);

          results.Add(udonPlaylist);
        }
        catch (Exception ex)
        {
          Debug.LogError($"Failed to create playlist component for '{item.playlistName}': {ex.Message}");
        }
      }

      return results;
    }
  }
}
