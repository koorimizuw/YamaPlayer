using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using Yamadev.YamaStream.Script;

using Object = UnityEngine.Object;

namespace Yamadev.YamaStream.Editor
{
    public static class PlaylistImporter
    {
        public static List<Playlist> ReadPlaylists(Object[] objects)
        {
            List<Playlist> results = new List<Playlist>();
            foreach (Object obj in objects)
            {
                if (obj is not GameObject go) continue;
                foreach (MonoBehaviour script in go.GetComponents<MonoBehaviour>())
                {
                    switch (script.GetType().ToString())
                    {
                        case "HoshinoLabs.IwaSync3.Playlist":
                            Playlist iwaSync3Playlist = ReadPlaylistFromIwaSync3(script);
                            if (iwaSync3Playlist.Tracks != null) results.Add(iwaSync3Playlist);
                            break;
                        case "HoshinoLabs.IwaSync3.ListTab":
                            results.AddRange(ReadPlaylistTabFromIwaSync3(script));
                            break;
                        case "Kinel.VideoPlayer.Scripts.KinelPlaylistScript":
                            Playlist kinelPlaylist = ReadPlaylistFromKinelVideoPlayer(script);
                            if (kinelPlaylist.Tracks != null) results.Add(kinelPlaylist);
                            break;
                        case "Kinel.VideoPlayer.Scripts.KinelPlaylistGroupManagerScript":
                            results.AddRange(ReadPlaylistsFromKinelVideoPlayer(script));
                            break;
                        case "JLChnToZ.VRC.VVMW.FrontendHandler":
                            results.AddRange(ReadPlaylistsFromVizVid(script));
                            break;
                        case "JLChnToZ.VRC.VVMW.Core":
                            Type frontendHandler = Utils.FindType("JLChnToZ.VRC.VVMW.FrontendHandler");
                            if (frontendHandler != null)
                                results.AddRange(ReadPlaylistsFromVizVid(script.GetComponentInChildren(frontendHandler)));
                            break;
                        case "UdonSharp.Video.USharpVideoPlayer":
                            results.Add(ReadPlaylistFromUSharpVideo(script));
                            break;
                        case "ArchiTech.ProTV.Playlist":
                            results.Add(ReadPlaylistFromProTV(script));
                            break;
                    }
                }
            }
            return results;
        }

