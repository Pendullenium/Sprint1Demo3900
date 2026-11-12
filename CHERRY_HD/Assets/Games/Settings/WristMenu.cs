using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class WristMenu : MonoBehaviour
{
    [Header("Pause")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private TMP_Text pauseButtonText;
    [SerializeField] private GameObject pausePanel;

    private bool isPaused = false;

    [Header("Settings")]
    [SerializeField] private PanelPopUp settingsPanel;

    [Header("Restart")]
    [SerializeField] private Button restartButton;
    [SerializeField] private TMP_Text restartButtonText;

    private bool restartPending = false;

    [Header("Menu")]
    [SerializeField] private Button menuButton;
    [SerializeField] private TMP_Text menuButtonText;

    private bool menuPending = false;

    private void Start()
    {
        Time.timeScale = 1f;

        pauseButtonText.text = "Pause";
        restartButtonText.text = "Restart";
        menuButtonText.text = "Menu";
    }

    // PAUSE / RESUME
    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0.01f;
            pauseButtonText.text = "Resume";
            pausePanel.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            pauseButtonText.text = "Pause";
            pausePanel.SetActive(false);
        }
    }

    // SETTINGS
    public void ShowSettings()
    {
        Debug.Log("Show Settings");
        settingsPanel.ShowPanel();
    }

    // RESTART
    public void RestartGame()
    {
        if (!restartPending)
        {
            restartPending = true;
            restartButtonText.text = "Confirm Restart?";
            return;
        }

        // Reset time scale before reloading
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // MENU
    public void GoToMenu()
    {
        if (!menuPending)
        {
            menuPending = true;
            menuButtonText.text = "Confirm Menu?";
            return;
        }

        // Reset time scale before leaving
        Time.timeScale = 1f;

        SceneManager.LoadScene("MenuLobby");
    }
}