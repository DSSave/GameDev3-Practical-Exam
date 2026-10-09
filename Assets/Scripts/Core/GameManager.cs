using UnityEngine;

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

        public static void ResetGameSession(string stageName)
        {
            CurrentScore = 0;
            RemainingTime = 30f;
            LastPlayedStage = stageName;
        }

        public static void AddScore(int amount)
        {
            CurrentScore = Mathf.Max(0, CurrentScore + amount);
        }

        public static void AddTime(float seconds)
        {
            RemainingTime = Mathf.Max(0f, RemainingTime + seconds);
        }
    }
}
