
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GolfMenuController : MonoBehaviour
{   // menu screens
    [Header("Menu Panels")]
    public GameObject welcomePanel;
    public GameObject gameSelectionPanel;
    public GameObject matchDurationPanel;
    public GameObject golfInstructionsPanel;

    // buttons to choose a game and then continue to next page
    [Header("Game Buttons")]
    public Button golfButton;
    public Button airHockeyButton;
    public Button tennisButton;
    public Button goalKeepingButton;
    public Button chestPassButton;
    public Button gameContinue;

    // buttons to choose the match duration
    [Header("Duration Buttons")]
    public Button shortButton;
    public Button mediumButton;
    public Button longButton;
    public Button continueButton;

    // store the selected game duration
    public static int SelectedDurationSeconds { get; private set; }

    // store the scene that the player selected
    private string currentGame = null;

    private void Start()
    {   // reset all menu seelctions
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
        // open the Golf Driving range
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
    {   // a game to be selected before duration is chosen
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
        // save the player's duration choice
        SelectedDurationSeconds = seconds;
        UpdateDurationButtons();
    }

    private void UpdateDurationButtons()
    {
        // disable the selected duration button
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

        // continue only after choosing a duration
        if (continueButton != null)
        {
            continueButton.interactable =
                SelectedDurationSeconds > 0;
        }
    }

    public void ShowGolfInstructions()
    {
        // instructions will show only after a game is chosen  and duration is selected
        if (SelectedDurationSeconds <= 0)
        {
            return;
        }

        ShowPanel(golfInstructionsPanel);
    }

    public void StartGame()
    {
        // check that a game was selected
        if (string.IsNullOrEmpty(currentGame))
        {
            Debug.LogWarning("No game has been selected.");
            return;
        }
        // check that a duration was selected
        if (SelectedDurationSeconds <= 0)
        {
            Debug.LogWarning("Please select a duration.");
            return;
        }

        // save the duration for the game scene
        Data.duration = SelectedDurationSeconds;

        // open the selected game
        SceneManager.LoadScene(currentGame);
    }

    private void ShowPanel(GameObject selectedPanel)
    {
        //  only the selected menu panel will be active, all others will be hidden
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
