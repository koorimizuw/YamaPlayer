using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Playables;
using VRC.SDKBase;

namespace Yamadev.YamaStream.Modules.TimelineSync
{
  [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
  public class TimelineSync : YamaPlayerModule
  {
    [SerializeField] private string[] _urls;
    [SerializeField] private PlayableDirector[] _timelines;
    [SerializeField] private bool[] _hideOnStop;
    private int _currentIndex = -1;
    private float _pendingTime = 0f;

    public override void Start()
    {
      base.Start();

      var len = _hideOnStop.Length;
      for (int i = 0; i < len; i++)
      {
        if (i >= _timelines.Length) break;
        if (_hideOnStop[i])
        {
          var timeline = _timelines[i];
          if (Utilities.IsValid(timeline)) timeline.gameObject.SetActive(false);
        }
      }
    }

    public string[] Urls => _urls;
    public PlayableDirector[] Timelines => _timelines;
    public PlayableDirector CurrentTimeline
    {
      get
      {
        if (_currentIndex < 0 || _currentIndex >= _timelines.Length) return null;
        return _timelines[_currentIndex];
      }
    }

    private PlayableDirector GetTimelineForUrl(string url)
    {
      if (!Utilities.IsValid(_urls) || !Utilities.IsValid(_timelines)) return null;
      if (string.IsNullOrEmpty(url)) return null;

      for (int i = 0; i < _urls.Length; i++)
      {
        if (i >= _timelines.Length) break;
        if (_urls[i] == url) return _timelines[i];
      }
      return null;
    }

    private void PlayTimeline(float time)
    {
      if (!Utilities.IsValid(CurrentTimeline)) return;

      if (!CurrentTimeline.gameObject.activeSelf || !CurrentTimeline.enabled)
      {
        CurrentTimeline.gameObject.SetActive(true);
        CurrentTimeline.enabled = true;
      }

      CurrentTimeline.time = time;
      CurrentTimeline.Play();
    }

    private void PauseTimeline(float time)
    {
      if (!Utilities.IsValid(CurrentTimeline)) return;

      CurrentTimeline.time = time;
      CurrentTimeline.Pause();
    }

    private void SetTimelineTime(float time)
    {
      if (!Utilities.IsValid(CurrentTimeline)) return;

      CurrentTimeline.time = time;
    }

    private void StopTimeline()
    {
      if (!Utilities.IsValid(CurrentTimeline)) return;

      CurrentTimeline.time = 0f;
      CurrentTimeline.Stop();
      if (_currentIndex >= 0 && _currentIndex < _hideOnStop.Length && _hideOnStop[_currentIndex])
        _timelines[_currentIndex].gameObject.SetActive(false);
      _currentIndex = -1;
    }

    private void UpdateCurrentTimeline()
    {
      if (!Utilities.IsValid(_controller) || !Utilities.IsValid(_controller.Track)) return;

      string url = TrackUtils.GetUrl(_controller.Track).Get();
      var timeline = GetTimelineForUrl(url);

      if (CurrentTimeline != timeline)
      {
        StopTimeline();
        _currentIndex = Array.IndexOf(_urls, url);
        if (_currentIndex >= 0 && _currentIndex < _hideOnStop.Length && _hideOnStop[_currentIndex])
          _timelines[_currentIndex].gameObject.SetActive(true);
      }
    }

    public override void AfterVideoReady()
    {
      UpdateCurrentTimeline();
      if (Utilities.IsValid(_controller) && _controller.SyncedState == PlayerState.Playing) PlayTimeline(_pendingTime);
      else PauseTimeline(_pendingTime);
    }

    public override void AfterVideoLooped()
    {
      PlayTimeline(0f);
    }

    public override void AfterVideoPlayed()
    {
      if (!Utilities.IsValid(_controller)) return;
      PlayTimeline(_controller.VideoTime);
    }

    public override void AfterVideoPaused()
    {
      if (!Utilities.IsValid(_controller)) return;
      PauseTimeline(_controller.VideoTime);
    }

    public override void AfterVideoStopped()
    {
      StopTimeline();
    }

    public override void AfterTimeChanged(float time)
    {
      _pendingTime = time;
      SetTimelineTime(time);
    }

    public override void AfterTrackLoaded()
    {
      if (!Utilities.IsValid(_controller)) return;
      _pendingTime = 0f;
      UpdateCurrentTimeline();
    }
  }
}
