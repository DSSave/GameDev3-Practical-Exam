using UnityEngine;
using UnityEngine.SceneManagement;

namespace Chisanuphong
{
    public class SceneController : MonoBehaviour
    {
        public void LoadMainMenu()
        {
            SceneManager.LoadScene(GameManager.SceneMainMenu);
        }

        public void LoadStageSelection()
        {
            SceneManager.LoadScene(GameManager.SceneStageSelection);
        }

        public void LoadStage1()
        {
            GameManager.ResetGameSession(GameManager.SceneStage1);
            SceneManager.LoadScene(GameManager.SceneStage1);
        }

        public void LoadStage2()
        {
            GameManager.ResetGameSession(GameManager.SceneStage2);
            SceneManager.LoadScene(GameManager.SceneStage2);
        }

        public void LoadOptions()
        {
            SceneManager.LoadScene(GameManager.SceneOptions);
        }

        public void LoadCredits()
        {
            SceneManager.LoadScene(GameManager.SceneCredits);
        }

        public void LoadWin()
        {
            SceneManager.LoadScene(GameManager.SceneWin);
        }

        public void LoadGameOver()
        {
            SceneManager.LoadScene(GameManager.SceneGameOver);
        }

        public void TryAgain()
        {
            string targetStage = string.IsNullOrEmpty(GameManager.LastPlayedStage)
                ? GameManager.SceneStage1
                : GameManager.LastPlayedStage;

            GameManager.ResetGameSession(targetStage);
            SceneManager.LoadScene(targetStage);
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
