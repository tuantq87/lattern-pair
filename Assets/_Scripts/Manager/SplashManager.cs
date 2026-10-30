using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LanternPair.Manager
{
    /// <summary>
    /// First scene. Creates the session-wide managers from their prefabs, then loads the main scene.
    /// SDK start-up (Firebase, ATT, ads) slots in here later, as in leap-wordlody's SplashManager.
    /// </summary>
    public class SplashManager : MonoBehaviour
    {
        [Header("Manager Prefabs")]
        [SerializeField] private GameObject[] managerPrefabs;

        [Header("Settings")]
        [SerializeField] private string mainSceneName = "Main";
        [SerializeField] private float minimumSplashSeconds = 0.8f;

        void Awake()
        {
            Application.targetFrameRate = 60;
        }

        IEnumerator Start()
        {
            float startedAt = Time.realtimeSinceStartup;

            // Each manager marks itself DontDestroyOnLoad in its own Awake.
            foreach (GameObject prefab in managerPrefabs)
            {
                if (prefab != null) Instantiate(prefab);
            }

            AsyncOperation load = SceneManager.LoadSceneAsync(mainSceneName);
            load.allowSceneActivation = false;

            while (load.progress < 0.9f || Time.realtimeSinceStartup - startedAt < minimumSplashSeconds)
            {
                yield return null;
            }

            load.allowSceneActivation = true;
        }
    }
}
