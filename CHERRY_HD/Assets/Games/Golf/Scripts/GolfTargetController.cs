
using System.Collections;
using UnityEngine;

public class GolfTargetController : MonoBehaviour
{
    [Header("Target")]
    public Transform hole;
    public GolfGameManager gameManager;

    [Header("Ball Movement")]
    public float rollSpeed = 2f;
    public float sinkSpeed = 0.5f;
    public float sinkDepth = 0.4f;

    [Header("Airborne Detection")]
    [Min(0f)]
    public float maxCaptureHeight = 0.35f;

    [Min(0f)]
    public float maxCaptureVerticalSpeed = 1.5f;

    [Header("Reset")]
    public float resetDelay = 1.2f;

    private bool processingHit;
    private Coroutine holeRoutine;

    private void OnTriggerEnter(Collider other)
    {
        HandleBallEntered(other);
    }

    // Detect a ball that lands while still inside the zone.
    private void OnTriggerStay(Collider other)
    {
        HandleBallEntered(other);
    }

    public void HandleBallEntered(Collider other)
    {
        if (processingHit ||
            hole == null ||
            gameManager == null)
        {
            return;
        }

        GolfBallController ball =
            other.GetComponentInParent<GolfBallController>();

        if (ball == null || !ball.HasBeenHit)
        {
            return;
        }

        // Only the active target is allowed to score.
        if (!gameManager.IsTargetActive(this))
        {
            return;
        }

        Rigidbody body = ball.GetComponent<Rigidbody>();

        if (body == null || body.isKinematic)
        {
            return;
        }

        // NEW: Do not capture the ball high in the air.
        float heightAboveHole =
            body.position.y - hole.position.y;

        if (heightAboveHole > maxCaptureHeight)
        {
            return;
        }

        // NEW: Avoid capturing a ball still moving rapidly
        // upwards or downwards through the scoring zone.
        if (Mathf.Abs(body.linearVelocity.y) >
            maxCaptureVerticalSpeed)
        {
            return;
        }

        // Preserve the existing hole ownership system.
        if (!ball.BeginHoleSequence(this))
        {
            return;
        }

        processingHit = true;

        Debug.Log(
            "[TARGET] Ball accepted near ground: " + name,
            this
        );

        holeRoutine = StartCoroutine(
            RollIntoHole(ball, body)
        );
    }

    private IEnumerator RollIntoHole(
        GolfBallController ball,
        Rigidbody body)
    {
        // Stop natural physics during the guided animation.
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.isKinematic = true;
        body.detectCollisions = false;

        // Preserve the current rolling height.
        Vector3 rollTarget = new Vector3(
            hole.position.x,
            body.position.y,
            hole.position.z
        );

        float moveSpeed = Mathf.Max(
            0.01f,
            rollSpeed
        );

        // Guide the ball towards the hole.
        while (Vector3.Distance(
            body.position,
            rollTarget) > 0.02f)
        {
            body.MovePosition(
                Vector3.MoveTowards(
                    body.position,
                    rollTarget,
                    moveSpeed * Time.fixedDeltaTime
                )
            );

            yield return new WaitForFixedUpdate();
        }

        body.MovePosition(rollTarget);
        yield return new WaitForFixedUpdate();

        // Sink the ball into the hole.
        Vector3 sinkTarget =
            rollTarget + Vector3.down * sinkDepth;

        float lowerSpeed = Mathf.Max(
            0.01f,
            sinkSpeed
        );

        while (Vector3.Distance(
            body.position,
            sinkTarget) > 0.02f)
        {
            body.MovePosition(
                Vector3.MoveTowards(
                    body.position,
                    sinkTarget,
                    lowerSpeed * Time.fixedDeltaTime
                )
            );

            yield return new WaitForFixedUpdate();
        }

        body.MovePosition(sinkTarget);

        // Award points only for the active target.
        gameManager.ScoreSuccess(this);

        Debug.Log(
            "[TARGET] Score recorded: " + name,
            this
        );

        yield return new WaitForSeconds(resetDelay);

        // Finish the animation before resetting the ball.
        processingHit = false;
        holeRoutine = null;

        ball.EndHoleSequence(this);
        ball.ResetBall();
    }

    // Called when a safety reset interrupts the animation.
    public void CancelHoleSequence()
    {
        if (holeRoutine != null)
        {
            StopCoroutine(holeRoutine);
            holeRoutine = null;
        }

        processingHit = false;

        Debug.Log(
            "[TARGET] Animation cancelled.",
            this
        );
    }

    private void OnDisable()
    {
        CancelHoleSequence();
    }
}
