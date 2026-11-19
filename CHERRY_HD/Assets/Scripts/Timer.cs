using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public float timeRemaining;
    public bool timerRunning = true;
    public TMP_Text timeDisplay;

    [SerializeField] private TennisBallHit tennisBall;
    [SerializeField] private FootballController goalkeepingBall;
    [SerializeField] private HockeyPuckController airHockeyPuck;
    [SerializeField] private FinalScorePanel finalScorePanel;

    private int sessionDurationSeconds;

    private void Start()
    {
        sessionDurationSeconds = GameMenu.SelectedDurationSeconds;
        timeRemaining = sessionDurationSeconds;
    }

    private void Update()
    {
        if (!timerRunning)
            return;

        timeRemaining = Mathf.Max(0f, timeRemaining - Time.deltaTime);

        int secondsLeft = Mathf.CeilToInt(timeRemaining);
        if (timeDisplay != null)
            timeDisplay.text = $"{secondsLeft / 60:00}:{secondsLeft % 60:00}";

        if (timeRemaining > 0f)
            return;

        timerRunning = false;

        if (tennisBall != null && sessionDurationSeconds > 0)
        {
            SessionCsvLogger.RecordSession(
                "Tennis", sessionDurationSeconds, tennisBall.Score);
            
            finalScorePanel.ShowFinalScore("Score: " + tennisBall.Score);
        }

        else if (goalkeepingBall != null && sessionDurationSeconds > 0)
        {
            try
            {
                SessionCsvLogger.RecordSession(
                    "Goalkeeping", sessionDurationSeconds, goalkeepingBall.Score);

                finalScorePanel.ShowFinalScore("Score: " + goalkeepingBall.Score);
            }
            catch (System.IO.IOException ex)
            {
                timeRemaining = 0;
                timerRunning = false;
                timeDisplay.text = string.Format("00:00");
                
                UnityEngine.Debug.LogWarning(
                    $"Could not save Goalkeeping result: {ex.Message}");
            }
        }

        else if (airHockeyPuck != null && sessionDurationSeconds > 0)
        {
            try
            {
                SessionCsvLogger.RecordSession(
                    "Air Hockey", sessionDurationSeconds, airHockeyPuck.Score);
                finalScorePanel.ShowFinalScore("Score: " + airHockeyPuck.Score);
            }
            catch (System.IO.IOException ex)
            {
                UnityEngine.Debug.LogWarning(
                    $"Could not save Air Hockey result: {ex.Message}");
            }
        }
    }
}