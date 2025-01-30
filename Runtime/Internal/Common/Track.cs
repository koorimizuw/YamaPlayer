using VRC.SDKBase;

namespace Yamadev.YamaStream
{
    public class Track : ObjectClass
    {
        // 0. VideoPlayerType
        // 1. string: title
        // 2. VRCUrl: url
        // 3. string: original url when use url resolve system
        public static Track New(VideoPlayerType player, string title, VRCUrl url)
        {
            object[] track = new object[] { player, title, url, string.Empty };
            return track.ForceCast<Track>();
        }

        public static Track New(VideoPlayerType player, string title, VRCUrl url, string originalUrl)
        {
            object[] track = new object[] { player, title, url, originalUrl };
            return track.ForceCast<Track>();
        }

        public static Track Empty()
        {
            object[] track = new object[] { VideoPlayerType.AVProVideoPlayer, string.Empty, VRCUrl.Empty, string.Empty };
            return track.ForceCast<Track>();
        }
    }

    public static class TrackExtentions
    {
        public static VideoPlayerType GetPlayerType(this Track track)
        {
            return (VideoPlayerType)track.UnPack()[0];
        }

        public static bool HasTitle(this Track track)
        {
            return !string.IsNullOrEmpty((string)track.UnPack()[1]);
        }

        public static string GetTitle(this Track track)
        {
            return (string)track.UnPack()[1];
        }

        public static void SetTitle(this Track track, string title)
        {
            ((object[])(object)track)[1] = title;
        }

        public static VRCUrl GetVRCUrl(this Track track)
        {
            return (VRCUrl)track.UnPack()[2];
        }

        public static string GetOriginalUrl(this Track track)
        {
            return (string)track.UnPack()[3];
        }

        public static string GetUrl(this Track track)
        {
            var arr = track.UnPack();
            string originalUrl = (string)arr[3];
            return string.IsNullOrEmpty(originalUrl) ? ((VRCUrl)arr[2]).Get() : originalUrl;
        }
    }
}