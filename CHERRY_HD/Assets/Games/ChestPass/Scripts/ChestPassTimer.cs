using UnityEngine;
using TMPro;

public class ChestPassTimer : MonoBehaviour
{
    public float timeRemaining = 0f;
    public bool timerRunning = true;
    public TMP_Text timeDisplay;

    private void Start()
    {
        RestartTimer();
    }

    public void RestartTimer()
    {
        int selectedDuration = GameMenu.SelectedDurationSeconds;

        timeRemaining = Mathf.Max(
            0f,
            selectedDuration > 0 ? selectedDuration : ChestPassData.duration
        );
        timerRunning = timeRemaining > 0f;
        UpdateTimeDisplay();
    }

    private void Update()
    {
        if (!timerRunning)
        {
            return;
        }

        if (timeRemaining > 0f)
        {
            timeRemaining -= Time.deltaTime;

            if (timeRemaining <= 0f)
            {
                timeRemaining = 0f;
                timerRunning = false;
            }
        }
        else
        {
            timeRemaining = 0f;
            timerRunning = false;
        }

        UpdateTimeDisplay();
    }

    private void UpdateTimeDisplay()
    {
        if (timeDisplay == null)
        {
            return;
        }

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);

        timeDisplay.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}