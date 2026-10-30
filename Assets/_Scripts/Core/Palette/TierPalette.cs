using System;
using LanternPair.Core.Planets;

namespace LanternPair.Core.Palette
{
    /// <summary>
    /// Every colour the scene needs for one planet at one ecological tier (each 150 m).
    /// A port of the prototype's `tierPal(pl, tier)`.
    /// </summary>
    public sealed class TierPalette
    {
        private static readonly double[] lightShifts = { 0, 0.03, -0.02, 0.05, -0.03, 0.02 };
        private const string Black = "#000000";
        private const string White = "#ffffff";

        private TierPalette() { }

        public string Deep { get; private set; }
        public string Mid { get; private set; }
        public string Warm { get; private set; }
        public string Light { get; private set; }

        public string SkyTop { get; private set; }
        public string SkyMid { get; private set; }
        public string SkyBottom { get; private set; }

        /// <summary>Parallax cliff layers, back to front.</summary>
        public string Far { get; private set; }
        public string Mid2 { get; private set; }
        public string Near { get; private set; }

        public string Wall { get; private set; }
        public string Spike { get; private set; }
        public string Planet { get; private set; }
        public string Fog { get; private set; }

        public static TierPalette For(PlanetDef planet, int tier)
        {
            double hueDelta = (tier % 2 != 0 ? 1 : -1) * 0.035 * Math.Ceiling(tier / 2.0) + tier * 0.012;
            double lightDelta = lightShifts[tier % lightShifts.Length] * 0.6;

            string deep = ColorMath.HueShift(planet.Palette[PaletteFamilies.Deep], hueDelta, lightDelta);
            string mid = ColorMath.HueShift(planet.Palette[PaletteFamilies.Mid], hueDelta, lightDelta);
            string warm = ColorMath.HueShift(planet.Palette[PaletteFamilies.Warm], hueDelta, lightDelta);
            string light = ColorMath.HueShift(planet.Palette[PaletteFamilies.Light], hueDelta, lightDelta);

            return new TierPalette
            {
                Deep = deep,
                Mid = mid,
                Warm = warm,
                Light = light,
                SkyTop = ColorMath.Mix(deep, Black, 0.25),
                SkyMid = ColorMath.Mix(deep, warm, 0.25),
                SkyBottom = ColorMath.Mix(warm, deep, 0.35),
                Far = ColorMath.Mix(ColorMath.Mix(warm, deep, 0.45), mid, 0.25),
                Mid2 = ColorMath.Mix(ColorMath.Mix(warm, deep, 0.65), mid, 0.35),
                Near = ColorMath.Mix(ColorMath.Mix(deep, mid, 0.45), Black, 0.1),
                Wall = ColorMath.Mix(mid, light, 0.12),
                Spike = ColorMath.Mix(deep, light, 0.55),
                Planet = ColorMath.Mix(warm, light, 0.35),
                Fog = ColorMath.Mix(warm, White, 0.4),
            };
        }
    }
}
