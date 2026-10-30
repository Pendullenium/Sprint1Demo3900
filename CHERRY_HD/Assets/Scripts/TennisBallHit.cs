using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class TennisBallHit : MonoBehaviour
{
    public Collider racketHead;
    public Vector3 returnTarget = new Vector3(0, 0.034f, 6);
    public float returnFlightTime = 1.5f;

    [Header("Court references")]
    public Collider courtCollider;
    public Collider netCollider;
    public Timer sessionTimer;

    [Header("Opposite court boundaries")]
    public float leftBoundary = -4f;
    public float rightBoundary = 4f;
    public float netZ = 0f;
    public float farBaselineZ = 9f;

    [Header("Scoreboard")]
    public TMP_Text scoreText;
    public TMP_Text successText;

    public int Score { get; private set; }

    private Rigidbody body;
    private bool waitingForLanding;
    private bool returnQueued;
    private bool alreadyHit;
    private Coroutine successRoutine;

    private bool MatchFinished =>
        sessionTimer != null &&
        (!sessionTimer.timerRunning ||
         sessionTimer.timeRemaining <= 0f);

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        UpdateScoreText();

        if (successText != null)
            successText.gameObject.SetActive(false);
    }

    // Called by the launcher before each new throw.
    public void PrepareForLaunch()
    {
        waitingForLanding = false;
        returnQueued = false;
        alreadyHit = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (MatchFinished || body.isKinematic)
            return;

        if (collision.collider == racketHead)
        {
            if (alreadyHit)
                return;

            alreadyHit = true;
            returnQueued = true;
            return;
        }

        if (!waitingForLanding)
            return;

        // Touching the net makes this return unsuccessful.
        if (collision.collider == netCollider)
        {
            waitingForLanding = false;
            return;
        }

        if (collision.collider == courtCollider)
        {
            // Only the first court landing can score.
            waitingForLanding = false;

            Vector3 landing = collision.GetContact(0).point;

            bool insideOppositeCourt =
                landing.x >= leftBoundary &&
                landing.x <= rightBoundary &&
                landing.z > netZ &&
                landing.z <= farBaselineZ;

            if (insideOppositeCourt)
                AwardPoint();
        }
        else
        {
            // A wall or another obstacle ends the attempt.
            waitingForLanding = false;
        }
    }

    private void FixedUpdate()
    {
        if (!returnQueued)
            return;

        returnQueued = false;

        if (MatchFinished || body.isKinematic)
            return;

        Vector3 start = body.position;
        Vector3 target = returnTarget;
        float duration = Mathf.Max(0.2f, returnFlightTime);

        // Increase flight time if needed to clear the net.
        float distanceZ = target.z - start.z;

        if (distanceZ > 0.01f && Physics.gravity.y < 0f)
        {
            float fraction = (netZ - start.z) / distanceZ;

            if (fraction > 0f && fraction < 1f)
            {
                float netTop = netCollider != null
                    ? netCollider.bounds.max.y
                    : 1f;

                float requiredHeight = netTop + 0.3f;
                float straightHeight =
                    Mathf.Lerp(start.y, target.y, fraction);

                float requiredTimeSquared =
                    2f * (requiredHeight - straightHeight) /
                    (-Physics.gravity.y *
                     fraction * (1f - fraction));

                if (requiredTimeSquared > 0f)
                {
                    duration = Mathf.Max(
                        duration,
                        Mathf.Sqrt(requiredTimeSquared));
                }
            }
        }

        body.linearVelocity =
            (target - start) / duration
            - 0.5f * Physics.gravity * duration;

        body.angularVelocity = Vector3.zero;
        waitingForLanding = true;
    }

    private void AwardPoint()
    {
        Score++;
        UpdateScoreText();

        if (successText == null)
            return;

        if (successRoutine != null)
            StopCoroutine(successRoutine);

        successRoutine = StartCoroutine(ShowSuccess());
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + Score;
    }

    private IEnumerator ShowSuccess()
    {
        successText.text = "Good shot! +1";
        successText.gameObject.SetActive(true);

        yield return new WaitForSeconds(1f);

        successText.gameObject.SetActive(false);
        successRoutine = null;
    }
}