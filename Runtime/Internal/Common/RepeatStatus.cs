
using UdonSharp;
using UnityEngine;

namespace Yamadev.YamaStream
{
    public class RepeatStatus : UdonSharpBehaviour { }

    public static class RepeatStatusExtentions
    {
        static object[] parse(this RepeatStatus _status)
        {
            return (object[])(object)_status;
        }

        public static RepeatStatus ToRepeatStatus(this Vector3 vect)
        {
            bool enabled = vect.x == 1f;
            return (RepeatStatus)(object)(object[]) new object[] { enabled, vect.y, vect.z };
        }

        public static Vector3 ToVector3 (this RepeatStatus _status)
        {
            object[] status = _status.parse();
            float x = (bool)status[0] ? 1f : 0f;
            return new Vector3 (x, _status.GetStartTime(), _status.GetEndTime());
        }

        public static float GetStartTime(this RepeatStatus _status)
        {
            object[] status = _status.parse();
            return (float)status[1];
        }

        public static float GetEndTime(this RepeatStatus _status)
        {
            object[] status = _status.parse();
            return (float)status[2];
        }

        public static void SetStartTime(this RepeatStatus _status, float startTime)
        {
            ((object[])(object)_status)[1] = startTime;
        }

        public static void SetEndTime(this RepeatStatus _status, float endTime)
        {
            ((object[])(object)_status)[2] = endTime;
        }

        public static bool IsOn(this RepeatStatus _status)
        {
            return (bool)_status.parse()[0];
        }

        public static void TurnOn(this RepeatStatus _status)
        {
            ((object[])(object)_status)[0] = true;
        }

        public static void TurnOff(this RepeatStatus _status)
        {
            ((object[])(object)_status)[0] = false;
        }
    }
}