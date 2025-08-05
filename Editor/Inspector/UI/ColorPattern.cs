using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Yamadev.YamaStream.Editor
{
    public class ColorPattern
    {
        public string Name { get; }
        public Color PrimaryColor { get; }
        public Color SecondaryColor { get; }

        public ColorPattern(string name, Color primaryColor, Color secondaryColor)
        {
            Name = name;
            PrimaryColor = primaryColor;
            SecondaryColor = secondaryColor;
        }

        public bool Matches(Color primaryColor, Color secondaryColor)
        {
            return ColorApproximately(PrimaryColor, primaryColor) &&
                   ColorApproximately(SecondaryColor, secondaryColor);
        }

        private static bool ColorApproximately(Color a, Color b, float tolerance = 0.01f)
        {
            return Mathf.Abs(a.r - b.r) < tolerance &&
                   Mathf.Abs(a.g - b.g) < tolerance &&
                   Mathf.Abs(a.b - b.b) < tolerance &&
                   Mathf.Abs(a.a - b.a) < tolerance;
        }
        public static ColorPattern CreateCustom(Color primaryColor, Color secondaryColor)
        {
            return new ColorPattern(Localization.Get("customColor"), primaryColor, secondaryColor);
        }
    }

    public static class ColorPatternPresets
    {
        private static readonly ColorPattern[] _presetPatterns = new ColorPattern[]
        {
            new ColorPattern("pinkColor",
                new Color(0.9372549f, 0.3843137f, 0.5686275f),
                new Color(0.9686275f, 0.7294118f, 0.8117647f, 0.1215686f)
            ),
            new ColorPattern("blueColor",
                new Color(0.01176471f, 0.6627451f, 0.9568627f),
                new Color(0.5058824f, 0.8313726f, 0.9803922f, 0.1215686f)
            ),
            new ColorPattern("greenColor",
                new Color(0.2980392f, 0.6862745f, 0.3137255f),
                new Color(0.6470588f, 0.8392157f, 0.654902f, 0.1215686f)
            ),
            new ColorPattern("orangeColor",
                new Color(1f, 0.5960785f, 0f),
                new Color(1f, 0.8f, 0.5019608f, 0.1215686f)
            ),
            new ColorPattern("purpleColor",
                new Color(0.6117647f, 0.1529412f, 0.6901961f),
                new Color(0.8078431f, 0.5764706f, 0.8470588f, 0.1215686f)
            )
        };

        public static List<ColorPattern> GetAllPatterns()
        {
            var patterns = new List<ColorPattern>(_presetPatterns);
            patterns.Add(CreateCustomPattern());
            return patterns;
        }

        public static ColorPattern[] GetPresetPatterns()
        {
            return _presetPatterns;
        }

        public static ColorPattern CreateCustomPattern()
        {
            return new ColorPattern("customColor",
                Color.white,
                new Color(1f, 1f, 1f, 0.1215686f)
            );
        }

        public static ColorPattern FindBestMatch(Color primaryColor, Color secondaryColor)
        {
            return _presetPatterns.FirstOrDefault(pattern =>
                pattern.Matches(primaryColor, secondaryColor)) ?? CreateCustomPattern();
        }
    }
}