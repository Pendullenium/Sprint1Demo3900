using System.Collections;
using UnityEngine;

public class GolfTargetController : MonoBehaviour
{
    // golf scoring hole
    [Header("Target")]
    public Transform hole;
    public GolfGameManager gameManager;

    // ball speed for sinking into the scoring hole
    [Header("Guided Ball Movement")]
    public float rollSpeed = 2f;
    public float sinkSpeed = 0.5f;
    public float sinkDepth = 0.4f;

    [Header("Reset")]
    public float resetDelay = 1.2f;

    private bool processingHit;
    private Coroutine holeRoutine;

    // handles when a ball enters the scoring zone
    public void HandleBallEntered(Collider other)
    {
        // ignore if already hit is made or if hole is not set or game ended
        if (processingHit ||
            hole == null ||
            gameManager == null ||
            gameManager.SessionEnded)
        {
            return;
        }

        GolfBallController ball =
            other.GetComponentInParent<GolfBallController>();

        // ball must first be hit by the golf club
        if (ball == null || !ball.HasBeenHit)
        {
            return;
        }

        // only the currently active target can score
        if (!gameManager.IsTargetActive(this))
        {
            return;
        }

        Rigidbody body = ball.GetComponent<Rigidbody>();

        if (body == null || body.isKinematic)
        {
            return;
        }

        // prevent duplicate scoring from repeated trigger events
        if (!ball.BeginHoleSequence(this))
        {
            return;
        }

        processingHit = true;

        holeRoutine = StartCoroutine(
            MoveBallIntoHole(ball, body)
        );
    }

    private IEnumerator MoveBallIntoHole(
        GolfBallController ball,
        Rigidbody body)
    {
        // disable physics and guide the ball safely
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.isKinematic = true;
        body.detectCollisions = false;

        // position the ball to roll into the hole
        Vector3 rollTarget = new Vector3(
            hole.position.x,
            body.position.y,
            hole.position.z
        );

        float moveSpeed = Mathf.Max(0.01f, rollSpeed);

       // roll the ball to the hole
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

        // set target position for sinking the ball into the hole
        Vector3 sinkTarget =
            rollTarget + Vector3.down * sinkDepth;

        float lowerSpeed = Mathf.Max(0.01f, sinkSpeed);

        // lower the ball into the hole
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

        // score exactly once
        gameManager.ScoreSuccess(this);

        yield return new WaitForSeconds(resetDelay);

        // stop and reset the ball after scoring
        processingHit = false;
        holeRoutine = null;

        ball.EndHoleSequence(this);
        ball.ResetBall();
    }

    public void CancelHoleSequence()
    {
        if (holeRoutine != null)
        {
            StopCoroutine(holeRoutine);
            holeRoutine = null;
        }

        processingHit = false;
    }

    // stop sinking ball into hole animation if the target is disabled
    private void OnDisable()
    {
        CancelHoleSequence();
    }
}