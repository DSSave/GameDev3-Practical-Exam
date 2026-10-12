using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Chisanuphong
{
    public class GameOverUI : MonoBehaviour
    {
        [Header("Scene Controller")]
        [SerializeField] private SceneController sceneController;

        [Header("Score Display")]
        [SerializeField] private TextMeshProUGUI scoreText;

        [Header("Buttons")]
        [SerializeField] private Button tryAgainButton;
        [SerializeField] private Button mainMenuButton;

        private void Awake()
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (UnityEngine.SceneManagement.SceneManager.sceneCount > 1)
            {
                SetupAdditiveMode();
            }
        }

        private void SetupAdditiveMode()
        {
            var rootObjects = gameObject.scene.GetRootGameObjects();
            foreach (var root in rootObjects)
            {
                foreach (var cam in root.GetComponentsInChildren<Camera>(true))
                    cam.gameObject.SetActive(false);
                foreach (var al in root.GetComponentsInChildren<AudioListener>(true))
                    al.enabled = false;
                foreach (var lt in root.GetComponentsInChildren<Light>(true))
                    lt.gameObject.SetActive(false);

                // Ensure Canvas in this additive scene renders on top of stage canvas
                foreach (var canvas in root.GetComponentsInChildren<Canvas>(true))
                {
                    canvas.overrideSorting = true;
                    canvas.sortingOrder = 100;
                }
            }

            var allEventSystems = Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsInactive.Exclude);
            if (allEventSystems.Length > 1)
            {
                foreach (var es in allEventSystems)
                {
                    if (es.gameObject.scene == gameObject.scene)
                        es.gameObject.SetActive(false);
                }
            }
        }

        private void Start()
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (sceneController == null)
            {
                sceneController = gameObject.AddComponent<SceneController>();
            }

            if (scoreText != null)
            {
                scoreText.text = $"Score : {GameManager.CurrentScore:D5}";
            }

            var card = transform.Find("Canvas/GameOverCard") ?? transform.Find("GameOverCard");
            if (card != null)
            {
                var rt = card.GetComponent<RectTransform>();
                if (rt != null) LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            }

            if (tryAgainButton != null && UnityEngine.EventSystems.EventSystem.current != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(tryAgainButton.gameObject);
            }
        }

        public void OnTryAgainClicked()
        {
            if (sceneController == null)
                sceneController = gameObject.AddComponent<SceneController>();

            sceneController.TryAgain();
        }

        public void OnMainMenuClicked()
        {
            if (sceneController == null)
                sceneController = gameObject.AddComponent<SceneController>();

            sceneController.LoadMainMenu();
        }

        private void Update()
        {
            if (Cursor.lockState != CursorLockMode.None || !Cursor.visible)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.rKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
                {
                    OnTryAgainClicked();
                    return;
                }
                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    OnMainMenuClicked();
                    return;
                }
            }
#endif
            try
            {
                if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    OnTryAgainClicked();
                    return;
                }
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    OnMainMenuClicked();
                    return;
                }
            }
            catch { }
        }
    }
}
