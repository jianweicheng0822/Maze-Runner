using UnityEngine;
using UnityEngine.SceneManagement;

namespace MazeRunner.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private string gameSceneName = "GameScene";

        void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void OnHostGame()
        {
            SceneManager.LoadScene(gameSceneName);
        }

        public void OnJoinGame()
        {
            // Network join logic will go here (Phase 5)
            SceneManager.LoadScene(gameSceneName);
        }

        public void OnSettings()
        {
            // Settings panel toggle
        }

        public void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
