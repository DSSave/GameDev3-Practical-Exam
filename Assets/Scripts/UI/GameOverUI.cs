using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

        private void Start()
        {
            Time.timeScale = 1f;
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

            if (tryAgainButton != null)
            {
                tryAgainButton.onClick.AddListener(sceneController.TryAgain);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(sceneController.LoadMainMenu);
            }
        }
    }
}
