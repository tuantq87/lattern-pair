namespace LanternPair.UI.Views
{
    /// <summary>Full-screen views of the main scene.</summary>
    public enum ViewType
    {
        /// <summary>Planet picker, start button, seed entry.</summary>
        Title,

        /// <summary>In-run HUD: altitude, score, shield, energy, dash cooldown.</summary>
        Game,

        /// <summary>Result panel with restart and menu.</summary>
        GameOver,
    }
}
