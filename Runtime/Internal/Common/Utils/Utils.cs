using System;
using UnityEngine;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
    public static class Utils
    {
        public static string GetProtocol(this string url)
        {
            int index = url.IndexOf("://");
            if (index == -1) return string.Empty;
            return url.Substring(0, index).ToLower();
        }

        public static string Protocol(this VRCUrl url) => url.Get().GetProtocol();

        public static bool IsValidUrl(this string url)
        {
            if (string.IsNullOrEmpty(url)) return false;
            string[] vaildProtocols = { "http", "https", "rtsp", "rtmp", "rtspt", "rtspu", "rtmps", "rtsps" };
            return Array.IndexOf(vaildProtocols, url.GetProtocol()) >= 0;
        }

        public static bool IsValid(this VRCUrl url) => url.Get().IsValidUrl();

        public static string GetSubString(string str, char end)
        {
            string temp = "";
            bool escape = false;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == end)
                {
                    if (escape) escape = false;
                    else return temp;
                }
                else
                {
                    if (str[i] == '\\') escape = true;
                    else temp += str[i];
                }
            }
            return temp;
        }

        public static string FindPairBrackets(string str, int start)
        {
            if (start > str.Length) return string.Empty;
            int end = start;
            int count = 0;
            while (end < str.Length)
            {
                if (str[end] == '{') count++;
                else if (str[end] == '}') count--;
                if (count == 0) return str.Substring(start, end - start + 1);
                end++;
            }
            return string.Empty;
        }
        
        public static DateTime ParseTimestamp(this int timestamp) => 
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp).ToLocalTime();

        public static void Resize<T>(ref T[] array, int newSize)
        {
            if (newSize < 0) array = new T[0];
            T[] array2 = array;
            if (array2 == null) array = new T[newSize];
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

        public static T[] Remove<T>(this T[] arr, T item)
        {
            int index = Array.IndexOf(arr, item);
            arr = arr.Remove(index);
            return arr;
        }

        public static bool TryFind(this Transform t, string n, out Transform result)
        {
            result = t.Find(n);
            return result != null;
        }

        public static bool TryGetComponentLocal<T>(this Transform t, out T component)
        {
            component = t.GetComponent<T>();
            return component != null;
        }

        public static T[] Populate<T>(this T[] arr, T value)
        {
            for (int i = 0; i < arr.Length; i++)
                arr[i] = value;
            return arr;
        }

        public static bool PickUpInHand(this VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player)) return false;
            return player.GetPickupInHand(VRC_Pickup.PickupHand.Left) != null ||
                player.GetPickupInHand(VRC_Pickup.PickupHand.Right) != null;
        }
    }
}