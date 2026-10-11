using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using StarterAssets;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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

        private StarterAssetsInputs playerInputs;
        private bool isGameEnding = false;
        private bool isPaused = false;

        private void Awake()
        {
            Time.timeScale = 1f;
        }

        private void OnEnable()
        {
            SceneManager.sceneUnloaded += OnSceneUnloadEventHandler;
        }

        private void OnDisable()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloadEventHandler;
        }

        private void OnSceneUnloadEventHandler(Scene scene)
        {
            // Per Chapter 6: When Options scene is unloaded, resume gameplay seamlessly
            if (scene.name == GameManager.SceneOptions)
            {
                isPaused = false;
                Time.timeScale = 1f;
                SetCursorLocked(true);
            }
        }

        private void Start()
        {
            if (sceneController == null)
            {
                sceneController = gameObject.AddComponent<SceneController>();
            }

            // Reset score and time when starting the stage freshly (using actual scene name)
            GameManager.ResetGameSession(gameObject.scene.name);

            // Find player and inputs if not assigned
            LocatePlayer();

            // Lock cursor for gameplay
            SetCursorLocked(true);

            if (stageTitleText != null)
                stageTitleText.text = stageName;

            UpdateHUD();
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

        private void Update()
        {
            if (isGameEnding) return;

            // If Options is currently open, skip gameplay input and timer
            if (GameManager.IsSceneOptionsLoaded || isPaused) return;

            // Esc key navigates to Scene Option
            if (IsEscapePressed())
            {
                GoToOptionsScene();
                return;
            }

            // Alt / Tab toggles cursor lock so HUD buttons can be clicked
            UpdateCursorToggle();

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
                    if (GameManager.CurrentScore >= targetWinningScore)
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

            if (GameManager.CurrentScore < targetWinningScore)
                GameManager.CurrentScore = targetWinningScore + 500;

            Time.timeScale = 0f;
            SetCursorLocked(false);

            // Chapter 6: Load WinScreen scene additively on top of the stage
            GameManager.LoadWinScene();
        }

        public void TriggerGameOver()
        {
            if (isGameEnding) return;
            isGameEnding = true;

            Time.timeScale = 0f;
            SetCursorLocked(false);

            // Chapter 6: Load GameOverScreen scene additively on top of the stage
            GameManager.LoadGameOverScene();
        }

        public void GoToOptionsScene()
        {
            if (isGameEnding || GameManager.IsSceneOptionsLoaded) return;
            isPaused = true;
            Time.timeScale = 0f;
            SetCursorLocked(false);
            GameManager.LoadOptionsScene(GameManager.SceneOptions);
        }

        private bool IsEscapePressed()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                return true;
#endif
            try
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                    return true;
            }
            catch { }
            return false;
        }

        private void UpdateCursorToggle()
        {
            bool togglePressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && (Keyboard.current.leftAltKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame))
                togglePressed = true;
#endif
            try
            {
                if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.Tab))
                    togglePressed = true;
            }
            catch { }

            if (togglePressed)
            {
                bool newLocked = Cursor.lockState != CursorLockMode.Locked;
                SetCursorLocked(newLocked);
            }
        }

        public void SetCursorLocked(bool locked)
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
                scoreText.color = GameManager.CurrentScore >= targetWinningScore ? new Color(0.35f, 1f, 0.5f) : Color.white;
            }
        }
    }
}
