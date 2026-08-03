using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Yamadev.YamaStream.Editor
{
  public static class PlaylistExporter
  {
    private class PlaylistsJson
    {
      public List<PlaylistData> playlists;
    }

    public static void Export(List<PlaylistItem> playlists)
    {
      var exportData = new PlaylistsJson
      {
        playlists = playlists.Where(p => p != null).Select(p => new PlaylistData
        {
          active = p.gameObject.activeSelf,
          name = p.playlistName,
          tracks = p.tracks?.ToList() ?? new List<PlaylistTrack>(),
          youtubeListId = p.youtubePlaylistId
        }).ToList()
      };

      var settings = new JsonSerializerSettings
      {
        Formatting = Formatting.Indented,
        ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
      };
      string jsonStr = JsonConvert.SerializeObject(exportData, settings);
      string filePath = EditorUtility.SaveFilePanel("Export playlists", Application.dataPath, "playlists", "json");
      if (!string.IsNullOrEmpty(filePath)) File.WriteAllText(filePath, jsonStr);
    }

    public static List<PlaylistData> Import()
    {
      List<PlaylistData> results = new List<PlaylistData>();

      string filePath = EditorUtility.OpenFilePanel("Import playlists", Application.dataPath, "json");
      if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return results;

      try
      {
        string jsonStr = File.ReadAllText(filePath);
        JObject root = JObject.Parse(jsonStr);
        JArray playlistsArray = root["playlists"] as JArray;
        if (playlistsArray == null) return results;

        foreach (JObject item in playlistsArray)
        {
          var data = ParsePlaylistData(item);
          if (data != null) results.Add(data);
        }
      }
      catch (Exception ex)
      {
        Debug.LogError($"Failed to import playlists: {ex.Message}");
      }
      return results;
    }

    private static PlaylistData ParsePlaylistData(JObject item)
    {
      bool active = item["active"]?.Value<bool>() ?? item["Active"]?.Value<bool>() ?? true;
      string name = item["name"]?.Value<string>() ?? item["Name"]?.Value<string>() ?? "";
      string youtubeListId = item["youtubeListId"]?.Value<string>() ?? item["YoutubeListId"]?.Value<string>() ?? "";

      JArray tracksArray = item["tracks"] as JArray ?? item["Tracks"] as JArray;
      List<PlaylistTrack> tracks = new List<PlaylistTrack>();

      if (tracksArray != null)
      {
        foreach (JObject trackObj in tracksArray)
        {
          var track = ParsePlaylistTrack(trackObj);
          tracks.Add(track);
        }
      }

      return new PlaylistData
      {
        active = active,
        name = name,
        tracks = tracks,
        youtubeListId = youtubeListId
      };
    }

    private static PlaylistTrack ParsePlaylistTrack(JObject trackObj)
    {
      int mode = trackObj["playerType"]?.Value<int>() ?? trackObj["mode"]?.Value<int>() ?? trackObj["Mode"]?.Value<int>() ?? 0;
      string title = trackObj["title"]?.Value<string>() ?? trackObj["Title"]?.Value<string>() ?? "";
      string url = trackObj["url"]?.Value<string>() ?? trackObj["Url"]?.Value<string>() ?? "";

      return new PlaylistTrack
      {
        playerType = (VideoPlayerType)mode,
        title = title,
        url = url
      };
    }
  }
}