        public static Playlist ReadPlaylistFromIwaSync3(dynamic li)
        {
            try
            {
                dynamic[] tracks = ((object)li).GetType().GetField("tracks", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(li);
                dynamic playlistUrl = ((object)li).GetType().GetField("playlistUrl", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(li);
                return new Playlist()
                {
                    Active = true,
                    Name = "IwaSync3 Playlist",
                    Tracks = tracks.Select((track) => new PlaylistTrack()
                    {
                        Mode = (VideoPlayerType)track.mode,
                        Title = track.title,
                        Url = track.url,
                    }).ToList(),
                    YoutubeListId = PlaylistUtils.GetPlaylistIdFromUrl(playlistUrl),
                };
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            return new Playlist();
        }

        public static List<Playlist> ReadPlaylistTabFromIwaSync3(dynamic listTab, List<dynamic> exclusion = null)
        {
            List<Playlist> results = new List<Playlist>();
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
                            Playlist li = ReadPlaylistFromIwaSync3(tab.list);
                            if (li.Tracks != null)
                            {
                                li.Name = tab.title;
                                results.Add(li);
                            }
                            break;
                        case "HoshinoLabs.IwaSync3.ListTab":
                            if (exclusion.IndexOf(tab.list) >= 0) break;
                            results.AddRange(ReadPlaylistTabFromIwaSync3(tab.list, exclusion));
                            break;
                    }
                }
                return results;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            return results;
        }

        public static Playlist ReadPlaylistFromKinelVideoPlayer(dynamic li)
        {
            try
            {
                return new Playlist()
                {
                    Active = true,
                    Name = "Kinel Playlist",
                    Tracks = ((dynamic[])li.videoDatas).Select((data) => new PlaylistTrack()
                    {
                        Title = data.title,
                        Url = data.url,
                        Mode = (VideoPlayerType)data.mode,
                    }).ToList(),
                    YoutubeListId = PlaylistUtils.GetPlaylistIdFromUrl(li.playlistUrl),
                };
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            return new Playlist();
        }

        public static List<Playlist> ReadPlaylistsFromKinelVideoPlayer(dynamic group)
        {
            List<Playlist> results = new List<Playlist>();
            try
            {
                Transform trans = ((MonoBehaviour)group).transform.Find("Playlist");
                for (int i = 0; i < group.playlists.Length; i++)
                {
                    dynamic script = trans.GetChild(i + 1)?.GetComponent(Utils.FindType("Kinel.VideoPlayer.Scripts.KinelPlaylistScript"));
                    if (script == null) continue;
                    Playlist li = ReadPlaylistFromKinelVideoPlayer(script);
                    if (li.Tracks != null)
                    {
                        li.Name = group.playlists[i];
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

        public static List<Playlist> ReadPlaylistsFromVizVid(dynamic handler)
        {
            List<Playlist> results = new List<Playlist>();
            try
            {
                string[] playListTitles = (string[])((UdonSharpBehaviour)handler).GetProgramVariable("playListTitles");
                int[] playListUrlOffsets = (int[])((UdonSharpBehaviour)handler).GetProgramVariable("playListUrlOffsets");
                VRCUrl[] playListUrls = (VRCUrl[])((UdonSharpBehaviour)handler).GetProgramVariable("playListUrls");
                string[] playListEntryTitles = (string[])((UdonSharpBehaviour)handler).GetProgramVariable("playListEntryTitles");
                byte[] playListPlayerIndex = (byte[])((UdonSharpBehaviour)handler).GetProgramVariable("playListPlayerIndex");
                string[] playerHandlers = ((dynamic[])((UdonSharpBehaviour)(handler.core)).GetProgramVariable("playerHandlers")).Select(i =>
                {
                    return (string)((object)i).GetType().GetField("playerName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(i);
                }).ToArray();
                for (int i = 0; i < playListTitles.Length; i++)
                {
                    var urlOffset = playListUrlOffsets[i];
                    var urlCount = (i < playListTitles.Length - 1 ? playListUrlOffsets[i + 1] : playListUrls.Length) - urlOffset;
                    var playList = new Playlist
                    {
                        Active = true,
                        Name = playListTitles[i],
                        Tracks = new List<PlaylistTrack>(urlCount)
                    };
                    for (int j = 0; j < urlCount; j++)
                    {
                        if (playerHandlers[playListPlayerIndex[urlOffset + j] - 1] == "ImageViewer") continue;
                        playList.Tracks.Add(new PlaylistTrack
                        {
                            Title = playListEntryTitles[urlOffset + j],
                            Url = playListUrls[urlOffset + j].Get(),
                            Mode = playerHandlers[playListPlayerIndex[urlOffset + j] - 1] == "BuiltInPlayer" ? VideoPlayerType.UnityVideoPlayer : VideoPlayerType.AVProVideoPlayer,
                        });
                    }
                    results.Add(playList);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            return results;
        }

        public static Playlist ReadPlaylistFromUSharpVideo(dynamic usharpVideo)
        {
            try
            {
                bool defaultStreamMode = (bool)((UdonSharpBehaviour)usharpVideo).GetProgramVariable("defaultStreamMode");
                return new Playlist()
                {
                    Active = true,
                    Name = "USharp Video Playlist",
                    Tracks = ((VRCUrl[])usharpVideo.playlist).Select((url) => new PlaylistTrack()
                    {
                        Mode = defaultStreamMode ? VideoPlayerType.AVProVideoPlayer : VideoPlayerType.UnityVideoPlayer,
                        Title = string.Empty,
                        Url = url.Get(),
                    }).ToList(),
                };
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            return new();
        }

        public static Playlist ReadPlaylistFromProTV(dynamic proTvPlaylist)
        {
            try
            {
                return new Playlist()
                {
                    Active = true,
                    Name = "ProTV Playlist",
                    Tracks = ((VRCUrl[])proTvPlaylist.mainUrls).Select((url, index) => new PlaylistTrack()
                    {
                        Mode = VideoPlayerType.AVProVideoPlayer,
                        Title = proTvPlaylist.titles[index],
                        Url = url.Get(),
                    }).ToList(),
                };
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            return new();
        }

        [Serializable]
        struct YouTubePlaylistTrack
        {
            public string title;
            public string url;
            public string playlist;
        }

        public static async UniTask<Playlist> GetYouTubePlaylist(string playlistId, bool publicVideoOnly = true)
        {
            Playlist result = new();
            try
            {
                List<string> jsonList = await YtdlpResolver.GetPlaylist(playlistId);
                if (jsonList.Count > 0)
                {
                    YouTubePlaylistTrack li = JsonUtility.FromJson<YouTubePlaylistTrack>(jsonList[0]);
                    result.Name = li.playlist;
                }
                List<PlaylistTrack> tracks = jsonList.Select(track =>
                {
                    YouTubePlaylistTrack pl = JsonUtility.FromJson<YouTubePlaylistTrack>(track);
                    return new PlaylistTrack
                    {
                        Mode = VideoPlayerType.AVProVideoPlayer,
                        Title = pl.title,
                        Url = pl.url,
                    };
                }).Where(track =>
                {
                    if (publicVideoOnly) return track.Title != "[Private video]";
                    return true;
                }).ToList();
                result.Tracks = tracks;

            } catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            return result;
        }
    }
}