using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class TennisBallHit : MonoBehaviour
{
    public Collider racketHead;

    public Vector3 returnTarget =
        new Vector3(0, 0.034f, 6);

    public float returnFlightTime = 1.5f;

    [Header("Court references")]
    public Collider courtCollider;
    public Collider netCollider;
    public Timer sessionTimer;

    [Header("Court boundaries")]
    public float leftBoundary = -4f;
    public float rightBoundary = 4f;
    public float netZ = 0f;
    public float farBaselineZ = 9f;
    public float nearBaselineZ = -9f;

    [Header("Scoreboard")]
    public TMP_Text scoreText;
    public TMP_Text successText;

    public int Score { get; private set; }

    private enum BallState
    {
        Inactive,
        ComingToPlayer,
        PlayerReturnQueued,
        GoingToOpponent,
        OpponentPreparing
    }

    private BallState state;
    private Rigidbody body;
    private TennisBallLauncher launcher;
    private int playerSideBounces;
    private Coroutine successRoutine;

    private bool MatchFinished =>
        sessionTimer != null &&
        (!sessionTimer.timerRunning ||
         sessionTimer.timeRemaining <= 0f);

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        launcher = GetComponent<TennisBallLauncher>();
    }

    private void Start()
    {
        UpdateScoreText();

        if (successText != null)
            successText.gameObject.SetActive(false);
    }

    public void PrepareForLaunch()
    {
        playerSideBounces = 0;
        state = BallState.ComingToPlayer;
    }

    public void CancelAttempt()
    {
        state = BallState.Inactive;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (MatchFinished || body.isKinematic ||
            state == BallState.Inactive)
        {
            return;
        }

        if (collision.collider == racketHead)
        {
            // Require exactly one bounce before the player's hit.
            if (state == BallState.ComingToPlayer &&
                playerSideBounces == 1)
            {
                state = BallState.PlayerReturnQueued;
                launcher.RefreshRally();
            }

            return;
        }

        if (state == BallState.PlayerReturnQueued ||
            state == BallState.OpponentPreparing)
        {
            return;
        }

        if (collision.collider == netCollider)
        {
            launcher.EndRally();
            return;
        }

        if (collision.collider != courtCollider)
        {
            launcher.EndRally();
            return;
        }

        Vector3 landing = collision.GetContact(0).point;

        bool insideWidth =
            landing.x >= leftBoundary &&
            landing.x <= rightBoundary;

        if (state == BallState.ComingToPlayer)
        {
            bool insidePlayerCourt =
                insideWidth &&
                landing.z < netZ &&
                landing.z >= nearBaselineZ;

            if (!insidePlayerCourt)
            {
                launcher.EndRally();
                return;
            }

            playerSideBounces++;

            if (playerSideBounces > 1)
                launcher.EndRally();
        }
        else if (state == BallState.GoingToOpponent)
        {
            bool insideOpponentCourt =
                insideWidth &&
                landing.z > netZ &&
                landing.z <= farBaselineZ;

            if (!insideOpponentCourt)
            {
                launcher.EndRally();
                return;
            }

            state = BallState.OpponentPreparing;

            AwardPoint();
            launcher.ReturnAfterBounce();
        }
    }

    private void FixedUpdate()
    {
        if (state != BallState.PlayerReturnQueued)
            return;

        if (MatchFinished || body.isKinematic)
            return;

        body.linearVelocity = CalculateVelocity(
            body.position, returnTarget, returnFlightTime);

        body.angularVelocity = Vector3.zero;
        state = BallState.GoingToOpponent;
    }

    public Vector3 CalculateVelocity(
        Vector3 start, Vector3 target, float flightDuration)
    {
        float duration = Mathf.Max(0.2f, flightDuration);
        float distanceZ = target.z - start.z;

        if (Mathf.Abs(distanceZ) > 0.01f &&
            Physics.gravity.y < 0f)
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

        return (target - start) / duration
            - 0.5f * Physics.gravity * duration;
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