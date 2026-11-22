using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class TennisBallHit : MonoBehaviour
{
    public Collider racketHead;

    // Target position for the ball after the player's hit
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

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip tennisHitSound;

    public int Score { get; private set; }

    // ball's current state in the rally
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
        {
            successText.gameObject.SetActive(false);
        }
    }

    // resets the bounce count for a new incoming ball
    public void PrepareForLaunch()
    {
        playerSideBounces = 0;
        state = BallState.ComingToPlayer;
    }

    // sets the ball's state as inactive to stop the current rally
    public void CancelAttempt()
    {
        state = BallState.Inactive;
    }

    private void OnCollisionEnter(Collision collision)
    {   
        // ignores collisions when the ball or match is inactive
        if (MatchFinished ||
            body.isKinematic ||
            state == BallState.Inactive)
        {
            return;
        }

        if (collision.collider.CompareTag("Racket"))
        {
            Debug.Log("Hit Tennis Racket");

            // allows a return hit only after one bounce on the player's side
            if (state == BallState.ComingToPlayer &&
                playerSideBounces == 1)
            {
                state = BallState.PlayerReturnQueued;
                launcher.RefreshRally();

                Debug.Log("Hit Tennis Sound");
                if (audioSource != null &&
                    tennisHitSound != null)
                {
                    Debug.Log("Hit Tennis Sound2");
                    audioSource.PlayOneShot(tennisHitSound);
                }
            }

            return;
        }

        // ignores collisions while waiting for a return hit
        if (state == BallState.PlayerReturnQueued ||
            state == BallState.OpponentPreparing)
        {
            return;
        }

        // ends the rally if the ball hits the net
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

        // stores the contact point where the ball landed
        Vector3 landing = collision.GetContact(0).point;

        bool insideWidth =
            landing.x >= leftBoundary &&
            landing.x <= rightBoundary;

        if (state == BallState.ComingToPlayer)
        {
            // checks if the bounce is on the player's side of the court
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

            // ends rally if the ball bounces more than once on the player's side
            if (playerSideBounces > 1)
            {
                launcher.EndRally();
            }
        }
        else if (state == BallState.GoingToOpponent)
        {   
            // checks if the player's return lands inside the opposite court
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

    // applies calculated velocity towards the opponent's court
    // clears the ball's spin and sets its state to GoingToOpponent
    private void FixedUpdate()
    {
        if (state != BallState.PlayerReturnQueued)
        {
            return;
        }

        if (MatchFinished || body.isKinematic)
        {
            return;
        }

        body.linearVelocity = CalculateVelocity(
            body.position,
            returnTarget,
            returnFlightTime
        );

        body.angularVelocity = Vector3.zero;
        state = BallState.GoingToOpponent;
    }

    // calculates the launch velocity needed to reach the target
    public Vector3 CalculateVelocity(
        Vector3 start,
        Vector3 target,
        float flightDuration)
    {
        float duration = Mathf.Max(0.2f, flightDuration);
        float distanceZ = target.z - start.z;

        if (Mathf.Abs(distanceZ) > 0.01f &&
            Physics.gravity.y < 0f)
        {
            // calculates the net's location along the ball's path
            float fraction = (netZ - start.z) / distanceZ;

            if (fraction > 0f && fraction < 1f)
            {
                float netTop = netCollider != null
                    ? netCollider.bounds.max.y
                    : 1f;

                float requiredHeight = netTop + 0.3f;

                float straightHeight =
                    Mathf.Lerp(
                        start.y,
                        target.y,
                        fraction
                    );

                // calculates flight time required to clear the net
                float requiredTimeSquared =
                    2f *
                    (requiredHeight - straightHeight) /
                    (-Physics.gravity.y *
                     fraction *
                     (1f - fraction));

                if (requiredTimeSquared > 0f)
                {
                    duration = Mathf.Max(
                        duration,
                        Mathf.Sqrt(requiredTimeSquared)
                    );
                }
            }
        }

        // calculates velocity needed to reach the target,
        // accounting for velocity
        Vector3 ret = (target - start) / duration
            - 0.5f * Physics.gravity * duration;
        Debug.Log($"duration = {duration}");
        Debug.Log($"return vallue = {ret}");
        return (target - start) / duration
            - 0.5f * Physics.gravity * duration;
    }

    // increments score, updates scoreboard and displays success message
    private void AwardPoint()
    {
        Score++;
        UpdateScoreText();

        if (successText == null)
        {
            return;
        }

        if (successRoutine != null)
        {
            StopCoroutine(successRoutine);
        }

        successRoutine = StartCoroutine(ShowSuccess());
    }

    // displays current score on the scoreboard
    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + Score;
        }
    }

    // displays successful hit text
    private IEnumerator ShowSuccess()
    {
        successText.text = "Good shot! +1";
        successText.gameObject.SetActive(true);

        yield return new WaitForSeconds(1f);

        successText.gameObject.SetActive(false);
        successRoutine = null;
    }
}