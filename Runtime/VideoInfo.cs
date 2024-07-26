
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
            if (urlStr.StartsWith("https://www.youtube.com") || urlStr.StartsWith("https://youtube.com"))
            {
                _info.SetValue(urlStr, Utils.FindSubString(result.Result, new string[] { "\"videoDetails\":", "title\":\"" }, '"'));
            }
            else if (urlStr.StartsWith("https://www.twitch.tv") || urlStr.StartsWith("https://twitch.tv"))
            {
                _info.SetValue(urlStr, Utils.FindSubString(result.Result, new string[] { "\"name\":\"" }, '"'));
            }
            else if (urlStr.StartsWith("https://www.nicovideo.jp") || urlStr.StartsWith("https://nicovideo.jp"))
            {
                _info.SetValue(urlStr, Utils.FindSubString(result.Result, new string[] { "\"name\":\"" }, '"'));
            }
            if (_controller.Track.GetTitle() == string.Empty)
            {
                Track track = _controller.Track;
                _controller.Track = Track.New(track.GetPlayer(), GetVideoInfo(result.Url), track.GetVRCUrl(), track.GetOriginalUrl(), track.GetDetails());
            }
            _controller.SendCustomVideoEvent(nameof(Listener.OnVideoInfoLoaded));
        }

        public override void OnStringLoadError(IVRCStringDownload result)
        {
            string urlStr = result.Url.Get();
            _info.SetValue(urlStr, string.Empty);
        }

        public override void OnUrlChanged()
        {
            if (_controller.Track.GetTitle() == string.Empty) DownloadVideoInfo(_controller.Track.GetVRCUrl());
        }
    }
}