using UdonSharp;
using VRC.SDK3.Data;
using VRC.SDKBase;
using Yamadev.YamaStream.Libraries.GenericDataContainer;

namespace Yamadev.YamaStream
{
    public class YouTubePlaylist : UdonSharpBehaviour
    {
        public static YouTubePlaylist New(string name, DataList<Track> tracks)
        {
            object[] result = new object[] { name, tracks };
            return (YouTubePlaylist)(object)result;
        }

        public static YouTubePlaylist Empty()
        {
            return (YouTubePlaylist)(object)new object[] { string.Empty, DataList<Track>.New() };
        }
    }

    public static class YouTubePlaylistExtentions
    {
        public static string GetName(this YouTubePlaylist obj)
        {
            return (string)((object[])(object)obj)[0];
        }

        public static DataList<Track> GetTracks(this YouTubePlaylist obj)
        {
            return (DataList<Track>)((object[])(object)obj)[1];
        }
    }

    public static class YouTube
    {
        public static YouTubePlaylist ParsePlaylist(string playlistJson)
        {
            if (string.IsNullOrEmpty(playlistJson) || !VRCJson.TryDeserializeFromJson(playlistJson, out var json)) 
                return YouTubePlaylist.Empty();
            var tracks = DataList<Track>.New();
            DataDictionary dict = json.DataDictionary["playlist"].DataDictionary;
            string playlistName = dict["title"].String;
            DataList contents = dict["contents"].DataList;
            for (int i = 0; i < contents.Count; i++)
            {
                if (contents[i].DataDictionary.TryGetValue("playlistPanelVideoRenderer", out var renderer))
                {
                    // Play both video and live in AVPro video player.
                    bool isLive = renderer.DataDictionary.TryGetValue("badges", out var badges) &&
                        badges.DataList.TryGetValue(0, out var badge) &&
                        badge.DataDictionary["metadataBadgeRenderer"].DataDictionary["icon"].DataDictionary["iconType"].String == "LIVE";
                    string title = renderer.DataDictionary["title"].DataDictionary["simpleText"].String;
                    string url = $"https://www.youtube.com/watch?v={renderer.DataDictionary["videoId"].String}";
                    tracks.Add(Track.New(VideoPlayerType.AVProVideoPlayer, title, VRCUrl.Empty, url));
                }
            }
            return YouTubePlaylist.New(playlistName, tracks);
        }

        public static DataList<Track> ParsePlaylistRenderer(string playlistJson)
        {
            var tracks = DataList<Track>.New();
            if (string.IsNullOrEmpty(playlistJson) || !VRCJson.TryDeserializeFromJson(playlistJson, out var json)) return tracks;
            DataList contents = json.DataDictionary["playlistVideoListRenderer"].DataDictionary["contents"].DataList;
            for (int i = 0; i < contents.Count; i++)
            {
                DataDictionary renderer = contents[i].DataDictionary["playlistVideoRenderer"].DataDictionary;
                // VRCUrl.TryCreateAllowlistedVRCUrl($"https://www.youtube.com/watch?v={renderer["videoId"].String}", out VRCUrl outputUrl);
                // Play both video and live in AVPro video player.
                bool isLive = renderer["thumbnailOverlays"].DataList[0].DataDictionary.TryGetValue("thumbnailOverlayTimeStatusRenderer", out var thu) &&
                    thu.DataDictionary["style"] == "LIVE";
                string title = renderer["title"].DataDictionary["runs"].DataList[0].DataDictionary["text"].String;
                string url = $"https://www.youtube.com/watch?v={renderer["videoId"].String}";
                tracks.Add(Track.New(VideoPlayerType.AVProVideoPlayer, title, VRCUrl.Empty, url));
            }
            return tracks;
        }

        public static YouTubePlaylist GetPlaylistFromHtml(string html)
        {
            int index = html.IndexOf("{\"playlist\":");
            if (index >= 0) return ParsePlaylist(Utils.FindPairBrackets(html, index));

            index = html.IndexOf("{\"playlistVideoListRenderer\":");
            if (index < 0) return YouTubePlaylist.Empty();

            int headerIndex = html.IndexOf("{\"pageHeaderRenderer\":");
            string headerString = Utils.FindPairBrackets(html, headerIndex);
            string playlistName = string.Empty;
            if (headerString != string.Empty && VRCJson.TryDeserializeFromJson(Utils.FindPairBrackets(html, headerIndex), out var headerJson))
                playlistName = headerJson.DataDictionary["pageHeaderRenderer"].DataDictionary["pageTitle"].String;
            DataList<Track> tracks = ParsePlaylistRenderer(Utils.FindPairBrackets(html, index));
            return YouTubePlaylist.New(playlistName, tracks);
        }
    }
}