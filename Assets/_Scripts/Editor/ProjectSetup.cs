using System.IO;
using LanternPair.Data;
using LanternPair.Gameplay;
using LanternPair.Input;
using LanternPair.Manager;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LanternPair.Editor
{
    /// <summary>
    /// One-click project setup: player settings, the balance asset, the managers prefab,
    /// the Splash and Main scenes and the build scene list. Creates only what is missing, so it is safe to run again.
    /// Menu: Lantern Pair > Setup Project. Batch mode: -executeMethod LanternPair.Editor.ProjectSetup.Run
    /// </summary>
    public static class ProjectSetup
    {
        private const string BundleId = "co.leap.apps.lanternpair";
        private const string SplashScenePath = "Assets/_Scenes/Splash.unity";
        private const string MainScenePath = "Assets/_Scenes/Main.unity";
        private const string BalancePath = "Assets/_Data/GameBalance.asset";
        private const string ManagersPrefabPath = "Assets/_Prefabs/Managers/Managers.prefab";

        // 1 world unit = 100 logical px, so the 844 px tall portrait canvas is 8.44 units high.
        private const float OrthographicSize = 8.44f / 2f;
        private static readonly Color NightBlue = new Color32(0x1B, 0x2A, 0x49, 0xFF);

        [MenuItem("Lantern Pair/Setup Project")]
        public static void Run()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            ApplyPlayerSettings();
            GameBalance balance = EnsureBalanceAsset();
            GameObject managersPrefab = EnsureManagersPrefab();
            EnsureSplashScene(managersPrefab);
            EnsureMainScene(balance);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(SplashScenePath, true),
                new EditorBuildSettingsScene(MainScenePath, true),
            };

            AssetDatabase.SaveAssets();
            Debug.Log("ProjectSetup: done. Open Assets/_Scenes/Splash.unity and press Play.");
        }

        private static void ApplyPlayerSettings()
        {
            PlayerSettings.companyName = "Leap";
            PlayerSettings.productName = "Lantern Pair";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, BundleId);
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, BundleId);
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
            PlayerSettings.iOS.targetOSVersionString = "15.0";
        }

        private static GameBalance EnsureBalanceAsset()
        {
            var balance = AssetDatabase.LoadAssetAtPath<GameBalance>(BalancePath);
            if (balance != null) return balance;

            balance = ScriptableObject.CreateInstance<GameBalance>();
            AssetDatabase.CreateAsset(balance, BalancePath);
            return balance;
        }

        private static GameObject EnsureManagersPrefab()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ManagersPrefabPath);
            if (prefab != null) return prefab;

            var managers = new GameObject("Managers", typeof(SaveDataManager), typeof(SettingsManager), typeof(AudioManager));
            prefab = PrefabUtility.SaveAsPrefabAsset(managers, ManagersPrefabPath);
            Object.DestroyImmediate(managers);
            return prefab;
        }

        private static void EnsureSplashScene(GameObject managersPrefab)
        {
            if (File.Exists(SplashScenePath)) return;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera();

            var splash = new GameObject("Splash", typeof(SplashManager)).GetComponent<SplashManager>();
            var serialized = new SerializedObject(splash);
            SerializedProperty prefabs = serialized.FindProperty("managerPrefabs");
            prefabs.arraySize = 1;
            prefabs.GetArrayElementAtIndex(0).objectReferenceValue = managersPrefab;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, SplashScenePath);
        }

        private static void EnsureMainScene(GameBalance balance)
        {
            if (File.Exists(MainScenePath)) return;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Camera camera = CreateCamera();

            var gestures = new GameObject("Input", typeof(GestureDetector)).GetComponent<GestureDetector>();
            SetReference(gestures, "balance", balance);

            new GameObject("Views", typeof(ViewManager));

            var game = new GameObject("Game", typeof(GameController)).GetComponent<GameController>();
            SetReference(game, "balance", balance);
            SetReference(game, "gestures", gestures);
            SetReference(game, "worldCamera", camera);

            EditorSceneManager.SaveScene(scene, MainScenePath);
        }

        private static Camera CreateCamera()
        {
            var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)) { tag = "MainCamera" };
            go.transform.position = new Vector3(0f, 0f, -10f);

            var camera = go.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = OrthographicSize;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = NightBlue;
            return camera;
        }

        private static void SetReference(Object target, string fieldName, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(fieldName).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
