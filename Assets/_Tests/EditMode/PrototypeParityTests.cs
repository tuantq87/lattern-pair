using System.Collections.Generic;
using LanternPair.Core;
using LanternPair.Core.Palette;
using LanternPair.Core.Planets;
using LanternPair.Core.Rng;
using NUnit.Framework;

namespace LanternPair.Tests
{
    /// <summary>
    /// Core must reproduce the HTML5 prototype exactly: a seed typed on the web build
    /// gives the same planet here. Expected values come from PrototypeVectors.cs,
    /// generated from Prototype/index.html by Tools/gen-parity-vectors.mjs.
    /// </summary>
    public class PrototypeParityTests
    {
        private static IEnumerable<object[]> HueShiftCases => PrototypeVectors.HueShifts;
        private static IEnumerable<object[]> MixCases => PrototypeVectors.Mixes;
        private static IEnumerable<object[]> TierCases => PrototypeVectors.Tiers;
        private static IEnumerable<PrototypeVectors.Planet> PlanetCases => PrototypeVectors.Planets;

        [Test]
        public void Mulberry32_ProducesThePrototypeSequence()
        {
            var rng = new Mulberry32(1);
            foreach (double expected in PrototypeVectors.RngSeed1)
            {
                Assert.That(rng.Next(), Is.EqualTo(expected));
            }
        }

        [TestCaseSource(nameof(HueShiftCases))]
        public void HueShift_MatchesPrototype(string hex, double hueDelta, double lightDelta, string expected)
        {
            Assert.That(ColorMath.HueShift(hex, hueDelta, lightDelta), Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(MixCases))]
        public void Mix_MatchesPrototype(string a, string b, double t, string expected)
        {
            Assert.That(ColorMath.Mix(a, b, t), Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(PlanetCases))]
        public void PlanetGenerator_MatchesPrototype(PrototypeVectors.Planet expected)
        {
            PlanetDef planet = PlanetGenerator.Make(expected.Seed);

            Assert.That(planet.Name, Is.EqualTo(expected.Name), "name");
            Assert.That(planet.Family, Is.EqualTo(expected.Family), "family");
            Assert.That(planet.HueJitter, Is.EqualTo(expected.HueJitter), "hue jitter");
            Assert.That(planet.Palette, Is.EqualTo(expected.Palette), "palette");
            Assert.That(planet.Density, Is.EqualTo(expected.Density), "density");
            Assert.That(planet.Wind, Is.EqualTo(expected.Wind), "wind");
            Assert.That(planet.CrawlerRate, Is.EqualTo(expected.CrawlerRate), "crawler rate");
            Assert.That(planet.DarknessSpeed, Is.EqualTo(expected.DarknessSpeed), "darkness speed");
            Assert.That(planet.TonicMidi, Is.EqualTo(expected.Tonic), "tonic");
            Assert.That(planet.Lead.ToString().ToLowerInvariant(), Is.EqualTo(expected.Lead), "lead");
            Assert.That(planet.Kind.ToString().ToLowerInvariant(), Is.EqualTo(expected.Kind), "kind");
            Assert.That(planet.HasMoon, Is.EqualTo(expected.Moon), "moon");
            Assert.That(planet.PlanetX, Is.EqualTo(expected.PlanetX), "planet x");
            Assert.That(planet.PlanetScale, Is.EqualTo(expected.PlanetScale), "planet scale");
        }

        [TestCaseSource(nameof(TierCases))]
        public void TierPalette_MatchesPrototype(int tier, string deep, string mid, string warm, string light,
            string skyTop, string skyMid, string skyBottom, string far, string mid2, string near,
            string wall, string spike, string planetColor, string fog)
        {
            TierPalette p = TierPalette.For(PlanetGenerator.Make(PlanetCatalog.SeedAt(0)), tier);

            Assert.That(
                new[] { p.Deep, p.Mid, p.Warm, p.Light, p.SkyTop, p.SkyMid, p.SkyBottom, p.Far, p.Mid2, p.Near, p.Wall, p.Spike, p.Planet, p.Fog },
                Is.EqualTo(new[] { deep, mid, warm, light, skyTop, skyMid, skyBottom, far, mid2, near, wall, spike, planetColor, fog }));
        }

        [Test]
        public void FirstCatalogPlanet_IsOslo9()
        {
            PlanetDef planet = PlanetGenerator.Make(PlanetCatalog.SeedAt(0));
            Assert.That(planet.Name, Is.EqualTo("Oslo-9"));
            Assert.That(planet.SeedLabel, Is.EqualTo("#4127"));
        }

        [Test]
        public void ToUint32_WrapsLikeJavaScript()
        {
            Assert.That(JsMath.ToUint32(4294967296.0 + 5), Is.EqualTo(5u));
            Assert.That(JsMath.ToUint32(-1), Is.EqualTo(4294967295u));
            Assert.That(JsMath.ToUint32(4127 * 2654435761.0), Is.EqualTo(2689780847u));
        }
    }
}
