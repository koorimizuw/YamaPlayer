
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace Yamadev.YamaStream
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VideoInfo : Listener
    {
        [SerializeField] Controller _controller;
        DataDictionary _info = new DataDictionary();

        void Start() => _controller.AddListener(this);

        public string GetVideoInfo(VRCUrl url)
        {
            if (string.IsNullOrEmpty(url.Get())) return string.Empty;
            if (_info.TryGetValue(url.Get(), TokenType.String, out var info)) return info.String;
            DownloadVideoInfo(url);
            return string.Empty;
        }

        public void DownloadVideoInfo(VRCUrl url)
        {
            if (!url.IsValid()) return;
            VRCStringDownloader.LoadUrl(url, (IUdonEventReceiver)this);
        }

        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            string urlStr = result.Url.Get();
            if (urlStr.StartsWith("https://www.youtube.com") || urlStr.StartsWith("https://youtube.com") || urlStr.StartsWith("https://youtu.be"))
                _info.SetValue(urlStr, GetYouTubeTitleFromHtml(result.Result));
            else if (urlStr.StartsWith("https://www.twitch.tv") || urlStr.StartsWith("https://twitch.tv"))
                _info.SetValue(urlStr, GetTwitchTitleFromTwitch(result.Result));
            else _info.SetValue(urlStr, string.Empty);

            if (!_controller.Track.HasTitle()) _controller.Track.SetTitle(GetVideoInfo(_controller.Track.GetVRCUrl()));
            _controller.SendCustomVideoEvent(nameof(OnVideoInfoLoaded));
        }

        public override void OnStringLoadError(IVRCStringDownload result)
        {
            string urlStr = result.Url.Get();
            _info.SetValue(urlStr, string.Empty);
        }

        public override void OnUrlChanged()
        {
            if (_controller.Track.GetTitle() == string.Empty) 
                DownloadVideoInfo(_controller.Track.GetVRCUrl());
        }

        #region HTML parser
        public string GetYouTubeTitleFromHtml(string html)
        {
            string keyword = "\"videoDetails\":";
            int start = html.IndexOf(keyword) + keyword.Length;
            if (start < 0) return string.Empty;

            string jsonString = Utils.FindPairBrackets(html, start);
            if (!string.IsNullOrEmpty(jsonString) &&
                VRCJson.TryDeserializeFromJson(jsonString, out var json) &&
                json.DataDictionary.TryGetValue("title", out var title))
                return title.String;
            return string.Empty;
        }

        public string GetTwitchTitleFromTwitch(string html)
        {
            int start = html.IndexOf("{\"@context\":");
            if (start < 0) return string.Empty;

            string jsonString = Utils.FindPairBrackets(html, start);
            if (!string.IsNullOrEmpty(jsonString) &&
                VRCJson.TryDeserializeFromJson(jsonString, out var json) &&
                json.DataDictionary.TryGetValue("@graph", out var graph) &&
                graph.DataList.Count > 0 &&
                graph.DataList[0].DataDictionary.TryGetValue("name", out var name))
                return name.String;
            return string.Empty;
        }
        #endregion
    }
}