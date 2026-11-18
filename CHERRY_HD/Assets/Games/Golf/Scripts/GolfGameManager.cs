using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GolfGameManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text scoreText;
    public TMP_Text feedbackText;
    public TMP_Text targetText;

    [Header("Session Results")]
    public GameObject resultsPanel;
    public TMP_Text resultsStats;
    public GolfTimer sharedTimer;
    public GolfBallController golfBall;

    [Header("Combined Scoring")]
    [Tooltip("Points awarded for every meaningful swing that hits the ball.")]
    [Min(0)]
    public int pointsPerValidHit = 1;

    [Tooltip("Extra points awarded when the ball reaches the active target.")]
    [Min(0)]
    public int targetBonusPoints = 2;

    [Header("Feedback")]
    public float feedbackDuration = 1.5f;

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

    [Header("Ring Materials")]
    public Material nearActiveMaterial;
    public Material midActiveMaterial;
    public Material farActiveMaterial;
    public Material lockedRingMaterial;

    private int score;
    public int Score => score;

    // 0 = NEAR, 1 = MID, 2 = FAR, 3 = complete.
    private int currentTarget;
    private int hitsOnCurrentTarget;

    private int totalValidHits;
    private int successfulShots;
    private int missedShots;

    private bool targetsReady;
    private bool sessionEnded;

    private Coroutine feedbackRoutine;

    public int CurrentTargetIndex => currentTarget;
    public bool SessionEnded => sessionEnded;

    private void Start()
    {
        hitsPerTarget = Mathf.Max(1, hitsPerTarget);

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

    private void InitialiseSession()
    {
        score = 0;

        currentTarget = 0;
        hitsOnCurrentTarget = 0;

        totalValidHits = 0;
        successfulShots = 0;
        missedShots = 0;

        sessionEnded = false;

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

    // Kept for compatibility with older scene events.
    public void ScoreSuccess()
    {
        Debug.LogWarning(
            "[GOLF] Use ScoreSuccess(target).",
            this
        );
    }

    public void ScoreSuccess(GolfTargetController target)
    {
        if (sessionEnded || !IsTargetActive(target))
        {
            return;
        }

        successfulShots++;
        hitsOnCurrentTarget++;
        score += targetBonusPoints;

        UpdateScore();

        if (hitsOnCurrentTarget >= hitsPerTarget)
        {
            string completedName =
                GetTargetName(currentTarget);

            currentTarget++;
            hitsOnCurrentTarget = 0;

            if (currentTarget >= 3)
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
                    GetTargetName(currentTarget) +
                    " UNLOCKED!"
                );
            }
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

    public void ShowMiss(string message)
    {
        if (sessionEnded)
        {
            return;
        }

        missedShots++;
        ShowFeedback(message);
    }

    public void ShowGuidance(string message)
    {
        if (!sessionEnded)
        {
            ShowFeedback(message);
        }
    }

    public void EndSession()
    {
        if (sessionEnded)
        {
            return;
        }

        sessionEnded = true;

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
                "ACTIVE TARGET: " +
                GetTargetName(currentTarget) +
                "\nPROGRESS: " +
                hitsOnCurrentTarget +
                " / " +
                hitsPerTarget;
        }
    }

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