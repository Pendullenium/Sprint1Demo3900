using UnityEngine;

public class GameMenu : MonoBehaviour
{
    public GameObject welcomePanel;
    public GameObject sportPanel;
    public GameObject durationPanel;

    public static int SelectedDurationSeconds { get; private set; }
    public string selectedSport;

    private void Start()
    {
        SelectedDurationSeconds = 0;
        ShowWelcome();
    }

    private void ShowPanel(GameObject panelToShow)
    {
        welcomePanel.SetActive(false);
        sportPanel.SetActive(false);
        durationPanel.SetActive(false);

        panelToShow.SetActive(true);
    }

    public void ShowWelcome()
    {
        ShowPanel(welcomePanel);
    }

    public void ShowSportMenu()
    {
        ShowPanel(sportPanel);
    }


    public void SelectGolf()
    {
        selectedSport = "Golf";
        ShowDurationMenu();
    }

    public void SelectTennis()
    {
        selectedSport = "Tennis";
        ShowDurationMenu();
    }

    public void SelectAirHockey()
    {
        selectedSport = "Air Hockey";
        ShowDurationMenu();
    }

    public void SelectGoalkeeping()
    {
        selectedSport = "Goalkeeping";
        ShowDurationMenu();
    }

    public void SelectChestPass()
    {
        selectedSport = "Chest Pass";
        ShowDurationMenu();
    }

    private void ShowDurationMenu()
    {
        ShowPanel(durationPanel);
    }

    public void printPressed()
    {
        Debug.Log(" Golf button pressed!");
    }
}
