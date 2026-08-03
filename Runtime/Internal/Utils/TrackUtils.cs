using System.Text;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
  public static class TrackUtils
  {
    public static object[] NewTrack(VideoPlayerType player, string title, VRCUrl url)
    {
      return new object[] { (int)player, title, url };
    }

    public static object[] NewTrackWithExtension(VideoPlayerType player, string title, VRCUrl url, byte[] extension)
    {
      return new object[] { (int)player, title, url, extension };
    }

    public static object[] NewTrackWithStringExtension(VideoPlayerType player, string title, VRCUrl url, string extension)
    {
      byte[] bytes = string.IsNullOrEmpty(extension) ? new byte[0] : Encoding.UTF8.GetBytes(extension);
      return new object[] { (int)player, title, url, bytes };
    }

    public static object[] CreateEmptyTrack()
    {
      return new object[] { 0, string.Empty, VRCUrl.Empty };
    }

    public static VideoPlayerType GetPlayerType(object[] track)
    {
      if (track == null || track.Length < 1 || track[0] == null) return 0;
      if (track[0].GetType() != typeof(int)) return 0;
      return (VideoPlayerType)track[0];
    }

    public static string GetTitle(object[] track)
    {
      if (track == null || track.Length < 2 || track[1] == null) return string.Empty;
      if (track[1].GetType() != typeof(string)) return string.Empty;
      return (string)track[1];
    }

    public static void SetTitle(object[] track, string title)
    {
      if (track == null || track.Length < 2) return;
      track[1] = title;
    }

    public static VRCUrl GetUrl(object[] track)
    {
      if (track == null || track.Length < 3 || track[2] == null) return VRCUrl.Empty;
      if (track[2].GetType() != typeof(VRCUrl)) return VRCUrl.Empty;
      return (VRCUrl)track[2];
    }

    public static void SetUrl(object[] track, VRCUrl url)
    {
      if (track == null || track.Length < 3) return;
      track[2] = url;
    }

    public static bool HasExtension(object[] track)
    {
      if (track == null || track.Length < 4 || track[3] == null) return false;
      if (track[3].GetType() != typeof(byte[])) return false;
      return ((byte[])track[3]).Length > 0;
    }

    public static byte[] GetExtension(object[] track)
    {
      if (!HasExtension(track)) return new byte[0];
      return (byte[])track[3];
    }

    public static string GetExtensionString(object[] track)
    {
      if (!HasExtension(track)) return string.Empty;
      return Encoding.UTF8.GetString((byte[])track[3]);
    }

    public static bool ExtensionsEqual(byte[] a, byte[] b)
    {
      int lengthA = a == null ? 0 : a.Length;
      int lengthB = b == null ? 0 : b.Length;
      if (lengthA != lengthB) return false;
      for (int i = 0; i < lengthA; i++)
      {
        if (a[i] != b[i]) return false;
      }
      return true;
    }

    public static bool Equals(object[] a, object[] b)
    {
      if (a == b) return true;
      if (a == null || b == null) return false;
      if (GetPlayerType(a) != GetPlayerType(b)) return false;
      if (GetTitle(a) != GetTitle(b)) return false;
      if (GetUrl(a).Get() != GetUrl(b).Get()) return false;
      return ExtensionsEqual(GetExtension(a), GetExtension(b));
    }
  }
}
