using UdonSharp;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
    public class Track : UdonSharpBehaviour
    {
        // 0. VideoPlayerType
        // 1. string: title
        // 2. VRCUrl: url
        // 3. string: original url when use url resolve system
        public static Track New(VideoPlayerType player, string title, VRCUrl url)
        {
            object[] track = new object[] { player, title, url, string.Empty };
            return (Track)(object)track;
        }

        public static Track New(VideoPlayerType player, string title, VRCUrl url, string originalUrl)
        {
            object[] track = new object[] { player, title, url, originalUrl };
            return (Track)(object)track;
        }

        public static Track Empty()
        {
            object[] track = new object[] { VideoPlayerType.AVProVideoPlayer, string.Empty, VRCUrl.Empty, string.Empty };
            return (Track)(object)track;
        }
    }

    public static class TrackExtentions
    {
        public static bool IsValid(this Track obj)
        {
            return ((object[])(object)obj).Length > 0;
        }

        public static VideoPlayerType GetPlayerType(this Track obj)
        {
            return (VideoPlayerType)((object[])(object)obj)[0];
        }

        public static bool HasTitle(this Track obj)
        {
            return !string.IsNullOrEmpty((string)((object[])(object)obj)[1]);
        }

        public static string GetTitle(this Track obj)
        {
            return (string)((object[])(object)obj)[1];
        }

        public static void SetTitle(this Track obj, string title)
        {
            ((object[])(object)obj)[1] = title;
        }

        public static VRCUrl GetVRCUrl(this Track obj)
        {
            return (VRCUrl)((object[])(object)obj)[2];
        }

        public static string GetOriginalUrl(this Track obj)
        {
            return (string)((object[])(object)obj)[3];
        }

        public static string GetUrl(this Track obj)
        {
            string originalUrl = (string)((object[])(object)obj)[3];
            return string.IsNullOrEmpty(originalUrl) ? ((VRCUrl)((object[])(object)obj)[2]).Get() : originalUrl;
        }
    }
}