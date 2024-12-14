using System;
using System.Collections.Generic;
using System.Linq;

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
    }
}