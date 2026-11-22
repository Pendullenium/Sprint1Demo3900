using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GolfGameManager : MonoBehaviour
{
    // text shown in the game
    [Header("UI")]
    public TMP_Text scoreText;
    public TMP_Text feedbackText;
    public TMP_Text targetText;

    // final session results
    [Header("Session Results")]
    public GameObject resultsPanel;
    public TMP_Text resultsStats;
    public GolfTimer sharedTimer;
    public GolfBallController golfBall;

    // points for swings + shots
    [Header("Combined Scoring")]
    [Tooltip("Points awarded for every meaningful swing that hits the ball.")]
    [Min(0)]
    public int pointsPerValidHit = 1;

    [Tooltip("Extra points awarded when the ball reaches the active target.")]
    [Min(0)]
    public int targetBonusPoints = 2;

    [Header("Feedback")]
    public float feedbackDuration = 1.5f;

    // NEAR, MID and FAR targets
    [Header("Target Progression")]
    public GolfTargetController nearTarget;
    public GolfTargetController midTarget;
    public GolfTargetController farTarget;

    [Header("Difficulty")]
    [Tooltip("Successful target shots required to unlock the next target.")]
    [Min(1)]
    public int hitsPerTarget = 3;

    [Header("Ring Renderers")]
    public Renderer nearRing;
    public Renderer midRing;
    public Renderer farRing;

    // materials for the target rings
    [Header("Ring Materials")]
    public Material nearActiveMaterial;
    public Material midActiveMaterial;
    public Material farActiveMaterial;
    public Material lockedRingMaterial;

    private int score;
    public int Score => score;

    // 0 = NEAR, 1 = MID, 2 = FAR, 3 = complete
    private int currentTarget;
    private int hitsOnCurrentTarget;

    // player performance stats
    private int totalValidHits;
    private int successfulShots;
    private int missedShots;

    // current game state
    private bool targetsReady;
    private bool sessionEnded;

    private Coroutine feedbackRoutine;

    public int CurrentTargetIndex => currentTarget;
    public bool SessionEnded => sessionEnded;

    private void Start()
    {
        // make sure at least one hit is required
        hitsPerTarget = Mathf.Max(1, hitsPerTarget);
       // checks that all three targets are assigned and are different from each other
        targetsReady =
            nearTarget != null &&
            midTarget != null &&
            farTarget != null &&
            nearTarget != midTarget &&
            nearTarget != farTarget &&
            midTarget != farTarget;

        if (!targetsReady)
        {
            Debug.LogError(
                "[GOLF] Assign three different target controllers.",
                this
            );
        }

        InitialiseSession();
    }

    // starts a new session, resetting all scores and targets
    private void InitialiseSession()
    {
        score = 0;

        currentTarget = 0;
        hitsOnCurrentTarget = 0;

        totalValidHits = 0;
        successfulShots = 0;
        missedShots = 0;

        sessionEnded = false;

        // stops any old feedback
        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
            feedbackRoutine = null;
        }

        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
        }

        if (resultsPanel != null)
        {
            resultsPanel.SetActive(false);
        }

        UpdateScore();
        UpdateTarget();
    }
    // responsible for recording a valid swing  and score addition
    public void RegisterValidHit()
    {
        if (sessionEnded)
        {
            return;
        }

        totalValidHits++;
        score += pointsPerValidHit;

        UpdateScore();

        Debug.Log(
            "[GOLF] Valid swing recorded. Total swings: " +
            totalValidHits,
            this
        );
    }

    // kept for compatibility with older scene events
    public void ScoreSuccess()
    {
        Debug.LogWarning(
            "[GOLF] Use ScoreSuccess(target).",
            this
        );
    }

    // records a successful target shot
    public void ScoreSuccess(GolfTargetController target)
    {
        // only count hits on the active target
        if (sessionEnded ||
            !IsTargetActive(target) ||
            hitsOnCurrentTarget >= hitsPerTarget)
        {
            return;
        }

        successfulShots++;
        hitsOnCurrentTarget++;
        score += targetBonusPoints;

        UpdateScore();

        // checks if the current target is complete
        if (hitsOnCurrentTarget >= hitsPerTarget)
        {
            string completedName =
                GetTargetName(currentTarget);

            if (currentTarget >= 2)
            {
                ShowFeedback(
                    "Target reached! +" +
                    targetBonusPoints +
                    " bonus\n" +
                    "ALL TARGETS COMPLETE!"
                );
            }
            else
            {
                ShowFeedback(
                    "Target reached! +" +
                    targetBonusPoints +
                    " bonus\n" +
                    completedName +
                    " COMPLETE\n" +
                    GetTargetName(currentTarget + 1) +
                    " UNLOCKED!"
                );
            }

            StartCoroutine(AdvanceTargetAfterDelay());
        }
        else
        {
            ShowFeedback(
                "Target reached! +" +
                targetBonusPoints +
                " bonus\n" +
                hitsOnCurrentTarget +
                "/" +
                hitsPerTarget +
                " target shots"
            );
        }

        UpdateTarget();
    }

    private IEnumerator AdvanceTargetAfterDelay()
    {
        yield return new WaitForSeconds(feedbackDuration);

        if (sessionEnded)
        {
            yield break;
        }

        currentTarget++;
        hitsOnCurrentTarget = 0;

        UpdateTarget();
    }

    // checks if this is the active target
    public bool IsTargetActive(GolfTargetController target)
    {
        if (!targetsReady ||
            target == null ||
            sessionEnded)
        {
            return false;
        }

        switch (currentTarget)
        {
            case 0:
                return target == nearTarget;

            case 1:
                return target == midTarget;

            case 2:
                return target == farTarget;

            default:
                return false;
        }
    }

    public void ShowMiss()
    {
        ShowMiss(
            "Hit counted! Keep looking toward the target and try again."
        );
    }

    // records a missed shot and shows feedback
    public void ShowMiss(string message)
    {
        if (sessionEnded)
        {
            return;
        }

        missedShots++;
        ShowFeedback(message);
    }

    // shows guidance text if the session is still active
    public void ShowGuidance(string message)
    {
        if (!sessionEnded)
        {
            ShowFeedback(message);
        }
    }

    // game end session showing results and saving to CSV
    public void EndSession()
    {
        if (sessionEnded)
        {
            return;
        }

        sessionEnded = true;

        try
        {
            SessionCsvLogger.RecordSession(
                "Golf", Mathf.RoundToInt(Data.duration), score);
        }
        catch (System.IO.IOException ex)
        {
            Debug.LogWarning($"Could not save Golf result: {ex.Message}");
        }

        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
            feedbackRoutine = null;
        }

        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
        }

        if (golfBall != null)
        {
            golfBall.ResetBall();
        }

        float accuracy =
            totalValidHits > 0
                ? (float)successfulShots /
                  totalValidHits * 100f
                : 0f;

        int targetsCompleted =
            Mathf.Clamp(currentTarget, 0, 3);

        if (targetText != null)
        {
            targetText.text = "SESSION COMPLETE";
        }

        // result summary text
        if (resultsStats != null)
        {
            resultsStats.text =
                "Total Valid Swings: " +
                totalValidHits + "\n" +

                "Successful Target Shots: " +
                successfulShots + "\n" +

                "Missed Shots: " +
                missedShots + "\n" +

                "Accuracy: " +
                accuracy.ToString("F0") +
                "%\n" +

                "Targets Completed: " +
                targetsCompleted +
                "/3\n" +

                "Total Score: " +
                score;
        }

        if (resultsPanel != null)
        {
            resultsPanel.SetActive(true);
            resultsPanel.transform.SetAsLastSibling();
        }
    }

    public void RestartSession()
    {
        Time.timeScale = 1f;

        Debug.Log(
            "[GOLF] Restart button pressed.",
            this
        );

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    private string GetTargetName(int targetNumber)
    {
        switch (targetNumber)
        {
            case 0:
                return "NEAR";

            case 1:
                return "MID";

            case 2:
                return "FAR";

            default:
                return "COMPLETE";
        }
    }

        // updates the current target text
        private void UpdateTarget()
    {
        UpdateRingMaterials();

        if (targetText == null)
        {
            return;
        }

        if (!targetsReady)
        {
            targetText.text = "TARGET SETUP ERROR";
        }
        else if (currentTarget >= 3)
        {
            targetText.text =
                "ALL TARGETS COMPLETE!";
        }
        else
        {
            targetText.text =
                "Target: " +
                GetTargetName(currentTarget) +
                " (" +
                hitsOnCurrentTarget +
                "/" +
                hitsPerTarget +
                ")";
        }
    }

   // updates the target ring materials
    private void UpdateRingMaterials()
    {
        if (!targetsReady)
        {
            return;
        }

        SetRingMaterial(
            nearRing,
            nearActiveMaterial
        );

        SetRingMaterial(
            midRing,
            currentTarget >= 1
                ? midActiveMaterial
                : lockedRingMaterial
        );

        SetRingMaterial(
            farRing,
            currentTarget >= 2
                ? farActiveMaterial
                : lockedRingMaterial
        );
    }

    private void SetRingMaterial(
        Renderer ring,
        Material material)
    {
        if (ring == null || material == null)
        {
            return;
        }

        if (ring.sharedMaterial != material)
        {
            ring.sharedMaterial = material;
        }
    }

    // updates the score on screen
    private void UpdateScore()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    private void ShowFeedback(string message)
    {
        if (feedbackText == null)
        {
            return;
        }

        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
        }

        feedbackRoutine =
            StartCoroutine(
                DisplayFeedback(message)
            );
    }

    private IEnumerator DisplayFeedback(string message)
    {
        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);

        yield return new WaitForSeconds(
            feedbackDuration
        );

        feedbackText.gameObject.SetActive(false);
        feedbackRoutine = null;
    }
}
