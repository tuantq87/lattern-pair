using System.Collections.Generic;

namespace LanternPair.Core.Planets
{
    /// <summary>Planet art variant (prototype: `kind`). Order matches the prototype's PKINDS.</summary>
    public enum PlanetKind
    {
        Band,
        Crater,
        Ring,
    }

    /// <summary>Lead instrument of the planet's lo-fi track (prototype: `lead`). Order matches LEADS.</summary>
    public enum LeadInstrument
    {
        Rhodes,
        Pluck,
        Bell,
        Kalimba,
    }

    /// <summary>
    /// Everything a seed decides about a planet. Immutable; build it with <see cref="PlanetGenerator.Make"/>.
    /// </summary>
    public sealed class PlanetDef
    {
        public PlanetDef(
            int seed, string name, int family, double hueJitter, IReadOnlyList<string> palette,
            double density, double wind, double crawlerRate, double darknessSpeed,
            int tonicMidi, LeadInstrument lead, PlanetKind kind, bool hasMoon,
            double planetX, double planetScale)
        {
            Seed = seed;
            Name = name;
            Family = family;
            HueJitter = hueJitter;
            Palette = palette;
            Density = density;
            Wind = wind;
            CrawlerRate = crawlerRate;
            DarknessSpeed = darknessSpeed;
            TonicMidi = tonicMidi;
            Lead = lead;
            Kind = kind;
            HasMoon = hasMoon;
            PlanetX = planetX;
            PlanetScale = planetScale;
        }

        public int Seed { get; }

        /// <summary>Compound name shown to the player, e.g. "Oslo-9".</summary>
        public string Name { get; }

        /// <summary>Index into <see cref="LanternPair.Core.Palette.PaletteFamilies"/>.</summary>
        public int Family { get; }

        public double HueJitter { get; }

        /// <summary>Four "#rrggbb" colours: deep, mid, warm, light.</summary>
        public IReadOnlyList<string> Palette { get; }

        /// <summary>Obstacle density multiplier (prototype: `dens`).</summary>
        public double Density { get; }

        /// <summary>Wind zone rate multiplier (prototype: `wind`).</summary>
        public double Wind { get; }

        /// <summary>Shadow crawler rate multiplier (prototype: `crit`).</summary>
        public double CrawlerRate { get; }

        /// <summary>Darkness speed multiplier (prototype: `dark`).</summary>
        public double DarknessSpeed { get; }

        /// <summary>MIDI note of the music's tonic.</summary>
        public int TonicMidi { get; }

        public LeadInstrument Lead { get; }

        public PlanetKind Kind { get; }

        public bool HasMoon { get; }

        /// <summary>Horizontal position of the backdrop planet, in logical px of the 390-wide canvas.</summary>
        public double PlanetX { get; }

        /// <summary>Size multiplier of the backdrop planet.</summary>
        public double PlanetScale { get; }

        /// <summary>The seed as the game shows it, e.g. "#4127".</summary>
        public string SeedLabel => "#" + Seed;
    }
}
