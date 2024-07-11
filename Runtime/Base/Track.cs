
using UdonSharp;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
    public class Track : UdonSharpBehaviour
    {
        public static Track New(VideoPlayerType player, string title, VRCUrl url)
        {
            return (Track)(object)TrackBase.New(player, title, url);
        }

        public static Track New(VideoPlayerType player, string title, VRCUrl url, string originalUrl)
        {
            return (Track)(object)TrackBase.New(player, title, url, originalUrl);
        }

        public static Track New(VideoPlayerType player, string title, VRCUrl url, string originalUrl, DataDictionary details)
        {
            return (Track)(object)TrackBase.New(player, title, url, originalUrl, details);
        }
    }

    public static class TrackExtentions
    {
        public static bool IsValid(this Track obj)
        {
            return ((object[])(object)obj).Length > 0;
        }

        public static VideoPlayerType GetPlayer(this Track obj)
        {
            return TrackBase.GetPlayer((object[])(object)obj);
        }

        public static bool HasTitle(this Track obj)
        {
            return TrackBase.HasTitle((object[])(object)obj);
        }

        public static string GetTitle(this Track obj)
        {
            return TrackBase.GetTitle((object[])(object)obj);
        }

        public static VRCUrl GetVRCUrl(this Track obj)
        {
            return TrackBase.GetVRCUrl((object[])(object)obj);
        }

        public static string GetOriginalUrl(this Track obj)
        {
            return TrackBase.GetOriginalUrl((object[])(object)obj);
        }

        public static string GetUrl(this Track obj)
        {
            return TrackBase.GetUrl((object[])(object)obj);
        }

        public static DataDictionary GetDetails(this Track obj)
        {
            return TrackBase.GetDetails((object[])(object)obj);
        }

        public static void SetOriginalUrl(this Track obj, string originalUrl)
        {
            TrackBase.SetOriginalUrl((object[])(object)obj, originalUrl);
        }

        public static void SetDetails(this Track obj, DataDictionary details)
        {
            TrackBase.SetDetails((object[])(object)obj, details);
        }

        public static void AddDetail(this Track obj, DataToken key, DataToken value)
        {
            TrackBase.AddDetail((object[])(object)obj, key, value);
        }
    }

    public static class TrackBase
    {
        public static object[] New(VideoPlayerType player, string title, VRCUrl url)
        {
            string originalUrl = "";
            DataDictionary details = new DataDictionary();
            return new object[] { player, title, url, originalUrl, details };
        }

        public static object[] New(VideoPlayerType player, string title, VRCUrl url, string originalUrl)
        {
            DataDictionary details = new DataDictionary();
            return new object[] { player, title, url, originalUrl, details };
        }

        public static object[] New(VideoPlayerType player, string title, VRCUrl url, string originalUrl, DataDictionary details)
        {
            return new object[] { player, title, url, originalUrl, details };
        }

        public static VideoPlayerType GetPlayer(object[] obj)
        {
            VideoPlayerType player = (VideoPlayerType)obj[0];
            return player;
        }

        public static bool HasTitle(object[] obj)
        {
            string title = (string)obj[1];
            return !title.Equals(string.Empty);
        }

        public static string GetTitle(object[] obj)
        {
            string title = (string)obj[1];
            return title;
        }

        public static VRCUrl GetVRCUrl(object[] obj)
        {
            VRCUrl url = (VRCUrl)obj[2];
            return url;
        }

        public static string GetOriginalUrl(object[] obj)
        {
            string originalUrl = (string)obj[3];
            return originalUrl;
        }

        public static string GetUrl(object[] obj)
        {
            VRCUrl url = (VRCUrl)obj[2];
            string originalUrl = (string)obj[3];
            return originalUrl == "" ? url.Get() : originalUrl;
        }

        public static DataDictionary GetDetails(object[] obj)
        {
            DataDictionary details = (DataDictionary)obj[4];
            return details;
        }

        public static void SetOriginalUrl(object[] obj, string originalUrl)
        {
            obj[3] = originalUrl;
        }

        public static void SetDetails(object[] obj, DataDictionary details)
        {
            obj[4] = details;
        }

        public static void AddDetail(object[] obj, DataToken key, DataToken value)
        {
            DataDictionary details = (DataDictionary)obj[4];
            details.Add(key, value);
        }

    }
}