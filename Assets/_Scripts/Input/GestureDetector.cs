using System;
using LanternPair.Core.Balance;
using LanternPair.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LanternPair.Input
{
    /// <summary>
    /// Turns raw pointer input (touch or mouse) into the prototype's gesture events: press, swipe up, release.
    /// Distances are measured in the prototype's logical px (screen height = 844) so the SPEC thresholds apply unchanged.
    /// What a press means (jump, hold-to-send, dispel) is decided by the gameplay layer, which knows what is under the finger.
    /// </summary>
    public class GestureDetector : MonoBehaviour
    {
        [SerializeField] private GameBalance balance;

        private readonly BalanceConfig fallbackConfig = new BalanceConfig();

        private Vector2 pressPosition;
        private float pressTime;
        private bool swipeFired;

        /// <summary>Pointer went down. Argument: screen position in pixels.</summary>
        public event Action<Vector2> Pressed;

        /// <summary>The current press moved up fast enough to count as a swipe. Fires at most once per press.</summary>
        public event Action SwipedUp;

        /// <summary>Pointer went up. Arguments: screen position in pixels, seconds the press lasted.</summary>
        public event Action<Vector2, float> Released;

        public bool IsPressed { get; private set; }

        public float HeldSeconds => IsPressed ? Time.unscaledTime - pressTime : 0f;

        /// <summary>Movement since the press, in logical px, y pointing up.</summary>
        public Vector2 LogicalDrag { get; private set; }

        private BalanceConfig Config => balance != null ? balance.Values : fallbackConfig;

        void Update()
        {
            Pointer pointer = Pointer.current;
            if (pointer == null) return;

            Vector2 position = pointer.position.ReadValue();

            if (pointer.press.wasPressedThisFrame)
            {
                IsPressed = true;
                pressPosition = position;
                pressTime = Time.unscaledTime;
                swipeFired = false;
                LogicalDrag = Vector2.zero;
                Pressed?.Invoke(position);
            }

            if (!IsPressed) return;

            LogicalDrag = (position - pressPosition) * (Config.canvasHeight / Screen.height);

            if (!swipeFired && LogicalDrag.y > Config.swipeMinDistance && HeldSeconds < Config.swipeMaxTime)
            {
                swipeFired = true;
                SwipedUp?.Invoke();
            }

            if (pointer.press.wasReleasedThisFrame)
            {
                float held = HeldSeconds;
                IsPressed = false;
                Released?.Invoke(position, held);
            }
        }
    }
}
