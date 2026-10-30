using LanternPair.Core.Palette;
using LanternPair.Core.Rng;

namespace LanternPair.Core.Planets
{
    /// <summary>
    /// Builds a planet from its seed. A line-for-line port of the prototype's `makePlanet(sd)`:
    /// the order of random draws is part of the contract, so do not reorder the calls below.
    /// </summary>
    public static class PlanetGenerator
    {
        private static readonly string[] syllablesA = { "Ve", "Ka", "Os", "Mi", "No", "Ul", "Ta", "Si", "Ry", "Ae", "Zo", "Le", "Ith", "Bra" };
        private static readonly string[] syllablesB = { "lun", "ri", "tha", "ra", "vi", "ze", "pe", "lo", "mar", "sen", "dra", "quin", "nox", "via" };
        private static readonly int[] tonics = { 57, 58, 60, 62, 55, 53, 59, 56 };

        private const int LeadCount = 4;
        private const int KindCount = 3;

        public static PlanetDef Make(int seed)
        {
            var r = new Mulberry32(JsMath.ToUint32(seed * 2654435761.0));
            r.Next();
            r.Next();

            int family = r.NextIndex(PaletteFamilies.Count);
            double hueJitter = (r.Next() - 0.5) * 0.06;
            string name = syllablesA[r.NextIndex(syllablesA.Length)] + syllablesB[r.NextIndex(syllablesB.Length)] + "-" + (1 + r.NextIndex(9));

            string[] palette = PaletteFamilies.Get(family);
            for (int i = 0; i < palette.Length; i++)
            {
                palette[i] = ColorMath.HueShift(palette[i], hueJitter);
            }

            double density = JsMath.ToFixed2(0.85 + r.Next() * 0.45);
            double wind = JsMath.ToFixed2(0.6 + r.Next() * 0.9);
            double crawlerRate = JsMath.ToFixed2(0.6 + r.Next() * 0.9);
            double darknessSpeed = JsMath.ToFixed2(0.92 + r.Next() * 0.16);
            int tonic = tonics[r.NextIndex(tonics.Length)];
            var lead = (LeadInstrument)r.NextIndex(LeadCount);
            var kind = (PlanetKind)r.NextIndex(KindCount);
            bool hasMoon = r.Next() < 0.6;
            double planetX = 60 + r.Next() * 270;
            double planetScale = 0.7 + r.Next() * 0.5;

            return new PlanetDef(seed, name, family, hueJitter, palette, density, wind, crawlerRate, darknessSpeed,
                tonic, lead, kind, hasMoon, planetX, planetScale);
        }
    }
}
