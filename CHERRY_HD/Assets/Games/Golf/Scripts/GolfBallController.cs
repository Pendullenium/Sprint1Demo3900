using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GolfBallController : MonoBehaviour
{   // ball reset settings
    [Header("Ball Reset")]
    public float resetAfterSeconds = 15f;
    public float stoppedSpeed = 0.2f;
    public float stoppedWait = 1.5f;

    // handles movement not caused by a valid shot
    [Header("Accidental Movement")]
    public float idleResetDistance = 0.4f;
    public float idleResetWait = 2f;

    // max time allowed for the hole animation
    [Header("Hole Safety")]
    public float holeSequenceTimeout = 12f;

    // links to the other golf scripts
    [Header("References")]
    public GolfGameManager gameManager;
    public GolfClubController golfClub;

    private Rigidbody ballBody;

    // original position and rotation of the ball on the mat
    private Vector3 startingPosition;
    private Quaternion startingRotation;

    // variables for ball 's current state
    private bool shotActive;
    private bool enteringHole;
    private bool resetting;

    // timers for ball resets and missed shots
    private float shotTime;
    private float stoppedTime;
    private float idleAwayTime;
    private float holeStartTime;

    private GolfTargetController activeHoleTarget;

    public bool HasBeenHit { get; private set; }

    private void Awake()
    {
        ballBody = GetComponent<Rigidbody>();

        // save the ball's starting position on the mat
        startingPosition = ballBody.position;
        startingRotation = ballBody.rotation;
    }

    private void FixedUpdate()
    {
         // do nothing while resetting or pause
        if (Time.timeScale == 0f || resetting)
        {
            return;
        }

        // stop checking the ball after the session ends
        if (gameManager != null &&
            gameManager.SessionEnded)
        {
            return;
        }

        // allow the target to control the ball while it sinks
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

        // reset a ball that falls outside the golf course area
        if (ballBody.position.y < -2f)
        {
            FinishMissedShot(
                "Try a gentler swing."
            );

            return;
        }

        // reset accidental movement that was not a valid swing
        if (!shotActive)
        {
            Vector3 offset =
                ballBody.position - startingPosition;

            // ignore veritcal movement when checking for accidental movement
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

        // check whether the ball has stopped moving
        if (ballBody.linearVelocity.magnitude <
            stoppedSpeed)
        {
            stoppedTime += Time.fixedDeltaTime;
        }
        else
        {
            stoppedTime = 0f;
        }

        // stopped ball counts as a missed target
        if (stoppedTime >= stoppedWait)
        {
            FinishMissedShot(
                "Almost! Adjust your aim or swing."
            );
        }
        // reset if shot takes too  long
        else if (shotTime >= resetAfterSeconds)
        {
            FinishMissedShot(
                "Try a gentler swing or aim closer."
            );
        }
    }

    public void RegisterClubHit()
    {   // ignore hits when ball is moving, going into hole  or session has ended
        if (shotActive ||
            enteringHole ||
            resetting ||
            (gameManager != null &&
             gameManager.SessionEnded))
        {
            return;
        }

        // mark this as one active attempt
        HasBeenHit = true;
        shotActive = true;

        // reset shot timers
        shotTime = 0f;
        stoppedTime = 0f;
        idleAwayTime = 0f;
    }

    // kept for compatibility with older calls
    public bool BeginHoleSequence()
    {
        return BeginHoleSequence(null);
    }

    public bool BeginHoleSequence(
        GolfTargetController target)
    {
        // only a valid golf shot rolls into hole
        if (!HasBeenHit ||
            enteringHole ||
            resetting ||
            (gameManager != null &&
             gameManager.SessionEnded))
        {
            return false;
        }

        // give the target control of the ball
        enteringHole = true;
        activeHoleTarget = target;
        holeStartTime = Time.time;

        return true;
    }
    // to end the current ball  sinking into hole after successful target is made
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

        // record only real shots as misses
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

        // keep the previous target so its animation can stop
        GolfTargetController previousTarget =
            activeHoleTarget;

        activeHoleTarget = null;
        enteringHole = false;

        // stop any unfinished sinking animation
        if (previousTarget != null)
        {
            previousTarget.CancelHoleSequence();
        }

        // disable physics while moving the ball
        ballBody.isKinematic = true;
        ballBody.detectCollisions = false;

        ballBody.position = startingPosition;
        ballBody.rotation = startingRotation;

        Physics.SyncTransforms();

        ballBody.linearVelocity = Vector3.zero;
        ballBody.angularVelocity = Vector3.zero;

        // restore normal ball physics
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