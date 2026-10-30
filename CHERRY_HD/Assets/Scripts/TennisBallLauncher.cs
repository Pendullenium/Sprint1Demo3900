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

    private Rigidbody body;
    private float countdown = 2f;
    private bool sessionEnded;
    private int nextLane;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.position = launchPosition;
    }

    private void Update()
    {
        if (sessionEnded)
            return;

        if (sessionTimer != null &&
            (!sessionTimer.timerRunning ||
             sessionTimer.timeRemaining <= 0f))
        {
            if (!body.isKinematic)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            body.isKinematic = true;
            sessionEnded = true;
            return;
        }

        countdown -= Time.deltaTime;

        if (countdown <= 0f)
        {
            LaunchBall();

            countdown =
                Mathf.Max(launchInterval, flightTime + 1f);
        }
    }

    private void LaunchBall()
    {
        GetComponent<TennisBallHit>().PrepareForLaunch();
        float duration = Mathf.Max(flightTime, 0.1f);
        
        float offset = 0f;

        if (nextLane == 1)
            offset = -sidewaysDistance;
        else if (nextLane == 2)
            offset = sidewaysDistance;

        nextLane = (nextLane + 1) % 3;

        Vector3 start = launchPosition;
        Vector3 destination = arrivalPosition;

        start.x += offset;
        destination.x += offset;

        body.isKinematic = true;
        body.position = start;
        body.rotation = Quaternion.identity;

        body.isKinematic = false;
        body.angularVelocity = Vector3.zero;

        body.linearVelocity =
            (destination - start) / duration
            - 0.5f * Physics.gravity * duration;
    }
}