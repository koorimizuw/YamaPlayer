
using UdonSharp;
using UnityEngine;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using Yamadev.YamachanWebUnit;

namespace Yamadev.YamaStream.Modules
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class VideoResolver : Receiver
    {
        [SerializeField] Client _client;
        [SerializeField] Controller _controller;
        [SerializeField] VRCUrl _callbackYoutubeUrl;
        [SerializeField] VRCUrl _callbackNiconicoUrl;
        VRCUrl _callbackUrl;
#if UNITY_ANDROID
        bool _isQuest = true;
#else
        bool _isQuest = false;
#endif

        void Start() => SendCustomEventDelayedFrames(nameof(SetHooks), 1);

        public void SetHooks()
        {
            _controller.ResolveTrack = UdonEvent.New(this, nameof(ResolveTrack));
        }

        public void ResolveTrack()
        {
            Track track = _controller.Track;
            if (!_isQuest)
            {
                if (track.GetUrl().StartsWith("https://www.nicovideo.jp") || track.GetUrl().StartsWith("https://nicovideo.jp"))
                {
                    PlayNicoVideo(track.GetUrl());
                    return;
                }
                string originalUrl = track.GetOriginalUrl();
                if (track.GetVRCUrl().Equals(VRCUrl.Empty) && originalUrl != string.Empty)
                {
                    if (originalUrl.StartsWith("https://www.youtube.com") || originalUrl.StartsWith("https://youtube.com")) PlayYoutubeVideo(originalUrl);
                    return;
                }
            }
            _controller.Resolve();
        }

        public override void OnRequestSuccess(IVRCStringDownload result) => _controller.VideoPlayerHandle.PlayUrl(_callbackUrl);

        public void PlayYoutubeVideo(string url)
        {
            Debug.Log($"[<color=#ff70ab>YamaStream</color>] Resolve youtube url: {url}");
            string id = url.Replace("https://youtube.com/watch?v=", "").Replace("https://www.youtube.com/watch?v=", "").Split('&')[0];
            _client.Request(VRCUrl.Empty, id, this);
            _callbackUrl = _callbackYoutubeUrl;
        }

        public void PlayNicoVideo(string url)
        {
            Debug.Log($"[<color=#ff70ab>YamaStream</color>] Resolve niconico url: {url}");
            string id = url.Replace("https://nicovideo.jp/watch/", "").Replace("https://www.nicovideo.jp/watch/", "").Split('?')[0];
            _client.Request(VRCUrl.Empty, id, this);
            _callbackUrl = _callbackNiconicoUrl;
        }
    }
}