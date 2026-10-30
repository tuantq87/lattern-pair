using System;

namespace LanternPair.Core.Planets
{
    /// <summary>
    /// The 9 hand-picked catalog planets. The first three are open; the rest unlock with banked star seeds.
    /// </summary>
    public static class PlanetCatalog
    {
        private static readonly int[] seeds = { 4127, 7310, 2589, 9043, 1776, 6652, 3318, 8401, 5260 };
        private static readonly int[] unlockCosts = { 0, 0, 0, 40, 80, 130, 190, 260, 340 };

        public static int Count => seeds.Length;

        public static int SeedAt(int index)
        {
            return seeds[index];
        }

        /// <summary>Star seeds needed to unlock the planet at <paramref name="index"/>; 0 means open from the start.</summary>
        public static int UnlockCostAt(int index)
        {
            return unlockCosts[index];
        }

        /// <summary>Catalog index of <paramref name="seed"/>, or -1 when it is not a catalog planet.</summary>
        public static int IndexOf(int seed)
        {
            return Array.IndexOf(seeds, seed);
        }
    }
}
