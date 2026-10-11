using UnityEngine;
using UnityEngine.SceneManagement;

namespace Chisanuphong
{
    public class SceneController : MonoBehaviour
    {
        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            GameManager.IsSceneWinLoaded = false;
            GameManager.IsSceneGameOverLoaded = false;
            GameManager.IsSceneOptionsLoaded = false;
            SceneManager.LoadScene(GameManager.SceneMainMenu, LoadSceneMode.Single);
        }

        public void LoadStageSelection()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            GameManager.IsSceneWinLoaded = false;
            GameManager.IsSceneGameOverLoaded = false;
            GameManager.IsSceneOptionsLoaded = false;
            SceneManager.LoadScene(GameManager.SceneStageSelection, LoadSceneMode.Single);
        }

        public void LoadStage1()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            GameManager.ResetGameSession(GameManager.SceneStage1);
            SceneManager.LoadScene(GameManager.SceneStage1, LoadSceneMode.Single);
        }

        public void LoadStage2()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            GameManager.ResetGameSession(GameManager.SceneStage2);
            SceneManager.LoadScene(GameManager.SceneStage2, LoadSceneMode.Single);
        }

        public void LoadOptions()
        {
            GameManager.LoadOptionsScene(GameManager.SceneOptions);
        }

        public void ReturnFromOptions()
        {
            if (GameManager.IsSceneOptionsLoaded || SceneManager.sceneCount > 1)
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

                SceneManager.LoadScene(targetScene, LoadSceneMode.Single);
            }
        }

        public void LoadCredits()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene(GameManager.SceneCredits, LoadSceneMode.Single);
        }

        public void LoadWin()
        {
            GameManager.LoadWinScene();
        }

        public void LoadGameOver()
        {
            GameManager.LoadGameOverScene();
        }

        public void TryAgain()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            GameManager.IsSceneWinLoaded = false;
            GameManager.IsSceneGameOverLoaded = false;
            GameManager.IsSceneOptionsLoaded = false;

            string targetStage = GameManager.LastPlayedStage;
            if (string.IsNullOrEmpty(targetStage) || targetStage.Contains("Stage 1") || targetStage.Contains("Stage1") || targetStage.Contains("Capsule"))
            {
                targetStage = GameManager.SceneStage1;
            }
            else if (targetStage.Contains("Stage 2") || targetStage.Contains("Stage2") || targetStage.Contains("Humanoid"))
            {
                targetStage = GameManager.SceneStage2;
            }
            else
            {
                targetStage = GameManager.SceneStage1;
            }

            GameManager.ResetGameSession(targetStage);
            SceneManager.LoadScene(targetStage, LoadSceneMode.Single);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
