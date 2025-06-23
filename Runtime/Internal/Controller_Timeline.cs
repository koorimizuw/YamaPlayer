using UnityEngine.Playables;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
    public partial class Controller
    {
        private PlayableDirector GetCurrentTimeline()
        {
            if (!Utilities.IsValid(Track)) return null;

            var timeline = Track.GetTimeline();
            if (!Utilities.IsValid(timeline)) return null;

            return timeline;
        }

        private void PlayTimeline(float time)
        {
            var timeline = GetCurrentTimeline();
            if (!Utilities.IsValid(timeline)) return;

            if (!timeline.gameObject.activeSelf || !timeline.enabled)
            {
                timeline.gameObject.SetActive(true);
                timeline.enabled = true;
            }

            timeline.time = time;
            timeline.Play();
        }

        private void PauseTimeline(float time)
        {
            var timeline = GetCurrentTimeline();
            if (!Utilities.IsValid(timeline)) return;

            timeline.time = time;
            timeline.Pause();
        }

        private void SetTimelineTime(float time)
        {
            var timeline = GetCurrentTimeline();
            if (!Utilities.IsValid(timeline)) return;

            timeline.time = time;
        }

        private void StopTimeline()
        {
            var timeline = GetCurrentTimeline();
            if (!Utilities.IsValid(timeline)) return;

            timeline.time = 0f;
            timeline.Stop();
            timeline.gameObject.SetActive(false);
        }
    }
}