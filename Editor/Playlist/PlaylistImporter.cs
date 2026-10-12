using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

using Object = UnityEngine.Object;

namespace Yamadev.YamaStream.Editor
{
  public static class PlaylistImporter
  {
    public static List<PlaylistData> ImportPlaylists(Object[] objects)
    {
      List<PlaylistData> results = new List<PlaylistData>();

      foreach (Object obj in objects)
      {
        if (obj is not GameObject go) continue;
        foreach (MonoBehaviour script in go.GetComponents<MonoBehaviour>())
        {
          var imported = ImportFromScript(script);
          if (imported != null) results.AddRange(imported);
        }
      }
      return results;
    }

    private static List<PlaylistData> ImportFromScript(MonoBehaviour script)
    {
      List<PlaylistData> results = new List<PlaylistData>();

      switch (script.GetType().ToString())
      {
        case "HoshinoLabs.IwaSync3.Playlist":
          var iwaSync3Playlist = ReadPlaylistDataFromIwaSync3(script);
          if (iwaSync3Playlist != null)
          {
            results.Add(iwaSync3Playlist);
          }
          break;
        case "HoshinoLabs.IwaSync3.ListTab":
          results.AddRange(ImportPlaylistTabFromIwaSync3(script));
          break;
        case "Kinel.VideoPlayer.Scripts.KinelPlaylistScript":
          var kinelPlaylist = ReadPlaylistDataFromKinelVideoPlayer(script);
          if (kinelPlaylist != null)
          {
            results.Add(kinelPlaylist);
          }
          break;
        case "Kinel.VideoPlayer.Scripts.KinelPlaylistGroupManagerScript":
          results.AddRange(ImportPlaylistsFromKinelVideoPlayer(script));
          break;
        case "JLChnToZ.VRC.VVMW.FrontendHandler":
          results.AddRange(ImportPlaylistsFromVizVid(script));
          break;
        case "JLChnToZ.VRC.VVMW.Core":
          Type frontendHandler = EditorUtils.FindType("JLChnToZ.VRC.VVMW.FrontendHandler");
          if (frontendHandler != null)
          {
            var handler = script.GetComponentInChildren(frontendHandler);
            if (handler != null)
              results.AddRange(ImportPlaylistsFromVizVid(handler as MonoBehaviour));
          }
          break;
        case "UdonSharp.Video.USharpVideoPlayer":
          var usharpTracks = ReadPlaylistDataFromUSharpVideo(script);
          if (usharpTracks.Count > 0)
          {
            results.Add(new PlaylistData
            {
              active = true,
              name = "USharp Video Playlist",
              tracks = usharpTracks,
              youtubeListId = ""
            });
          }
          break;
        case "ArchiTech.ProTV.Playlist":
          var protvTracks = ReadPlaylistDataFromProTV(script);
          if (protvTracks.Count > 0)
          {
            results.Add(new PlaylistData
            {
              active = true,
              name = "ProTV Playlist",
              tracks = protvTracks,
              youtubeListId = ""
            });
          }
          break;
      }

      return results;
    }

