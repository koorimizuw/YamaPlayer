using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Yamadev.YamaStream.Script;

namespace Yamadev.YamaStream.Editor
{
    public static class PlaylistUtils
    {
        public static string GetPlaylistIdFromUrl(string url)
        {
            if (!url.StartsWith("https://")) return url;
            Uri uri = new Uri(url);
            Dictionary<string, string> queries = uri.Query.Replace("?", "").Split('&').ToDictionary(pair => pair.Split('=').First(), pair => pair.Split('=').Last());
            if (queries.TryGetValue("list", out var list)) return list;
            return string.Empty;
        }

        public static List<Playlist> ReadPlaylists(this PlayListContainer container)
        {
            List<Playlist> results = new();
            if (container == null) return results;
            for (int i = 1; i < container.transform.childCount; i++)
            {
                PlayList li = container.transform.GetChild(i).GetComponent<PlayList>();
                results.Add(new Playlist()
                {
                    Active = li.gameObject.activeSelf,
                    Name = li.PlayListName,
                    Tracks = li.Tracks.ToList(),
                    YoutubeListId = li.YouTubePlayListID,
                });
            }
            return results;
        }
    }
}