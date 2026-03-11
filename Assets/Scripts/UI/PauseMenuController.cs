using UnityEngine;

namespace Evetero
{
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject settingsPanel;

        private bool _isPaused;

        private void Start()
        {
            if (pausePanel != null)
                pausePanel.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                TogglePause();
        }

        public void TogglePause()
        {
            if (_isPaused)
                Resume();
            else
                Pause();
        }

        private void Pause()
        {
            _isPaused = true;
            Time.timeScale = 0f;
            if (pausePanel != null)
                pausePanel.SetActive(true);
        }

        public void Resume()
        {
            _isPaused = false;
            Time.timeScale = 1f;
            if (pausePanel != null)
                pausePanel.SetActive(false);
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }

        public void OpenSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(true);
        }

        public void QuitGame()
        {
            Time.timeScale = 1f;
            Application.Quit();
        }
    }
}
