using System;
using UnityEngine;

namespace Yamadev.YamaStream
{
    public static class ColorExtensions
    {
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
    }
}