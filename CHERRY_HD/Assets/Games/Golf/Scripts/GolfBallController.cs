using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GolfBallController : MonoBehaviour
{
    [Header("Ball Reset")]
    public float resetAfterSeconds = 15f;
    public float stoppedSpeed = 0.2f;
    public float stoppedWait = 1.5f;

    [Header("Accidental Movement")]
    public float idleResetDistance = 0.4f;
    public float idleResetWait = 2f;

    [Header("Hole Safety")]
    public float holeSequenceTimeout = 12f;

    [Header("References")]
    public GolfGameManager gameManager;
    public GolfClubController golfClub;

    private Rigidbody ballBody;

    private Vector3 startingPosition;
    private Quaternion startingRotation;

    private bool shotActive;
    private bool enteringHole;
    private bool resetting;

    private float shotTime;
    private float stoppedTime;
    private float idleAwayTime;
    private float holeStartTime;

    private GolfTargetController activeHoleTarget;

    public bool HasBeenHit { get; private set; }

    private void Awake()
    {
        ballBody = GetComponent<Rigidbody>();

        // Save the ball's starting position on the mat.
        startingPosition = ballBody.position;
        startingRotation = ballBody.rotation;
    }

    private void FixedUpdate()
    {
        if (Time.timeScale == 0f || resetting)
        {
            return;
        }

        // Stop checking the ball after the session ends.
        if (gameManager != null &&
            gameManager.SessionEnded)
        {
            return;
        }

        // Allow the target to control the ball while it sinks.
        if (enteringHole)
        {
            if (Time.time - holeStartTime >=
                holeSequenceTimeout)
            {
                FinishMissedShot(
                    "Almost! Try aiming toward the centre."
                );
            }

            return;
        }

        // Reset a ball that falls outside the course.
        if (ballBody.position.y < -2f)
        {
            FinishMissedShot(
                "Try a gentler swing."
            );

            return;
        }

        // Reset accidental movement that was not a valid swing.
        if (!shotActive)
        {
            Vector3 offset =
                ballBody.position - startingPosition;

            offset.y = 0f;

            if (offset.magnitude >
                idleResetDistance)
            {
                idleAwayTime +=
                    Time.fixedDeltaTime;

                if (idleAwayTime >= idleResetWait)
                {
                    ResetBall();
                }
            }
            else
            {
                idleAwayTime = 0f;
            }

            return;
        }

        shotTime += Time.fixedDeltaTime;

        // Check whether the ball has stopped moving.
        if (ballBody.linearVelocity.magnitude <
            stoppedSpeed)
        {
            stoppedTime += Time.fixedDeltaTime;
        }
        else
        {
            stoppedTime = 0f;
        }

        // A stopped ball counts as a missed target.
        if (stoppedTime >= stoppedWait)
        {
            FinishMissedShot(
                "Almost! Adjust your aim or swing."
            );
        }
        else if (shotTime >= resetAfterSeconds)
        {
            FinishMissedShot(
                "Try a gentler swing or aim closer."
            );
        }
    }

    public void RegisterClubHit()
    {
        if (shotActive ||
            enteringHole ||
            resetting ||
            (gameManager != null &&
             gameManager.SessionEnded))
        {
            return;
        }

        // Mark this as one active attempt.
        HasBeenHit = true;
        shotActive = true;

        shotTime = 0f;
        stoppedTime = 0f;
        idleAwayTime = 0f;
    }

    // Kept for compatibility with older calls.
    public bool BeginHoleSequence()
    {
        return BeginHoleSequence(null);
    }

    public bool BeginHoleSequence(
        GolfTargetController target)
    {
        if (!HasBeenHit ||
            enteringHole ||
            resetting ||
            (gameManager != null &&
             gameManager.SessionEnded))
        {
            return false;
        }

        // Give the target control of the ball.
        enteringHole = true;
        activeHoleTarget = target;
        holeStartTime = Time.time;

        return true;
    }

    public void EndHoleSequence(
        GolfTargetController target)
    {
        if (activeHoleTarget == target)
        {
            activeHoleTarget = null;
            enteringHole = false;
            holeStartTime = 0f;
        }
    }

    private void FinishMissedShot(
        string feedbackMessage)
    {
        if (resetting)
        {
            return;
        }

        // Record only real shots as misses.
        if (shotActive && gameManager != null)
        {
            gameManager.ShowMiss(
                feedbackMessage
            );
        }

        ResetBall();
    }

    public void ResetBall()
    {
        if (resetting)
        {
            return;
        }

        resetting = true;

        GolfTargetController previousTarget =
            activeHoleTarget;

        activeHoleTarget = null;
        enteringHole = false;

        // Stop any unfinished sinking animation.
        if (previousTarget != null)
        {
            previousTarget.CancelHoleSequence();
        }

        // Disable physics while moving the ball.
        ballBody.isKinematic = true;
        ballBody.detectCollisions = false;

        ballBody.position = startingPosition;
        ballBody.rotation = startingRotation;

        Physics.SyncTransforms();

        ballBody.linearVelocity = Vector3.zero;
        ballBody.angularVelocity = Vector3.zero;

        // Restore normal ball physics.
        ballBody.detectCollisions = true;
        ballBody.isKinematic = false;

        HasBeenHit = false;
        shotActive = false;
        enteringHole = false;

        shotTime = 0f;
        stoppedTime = 0f;
        idleAwayTime = 0f;
        holeStartTime = 0f;

        
        resetting = false;
    }
}