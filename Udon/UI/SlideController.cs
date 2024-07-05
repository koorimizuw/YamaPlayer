
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
        int _currentPage = -1;
        int _pageCount = -1;
        bool _slideMode;

        private void Start()
        {
            if (_controller == null) return;
            _controller.AddListener(this);
            _controller.RawImageScreens = _controller.RawImageScreens.Add(_screen);
        }

        public void Load()
        {
            if (_urlInputField == null || !Utils.IsValid(_urlInputField.GetUrl())) return;
            _controller.SetMeOwner();
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
            _controller.SetMeOwner();
            _controller.Paused = true;
            _pageCount = Mathf.FloorToInt(_controller.Duration);
            SetPage(0);
        }

        public void SetPage(int page)
        {
            if (!_slideMode || page < 0 || page >= _pageCount) return;
            _controller.SetMeOwner();
            if (_controller.SetTime(page + 0.5f))
            {
                _currentPage = page;
                UpdateUI();
            }
        }

        public void Up() => SetPage(_currentPage - 1);

        public void Down() => SetPage(_currentPage + 1);

        public void Stop()
        {
            _controller.SetMeOwner();
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