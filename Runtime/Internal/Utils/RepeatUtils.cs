using System;

namespace Yamadev.YamaStream
{
  public static class RepeatUtils
  {
    public static object[] NewRepeatStatus(bool flag, float start, float end)
    {
      return new object[] { flag, start, end };
    }

    public static object[] NewRepeatStatus(ulong packedData)
    {
      bool flag = (packedData & (1ul << 63)) != 0;
      ulong clearedPacked = packedData & ~(1ul << 63);
      uint startBits = (uint)((clearedPacked >> 32) & 0xFFFFFFFF);
      uint endBits = (uint)(clearedPacked & 0xFFFFFFFF);

      float start = BitConverter.Int32BitsToSingle((int)startBits);
      float end = BitConverter.Int32BitsToSingle((int)endBits);

      return new object[] { flag, start, end };
    }

    public static object[] CreateEmptyRepeatStatus()
    {
      return new object[] { false, 0f, 0f };
    }

    public static ulong GetPackedData(object[] repeat)
    {
      ulong flagBit = ((bool)repeat[0] ? 1ul : 0ul) << 63;
      float startTime = (float)repeat[1];
      if (startTime < 0f) startTime = 0f;
      uint startBits = (uint)BitConverter.SingleToInt32Bits(startTime);
      uint endBits = (uint)BitConverter.SingleToInt32Bits((float)repeat[2]);
      return flagBit | ((ulong)startBits << 32) | endBits;
    }

    public static bool IsOn(object[] repeat)
    {
      return (bool)repeat[0];
    }

    public static bool IsOn(ulong packedData)
    {
      return (packedData & (1ul << 63)) != 0;
    }

    public static float GetStartTime(object[] repeat)
    {
      return (float)repeat[1];
    }

    public static float GetStartTime(ulong packedData)
    {
      ulong clearedPacked = packedData & ~(1ul << 63);
      return BitConverter.Int32BitsToSingle((int)(uint)((clearedPacked >> 32) & 0xFFFFFFFF));
    }

    public static float GetEndTime(object[] repeat)
    {
      return (float)repeat[2];
    }

    public static float GetEndTime(ulong packedData)
    {
      return BitConverter.Int32BitsToSingle((int)(uint)(packedData & 0xFFFFFFFF));
    }

    public static void SetStartTime(object[] repeat, float startTime)
    {
      repeat[1] = startTime;
    }

    public static void SetEndTime(object[] repeat, float endTime)
    {
      repeat[2] = endTime;
    }

    public static void TurnOn(object[] repeat)
    {
      repeat[0] = true;
    }

    public static void TurnOff(object[] repeat)
    {
      repeat[0] = false;
    }
  }
}
