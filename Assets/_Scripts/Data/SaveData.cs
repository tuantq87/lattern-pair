using System;
using System.Collections.Generic;
using LanternPair.Core.Planets;

namespace LanternPair.Data
{
    [Serializable]
    public class PlanetBest
    {
        public int seed;
        public int score;
    }

    /// <summary>
    /// Everything kept between sessions: best scores, banked star seeds, unlocked and visited planets.
    /// Saved as JSON by SaveDataManager (JsonUtility serializes fields only, so no dictionaries here).
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public int overallBest;
        public int bankedStarSeeds;
        public List<PlanetBest> planetBests = new List<PlanetBest>();
        public List<int> unlockedCatalogIndices = new List<int>();

        /// <summary>Seeds the player has flown, oldest first. Only these can be re-entered on the seed keypad.</summary>
        public List<int> visitedSeeds = new List<int>();

        /// <summary>
        /// Fills lists missing from older or hand-edited files and opens the free catalog planets.
        /// </summary>
        public void EnsureInitialized()
        {
            planetBests ??= new List<PlanetBest>();
            unlockedCatalogIndices ??= new List<int>();
            visitedSeeds ??= new List<int>();

            for (int i = 0; i < PlanetCatalog.Count; i++)
            {
                if (PlanetCatalog.UnlockCostAt(i) == 0 && !unlockedCatalogIndices.Contains(i))
                {
                    unlockedCatalogIndices.Add(i);
                }
            }
        }

        public int BestFor(int seed)
        {
            PlanetBest entry = planetBests.Find(b => b.seed == seed);
            return entry?.score ?? 0;
        }

        /// <summary>
        /// Records a finished run. Returns true when the score beats the planet's previous best.
        /// </summary>
        public bool RecordRun(int seed, int score, int starSeedsCollected)
        {
            bankedStarSeeds += starSeedsCollected;
            if (!visitedSeeds.Contains(seed)) visitedSeeds.Add(seed);
            if (score > overallBest) overallBest = score;

            PlanetBest entry = planetBests.Find(b => b.seed == seed);
            if (entry == null)
            {
                planetBests.Add(new PlanetBest { seed = seed, score = score });
                return score > 0;
            }

            if (score <= entry.score) return false;
            entry.score = score;
            return true;
        }
    }
}
