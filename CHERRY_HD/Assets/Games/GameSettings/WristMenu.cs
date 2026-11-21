using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class WristMenu : MonoBehaviour
{
    // [Header("Pause")]
    // [SerializeField] private Button pauseButton;
    // [SerializeField] private TMP_Text pauseButtonText;
    // [SerializeField] private GameObject pausePanel;

    // private bool isPaused = false;

    [Header("Settings")]
    [SerializeField] private PanelPopUp settingsPanel;
    [SerializeField] private TMP_Text settingsButtonText;

    private bool isSettingsVisible = false;

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

        // pauseButtonText.text = "Pause";
        restartButtonText.text = "Restart";
        menuButtonText.text = "Menu";
    }

    // PAUSE / RESUME (removed due to issues)
    // public void TogglePause()
    // {
    //     isPaused = !isPaused;

    //     if (isPaused)
    //     {
    //         Time.timeScale = 0.01f;
    //         pauseButtonText.text = "Resume";
    //         pausePanel.SetActive(true);
    //     }
    //     else
    //     {
    //         Time.timeScale = 1f;
    //         pauseButtonText.text = "Pause";
    //         pausePanel.SetActive(false);
    //     }
    // }

//Show or hide the settings panel
    public void ToggleSettings()
{
    isSettingsVisible = !isSettingsVisible;

    if (isSettingsVisible)
    {
        Debug.Log("Showing settings panel");
        settingsPanel.ShowPanel();
        settingsButtonText.text = "Hide Settings";
    }
    else
    {
        settingsPanel.HidePanel();
        settingsButtonText.text = "Settings";
    }
}

    // RESTART the game with confirmation
    public void RestartGame()
    {
        if (!restartPending)
        {
            restartPending = true;
            restartButtonText.text = "Confirm Restart?";
            StartCoroutine(ResetRestartConfirmation());
            return;
        }

        // Reset time scale before reloading
        Time.timeScale = 1f;

        // Reload the current scene
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // Reset the restart confirmation after a delay
    private IEnumerator ResetRestartConfirmation()
    {
        yield return new WaitForSeconds(5f);

        restartPending = false;
        restartButtonText.text = "Restart";
    }

    // MENU
    public void GoToMenu()
    {
        // If the menu button is pressed for the first time, ask for confirmation
        if (!menuPending)
        {
            menuPending = true;
            menuButtonText.text = "Confirm Menu?";

            StartCoroutine(ResetMenuConfirmation());
            return;
        }

        // Reset time scale before leaving
        Time.timeScale = 1f;

        SceneManager.LoadScene("MenuLobby");
    }
    // Reset the menu confirmation after a delay
    private IEnumerator ResetMenuConfirmation()
    {
        yield return new WaitForSeconds(5f);

        menuPending = false;
        menuButtonText.text = "Menu";
    }
}