using System;
using System.Collections;
using System.Diagnostics;
using TMPro;
using UnityEngine;

public class ChestPassGame : MonoBehaviour
{
    private enum Phase { Idle, Incoming, Turning, Complete }

    [Header("Shared systems")]
    [SerializeField] private ChestPassTimer sharedTimer;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text statusText;

    [Header("Results Screen")]
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TMP_Text resultsStats;


    [Header("Scene references")]
    [SerializeField] private Transform playerHead;
    [SerializeField] private Transform[] passerPositions;
    [SerializeField] private ChestPassBall basketballPrefab;
    [SerializeField] private GameObject leftArrow;
    [SerializeField] private GameObject rightArrow;


    [Header("Difficulty")]
    [SerializeField, Min(0.2f)]
    private float startingBallSpeed = 2.5f;

    [SerializeField, Min(0.2f)]
    private float maximumBallSpeed = 4.0f;

    [SerializeField, Min(0f)]
    private float minimumTurnPause = 0.15f;

    [SerializeField, Min(1)]
    private int catchesPerLevel = 3;

    [SerializeField, Range(5f, 45f)]
    private float facingToleranceDegrees = 18f;

    [SerializeField, Min(0.2f)]
    private float missedBallLifetime = 4.0f;

    [SerializeField]
    private Vector3 chestOffset = new Vector3(0f, -0.3f, 0f);


    private Phase phase = Phase.Idle;
    private ChestPassBall currentBall;
    private float launchTime;
    private int score;
    private int caught;
    private int missed;
    private int currentPasser = -1;
    private int targetPasser;
    private Coroutine transition;

    private void Start()
    {
        SetArrows(false, false);
        SetScore();
        if (sharedTimer == null || playerHead == null || basketballPrefab == null ||
            passerPositions == null || passerPositions.Length != 4)
        {
            UnityEngine.Debug.LogError("Missing Chest Pass references."); 
            enabled = false;
            return;
        }

        // Timer.Start() loads Data.duration. Do not override its countdown here.
        StartSession();
    }


    public void StartSession()
    {
        if (transition != null)
        {
            StopCoroutine(transition);
            transition = null;
        }

        if (currentBall != null)
        {
            Destroy(currentBall.gameObject);
            currentBall = null;
        }

        score = caught = missed = 0;
        currentPasser = -1;

        if (resultsPanel != null)
            resultsPanel.SetActive(false);

        phase = Phase.Turning;

        SetArrows(false, false);
        SetScore();
        SelectNextPasser();
    }



    private void Update()
    {
        if (phase == Phase.Complete)
        {
            if (UnityEngine.InputSystem.Keyboard.current != null &&
                UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame)
            {
                RestartSession();
            }

            return;
        }

        if (!sharedTimer.timerRunning || sharedTimer.timeRemaining <= 0f)
        {
            EndSession();
            return;
        }

        if (phase == Phase.Turning)
        {
            float angle = AngleToPasser(targetPasser);

            bool turnLeft = angle < -facingToleranceDegrees;
            bool turnRight = angle > facingToleranceDegrees;

            SetArrows(turnLeft, turnRight);

            if (turnLeft)
            {
                SetStatus("TURN LEFT 90°");
            }
            else if (turnRight)
            {
                SetStatus("TURN RIGHT 90°");
            }
            else
            {
                SetArrows(false, false);
                SetStatus("Great! Get ready to catch.");

                phase = Phase.Idle;
                transition = StartCoroutine(LaunchAfterDelay());
            }
        }

        else if (phase == Phase.Incoming && currentBall != null)
        {
            Rigidbody body = currentBall.GetComponent<Rigidbody>();
            Vector3 relative = currentBall.transform.position - (playerHead.position + chestOffset);
            Vector3 velocity = body.linearVelocity;
            // Once a ball has crossed the player's chest plane it counts as a miss.
            if (Vector3.Dot(relative, velocity) > 0f && Time.time - launchTime > 0.15f)
                currentBall.Miss();
            else if (Time.time - launchTime >= missedBallLifetime)
                currentBall.Miss();
        }
    }



    private int GetDifficultyLevel()
    {
        return Mathf.Clamp(
            caught / Mathf.Max(1, catchesPerLevel),
            0,
            5
        );
    }

    private int GetPreparationSeconds()
    {
        int level = GetDifficultyLevel();

        return Mathf.Max(0, 3 - level);
    }

    private float GetCurrentBallSpeed()
    {
        int level = GetDifficultyLevel();

        if (level <= 3)
            return startingBallSpeed;

        float speedProgress = (level - 3) / 2f;

        return Mathf.Lerp(
            startingBallSpeed,
            maximumBallSpeed,
            speedProgress
        );
    }

