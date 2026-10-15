using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TMPro;

public class Timer : MonoBehaviour
{
    public float timeRemaining = 0;     // in seconds
    public bool timerRunning = false;
    public TMP_Text timeDisplay;

    public void Set(float time)
    {
        timeRemaining = time >= 0 ? time : 0;
    }

    public void Start()
    {
        timerRunning = true;
    }

    private void Update()
    {
        if (timerRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;

                float mins = Mathf.FloorToInt(timeRemaining / 60);
                float secs = Mathf.FloorToInt(timeRemaining % 60);

                timeDisplay.text = string.Format("{0:00}:{1:00}", mins, secs);
            }
            else
            {
                timeRemaining = 0;
                timerRunning = false;
                timeDisplay.text = string.Format("00:00");
            }
        }
    }
}