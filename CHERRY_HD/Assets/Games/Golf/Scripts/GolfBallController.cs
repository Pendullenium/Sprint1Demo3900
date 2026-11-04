
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

        // Save the original position on the white mat circle.
        startingPosition = ballBody.position;
        startingRotation = ballBody.rotation;
    }

    private void FixedUpdate()
    {
        if (Time.timeScale == 0f || resetting)
            return;

        // If a hole animation gets stuck, cancel and reset it.
        if (enteringHole)
        {
            if (Time.time - holeStartTime >= holeSequenceTimeout)
            {
                Debug.LogWarning("[BALL] Hole timeout.", this);
                ResetBall();
            }

            return;
        }

        // Recover balls that fall off the course.
        if (ballBody.position.y < -2f)
        {
            FinishMissedShot();
            return;
        }

        // Important: the club can sometimes push the ball
        // without registering a proper shot.
        // Return it if that happens.
        if (!shotActive)
        {
            Vector3 offset =
                ballBody.position - startingPosition;

            offset.y = 0f;

            if (offset.magnitude > idleResetDistance)
            {
                idleAwayTime += Time.fixedDeltaTime;

                if (idleAwayTime >= idleResetWait)
                {
                    Debug.Log(
                        "[BALL] Accidental movement detected.",
                        this
                    );

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

        // Check whether the ball has stopped rolling.
        if (ballBody.linearVelocity.magnitude < stoppedSpeed)
            stoppedTime += Time.fixedDeltaTime;
        else
            stoppedTime = 0f;

        // Every valid missed shot eventually ends.
        if (stoppedTime >= stoppedWait ||
            shotTime >= resetAfterSeconds)
        {
            FinishMissedShot();
        }
    }

    // Called after the golf club makes a valid hit.
    public void RegisterClubHit()
    {
        if (shotActive || enteringHole || resetting)
            return;

        HasBeenHit = true;
        shotActive = true;

        shotTime = 0f;
        stoppedTime = 0f;
        idleAwayTime = 0f;

        Debug.Log("[BALL] Shot registered.", this);
    }

    // Retained for compatibility with existing calls.
    public bool BeginHoleSequence()
    {
        return BeginHoleSequence(null);
    }

    public bool BeginHoleSequence(GolfTargetController target)
    {
        if (!HasBeenHit || enteringHole || resetting)
            return false;

        enteringHole = true;
        activeHoleTarget = target;
        holeStartTime = Time.time;

        Debug.Log("[BALL] Entering active hole.", this);

        return true;
    }

    public void EndHoleSequence(GolfTargetController target)
    {
        if (activeHoleTarget == target)
        {
            activeHoleTarget = null;
            enteringHole = false;
            holeStartTime = 0f;
        }
    }

    private void FinishMissedShot()
    {
        if (resetting)
            return;

        if (shotActive && gameManager != null)
            gameManager.ShowMiss();

        ResetBall();
    }

    public void ResetBall()
    {
        if (resetting)
            return;

        resetting = true;

        // Cancel an unfinished target animation before
        // teleporting the ball back to the mat.
        GolfTargetController previousTarget = activeHoleTarget;
        activeHoleTarget = null;
        enteringHole = false;

        if (previousTarget != null)
            previousTarget.CancelHoleSequence();

        // Temporarily disable physical movement.
        ballBody.isKinematic = true;
        ballBody.detectCollisions = false;

        ballBody.position = startingPosition;
        ballBody.rotation = startingRotation;

        Physics.SyncTransforms();

        // Resume normal ball physics.
        ballBody.linearVelocity = Vector3.zero;
        ballBody.angularVelocity = Vector3.zero;

        ballBody.detectCollisions = true;
        ballBody.isKinematic = false;

        HasBeenHit = false;
        shotActive = false;
        enteringHole = false;

        shotTime = 0f;
        stoppedTime = 0f;
        idleAwayTime = 0f;
        holeStartTime = 0f;

        Debug.Log("[BALL] RESET COMPLETE.", this);

        // Return the club after the shot as well.
        if (golfClub != null)
            golfClub.ResetClub();
        else
            Debug.LogWarning("[BALL] Golf Club not assigned.", this);

        resetting = false;
    }
}
