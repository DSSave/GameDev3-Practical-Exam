using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Chisanuphong
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Scene Controller")]
        [SerializeField] private SceneController sceneController;

        [Header("Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button exitButton;

        [Header("Student Info Text")]
        [SerializeField] private TextMeshProUGUI headerTitleText;
        [SerializeField] private TextMeshProUGUI studentInfoText;

        private void Awake()
        {
            if (sceneController == null)
            {
                sceneController = gameObject.AddComponent<SceneController>();
            }

            if (startButton != null)
                startButton.onClick.AddListener(sceneController.LoadStageSelection);

            if (optionsButton != null)
                optionsButton.onClick.AddListener(sceneController.LoadOptions);

            if (creditsButton != null)
                creditsButton.onClick.AddListener(sceneController.LoadCredits);

            if (exitButton != null)
                exitButton.onClick.AddListener(sceneController.QuitGame);

            if (studentInfoText != null)
            {
                studentInfoText.text = "Student Code: 672110088\nName: Chisanuphong Khiankrachang";
            }
        }
    }
}
