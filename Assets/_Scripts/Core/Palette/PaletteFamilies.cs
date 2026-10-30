namespace LanternPair.Core.Palette
{
    /// <summary>
    /// The six 4-colour palette families from SPEC.md: deep, mid, warm accent, light.
    /// Each planet picks one and hue-jitters it; each 150 m tier shifts it again.
    /// </summary>
    public static class PaletteFamilies
    {
        public const int Deep = 0, Mid = 1, Warm = 2, Light = 3;

        private static readonly string[][] families =
        {
            new[] { "#1B2A49", "#2E6B5E", "#F49AC1", "#FFD66B" }, // night blue, moss, sunset pink, lamp yellow
            new[] { "#2A1633", "#B5523B", "#3FC1B5", "#FFE3A3" }, // red sand + turquoise
            new[] { "#141A3A", "#5B5BA8", "#8FE3F0", "#FFF1C9" }, // violet ice
            new[] { "#1E1424", "#7A3B5C", "#FF9E5E", "#FFE08A" }, // ember
            new[] { "#0F2A33", "#2F7F86", "#F7B267", "#FDF3D0" }, // teal dusk
            new[] { "#22183D", "#6E4E9E", "#F7A8B8", "#D8F6A0" }, // lilac
        };

        public static int Count => families.Length;

        /// <summary>
        /// A copy of one family's four colours, in deep / mid / warm / light order.
        /// </summary>
        public static string[] Get(int family)
        {
            return (string[])families[family].Clone();
        }
    }
}
