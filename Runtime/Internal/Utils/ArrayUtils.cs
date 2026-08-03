using System;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
  public static class ArrayUtils
  {
    public static void Resize<T>(ref T[] array, int newSize)
    {
      if (newSize < 0) { array = new T[0]; return; }
      T[] array2 = array;
      if (!Utilities.IsValid(array2)) array = new T[newSize];
      else if (array2.Length != newSize)
      {
        T[] array3 = new T[newSize];
        Array.Copy(array2, 0, array3, 0, (array2.Length > newSize) ? newSize : array2.Length);
        array = array3;
      }
    }

    public static T[] Add<T>(this T[] arr, T item)
    {
      Resize(ref arr, arr.Length + 1);
      arr[arr.Length - 1] = item;
      return arr;
    }

    public static T[] Remove<T>(this T[] arr, int index)
    {
      if (index < 0 || index > arr.Length - 1) return arr;
      for (int a = index; a < arr.Length - 1; a++) arr[a] = arr[a + 1];
      Resize(ref arr, arr.Length - 1);
      return arr;
    }

    public static T[] Populate<T>(this T[] arr, T value)
    {
      for (int i = 0; i < arr.Length; i++)
        arr[i] = value;
      return arr;
    }
  }
}