
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GolfMenuController : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject welcomePanel;
    public GameObject gameSelectionPanel;
    public GameObject matchDurationPanel;
    public GameObject golfInstructionsPanel;

    [Header("Game Buttons")]
    public Button golfButton;
    public Button airHockeyButton;
    public Button tennisButton;
    public Button goalKeepingButton;
    public Button chestPassButton;
    public Button gameContinue;

    [Header("Duration Buttons")]
    public Button shortButton;
    public Button mediumButton;
    public Button longButton;
    public Button continueButton;

    // Store the selected game duration.
    public static int SelectedDurationSeconds { get; private set; }

    // Store the scene that the player selected.
    private string currentGame = null;

    private void Start()
    {
        SelectedDurationSeconds = 0;

        UpdateDurationButtons();
        ShowWelcome();
    }

    public void ShowWelcome()
    {
        ShowPanel(welcomePanel);
    }

    public void ShowGameSelection()
    {
        ShowPanel(gameSelectionPanel);
    }

    public void SelectGame(string game)
    {
        // Open the new driving range for Golf.
        if (game == "Golf")
        {
            currentGame = "GolfDrivingRange";
        }
        else
        {
            currentGame = game;
        }
    }

    public void ShowMatchDuration()
    {
        if (string.IsNullOrEmpty(currentGame))
        {
            return;
        }

        ShowPanel(matchDurationPanel);
        UpdateDurationButtons();
    }

    public void SelectShort()
    {
        SelectDuration(60);
    }

    public void SelectMedium()
    {
        SelectDuration(120);
    }

    public void SelectLong()
    {
        SelectDuration(180);
    }

    private void SelectDuration(int seconds)
    {
        SelectedDurationSeconds = seconds;
        UpdateDurationButtons();
    }

    private void UpdateDurationButtons()
    {
        // Disable the selected duration button.
        if (shortButton != null)
        {
            shortButton.interactable =
                SelectedDurationSeconds != 60;
        }

        if (mediumButton != null)
        {
            mediumButton.interactable =
                SelectedDurationSeconds != 120;
        }

        if (longButton != null)
        {
            longButton.interactable =
                SelectedDurationSeconds != 180;
        }

        // Continue only after choosing a duration.
        if (continueButton != null)
        {
            continueButton.interactable =
                SelectedDurationSeconds > 0;
        }
    }

    public void ShowGolfInstructions()
    {
        if (SelectedDurationSeconds <= 0)
        {
            return;
        }

        ShowPanel(golfInstructionsPanel);
    }

    public void StartGame()
    {
        if (string.IsNullOrEmpty(currentGame))
        {
            Debug.LogWarning("No game has been selected.");
            return;
        }

        if (SelectedDurationSeconds <= 0)
        {
            Debug.LogWarning("Please select a duration.");
            return;
        }

        // Save the duration for the game scene.
        Data.duration = SelectedDurationSeconds;

        // Open the selected game.
        SceneManager.LoadScene(currentGame);
    }

    private void ShowPanel(GameObject selectedPanel)
    {
        if (welcomePanel != null)
        {
            welcomePanel.SetActive(
                selectedPanel == welcomePanel);
        }

        if (gameSelectionPanel != null)
        {
            gameSelectionPanel.SetActive(
                selectedPanel == gameSelectionPanel);
        }

        if (matchDurationPanel != null)
        {
            matchDurationPanel.SetActive(
                selectedPanel == matchDurationPanel);
        }

        if (golfInstructionsPanel != null)
        {
            golfInstructionsPanel.SetActive(
                selectedPanel == golfInstructionsPanel);
        }
    }
}
