using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Chisanuphong
{
    public class StageUI : MonoBehaviour
    {
        [Header("Scene Controller")]
        [SerializeField] private SceneController sceneController;

        [Header("HUD Labels")]
        [SerializeField] private TextMeshProUGUI stageTitleText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI scoreText;

        [Header("Stage Navigation / Test Buttons (Per Fig 1)")]
        [SerializeField] private Button winButton;
        [SerializeField] private Button gameOverButton;
        [SerializeField] private Button backButton;

        [Header("Settings")]
        [SerializeField] private string stageName = "Stage 1 - Capsule Player Minigame";
        [SerializeField] private bool runCountdown = false;

        private void Start()
        {
            if (sceneController == null)
            {
                sceneController = gameObject.AddComponent<SceneController>();
            }

            if (stageTitleText != null)
                stageTitleText.text = stageName;

            UpdateHUD();

            if (winButton != null)
            {
                winButton.onClick.AddListener(() =>
                {
                    GameManager.CurrentScore = Mathf.Max(GameManager.CurrentScore, 10500);
                    sceneController.LoadWin();
                });
            }

            if (gameOverButton != null)
            {
                gameOverButton.onClick.AddListener(() =>
                {
                    sceneController.LoadGameOver();
                });
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(sceneController.LoadStageSelection);
            }
        }

        private void Update()
        {
            if (runCountdown && GameManager.RemainingTime > 0f)
            {
                GameManager.RemainingTime -= Time.deltaTime;
                if (GameManager.RemainingTime <= 0f)
                {
                    GameManager.RemainingTime = 0f;
                    if (GameManager.CurrentScore >= 10000)
                        sceneController.LoadWin();
                    else
                        sceneController.LoadGameOver();
                }
            }

            UpdateHUD();
        }

        public void UpdateHUD()
        {
            if (timerText != null)
                timerText.text = $"Time : {Mathf.CeilToInt(GameManager.RemainingTime)}s";

            if (scoreText != null)
                scoreText.text = $"Score : {GameManager.CurrentScore:D5}";
        }
    }
}
