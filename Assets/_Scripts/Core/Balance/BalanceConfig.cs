using System;

namespace LanternPair.Core.Balance
{
    /// <summary>
    /// Tuning values taken from the prototype (Prototype/SPEC.md and the constants at the top of index.html).
    /// Distances are logical pixels of the 390×844 portrait canvas, like the prototype;
    /// the Gameplay layer converts them to world units. Serializable so a GameBalance asset can expose it in the Inspector.
    /// </summary>
    [Serializable]
    public sealed class BalanceConfig
    {
        // Logical canvas and layout
        public float canvasWidth = 390f;
        public float canvasHeight = 844f;
        public float wallLeftX = 64f;
        public float wallRightX = 326f;
        public float girlScreenY = 610f;

        // Wall jump (tap)
        public float jumpRise = 70f;
        public float jumpArc = 46f;
        public float jumpDuration = 0.46f;

        // Dash (swipe up while airborne)
        public float dashDistance = 150f;
        public float dashDuration = 0.22f;
        public float dashCooldown = 2f;

        // Input thresholds
        public float holdToSendDuration = 0.32f;
        public float holdCancelDistance = 28f;
        public float swipeMinDistance = 36f;
        public float swipeMaxTime = 0.45f;

        // Wisp energy
        public int wispSendCost = 20;
        public int wispDispelCost = 25;
        public int flowerEnergyGain = 50;

        // Activated objects
        public float doorShortcut = 210f;
        public float mushroomBounce = 230f;
        public float bridgeRestSeconds = 3f;
        public float bridgeDarknessPushback = 140f;

        // Darkness and progression
        public float darknessMaxLag = 600f;
        public float tierHeightMeters = 150f;

        // Score
        public int chainBonusEvery = 10;
        public int starSeedScore = 5;
        public int rootDashBonus = 5;
    }
}
