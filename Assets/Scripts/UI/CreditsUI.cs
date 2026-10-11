using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Chisanuphong
{
    public class CreditsUI : MonoBehaviour
    {
        [Header("Scene Controller")]
        [SerializeField] private SceneController sceneController;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI courseText;
        [SerializeField] private TextMeshProUGUI studentCodeText;
        [SerializeField] private TextMeshProUGUI studentNameText;
        [SerializeField] private TextMeshProUGUI departmentText;

        [Header("Buttons (Wired via Inspector On Click ())")]
        [SerializeField] private Button backButton;

        private void Awake()
        {
            if (sceneController == null)
            {
                sceneController = gameObject.AddComponent<SceneController>();
            }

            if (courseText != null)
                courseText.text = "Course: 958321 Game Development 3";

            if (studentCodeText != null)
                studentCodeText.text = "Student Code: 672110088";

            if (studentNameText != null)
                studentNameText.text = "Name: Chisanuphong Khiankrachang";

            if (departmentText != null)
                departmentText.text = "Department of Digital Game\nCollege of Arts, Media and Technology\nChiang Mai University";
        }
    }
}
