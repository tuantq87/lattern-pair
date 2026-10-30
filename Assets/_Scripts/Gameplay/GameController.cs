using LanternPair.Core.Palette;
using LanternPair.Core.Planets;
using LanternPair.Data;
using LanternPair.Input;
using UnityEngine;

namespace LanternPair.Gameplay
{
    /// <summary>
    /// Scene entry point for a run. Builds the planet from its seed and paints the sky from its palette.
    /// The run simulation (Core) and the world views (girl, walls, hazards, darkness) plug in here next.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [SerializeField] private GameBalance balance;
        [SerializeField] private GestureDetector gestures;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private int startSeed = 4127;

        public PlanetDef Planet { get; private set; }

        public TierPalette Palette { get; private set; }

        void OnEnable()
        {
            if (gestures == null) return;
            gestures.Pressed += OnPressed;
            gestures.SwipedUp += OnSwipedUp;
            gestures.Released += OnReleased;
        }

        void OnDisable()
        {
            if (gestures == null) return;
            gestures.Pressed -= OnPressed;
            gestures.SwipedUp -= OnSwipedUp;
            gestures.Released -= OnReleased;
        }

        void Start()
        {
            StartRun(startSeed);
        }

        public void StartRun(int seed)
        {
            Planet = PlanetGenerator.Make(seed);
            Palette = TierPalette.For(Planet, 0);

            if (worldCamera != null && ColorUtility.TryParseHtmlString(Palette.SkyMid, out Color sky))
            {
                worldCamera.backgroundColor = sky;
            }

            Debug.Log($"GameController: run started on {Planet.Name} {Planet.SeedLabel} " +
                      $"(palette family {Planet.Family}, {Planet.Kind} planet, lead {Planet.Lead})");
        }

        // Input is only logged until the run simulation lands; it proves touch and mouse reach the game.
        private void OnPressed(Vector2 screenPosition)
        {
            Debug.Log($"GameController: press at {screenPosition}");
        }

        private void OnSwipedUp()
        {
            Debug.Log("GameController: swipe up");
        }

        private void OnReleased(Vector2 screenPosition, float heldSeconds)
        {
            Debug.Log($"GameController: release after {heldSeconds:0.00}s");
        }
    }
}
