
using System.Collections;
using UnityEngine;
using TMPro;

public class GolfGameManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text scoreText;
    public TMP_Text feedbackText;

    [Header("Feedback")]
    public float feedbackDuration = 1.5f;

    [Header("Target Progression")]
    public GolfTargetController nearTarget;
    public GolfTargetController midTarget;
    public GolfTargetController farTarget;
    public TMP_Text targetText;

    [Header("Difficulty")]
    [Min(1)]
    public int hitsPerTarget = 3;

    [Header("Ring Labels")]
    public TMP_Text nearLabel;
    public TMP_Text midLabel;
    public TMP_Text farLabel;

    [Header("Label Colours")]
    public Color activeColor = new Color32(55, 210, 105, 255);
    public Color lockedColor = new Color32(155, 165, 175, 255);
    public Color completedColor = new Color32(120, 130, 135, 255);

    [Header("Ring Renderers")]
    public Renderer nearRing;
    public Renderer midRing;
    public Renderer farRing;

    [Header("Ring Materials")]
    public Material nearActiveMaterial;
    public Material midActiveMaterial;
    public Material farActiveMaterial;
    public Material lockedRingMaterial;

    // Game state
    private int score = 0;
    private int currentTarget = 0;
    private int hitsOnCurrentTarget = 0;

    // Allows GolfClubController to identify the active level.
    // 0 = NEAR, 1 = MID, 2 = FAR, 3 = COMPLETE
    public int CurrentTargetIndex => currentTarget;

    private bool targetsReady = false;
    private Coroutine feedbackRoutine;

    private void Start()
    {
        hitsPerTarget = Mathf.Max(1, hitsPerTarget);

        score = 0;
        currentTarget = 0;
        hitsOnCurrentTarget = 0;

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

        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
        }

        UpdateScore();
        UpdateTarget();
    }

    // Retained for compatibility with existing scripts.
    public void ScoreSuccess()
    {
        Debug.LogWarning(
            "[GOLF] ScoreSuccess() called without a target. " +
            "Use ScoreSuccess(GolfTargetController target).",
            this
        );
    }

    public void ScoreSuccess(GolfTargetController target)
    {
        // Only the currently unlocked target can score.
        if (!IsTargetActive(target))
        {
            return;
        }

        score++;
        hitsOnCurrentTarget++;

        UpdateScore();

        if (hitsOnCurrentTarget >= hitsPerTarget)
        {
            string completedName = GetTargetName(currentTarget);

            currentTarget++;
            hitsOnCurrentTarget = 0;

            if (currentTarget >= 3)
            {
                ShowFeedback(
                    "Great shot! +1\nAll targets completed!"
                );
            }
            else
            {
                ShowFeedback(
                    completedName + " COMPLETE!\n" +
                    GetTargetName(currentTarget) + " UNLOCKED!"
                );
            }
        }
        else
        {
            ShowFeedback(
                "Good shot! +1\n" +
                hitsOnCurrentTarget + "/" +
                hitsPerTarget + " successful shots"
            );
        }

        // Update HUD, ring labels and materials after scoring.
        UpdateTarget();
    }

    public bool IsTargetActive(GolfTargetController target)
    {
        if (!targetsReady || target == null)
        {
            return false;
        }

        if (currentTarget == 0)
        {
            return target == nearTarget;
        }

        if (currentTarget == 1)
        {
            return target == midTarget;
        }

        if (currentTarget == 2)
        {
            return target == farTarget;
        }

        return false;
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

    // Automatically updates the target shown in the top HUD.
    private void UpdateTarget()
    {
        UpdateRingLabels();
        UpdateRingMaterials();

        if (targetText == null)
        {
            return;
        }

        if (!targetsReady)
        {
            targetText.text = "Target: SETUP ERROR";
        }
        else if (currentTarget >= 3)
        {
            targetText.text = "Target: COMPLETE";
        }
        else
        {
            targetText.text =
                "Target: " + GetTargetName(currentTarget);
        }
    }

    private void UpdateRingLabels()
    {
        if (!targetsReady)
        {
            return;
        }

        UpdateRingLabel(nearLabel, "NEAR", 0);
        UpdateRingLabel(midLabel, "MID", 1);
        UpdateRingLabel(farLabel, "FAR", 2);
    }

    private void UpdateRingLabel(
        TMP_Text label,
        string targetName,
        int targetNumber)
    {
        if (label == null)
        {
            return;
        }

        label.alignment = TextAlignmentOptions.Center;

        if (targetNumber < currentTarget)
        {
            label.text =
                targetName +
                "\n<size=60%>COMPLETE</size>";

            label.color = completedColor;
        }
        else if (targetNumber == currentTarget)
        {
            label.text =
                targetName +
                "\n<size=60%>" +
                hitsOnCurrentTarget + "/" +
                hitsPerTarget +
                " HITS</size>";

            label.color = activeColor;
        }
        else
        {
            label.text =
                targetName +
                "\n<size=60%>LOCKED</size>";

            label.color = lockedColor;
        }
    }

    // Update ring materials based on level progression.
    private void UpdateRingMaterials()
    {
        if (!targetsReady)
        {
            return;
        }

        // NEAR always stays blue.
        SetRingMaterial(nearRing, nearActiveMaterial);

        // MID stays grey until NEAR is completed.
        SetRingMaterial(
            midRing,
            currentTarget >= 1
                ? midActiveMaterial
                : lockedRingMaterial
        );

        // FAR stays grey until MID is completed.
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
        // Missing references should not interrupt gameplay.
        if (ring == null || material == null)
        {
            return;
        }

        if (ring.sharedMaterial != material)
        {
            ring.sharedMaterial = material;
        }
    }

    public void ShowMiss()
    {
        if (currentTarget >= 3)
        {
            return;
        }

        ShowFeedback("Oops! Try again!");
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
            StartCoroutine(DisplayFeedback(message));
    }

    private IEnumerator DisplayFeedback(string message)
    {
        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);

        yield return new WaitForSeconds(feedbackDuration);

        feedbackText.gameObject.SetActive(false);
        feedbackRoutine = null;
    }
}
