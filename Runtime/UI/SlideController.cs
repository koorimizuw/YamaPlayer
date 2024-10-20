
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SlideController : Listener
    {
        [SerializeField] Controller _controller;
        [SerializeField] VRCUrlInputField _urlInputField;
        [SerializeField] RawImage _screen;
        [SerializeField] Text _page;
        [SerializeField] InputField _jump;
        [SerializeField] Dropdown _dropdown;
        int _currentPage = -1;
        int _pageCount = -1;
        int _seconds = 1;
        bool _slideMode;

        private void Start()
        {
            if (_controller == null) return;
            _controller.AddListener(this);
            _controller.AddScreen(ScreenType.RawImage, _screen);
        }

        public void Load()
        {
            if (_urlInputField == null || !Utils.IsValid(_urlInputField.GetUrl())) return;
            _seconds = _dropdown.value + 1;
            _controller.TakeOwnership();
            _controller.PlayTrack(Track.New(VideoPlayerType.AVProVideoPlayer, string.Empty, _urlInputField.GetUrl()));
            _urlInputField.SetUrl(VRCUrl.Empty);
            _slideMode = true;
        }

        public void ResetSlide()
        {
            _currentPage = -1;
            _pageCount = -1;
            _slideMode = false;
            UpdateUI();
        }

        public void InitilizeSlide()
        {
            if (!_slideMode) return;
            _controller.TakeOwnership();
            _controller.Paused = true;
            _pageCount = Mathf.FloorToInt(_controller.Duration / _seconds);
            SetPage(0);
        }

        public void SetPage(int page)
        {
            if (!_slideMode || page < 0 || page >= _pageCount) return;
            _controller.TakeOwnership();
            _controller.SetTime(page * _seconds + 0.5f);
            _currentPage = page;
            UpdateUI();
        }

        public void Jump()
        {
            if (_jump == null) return;
            if (Int32.TryParse(_jump.text, out int page)) SetPage(page - 1);
            _jump.text = string.Empty;
        }

        public void Up() => SetPage(_currentPage - 1);

        public void Down() => SetPage(_currentPage + 1);

        public void Stop()
        {
            _controller.TakeOwnership();
            _controller.Stopped = true;
        }

        public void UpdateUI()
        {
            _page.text = _currentPage >= 0 ? $"Page {_currentPage + 1} / {_pageCount}" : string.Empty;
        }

        public override void OnVideoStart() => InitilizeSlide();

        public override void OnVideoStop() => ResetSlide();
    }
}