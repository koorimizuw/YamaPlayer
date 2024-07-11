
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components.Video;

namespace Yamadev.YamaStream
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DebugInfo : Listener
    {
        [SerializeField] Controller _controller;
        [SerializeField] LatencyManager _latencyManager;
        [SerializeField] Color _debugColor;
        DateTime[] _logTime = new DateTime[] { };
        string[] _logText = new string[] { };

        [SerializeField] GameObject _debugInfoPage;
        [SerializeField] Text _trackTitleText;
        [SerializeField] Text _trackUrlText;
        [SerializeField] Text _trackOriginalUrlText;
        [SerializeField] Text _playlistText;
        [SerializeField] Text _playlistIndexText;
        [SerializeField] Text _networkDelayText;
        [SerializeField] Text _videoOffsetText;
        int _count = 0;
        float _lastClick = 0;

        void Start() => _controller.AddListener(this);

        string _prefix => $"[<color={_debugColor.ToHexColorCode()}>YamaStream</color>]";
        public string Logs
        {
            get
            {
                string result = string.Empty;
                for (int i = 0; i <= _logTime.Length - 1; i++) result += $"{_logTime[i]} {_logText[i]}\n";
                return result;

            }
        }

        public void AddLog(string log, bool printLog = false)
        {
            _logTime = _logTime.Add(DateTime.Now);
            _logText = _logText.Add(log);
            if (printLog) Debug.Log($"{_prefix} {log}");
        }
        void Update()
        {
            if (_debugInfoPage != null && !_debugInfoPage.activeSelf) return;
            if (_trackTitleText != null) _trackTitleText.text = _controller.Track.GetTitle();
            if (_trackUrlText != null) _trackUrlText.text = _controller.Track.GetVRCUrl().Get();
            if (_trackOriginalUrlText != null) _trackOriginalUrlText.text = _controller.Track.GetOriginalUrl();
            if (_playlistText != null) _playlistText.text = _controller.ActivePlaylist != null ? _controller.ActivePlaylist.PlaylistName : "None";
            if (_playlistIndexText != null) _playlistIndexText.text = _controller.PlayingTrackIndex.ToString();
            if (_networkDelayText != null && _latencyManager != null) _networkDelayText.text = $"{_latencyManager.GetServerDelayseconds()}s";
            if (_videoOffsetText != null) _videoOffsetText.text = _controller.VideoStandardDelay.ToString();
        }
        
        public void OnTriggerClicked()
        {
            if (Time.time <= _lastClick + 0.5f) _count++;
            else _count = 1;
            _lastClick = Time.time;
            if (_count >= 5)
            {
                _debugInfoPage.SetActive(true);
                _count = 0;
            }
        }

        #region Video Event
        public override void OnPlayerChanged() => AddLog($"Video player change to {_controller.VideoPlayerType}.", true);
        public override void OnVideoReady() => AddLog($"[{_controller.VideoPlayerType}] Video ready.", true);
        public override void OnVideoStart() => AddLog($"[{_controller.VideoPlayerType}] Video start.", true);
        public override void OnVideoPlay() => AddLog($"[{_controller.VideoPlayerType}] Video play.", true);
        public override void OnVideoPause() => AddLog($"[{_controller.VideoPlayerType}] Video pause.", true);
        public override void OnVideoStop() => AddLog($"[{_controller.VideoPlayerType}] Video stop.", true);
        public override void OnVideoLoop() => AddLog($"[{_controller.VideoPlayerType}] Video loop.", true);
        public override void OnVideoEnd() => AddLog($"[{_controller.VideoPlayerType}] Video end.", true);
        public override void OnVideoError(VideoError videoError) => AddLog($"[{_controller.VideoPlayerType}] Video error: {videoError}.", true);
        public override void OnTrackSynced(string url) => AddLog($"[{_controller.VideoPlayerType}] On track synced: {url}", true);
        public override void OnUrlChanged() => AddLog($"[{_controller.VideoPlayerType}] Play track: {_controller.Track.GetUrl()}, reload: {_controller.IsReload}.", true);
        public override void OnSetTime(float time) => AddLog($"Set video time: {time}.", true);
        public override void OnLoopChanged() => AddLog($"Set loop: {_controller.Loop}.", true);
        public override void OnSpeedChanged() => AddLog($"Set speed: {_controller.Speed:F2}x.", true);
        public override void OnRepeatChanged()
        {
            RepeatStatus status = _controller.Repeat.ToRepeatStatus();
            AddLog($"Set repeat: {status.IsOn()}, {(status.IsOn() ? $" start: {status.GetStartTime()}, end: {status.GetEndTime()}" : string.Empty)}", true);
        }
        public override void OnVolumeChanged() => AddLog($"Set volume: {_controller.Volume * 100}%.", true);
        public override void OnMuteChanged() => AddLog($"Set mute: {_controller.Mute}.", true);
        #endregion
    }
}