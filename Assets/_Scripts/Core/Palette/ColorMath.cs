using System;
using System.Globalization;

namespace LanternPair.Core.Palette
{
    /// <summary>
    /// Hex/HSL colour helpers ported from the prototype (`hex2rgb`, `rgb2hsl`, `hueShift`, `mixc`).
    /// Colours stay "#rrggbb" strings in Core; the Unity layer parses them with ColorUtility.
    /// </summary>
    public static class ColorMath
    {
        public static double[] HexToRgb(string hex)
        {
            if (hex == null || hex.Length != 7 || hex[0] != '#')
            {
                throw new ArgumentException($"Expected a #rrggbb colour, got '{hex}'", nameof(hex));
            }

            return new double[]
            {
                int.Parse(hex.Substring(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                int.Parse(hex.Substring(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                int.Parse(hex.Substring(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
            };
        }

        public static string RgbToHex(double r, double g, double b)
        {
            return "#" + Channel(r) + Channel(g) + Channel(b);
        }

        /// <summary>
        /// Rotates the hue by <paramref name="hueDelta"/> (in turns) and shifts lightness by
        /// <paramref name="lightDelta"/>, exactly like the prototype's `hueShift(c, dh, dl)`.
        /// </summary>
        public static string HueShift(string hex, double hueDelta, double lightDelta = 0)
        {
            double[] hsl = RgbToHsl(HexToRgb(hex));
            hsl[0] = (hsl[0] + hueDelta + 1) % 1;
            hsl[2] = Clamp(hsl[2] + lightDelta, 0, 1);
            double[] rgb = HslToRgb(hsl);
            return RgbToHex(rgb[0], rgb[1], rgb[2]);
        }

        /// <summary>
        /// Linear mix from <paramref name="a"/> towards <paramref name="b"/> by <paramref name="t"/> (the prototype's `mixc`).
        /// </summary>
        public static string Mix(string a, string b, double t)
        {
            double[] x = HexToRgb(a);
            double[] y = HexToRgb(b);
            return RgbToHex(x[0] + (y[0] - x[0]) * t, x[1] + (y[1] - x[1]) * t, x[2] + (y[2] - x[2]) * t);
        }

        public static double[] RgbToHsl(double[] rgb)
        {
            double r = rgb[0] / 255, g = rgb[1] / 255, b = rgb[2] / 255;
            double max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b));
            double h = 0, s = 0, l = (max + min) / 2;

            if (max != min)
            {
                double d = max - min;
                s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
                h = max == r ? (g - b) / d + (g < b ? 6 : 0)
                  : max == g ? (b - r) / d + 2
                  : (r - g) / d + 4;
                h /= 6;
            }

            return new[] { h, s, l };
        }

        public static double[] HslToRgb(double[] hsl)
        {
            double h = hsl[0], s = hsl[1], l = hsl[2];
            if (s == 0) return new[] { l * 255, l * 255, l * 255 };

            double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            double p = 2 * l - q;
            return new[] { HueToChannel(p, q, h + 1.0 / 3) * 255, HueToChannel(p, q, h) * 255, HueToChannel(p, q, h - 1.0 / 3) * 255 };
        }

        private static double HueToChannel(double p, double q, double t)
        {
            t = (t + 1) % 1;
            if (t < 1.0 / 6) return p + (q - p) * 6 * t;
            if (t < 0.5) return q;
            if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;
            return p;
        }

        private static string Channel(double v)
        {
            int c = (int)Clamp(JsMath.Round(v), 0, 255);
            return c.ToString("x2", CultureInfo.InvariantCulture);
        }

        private static double Clamp(double v, double min, double max)
        {
            return v < min ? min : v > max ? max : v;
        }
    }
}