    private IEnumerator LaunchAfterDelay()
    {
        // Give the player time to prepare.

        int preparationSeconds = GetPreparationSeconds();

        for (int count = preparationSeconds; count >= 1; count--)
        {
            if (phase != Phase.Idle ||
                !sharedTimer.timerRunning ||
                sharedTimer.timeRemaining <= 0f)
            {
                transition = null;
                yield break;
            }

            SetStatus("GET READY! " + count);
            yield return new WaitForSeconds(1f);
        }

        if (preparationSeconds == 0)
        {
            SetStatus("GET READY!");

            yield return new WaitForSeconds(minimumTurnPause);
        }


        transition = null;

        if (phase != Phase.Idle ||
            !sharedTimer.timerRunning ||
            sharedTimer.timeRemaining <= 0f)
        {
            yield break;
        }

        currentPasser = targetPasser;

        Transform origin = passerPositions[currentPasser];

        currentBall = Instantiate(
            basketballPrefab,
            origin.position,
            Quaternion.identity
        );

        currentBall.Initialise(this);

        Rigidbody body = currentBall.GetComponent<Rigidbody>();
        body.useGravity = false;

        Vector3 target = playerHead.position + chestOffset;

        body.linearVelocity =
    (target - origin.position).normalized
    * GetCurrentBallSpeed();

        launchTime = Time.time;
        phase = Phase.Incoming;

        SetStatus("CATCH!");
    }


    public void ResolveBall(ChestPassBall ball, bool success)
    {
        if (phase != Phase.Incoming || ball != currentBall) return;
        phase = Phase.Idle;
        if (success) { caught++; score++; SetStatus("Great catch!"); }
        else { missed++; SetStatus("Missed. Turn for the next pass."); }
        SetScore();
        Destroy(ball.gameObject);
        currentBall = null;
        SelectNextPasser();
    }

    // Passer positions MUST be ordered: Front, Right, Back, Left.
    // Each new direction is exactly one 90-degree turn from the last passer.
    private void SelectNextPasser()
    {
        if (currentPasser < 0)
        {
            // Begin facing the Front station; the first ball comes from Front.
            targetPasser = 0;
        }
        else
        {
            // Clockwise is right (+1), anticlockwise is left (-1).
            int turn = UnityEngine.Random.value < 0.5f ? -1 : 1;
            targetPasser = (currentPasser + turn + 4) % 4;
        }

        phase = Phase.Turning;
        SetStatus("Turn towards the next passer");
    }

    private float AngleToPasser(int index)
    {
        Vector3 direction = passerPositions[index].position - playerHead.position;
        direction.y = 0f;
        Vector3 facing = playerHead.forward;
        facing.y = 0f;
        if (facing.sqrMagnitude < 0.001f || direction.sqrMagnitude < 0.001f) return 0f;
        return Vector3.SignedAngle(facing, direction, Vector3.up);
    }

    private void SetArrows(bool left, bool right)
    {
        if (leftArrow != null) leftArrow.SetActive(left);
        if (rightArrow != null) rightArrow.SetActive(right);
    }

    private void SetScore()
    {
        if (scoreText != null) scoreText.text = "Score: " + score + "   Caught: " + caught + "   Missed: " + missed;
    }

    private void SetStatus(string value)
    {
        if (statusText != null) statusText.text = value;
    }

    public void RestartSession()
    {
        sharedTimer.RestartTimer();
        StartSession();
    }


    public void EndSession()
    {
        if (phase == Phase.Complete)
            return;

        phase = Phase.Complete;

        if (transition != null)
        {
            StopCoroutine(transition);
            transition = null;
        }

        if (currentBall != null)
        {
            Destroy(currentBall.gameObject);
            currentBall = null;
        }

        SetArrows(false, false);

        int totalAttempts = caught + missed;

        float accuracy = totalAttempts > 0
            ? (float)caught / totalAttempts * 100f
            : 0f;

        int difficultyReached = GetDifficultyLevel() + 1;

        if (resultsStats != null)
        {
            resultsStats.text =
                "Successful Catches: " + caught + "\n" +
                "Missed Passes: " + missed + "\n" +
                "Catch Accuracy: " + accuracy.ToString("F0") + "%\n" +
                "Difficulty Reached: Level " + difficultyReached;
        }

        SetStatus("");

        if (resultsPanel != null)
            resultsPanel.SetActive(true);

        if (sharedTimer.SessionDurationSeconds > 0)
        {
            try
            {
                SessionCsvLogger.RecordSession(
                    "Chest Passes", sharedTimer.SessionDurationSeconds, score);
            }
            catch (System.IO.IOException ex)
            {
                UnityEngine.Debug.LogWarning($"Could not save Chest Passes result: {ex.Message}");
            }
        }
    }

}
