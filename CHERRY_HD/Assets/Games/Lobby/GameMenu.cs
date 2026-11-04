using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


[System.Serializable]
public class GameDurations
{
    public int shortDuration = 60;
    public int mediumDuration = 180;
    public int longDuration = 300;
}


public class GameMenu : MonoBehaviour
{
    public GameObject welcomePanel;
    public GameObject sportPanel;
    public GameObject durationPanel;
    public GameObject instructionPanelObject;
    public InstructionPanel instructionPanel;

    public Button sportContinueButton;
    public Button durationContinueButton;

     [Header("Game Durations (seconds)")]
    public GameDurations golfDurations;
    public GameDurations tennisDurations;
    public GameDurations airHockeyDurations;
    public GameDurations goalkeepingDurations;
    public GameDurations chestPassDurations;



    [Header("Game Scene Names")]
    public string golfScene = "GolfGamePlay";
    public string airHockeyScene = "AirHockeyPlay";
    public string tennisScene = "TennisPlay";
    public string goalkeepingScene = "GoalkeepingPlay";
    public string chestPassScene = "ChestPassPlay";

    public static int SelectedDurationSeconds { get; private set; }

    public string selectedSport;
    private string selectedScene;

    public UISelectedManager selectionGlow;
    public GameObject[] sportButtons;
    public GameObject[] durationButtons;

    private void Start()
    {
        SelectedDurationSeconds = 0;
        durationContinueButton.interactable = false;
        ShowWelcome();
    }

    private void ShowPanel(GameObject panelToShow)
    {
        welcomePanel.SetActive(false);
        sportPanel.SetActive(false);
        durationPanel.SetActive(false);
        instructionPanelObject.SetActive(false);

        panelToShow.SetActive(true);
    }

    public void ShowWelcome()
    {
        ShowPanel(welcomePanel);
    }

    public void ShowSportMenu()
    {
        selectionGlow.ResetButtons(sportButtons);
        sportContinueButton.interactable = false;
        ShowPanel(sportPanel);
    }

    private void SelectSport(string sport, string scene)
    {
        selectedSport = sport;
        selectedScene = scene;

        // Require a new duration selection for each sport.
        SelectedDurationSeconds = 0;
        sportContinueButton.interactable = true;
    }


    public void SelectGolf()
    {
        selectionGlow.SelectButton(sportButtons, sportButtons[0]);
        SelectSport("Golf", golfScene);
    }

    public void SelectAirHockey()
    {
        selectionGlow.SelectButton(sportButtons, sportButtons[1]);
        SelectSport("Air Hockey", airHockeyScene);
    }

    public void SelectTennis()
    {
        selectionGlow.SelectButton(sportButtons, sportButtons[2]);
        SelectSport("Tennis", tennisScene);
    }

    public void SelectGoalkeeping()
    {
        selectionGlow.SelectButton(sportButtons, sportButtons[3]);
        SelectSport("Goalkeeping", goalkeepingScene);
    }

    public void SelectChestPass()
    {
        selectionGlow.SelectButton(sportButtons, sportButtons[4]);
        SelectSport("Chest Pass", chestPassScene);
    }

    public void ShowDurationMenu()
    {
        selectionGlow.ResetButtons(durationButtons);
        durationContinueButton.interactable = false;
        ShowPanel(durationPanel);
    }

    public void ShowSportInstructions()
    {
        instructionPanel.ChangeInstructions(selectedSport);
        ShowPanel(instructionPanelObject);
    }

    public void SelectShortDuration()
    {
        selectionGlow.SelectButton(durationButtons, durationButtons[0]);
        SelectDuration("Short");
    }

    public void SelectMediumDuration()
    {
        selectionGlow.SelectButton(durationButtons, durationButtons[1]);
        SelectDuration("Medium");
    }

    public void SelectLongDuration()
    {
        selectionGlow.SelectButton(durationButtons, durationButtons[2]);
        SelectDuration("Long");
    }

    private void SelectDuration(string duration)
    {
        GameDurations durations = GetSelectedDurations();

        if (durations == null)
            return;

        switch (duration)
        {
            case "Short":
                SelectedDurationSeconds = durations.shortDuration;
                break;

            case "Medium":
                SelectedDurationSeconds = durations.mediumDuration;
                break;

            case "Long":
                SelectedDurationSeconds = durations.longDuration;
                break;
        }

        durationContinueButton.interactable = true;

        Debug.Log(selectedSport + ": " +
                  duration + " selected (" +
                  SelectedDurationSeconds + " seconds)");
    }

    private GameDurations GetSelectedDurations()
    {
        switch (selectedSport)
        {
            case "Golf":
                return golfDurations;

            case "Tennis":
                return tennisDurations;

            case "Air Hockey":
                return airHockeyDurations;

            case "Goalkeeping":
                return goalkeepingDurations;

            case "Chest Pass":
                return chestPassDurations;

            default:
                return null;
        }
    }

    public void ContinueToGame()
    {
        if (SelectedDurationSeconds <= 0)
            return;
        Debug.Log("Loading scene: " + selectedScene);
        SceneManager.LoadScene(selectedScene);
    }

    
}
