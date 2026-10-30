using UnityEngine;

namespace LanternPair.Manager
{
    /// <summary>
    /// Session-wide music and sound effect playback. Follows <see cref="SettingsManager.SoundOn"/>.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            sfxSource.playOnAwake = false;
        }

        void Start()
        {
            if (SettingsManager.Instance != null) SettingsManager.Instance.Changed += ApplySettings;
            ApplySettings();
        }

        void OnDestroy()
        {
            if (SettingsManager.Instance != null) SettingsManager.Instance.Changed -= ApplySettings;
        }

        public void PlayMusic(AudioClip clip)
        {
            if (clip == null || musicSource.clip == clip) return;
            musicSource.clip = clip;
            musicSource.Play();
        }

        public void PlaySfx(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            sfxSource.PlayOneShot(clip, volume);
        }

        private void ApplySettings()
        {
            bool soundOn = SettingsManager.Instance == null || SettingsManager.Instance.SoundOn;
            musicSource.mute = !soundOn;
            sfxSource.mute = !soundOn;
        }
    }
}
