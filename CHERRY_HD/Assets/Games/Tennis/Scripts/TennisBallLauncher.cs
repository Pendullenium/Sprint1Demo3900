using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TennisBallLauncher : MonoBehaviour
{
    public Timer sessionTimer;
    public Transform racketHead;

    public Vector3 launchPosition =
        new Vector3(0.35f, 1.5f, 3f);

    public Vector3 arrivalPosition =
        new Vector3(0.35f, 0.034f, -4.8f);

    public float flightTime = 1.3f;

    public float launchInterval = 5f;

    [Header("Left and right throws")]
    public float sidewaysDistance = 0.45f;

    [Header("Opponent animation")]
    public Transform opponentRacket;
    public Transform opponentRacketHead;
    public float backswingTime = 0.5f;
    public float followThroughTime = 0.4f;

    [Header("Rally")]
    public float restartDelay = 2f;
    public float rallyTimeout = 8f;

    private Rigidbody body;
    private TennisBallHit hitController;
    private Quaternion readyRotation;

    private float countdown = 2f;
    private float deadline;
    private bool rallyActive;
    private bool animating;
    private bool sessionEnded;
    private int nextLane;

    private bool MatchFinished =>
        sessionTimer != null &&
        (!sessionTimer.timerRunning ||
         sessionTimer.timeRemaining <= 0f);

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        hitController = GetComponent<TennisBallHit>();

        body.isKinematic = true;
        body.position = launchPosition;
    }

    private void Start()
    {
        if (opponentRacket != null)
            readyRotation = opponentRacket.rotation;
    }

    private void Update()
    {
        if (sessionEnded)
            return;

        if (MatchFinished)
        {
            StopAllCoroutines();
            FreezeBall();
            sessionEnded = true;
            return;
        }

        if (animating)
            return;

        if (rallyActive)
        {
            Vector3 p = body.position;

            if (Time.time > deadline ||
                p.y < -1f ||
                p.z < -10f ||
                p.z > 11f ||
                Mathf.Abs(p.x) > 5.5f)
            {
                EndRally();
            }

            return;
        }

        countdown -= Time.deltaTime;

        if (countdown <= 0f)
        {
            rallyActive = true;

            StartCoroutine(
                OpponentShot(launchPosition, true));
        }
    }

    public void RefreshRally()
    {
        deadline = Time.time + rallyTimeout;
    }

    public void ReturnAfterBounce()
    {
        if (!rallyActive || animating || sessionEnded)
            return;

        animating = true;
        StartCoroutine(PrepareOpponentReturn());
    }

    private IEnumerator PrepareOpponentReturn()
    {
        yield return new WaitForSeconds(0.2f);

        Vector3 contactPosition = body.position;

        yield return OpponentShot(contactPosition, false);
    }

    public void EndRally()
    {
        if (sessionEnded)
            return;

        StopAllCoroutines();

        rallyActive = false;
        animating = false;
        countdown = restartDelay;

        hitController.CancelAttempt();
        FreezeBall();

        body.position = launchPosition;
    }

    private void FreezeBall()
    {
        if (!body.isKinematic)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        body.isKinematic = true;
    }

    private IEnumerator OpponentShot(
        Vector3 start, bool freshServe)
    {
        animating = true;
        FreezeBall();

        float offset = 0f;

        if (nextLane == 1)
            offset = -sidewaysDistance;
        else if (nextLane == 2)
            offset = sidewaysDistance;

        nextLane = (nextLane + 1) % 3;

        Vector3 destination = arrivalPosition;
        destination.x += offset;

        if (freshServe)
            start.x += offset;

        bool hasOpponent =
            opponentRacket != null &&
            opponentRacketHead != null;

        if (hasOpponent)
        {
            opponentRacket.rotation = readyRotation;

            Vector3 headOffset =
                opponentRacketHead.position -
                opponentRacket.position;

            opponentRacket.position =
                start + Vector3.forward * 0.06f - headOffset;

            float elapsed = 0f;
            float duration = Mathf.Max(0.1f, backswingTime);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                opponentRacket.rotation =
                    readyRotation *
                    Quaternion.Euler(
                        Mathf.Lerp(-50f, 0f, t), 0f, 0f);

                body.position =
                    opponentRacketHead.position -
                    Vector3.forward * 0.06f;

                yield return null;
            }

            opponentRacket.rotation = readyRotation;
        }

        body.position = start;
        body.rotation = Quaternion.identity;
        body.isKinematic = false;

        hitController.PrepareForLaunch();

        body.linearVelocity = hitController.CalculateVelocity(
            start, destination, flightTime);

        body.angularVelocity = Vector3.zero;
        RefreshRally();

        if (hasOpponent)
        {
            float elapsed = 0f;
            float duration =
                Mathf.Max(0.1f, followThroughTime);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                opponentRacket.rotation =
                    readyRotation *
                    Quaternion.Euler(
                        Mathf.Lerp(0f, 55f, t), 0f, 0f);

                yield return null;
            }
        }

        animating = false;
    }
}