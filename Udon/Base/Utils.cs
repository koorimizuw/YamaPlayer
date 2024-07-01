
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
    public static class Utils
    {
        public static string Protocol(this string url)
        {
            int index = url.IndexOf("://");
            if (index == -1) return string.Empty;
            return url.Substring(0, index).ToLower();
        }
        public static string Protocol(this VRCUrl url) => url.Get().Protocol();
        public static bool IsValidUrl(this string url)
        {
            if (string.IsNullOrEmpty(url)) return false;
            string[] vaildProtocols = { "http", "https", "rtsp", "rtmp", "rtspt", "rtspu", "rtmps", "rtsps" };
            return Array.IndexOf(vaildProtocols, url.Protocol()) >= 0;
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

        public static string FindSubString(string str, string[] keys, char end)
        {
            foreach (string key in keys)
            {
                int index = str.IndexOf(key);
                if (index == -1) return string.Empty;
                str = str.Substring(index);
            }
            return GetSubString(str.Substring(keys[keys.Length - 1].Length), end);
        }

        public static string GetYoutubePlayListID(string str)
        {
            if (!str.StartsWith("https://www.youtube.com/")) return str;

            string target = "list=";
            int index = str.IndexOf(target);
            if (index == -1) return "";

            return GetSubString(str.Substring(index + target.Length), '&');
        }

        public static string FindPairBrackets(string str, int start)
        {
            if (start > str.Length) return string.Empty;
            int end = start;
            int count = 1;
            while (end < str.Length)
            {
                if (str[end] == '{') count++;
                else if (str[end] == '}') count--;
                if (count == 0) return str.Substring(start, end - start);
                end++;
            }
            return string.Empty;
        }

        // Thanks @KIBA_
        public static Color ParseHexColorCode(this string strC)
        {
            if (strC.Contains("#")) strC = strC.Replace("#", "");
            if (strC.Length != 6) return Color.black;
            var r = Convert.ToByte(strC.Substring(0, 2), 16);
            var g = Convert.ToByte(strC.Substring(2, 2), 16);
            var b = Convert.ToByte(strC.Substring(4, 2), 16);
            return new Color(r / 255f, g / 255f, b / 255f, 1);
        }

        public static string ToHexColorCode(this Color c) => String.Format("#{0}{1}{2}",
            ((int)(c.r * 255)).ToString("x2").ToUpper(),
            ((int)(c.g * 255)).ToString("x2").ToUpper(),
            ((int)(c.b * 255)).ToString("x2").ToUpper()
        );
        
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

        public static void SetMeOwner(this UdonSharpBehaviour udon)
        {
            if (!Networking.IsOwner(udon.gameObject)) Networking.SetOwner(Networking.LocalPlayer, udon.gameObject);
        }

        public static void SyncVariables(this UdonSharpBehaviour udon)
        {
            udon.SetMeOwner();
            udon.RequestSerialization();
        }

        public static Toggle[] GetActiveToggles(this Toggle[] toggles)
        {
            Toggle[] results = new Toggle[0];
            foreach (Toggle toggle in toggles) if (toggle.isOn) results = results.Add(toggle);
            return results;
        }
        public static Toggle GetFirstActiveToggle(this Toggle[] toggles)
        {
            Toggle[] activeToggles = toggles.GetActiveToggles();
            if (activeToggles.Length == 0) return null;
            return activeToggles[0];
        }

        public static Vector3 GetMousePosition(VRCPlayerApi.TrackingDataType type = VRCPlayerApi.TrackingDataType.Head)
        {
            var tracking = Networking.LocalPlayer.GetTrackingData(type);
            Quaternion rot = tracking.rotation;
            if (type == VRCPlayerApi.TrackingDataType.LeftHand || type == VRCPlayerApi.TrackingDataType.RightHand) rot *= Quaternion.Euler(0, 40f, 0);
            Physics.Raycast(tracking.position, rot * Vector3.forward, out RaycastHit hit, Mathf.Infinity);
            return hit.point;
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
    }
}