using VRC.SDK3.Data;
using System.Text.RegularExpressions;

namespace Yamadev.YamaStream.Modules.VideoInfoDownloader
{
  public static class YouTubeHtmlParser
  {
    public static string GetTitleFromHtml(string html)
    {
      if (string.IsNullOrEmpty(html)) return string.Empty;

      var prMatch = Regex.Match(html, @"ytInitialPlayerResponse\s*=\s*(\{[\s\S]*?\});", RegexOptions.IgnoreCase);
      if (prMatch.Success)
      {
        string prJson = prMatch.Groups[1].Value;
        if (VRCJson.TryDeserializeFromJson(prJson, out var json) &&
            json.TokenType == TokenType.DataDictionary &&
            json.DataDictionary.TryGetValue("videoDetails", out var videoDetails) &&
            videoDetails.TokenType == TokenType.DataDictionary &&
            videoDetails.DataDictionary.TryGetValue("title", out var titleToken))
        {
          return titleToken.String;
        }
        var direct = Regex.Match(prJson, @"""videoDetails""[\s\S]*?""title""\s*:\s*""([^""]+)""", RegexOptions.IgnoreCase);
        if (direct.Success) return direct.Groups[1].Value;
      }

      return string.Empty;
    }
  }
}
