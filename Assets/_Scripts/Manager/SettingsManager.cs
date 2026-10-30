using System;
using UnityEngine;

namespace LanternPair.Manager
{
    /// <summary>
    /// Player settings kept in PlayerPrefs: sound on/off (the prototype's mute button) and haptics on/off.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        private const string SoundKey = "settings.sound";
        private const string HapticsKey = "settings.haptics";

        public static SettingsManager Instance { get; private set; }

        /// <summary>Raised after any setting changes.</summary>
        public event Action Changed;

        public bool SoundOn { get; private set; }
        public bool HapticsOn { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            SoundOn = PlayerPrefs.GetInt(SoundKey, 1) == 1;
            HapticsOn = PlayerPrefs.GetInt(HapticsKey, 1) == 1;
        }

        public void SetSound(bool on)
        {
            SoundOn = on;
            Persist(SoundKey, on);
        }

        public void SetHaptics(bool on)
        {
            HapticsOn = on;
            Persist(HapticsKey, on);
        }

        private void Persist(string key, bool on)
        {
            PlayerPrefs.SetInt(key, on ? 1 : 0);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }
    }
}
