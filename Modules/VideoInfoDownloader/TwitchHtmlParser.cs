using VRC.SDK3.Data;
using System.Text.RegularExpressions;

namespace Yamadev.YamaStream.Modules.VideoInfoDownloader
{
  public static class TwitchHtmlParser
  {
    public static string GetTitleFromHtml(string html)
    {
      if (string.IsNullOrEmpty(html)) return string.Empty;

      var ldMatches = Regex.Matches(html,
        @"<script[^>]*type\s*=\s*[""']application/ld\+json[""'][^>]*>([\s\S]*?)</script>",
        RegexOptions.IgnoreCase);

      for (int i = 0; i < ldMatches.Count; i++)
      {
        string jsonString = ldMatches[i].Groups[1].Value;
        if (VRCJson.TryDeserializeFromJson(jsonString, out var json))
        {
          if (json.TokenType == TokenType.DataDictionary)
          {
            var dict = json.DataDictionary;
            if (dict.TryGetValue("@graph", out var graph) &&
                graph.TokenType == TokenType.DataList &&
                graph.DataList.Count > 0)
            {
              var first = graph.DataList[0];
              if (first.TokenType == TokenType.DataDictionary &&
                  first.DataDictionary.TryGetValue("name", out var nameFromGraph))
                return nameFromGraph.String;
            }
            if (dict.TryGetValue("name", out var directName))
            {
              return directName.String;
            }
          }
          else if (json.TokenType == TokenType.DataList && json.DataList.Count > 0)
          {
            var first = json.DataList[0];
            if (first.TokenType == TokenType.DataDictionary &&
                first.DataDictionary.TryGetValue("name", out var nameFromArray))
              return nameFromArray.String;
          }
        }
        else
        {
          var nameMatch = Regex.Match(jsonString, @"""name""\s*:\s*""([^""]+)""",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
          if (nameMatch.Success)
          {
            return nameMatch.Groups[1].Value;
          }
        }
      }

      return string.Empty;
    }
  }
}
