using UdonSharp;
using VRC.SDK3.Data;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using System.Text.RegularExpressions;

namespace Yamadev.YamaStream.Modules.VideoInfoDownloader
{
  [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
  public class VideoInfoDownloader : YamaPlayerModule
  {
    private DataDictionary _videoInfo = new DataDictionary();
    private DataDictionary _pending = new DataDictionary();
    private DataDictionary _error = new DataDictionary();

    private DataDictionary[] _providers = new DataDictionary[]
    {
      new DataDictionary() {
        {"provider", "YouTube"},
        {"pattern", @"^https?:\/\/(?:www\.)?(?:youtube\.com|youtu\.be)\/"}
      },
      new DataDictionary() {
        {"provider", "Twitch"},
        {"pattern", @"^https?:\/\/(?:www\.)?twitch\.tv\/"}
      }
    };

    public string GetVideoInfo(string urlStr)
    {
      if (string.IsNullOrEmpty(urlStr) || string.IsNullOrEmpty(GetProviderFromUrl(urlStr)))
      {
        return string.Empty;
      }

      if (_videoInfo.TryGetValue(urlStr, TokenType.String, out var info))
      {
        return info.String;
      }

      return string.Empty;
    }

    private string GetProviderFromUrl(string urlStr)
    {
      if (string.IsNullOrEmpty(urlStr)) return string.Empty;

      int len = _providers.Length;
      for (int i = 0; i < len; i++)
      {
        var provider = _providers[i];
        if (Regex.IsMatch(urlStr, provider["pattern"].String, RegexOptions.IgnoreCase))
        {
          return provider["provider"].String;
        }
      }
      return string.Empty;
    }

    private void DownloadVideoInfo(VRCUrl url)
    {
      if (!Utilities.IsValid(url)) return;

      string urlStr = url.Get();
      if (string.IsNullOrEmpty(urlStr) || string.IsNullOrEmpty(GetProviderFromUrl(urlStr)) || _error.ContainsKey(urlStr)) return;

      if (_pending.ContainsKey(urlStr)) return;
      _pending.SetValue(urlStr, true);
      VRCStringDownloader.LoadUrl(url, (IUdonEventReceiver)this);
    }

    public override void OnStringLoadSuccess(IVRCStringDownload result)
    {
      string urlStr = result.Url.Get();
      string html = result.Result;

      _pending.Remove(urlStr);

      string provider = GetProviderFromUrl(urlStr);
      string title;

      switch (provider)
      {
        case "YouTube":
          title = YouTubeHtmlParser.GetTitleFromHtml(html);
          break;
        case "Twitch":
          title = TwitchHtmlParser.GetTitleFromHtml(html);
          break;
        default:
          return;
      }

      if (string.IsNullOrEmpty(title))
      {
        _error.SetValue(urlStr, true);
        return;
      }

      _videoInfo.SetValue(urlStr, title);
      PrintLog($"Loaded video info from {provider}: {title} ({urlStr})");

      UpdateVideoInfoToTrack(urlStr, title);
      UpdateVideoInfoToQueue(urlStr, title);
    }

    private void UpdateVideoInfoToTrack(string urlStr, string title)
    {
      if (string.IsNullOrEmpty(urlStr) || string.IsNullOrEmpty(title)) return;

      if (!Utilities.IsValid(_controller)) return;

      var track = _controller.Track;
      if (!Utilities.IsValid(track) || TrackUtils.GetUrl(track).Get() != urlStr) return;

      var currentTitle = TrackUtils.GetTitle(track);
      if (!string.IsNullOrEmpty(currentTitle)) return;

      TrackUtils.SetTitle(track, title);
      _controller.SendCustomVideoEvent(nameof(AfterTrackUpdated));
    }

    private void UpdateVideoInfoToQueue(string urlStr, string title)
    {
      if (string.IsNullOrEmpty(urlStr) || string.IsNullOrEmpty(title)) return;

      if (!Utilities.IsValid(_controller)) return;

      var queue = _controller.Queue;
      if (!Utilities.IsValid(queue) || queue.TrackCount == 0) return;

      var trackCount = queue.TrackCount;
      int changedCount = 0;
      for (int i = 0; i < trackCount; i++)
      {
        var track = queue.GetTrack(i);
        if (!Utilities.IsValid(track)) continue;
        if (TrackUtils.GetUrl(track).Get() != urlStr || !string.IsNullOrEmpty(TrackUtils.GetTitle(track))) continue;

        TrackUtils.SetTitle(track, title);
        changedCount++;
      }

      if (changedCount > 0)
      {
        _controller.SendCustomVideoEvent(nameof(AfterQueueUpdated));
      }
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
      string urlStr = result.Url.Get();
      _pending.Remove(urlStr);
      _error.SetValue(urlStr, true);
      PrintLog($"Failed to load video info: {urlStr}");
    }

    public override void AfterTrackLoaded()
    {
      if (!Utilities.IsValid(_controller)) return;

      var track = _controller.Track;
      if (!Utilities.IsValid(track) || track.Length == 0) return;

      var title = TrackUtils.GetTitle(track);
      if (string.IsNullOrEmpty(title))
      {
        DownloadVideoInfo(TrackUtils.GetUrl(track));
      }
    }

    public override void AfterQueueUpdated()
    {
      if (!Utilities.IsValid(_controller)) return;

      var queue = _controller.Queue;
      if (!Utilities.IsValid(queue)) return;

      int foundCount = 0;
      int trackCount = queue.TrackCount;
      for (int i = 0; i < trackCount; i++)
      {
        var track = queue.GetTrack(i);
        if (!Utilities.IsValid(track)) continue;

        if (!string.IsNullOrEmpty(TrackUtils.GetTitle(track))) continue;

        var url = TrackUtils.GetUrl(track);
        if (!Utilities.IsValid(url)) continue;

        var urlStr = url.Get();
        if (string.IsNullOrEmpty(urlStr) || _error.ContainsKey(urlStr)) continue;

        var videoInfo = GetVideoInfo(urlStr);
        if (!string.IsNullOrEmpty(videoInfo))
        {
          TrackUtils.SetTitle(track, videoInfo);
          foundCount++;
          continue;
        }

        DownloadVideoInfo(url);
      }

      if (foundCount > 0)
      {
        _controller.SendCustomVideoEvent(nameof(AfterQueueUpdated));
      }
    }
  }
}
