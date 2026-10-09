using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Chisanuphong
{
    public class OptionsUI : MonoBehaviour
    {
        [Header("Scene Controller")]
        [SerializeField] private SceneController sceneController;

        [Header("Sliders")]
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;

        [Header("Percentage Value Texts")]
        [SerializeField] private TextMeshProUGUI bgmValueText;
        [SerializeField] private TextMeshProUGUI sfxValueText;

        [Header("Buttons")]
        [SerializeField] private Button backButton;

        private void Start()
        {
            if (sceneController == null)
            {
                sceneController = gameObject.AddComponent<SceneController>();
            }

            float savedBgm = PlayerPrefs.GetFloat(GameManager.BGMVolumeKey, 0.75f);
            float savedSfx = PlayerPrefs.GetFloat(GameManager.SFXVolumeKey, 0.75f);

            if (bgmSlider != null)
            {
                bgmSlider.value = savedBgm;
                bgmSlider.onValueChanged.AddListener(OnBGMChanged);
                UpdateBGMText(savedBgm);
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = savedSfx;
                sfxSlider.onValueChanged.AddListener(OnSFXChanged);
                UpdateSFXText(savedSfx);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(sceneController.LoadMainMenu);
            }
        }

        private void OnBGMChanged(float val)
        {
            PlayerPrefs.SetFloat(GameManager.BGMVolumeKey, val);
            PlayerPrefs.Save();
            UpdateBGMText(val);
        }

        private void OnSFXChanged(float val)
        {
            PlayerPrefs.SetFloat(GameManager.SFXVolumeKey, val);
            PlayerPrefs.Save();
            UpdateSFXText(val);
        }

        private void UpdateBGMText(float val)
        {
            if (bgmValueText != null)
                bgmValueText.text = Mathf.RoundToInt(val * 100f) + "%";
        }

        private void UpdateSFXText(float val)
        {
            if (sfxValueText != null)
                sfxValueText.text = Mathf.RoundToInt(val * 100f) + "%";
        }
    }
}
