using VRC.SDKBase;

namespace Yamadev.YamaStream
{
  public static class UrlUtils
  {
    public static string GetProtocolFromUrl(string url)
    {
      if (string.IsNullOrEmpty(url)) return string.Empty;
      int index = url.IndexOf("://");
      if (index == -1) return string.Empty;
      return url.Substring(0, index).ToLower();
    }

    public static string GetProtocol(this VRCUrl url) => GetProtocolFromUrl(url.Get());
  }
}
