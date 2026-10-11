using UnityEngine;
using UnityEngine.UI;

namespace Chisanuphong
{
    public class StageSelectionUI : MonoBehaviour
    {
        [Header("Scene Controller")]
        [SerializeField] private SceneController sceneController;

        [Header("Buttons (Wired via Inspector On Click ())")]
        [SerializeField] private Button stage1Button;
        [SerializeField] private Button stage2Button;
        [SerializeField] private Button backButton;

        private void Awake()
        {
            if (sceneController == null)
            {
                sceneController = gameObject.AddComponent<SceneController>();
            }
        }
    }
}
