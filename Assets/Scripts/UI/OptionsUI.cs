using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (UnityEngine.SceneManagement.SceneManager.sceneCount > 1 || GameManager.IsSceneOptionsLoaded)
            {
                // Additive mode per Chapter 6
                Time.timeScale = 0f;
                SetupAdditiveMode();
            }
            else
            {
                Time.timeScale = 1f;
            }
        }

        private void SetupAdditiveMode()
        {
            // Per Chapter 6 Section 6.9: Disable extra Camera, AudioListener, and Light in Options scene
            var rootObjects = gameObject.scene.GetRootGameObjects();
            foreach (var root in rootObjects)
            {
                foreach (var cam in root.GetComponentsInChildren<Camera>(true))
                {
                    cam.gameObject.SetActive(false);
                }

                foreach (var al in root.GetComponentsInChildren<AudioListener>(true))
                {
                    al.enabled = false;
                }

                foreach (var lt in root.GetComponentsInChildren<Light>(true))
                {
                    lt.gameObject.SetActive(false);
                }
            }

            // Resolve duplicate EventSystems if one already exists in the calling scene
            var allEventSystems = Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsInactive.Exclude);
            if (allEventSystems.Length > 1)
            {
                foreach (var es in allEventSystems)
                {
                    if (es.gameObject.scene == gameObject.scene)
                    {
                        es.gameObject.SetActive(false);
                    }
                }
            }
        }

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
        }

        private void Update()
        {
            if (IsEscapePressed())
            {
                OnBackClicked();
            }
        }

        public void OnBackClicked()
        {
            if (sceneController != null)
            {
                sceneController.ReturnFromOptions();
            }
            else
            {
                if (GameManager.IsSceneOptionsLoaded || UnityEngine.SceneManagement.SceneManager.sceneCount > 1)
                {
                    GameManager.UnloadOptionsScene(GameManager.SceneOptions);
                }
                else
                {
                    Time.timeScale = 1f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    string targetScene = string.IsNullOrEmpty(GameManager.PreviousSceneForOptions)
                        ? GameManager.SceneMainMenu
                        : GameManager.PreviousSceneForOptions;
                    UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene, UnityEngine.SceneManagement.LoadSceneMode.Single);
                }
            }
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
