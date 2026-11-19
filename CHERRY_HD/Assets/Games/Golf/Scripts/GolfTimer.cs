using TMPro;
using UnityEngine;

public class GolfTimer : MonoBehaviour
{
    public float timeRemaining;
    public bool timerRunning = true;
    public TMP_Text timeDisplay;

    [Header("Golf Session")]
    public GolfGameManager gameManager;

    private bool endReported;

    private void Start()
    {
        timeRemaining = Mathf.Max(0, Data.duration);
        timerRunning = timeRemaining > 0;
        endReported = false;

        UpdateDisplay();

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

        timeRemaining -= Time.deltaTime;

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

    private void UpdateDisplay()
    {
        if (timeDisplay == null)
        {
            return;
        }

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

    public void RestartTimer()
    {
        timeRemaining = Mathf.Max(0, Data.duration);
        timerRunning = timeRemaining > 0;
        endReported = false;

        UpdateDisplay();
    }
}