using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GolfMenuController : MonoBehaviour
{
    [Header("Menu panels")]
    public GameObject welcomePanel;
    public GameObject matchDurationPanel;
    public GameObject golfInstructionsPanel;

    [Header("Duration buttons")]
    public Button shortButton;
    public Button mediumButton;
    public Button longButton;
    public Button continueButton;

    // Other scripts can read this; only this class can change it.
    public static int SelectedDurationSeconds { get; private set; }

    private void Start()
    {
        SelectedDurationSeconds = 0;
        UpdateDurationButtons();
        ShowWelcome();
    }

    public void StartGame()
    {
        Data.duration = SelectedDurationSeconds;
        SceneManager.LoadScene("GolfGamePlay");
    }
    
    public void ShowWelcome()
    {
        ShowPanel(welcomePanel);
    }

    public void ShowMatchDuration()
    {
        ShowPanel(matchDurationPanel);
        UpdateDurationButtons();
    }

    public void SelectShort()
    {
        SelectDuration(60); // 1 minute
    }

    public void SelectMedium()
    {
        SelectDuration(120); // 2 minutes
    }

    public void SelectLong()
    {
        SelectDuration(180); // 3 minutes
    }

    private void SelectDuration(int seconds)
    {
        SelectedDurationSeconds = seconds;
        UpdateDurationButtons();
    }

    private void UpdateDurationButtons()
    {
        // Disable the selected button; enable the others.
        if (shortButton != null)
        {
            if (SelectedDurationSeconds == 60)
            {
                shortButton.interactable = false;
            }
            else
            {
                shortButton.interactable = true;
            }
        }

        if (mediumButton != null)
        {
            if (SelectedDurationSeconds == 120)
            {
                mediumButton.interactable = false;
            }
            else
            {
                mediumButton.interactable = true;
            }
        }

        if (longButton != null)
        {
            if (SelectedDurationSeconds == 180)
            {
                longButton.interactable = false;
            }
            else
            {
                longButton.interactable = true;
            }
        }

        // Continue is enabled only after choosing a duration.
        if (continueButton != null)
        {
            if (SelectedDurationSeconds > 0)
            {
                continueButton.interactable = true;
            }
            else
            {
                continueButton.interactable = false;
            }
        }
    }

    public void ShowGolfInstructions()
    {
        // Stop here if no duration has been selected.
        if (SelectedDurationSeconds <= 0)
        {
            return;
        }

        ShowPanel(golfInstructionsPanel);
    }

    private void ShowPanel(GameObject selectedPanel)
    {
        if (welcomePanel != null)
        {
            welcomePanel.SetActive(selectedPanel == welcomePanel);
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