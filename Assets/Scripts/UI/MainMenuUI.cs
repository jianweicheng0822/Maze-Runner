using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MazeRunner.Core;

namespace MazeRunner.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private string gameSceneName = "TestScene";

        void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (settingsPanel != null)
                settingsPanel.SetActive(false);

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = 0.7f;
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = 0.7f;
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayMenuMusic();
        }

        public void OnSinglePlayer()
        {
            SceneManager.LoadScene(gameSceneName);
        }

        public void OnMultiplayer()
        {
            // Coming Soon - button is non-interactable
        }

        public void OnSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(!settingsPanel.activeSelf);
        }

        public void OnCloseSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }

        public void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnMusicVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.SetMusicVolume(value);
        }

        private void OnSFXVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.SetSFXVolume(value);
        }
    }
}
