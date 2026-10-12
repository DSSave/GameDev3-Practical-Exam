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

        [Header("Sound Manager")]
        [SerializeField] private SoundManager soundManager;

        [Header("Sliders")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;

        [Header("Percentage Value Texts")]
        [SerializeField] private TextMeshProUGUI masterValueText;
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

            if (soundManager == null)
            {
                soundManager = GetComponent<SoundManager>() ?? Object.FindAnyObjectByType<SoundManager>();
            }

            if (masterSlider != null)
            {
                masterSlider.onValueChanged.AddListener(OnMasterChanged);
                UpdateMasterText(masterSlider.value);
            }

            if (bgmSlider != null)
            {
                bgmSlider.onValueChanged.AddListener(OnBGMChanged);
                UpdateBGMText(bgmSlider.value);
            }

            if (sfxSlider != null)
            {
                sfxSlider.onValueChanged.AddListener(OnSFXChanged);
                UpdateSFXText(sfxSlider.value);
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

        public void OnMasterChanged(float val)
        {
            if (soundManager != null)
                soundManager.SetMasterVolume(val);
            UpdateMasterText(val);
        }

        public void OnBGMChanged(float val)
        {
            if (soundManager != null)
                soundManager.SetMusicVolume(val);
            UpdateBGMText(val);
        }

        public void OnSFXChanged(float val)
        {
            if (soundManager != null)
                soundManager.SetSFXVolume(val);
            UpdateSFXText(val);
        }

        private void UpdateMasterText(float val)
        {
            if (masterValueText != null)
            {
                float pct = Mathf.InverseLerp(-80f, 20f, val);
                masterValueText.text = Mathf.RoundToInt(pct * 100f) + "%";
            }
        }

        private void UpdateBGMText(float val)
        {
            if (bgmValueText != null)
            {
                float pct = Mathf.InverseLerp(-80f, 20f, val);
                bgmValueText.text = Mathf.RoundToInt(pct * 100f) + "%";
            }
        }

        private void UpdateSFXText(float val)
        {
            if (sfxValueText != null)
            {
                float pct = Mathf.InverseLerp(-80f, 20f, val);
                sfxValueText.text = Mathf.RoundToInt(pct * 100f) + "%";
            }
        }
    }
}
