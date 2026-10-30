using System;
using System.IO;
using LanternPair.Data;
using UnityEngine;

namespace LanternPair.Manager
{
    /// <summary>
    /// Session-wide owner of <see cref="SaveData"/>: loads it from JSON in persistentDataPath at start-up
    /// and writes it back when the app is paused or quit, or when <see cref="Save"/> is called.
    /// </summary>
    public class SaveDataManager : MonoBehaviour
    {
        private const string FileName = "save.json";

        public static SaveDataManager Instance { get; private set; }

        public SaveData Data { get; private set; }

        private string savePath;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            savePath = Path.Combine(Application.persistentDataPath, FileName);
            Load();
        }

        public void Load()
        {
            Data = null;

            if (File.Exists(savePath))
            {
                try
                {
                    Data = JsonUtility.FromJson<SaveData>(File.ReadAllText(savePath));
                }
                catch (Exception e)
                {
                    Debug.LogError($"SaveDataManager: could not read {savePath}, starting a new save.\n{e.Message}");
                }
            }

            Data ??= new SaveData();
            Data.EnsureInitialized();
        }

        public void Save()
        {
            try
            {
                File.WriteAllText(savePath, JsonUtility.ToJson(Data));
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveDataManager: could not write {savePath}.\n{e.Message}");
            }
        }

        void OnApplicationPause(bool paused)
        {
            if (paused) Save();
        }

        void OnApplicationQuit()
        {
            Save();
        }
    }
}
