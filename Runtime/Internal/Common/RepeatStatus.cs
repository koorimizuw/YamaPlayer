using System;
using UnityEngine;

namespace Yamadev.YamaStream
{
    public class RepeatStatus : ObjectClass
    {
        public static RepeatStatus New(ulong packed)
        {
            bool flag = (packed & (1UL << 63)) != 0;
            ulong clearedPacked = packed & ~(1UL << 63);
            uint startBits = (uint)((clearedPacked >> 32) & 0xFFFFFFFF);
            uint endBits = (uint)(clearedPacked & 0xFFFFFFFF);

            float start = BitConverter.ToSingle(BitConverter.GetBytes(startBits), 0);
            float end = BitConverter.ToSingle(BitConverter.GetBytes(endBits), 0);

            var result = new object[] { flag, start, end };
            return result.ForceCast<RepeatStatus>();
        }

        public static RepeatStatus New(bool flag, float start, float end)
        {
            var result = new object[] { flag, start, end };
            return result.ForceCast<RepeatStatus>();
        }
    }

    public static class RepeatStatusExtentions
    {
        public static ulong Pack(this RepeatStatus status)
        {
            var arr = status.UnPack();
            ulong flagBit = ((bool)arr[0] ? 1UL : 0UL) << 63;
            uint startBits = BitConverter.ToUInt32(BitConverter.GetBytes((float)arr[1]), 0);
            uint endBits = BitConverter.ToUInt32(BitConverter.GetBytes((float)arr[2]), 0);

            return flagBit | ((ulong)startBits << 32) | endBits;
        }

        public static float GetStartTime(this RepeatStatus status)
        {
            var arr = status.UnPack();
            return (float)arr[1];
        }

        public static float GetEndTime(this RepeatStatus status)
        {
            var arr = status.UnPack();
            float value = (float)arr[2];
            return value == 0f ? Mathf.Infinity : value;
        }

        public static void SetStartTime(this RepeatStatus status, float startTime)
        {
            ((object[])(object)status)[1] = startTime;
            var arr = status.UnPack();
        }

        public static void SetEndTime(this RepeatStatus status, float endTime)
        {
            ((object[])(object)status)[2] = endTime;
        }

        public static bool IsOn(this RepeatStatus status)
        {
            return (bool)status.UnPack()[0];
        }

        public static void TurnOn(this RepeatStatus status)
        {
            ((object[])(object)status)[0] = true;
        }

        public static void TurnOff(this RepeatStatus status)
        {
            ((object[])(object)status)[0] = false;
        }
    }
}