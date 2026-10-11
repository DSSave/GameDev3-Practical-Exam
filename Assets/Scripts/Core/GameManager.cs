using UnityEngine;
using UnityEngine.SceneManagement;

namespace Chisanuphong
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public const string SceneMainMenu = "MainMenu";
        public const string SceneStageSelection = "StageSelection";
        public const string SceneStage1 = "Stage1_Capsule";
        public const string SceneStage2 = "Stage2_Humanoid";
        public const string SceneOptions = "Options";
        public const string SceneCredits = "Credits";
        public const string SceneWin = "WinScreen";
        public const string SceneGameOver = "GameOverScreen";

        [Header("Runtime Game State")]
        public static int CurrentScore = 0;
        public static float RemainingTime = 30f;
        public static string LastPlayedStage = SceneStage1;
        public static string PreviousSceneForOptions = SceneMainMenu;

        [Header("Chapter 6: Scene Management Flags")]
        public static bool IsSceneOptionsLoaded = false;
        public static bool IsSceneWinLoaded = false;
        public static bool IsSceneGameOverLoaded = false;

        [Header("Audio Settings Keys")]
        public const string BGMVolumeKey = "BGM_Volume";
        public const string SFXVolumeKey = "SFX_Volume";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += SceneLoadedEventHandler;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= SceneLoadedEventHandler;
        }

        private void SceneLoadedEventHandler(Scene scene, LoadSceneMode mode)
        {
            // Per Chapter 6 Source Code 6.5: If loaded scene is not Options/Win/GameOver, reset flags
            if (scene.name.CompareTo(SceneOptions) != 0)
                IsSceneOptionsLoaded = false;
            if (scene.name.CompareTo(SceneWin) != 0)
                IsSceneWinLoaded = false;
            if (scene.name.CompareTo(SceneGameOver) != 0)
                IsSceneGameOverLoaded = false;
        }

        public static void ResetGameSession(string stageName = null)
        {
            CurrentScore = 0;
            RemainingTime = 30f;
            
            // Normalize stage name to ensure it matches the actual scene in Build Settings
            if (string.IsNullOrEmpty(stageName) || stageName.Contains("Stage 1") || stageName.Contains("Stage1") || stageName.Contains("Capsule"))
            {
                LastPlayedStage = SceneStage1;
            }
            else if (stageName.Contains("Stage 2") || stageName.Contains("Stage2") || stageName.Contains("Humanoid"))
            {
                LastPlayedStage = SceneStage2;
            }
            else
            {
                LastPlayedStage = SceneStage1;
            }

            IsSceneOptionsLoaded = false;
            IsSceneWinLoaded = false;
            IsSceneGameOverLoaded = false;
        }

        public static void AddScore(int amount)
        {
            CurrentScore = Mathf.Max(0, CurrentScore + amount);
        }

        public static void AddTime(float seconds)
        {
            RemainingTime = Mathf.Max(0f, RemainingTime + seconds);
        }

        // --- Chapter 6 Section 6.2.1 / Source Code 6.5 Additive Scene Methods ---
        public static void LoadOptionsScene(string optionSceneName = SceneOptions)
        {
            if (!IsSceneOptionsLoaded)
            {
                IsSceneOptionsLoaded = true;
                PreviousSceneForOptions = SceneManager.GetActiveScene().name;
                SceneManager.LoadScene(optionSceneName, LoadSceneMode.Additive);
            }
        }

        public static void UnloadOptionsScene(string optionSceneName = SceneOptions)
        {
            if (IsSceneOptionsLoaded)
            {
                SceneManager.UnloadSceneAsync(optionSceneName);
                IsSceneOptionsLoaded = false;
            }
        }

        public static void LoadWinScene()
        {
            if (!IsSceneWinLoaded)
            {
                IsSceneWinLoaded = true;
                Time.timeScale = 0f;
                SceneManager.LoadScene(SceneWin, LoadSceneMode.Additive);
            }
        }

        public static void LoadGameOverScene()
        {
            if (!IsSceneGameOverLoaded)
            {
                IsSceneGameOverLoaded = true;
                Time.timeScale = 0f;
                SceneManager.LoadScene(SceneGameOver, LoadSceneMode.Additive);
            }
        }
    }
}
