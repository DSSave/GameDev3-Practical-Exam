using UnityEngine;
using UnityEngine.UI;

namespace Chisanuphong
{
    public class StageSelectionUI : MonoBehaviour
    {
        [Header("Scene Controller")]
        [SerializeField] private SceneController sceneController;

        [Header("Buttons")]
        [SerializeField] private Button stage1Button;
        [SerializeField] private Button stage2Button;
        [SerializeField] private Button backButton;

        private void Awake()
        {
            if (sceneController == null)
            {
                sceneController = gameObject.AddComponent<SceneController>();
            }

            if (stage1Button != null)
                stage1Button.onClick.AddListener(sceneController.LoadStage1);

            if (stage2Button != null)
                stage2Button.onClick.AddListener(sceneController.LoadStage2);

            if (backButton != null)
                backButton.onClick.AddListener(sceneController.LoadMainMenu);
        }
    }
}
