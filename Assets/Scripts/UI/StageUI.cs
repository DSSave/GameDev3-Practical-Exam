using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using StarterAssets;

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

        [Header("HUD Buttons")]
        [SerializeField] private Button optionsHUDButton;
        [SerializeField] private Button winDebugButton;
        [SerializeField] private Button gameOverDebugButton;
        [SerializeField] private Button backHUDButton;

        [Header("Settings & Conditions")]
        [SerializeField] private string stageName = "Stage 1 - Capsule Player Minigame";
        [SerializeField] private bool runCountdown = true;
        [SerializeField] private int targetWinningScore = 10000;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float fallThresholdY = -6f;

        [Header("In-Scene Overlay Panels")]
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject optionsPanel;

        [Header("Win Panel Elements")]
        [SerializeField] private TextMeshProUGUI winScoreText;
        [SerializeField] private Button winTryAgainButton;
        [SerializeField] private Button winStageSelectButton;
        [SerializeField] private Button winMainMenuButton;

        [Header("Game Over Panel Elements")]
        [SerializeField] private TextMeshProUGUI gameOverScoreText;
        [SerializeField] private Button gameOverTryAgainButton;
        [SerializeField] private Button gameOverStageSelectButton;
        [SerializeField] private Button gameOverMainMenuButton;

        [Header("Options Panel Elements")]
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private TextMeshProUGUI bgmValueText;
        [SerializeField] private TextMeshProUGUI sfxValueText;
        [SerializeField] private Button optionsResumeButton;
        [SerializeField] private Button optionsMainMenuButton;

        private StarterAssetsInputs playerInputs;
        private bool isGameEnding = false;
        private bool isPaused = false;

        private void Awake()
        {
            Time.timeScale = 1f;
        }

        private void Start()
        {
            if (sceneController == null)
            {
                sceneController = gameObject.AddComponent<SceneController>();
            }

            // Reset score and time when starting the stage
            GameManager.ResetGameSession(stageName);

            // Find player and inputs if not assigned
            LocatePlayer();

            // Lock cursor for gameplay
            SetCursorLocked(true);

            // Close all overlay panels initially
            if (winPanel != null) winPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (optionsPanel != null) optionsPanel.SetActive(false);

            if (stageTitleText != null)
                stageTitleText.text = stageName;

            UpdateHUD();
            SetupButtonListeners();
            SetupOptionsSliders();
        }

        private void LocatePlayer()
        {
            if (playerTransform == null)
            {
                GameObject p = GameObject.FindWithTag("Player");
                if (p != null) playerTransform = p.transform;
            }

            if (playerTransform != null)
            {
                playerInputs = playerTransform.GetComponent<StarterAssetsInputs>() 
                    ?? playerTransform.GetComponentInParent<StarterAssetsInputs>();
            }
        }

        private void SetupButtonListeners()
        {
            // HUD Option button
            if (optionsHUDButton != null)
                optionsHUDButton.onClick.AddListener(OpenOptions);

            // Debug/Test buttons from Fig 1
            if (winDebugButton != null)
            {
                winDebugButton.onClick.AddListener(() =>
                {
                    if (isGameEnding) return;
                    GameManager.CurrentScore = Mathf.Max(GameManager.CurrentScore, 10500);
                    TriggerWin();
                });
            }

            if (gameOverDebugButton != null)
            {
                gameOverDebugButton.onClick.AddListener(() =>
                {
                    if (isGameEnding) return;
                    TriggerGameOver();
                });
            }

            if (backHUDButton != null)
            {
                backHUDButton.onClick.AddListener(() =>
                {
                    Time.timeScale = 1f;
                    SetCursorLocked(false);
                    sceneController.LoadStageSelection();
                });
            }

            // Win Panel buttons
            if (winTryAgainButton != null)
                winTryAgainButton.onClick.AddListener(RestartStage);
            if (winStageSelectButton != null)
                winStageSelectButton.onClick.AddListener(GoToStageSelection);
            if (winMainMenuButton != null)
                winMainMenuButton.onClick.AddListener(GoToMainMenu);

            // Game Over Panel buttons
            if (gameOverTryAgainButton != null)
                gameOverTryAgainButton.onClick.AddListener(RestartStage);
            if (gameOverStageSelectButton != null)
                gameOverStageSelectButton.onClick.AddListener(GoToStageSelection);
            if (gameOverMainMenuButton != null)
                gameOverMainMenuButton.onClick.AddListener(GoToMainMenu);

            // Options Panel buttons
            if (optionsResumeButton != null)
                optionsResumeButton.onClick.AddListener(CloseOptions);
            if (optionsMainMenuButton != null)
                optionsMainMenuButton.onClick.AddListener(GoToMainMenu);
        }

        private void SetupOptionsSliders()
        {
            float savedBgm = PlayerPrefs.GetFloat(GameManager.BGMVolumeKey, 0.75f);
            float savedSfx = PlayerPrefs.GetFloat(GameManager.SFXVolumeKey, 0.75f);

            if (bgmSlider != null)
            {
                bgmSlider.value = savedBgm;
                bgmSlider.onValueChanged.AddListener((val) =>
                {
                    PlayerPrefs.SetFloat(GameManager.BGMVolumeKey, val);
                    PlayerPrefs.Save();
                    if (bgmValueText != null) bgmValueText.text = $"{Mathf.RoundToInt(val * 100f)}%";
                });
                if (bgmValueText != null) bgmValueText.text = $"{Mathf.RoundToInt(savedBgm * 100f)}%";
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = savedSfx;
                sfxSlider.onValueChanged.AddListener((val) =>
                {
                    PlayerPrefs.SetFloat(GameManager.SFXVolumeKey, val);
                    PlayerPrefs.Save();
                    if (sfxValueText != null) sfxValueText.text = $"{Mathf.RoundToInt(val * 100f)}%";
                });
                if (sfxValueText != null) sfxValueText.text = $"{Mathf.RoundToInt(savedSfx * 100f)}%";
            }
        }

        private void Update()
        {
            if (isGameEnding || isPaused) return;

            // 1. Fall detection (Losing Condition 1)
            if (playerTransform != null && playerTransform.position.y < fallThresholdY)
            {
                TriggerGameOver();
                return;
            }

            // 2. Countdown timer
            if (runCountdown)
            {
                if (GameManager.RemainingTime > 0f)
                {
                    GameManager.RemainingTime -= Time.deltaTime;
                    if (GameManager.RemainingTime < 0f)
                        GameManager.RemainingTime = 0f;
                }

                // Check time expiration (Both regular ticking AND negative item collection!)
                if (GameManager.RemainingTime <= 0f)
                {
                    GameManager.RemainingTime = 0f;
                    if (GameManager.CurrentScore > targetWinningScore)
                        TriggerWin();
                    else
                        TriggerGameOver();
                    return;
                }
            }

            UpdateHUD();
        }

        public void TriggerWin()
        {
            if (isGameEnding) return;
            isGameEnding = true;

            Time.timeScale = 0f;
            SetCursorLocked(false);

            if (winScoreText != null)
                winScoreText.text = $"Score : {GameManager.CurrentScore:D5}";

            if (winPanel != null)
            {
                winPanel.SetActive(true);
            }
            else
            {
                sceneController.LoadWin();
            }
        }

        public void TriggerGameOver()
        {
            if (isGameEnding) return;
            isGameEnding = true;

            Time.timeScale = 0f;
            SetCursorLocked(false);

            if (gameOverScoreText != null)
                gameOverScoreText.text = $"Score : {GameManager.CurrentScore:D5}";

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
            else
            {
                sceneController.LoadGameOver();
            }
        }

        public void OpenOptions()
        {
            if (isGameEnding) return;
            isPaused = true;
            Time.timeScale = 0f;
            SetCursorLocked(false);

            if (optionsPanel != null)
                optionsPanel.SetActive(true);
        }

        public void CloseOptions()
        {
            if (isGameEnding) return;
            isPaused = false;
            Time.timeScale = 1f;
            SetCursorLocked(true);

            if (optionsPanel != null)
                optionsPanel.SetActive(false);
        }

        private void RestartStage()
        {
            Time.timeScale = 1f;
            SetCursorLocked(true);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void GoToStageSelection()
        {
            Time.timeScale = 1f;
            SetCursorLocked(false);
            SceneManager.LoadScene(GameManager.SceneStageSelection);
        }

        private void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SetCursorLocked(false);
            SceneManager.LoadScene(GameManager.SceneMainMenu);
        }

        private void SetCursorLocked(bool locked)
        {
            if (playerInputs == null) LocatePlayer();

            if (playerInputs != null)
            {
                playerInputs.cursorLocked = locked;
                playerInputs.cursorInputForLook = locked;
            }

            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        public void UpdateHUD()
        {
            if (timerText != null)
            {
                int seconds = Mathf.CeilToInt(GameManager.RemainingTime);
                timerText.text = $"Time : {seconds}s";
                timerText.color = seconds <= 10 ? new Color(1f, 0.35f, 0.35f) : new Color(1f, 0.84f, 0f);
            }

            if (scoreText != null)
            {
                scoreText.text = $"Score : {GameManager.CurrentScore:D5} / {targetWinningScore}";
                scoreText.color = GameManager.CurrentScore > targetWinningScore ? new Color(0.35f, 1f, 0.5f) : Color.white;
            }
        }
    }
}