    private static PlaylistData ReadPlaylistDataFromIwaSync3(dynamic li)
    {
      try
      {
        dynamic[] tracks = ((object)li).GetType().GetField("tracks", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(li);
        dynamic playlistUrl = ((object)li).GetType().GetField("playlistUrl", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(li);
        return new PlaylistData
        {
          active = true,
          name = "IwaSync3 Playlist",
          tracks = tracks.Select((track) => new PlaylistTrack()
          {
            playerType = (VideoPlayerType)track.mode,
            title = track.title,
            url = track.url,
          }).ToList(),
          youtubeListId = YtdlpResolver.GetYoutubePlaylistIdFromUrl(playlistUrl)
        };
      }
      catch (Exception ex)
      {
        Debug.LogException(ex);
      }
      return null;
    }

    private static List<PlaylistData> ImportPlaylistTabFromIwaSync3(dynamic listTab, List<dynamic> exclusion = null)
    {
      List<PlaylistData> results = new List<PlaylistData>();
      if (exclusion == null) exclusion = new List<dynamic>() { listTab };
      else exclusion.Add(listTab);
      try
      {
        dynamic[] tabs = ((object)listTab).GetType().GetField("tabs", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(listTab);
        foreach (dynamic tab in tabs)
        {
          switch (((object)tab.list).GetType().ToString())
          {
            case "HoshinoLabs.IwaSync3.Playlist":
              var li = ReadPlaylistDataFromIwaSync3(tab.list);
              if (li?.tracks != null)
              {
                results.Add(li);
              }
              break;
            case "HoshinoLabs.IwaSync3.ListTab":
              if (exclusion.IndexOf(tab.list) >= 0) break;
              results.AddRange(ImportPlaylistTabFromIwaSync3(tab.list, exclusion));
              break;
          }
        }
      }
      catch (Exception ex)
      {
        Debug.LogException(ex);
      }
      return results;
    }

    private static PlaylistData ReadPlaylistDataFromKinelVideoPlayer(dynamic li)
    {
      try
      {
        return new PlaylistData
        {
          active = true,
          name = "Kinel Playlist",
          tracks = ((dynamic[])li.videoDatas).Select((data) => new PlaylistTrack()
          {
            title = data.title,
            url = data.url,
            playerType = (VideoPlayerType)data.mode,
          }).ToList(),
          youtubeListId = YtdlpResolver.GetYoutubePlaylistIdFromUrl(li.playlistUrl)
        };
      }
      catch (Exception ex)
      {
        Debug.LogException(ex);
      }
      return null;
    }

    private static List<PlaylistData> ImportPlaylistsFromKinelVideoPlayer(dynamic group)
    {
      List<PlaylistData> results = new List<PlaylistData>();
      try
      {
        Transform trans = ((MonoBehaviour)group).transform.Find("Playlist");
        for (int i = 0; i < group.playlists.Length; i++)
        {
          dynamic script = trans.GetChild(i + 1)?.GetComponent(EditorUtils.FindType("Kinel.VideoPlayer.Scripts.KinelPlaylistScript"));
          if (script == null) continue;
          var li = ReadPlaylistDataFromKinelVideoPlayer(script);
          if (li?.tracks != null)
          {
            results.Add(li);
          }
        }
      }
      catch (Exception ex)
      {
        Debug.LogException(ex);
      }
      return results;
    }

    private static List<PlaylistData> ImportPlaylistsFromVizVid(MonoBehaviour handler)
    {
      List<PlaylistData> results = new List<PlaylistData>();
      try
      {
        string[] playListTitles = (string[])((UdonSharpBehaviour)handler).GetProgramVariable("playListTitles");
        int[] playListUrlOffsets = (int[])((UdonSharpBehaviour)handler).GetProgramVariable("playListUrlOffsets");
        VRCUrl[] playListUrls = (VRCUrl[])((UdonSharpBehaviour)handler).GetProgramVariable("playListUrls");
        string[] playListEntryTitles = (string[])((UdonSharpBehaviour)handler).GetProgramVariable("playListEntryTitles");
        byte[] playListPlayerIndex = (byte[])((UdonSharpBehaviour)handler).GetProgramVariable("playListPlayerIndex");
        dynamic handlerDynamic = handler;
        string[] playerHandlers = ((dynamic[])((UdonSharpBehaviour)(handlerDynamic.core)).GetProgramVariable("playerHandlers")).Select(i =>
        {
          return (string)((object)i).GetType().GetField("playerName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(i);
        }).ToArray();

        for (int i = 0; i < playListTitles.Length; i++)
        {
          var urlOffset = playListUrlOffsets[i];
          var urlCount = (i < playListTitles.Length - 1 ? playListUrlOffsets[i + 1] : playListUrls.Length) - urlOffset;
          var tracks = new List<PlaylistTrack>();

          for (int j = 0; j < urlCount; j++)
          {
            if (playerHandlers[playListPlayerIndex[urlOffset + j] - 1] == "ImageViewer")
            {
              tracks.Add(new PlaylistTrack
              {
                playerType = VideoPlayerType.ImageViewer,
                title = playListEntryTitles[urlOffset + j],
                url = playListUrls[urlOffset + j].Get(),
              });
              continue;
            }
            tracks.Add(new PlaylistTrack
            {
              playerType = playerHandlers[playListPlayerIndex[urlOffset + j] - 1] == "BuiltInPlayer" ? VideoPlayerType.UnityVideoPlayer : VideoPlayerType.AVProVideoPlayer,
              title = playListEntryTitles[urlOffset + j],
              url = playListUrls[urlOffset + j].Get(),
            });
          }

          results.Add(new PlaylistData
          {
            active = true,
            name = playListTitles[i],
            tracks = tracks,
            youtubeListId = ""
          });
        }
      }
      catch (Exception ex)
      {
        Debug.LogException(ex);
      }
      return results;
    }

    private static List<PlaylistTrack> ReadPlaylistDataFromUSharpVideo(dynamic usharpVideo)
    {
      try
      {
        bool defaultStreamMode = (bool)((UdonSharpBehaviour)usharpVideo).GetProgramVariable("defaultStreamMode");
        return ((VRCUrl[])usharpVideo.playlist).Select((url) => new PlaylistTrack()
        {
          playerType = defaultStreamMode ? VideoPlayerType.AVProVideoPlayer : VideoPlayerType.UnityVideoPlayer,
          title = string.Empty,
          url = url.Get(),
        }).ToList();
      }
      catch (Exception ex)
      {
        Debug.LogException(ex);
      }
      return new List<PlaylistTrack>();
    }

    private static List<PlaylistTrack> ReadPlaylistDataFromProTV(dynamic proTvPlaylist)
    {
      try
      {
        return ((VRCUrl[])proTvPlaylist.mainUrls).Select((url, index) => new PlaylistTrack()
        {
          playerType = VideoPlayerType.AVProVideoPlayer,
          title = proTvPlaylist.titles[index],
          url = url.Get(),
        }).ToList();
      }
      catch (Exception ex)
      {
        Debug.LogException(ex);
      }
      return new List<PlaylistTrack>();
    }

    [Serializable]
    struct YouTubePlaylistTrack
    {
      public string title;
      public string url;
      public string playlist;
      public int duration;
    }

    public static async UniTask<PlaylistData> GetYouTubePlaylistData(string playlistId)
    {
      try
      {
        List<string> jsonList = await YtdlpResolver.GetPlaylist(playlistId);
        List<YouTubePlaylistTrack> tracks = jsonList.Select(x => JsonUtility.FromJson<YouTubePlaylistTrack>(x)).ToList();
        string name = tracks.Count > 0 ? tracks[0].playlist : "";
        var playlistTracks = tracks.Select(track => new PlaylistTrack
        {
          playerType = VideoPlayerType.AVProVideoPlayer,
          title = track.title,
          url = track.url,
        }).ToList();

        return new PlaylistData
        {
          active = true,
          name = name,
          tracks = playlistTracks,
          youtubeListId = playlistId
        };
      }
      catch (Exception ex)
      {
        Debug.LogException(ex);
      }
      return null;
    }
  }
}
