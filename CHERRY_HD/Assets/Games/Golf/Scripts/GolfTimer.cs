using TMPro;
using UnityEngine;

public class GolfTimer : MonoBehaviour
{
    // stores remaining session time in sec
    public float timeRemaining;
    public bool timerRunning = true;
    public TMP_Text timeDisplay;

    // link timer to main game manager to end session when time runs out
    [Header("Golf Session")]
    public GolfGameManager gameManager;

    // flag to end game properly
    private bool endReported;

    private void Start()
    {
        // initialize timer with session duration
        timeRemaining = Mathf.Max(0, Data.duration);
        timerRunning = timeRemaining > 0;
        endReported = false;

        UpdateDisplay();

        // if the timer is not running at the start, end the session immediately
        if (!timerRunning)
        {
            FinishTimer();
        }
    }

    private void Update()
    {
        if (!timerRunning)
        {
            return;
        }
        // count down the remaining time and display in UI
        timeRemaining -= Time.deltaTime;

        // end the session if time runs out
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            timerRunning = false;

            UpdateDisplay();
            FinishTimer();
            return;
        }

        UpdateDisplay();
    }

    // update the timer display in the UI
    private void UpdateDisplay()
    {
        if (timeDisplay == null)
        {
            return;
        }

        // convert remaining time to minutes and seconds
        int totalSeconds =
            Mathf.CeilToInt(timeRemaining);

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

       timeDisplay.text = string.Format(
            "Time: {0:00}:{1:00}",
            minutes,
            seconds
        );
    }

   // informs game manager to end session when timer reaches zero
    private void FinishTimer()
    {
        if (endReported)
        {
            return;
        }

        endReported = true;

        if (gameManager != null)
        {
            gameManager.EndSession();
        }
    }

    // starts timer again with the original session duration
    public void RestartTimer()
    {
        timeRemaining = Mathf.Max(0, Data.duration);
        timerRunning = timeRemaining > 0;
        endReported = false;

        UpdateDisplay();
    }
}