
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody))]
public class GolfClubController : MonoBehaviour
{
    [Header("Club")]
    public Transform clubHead;
    public Collider headCollider;

    [Header("Game Manager")]
    public GolfGameManager gameManager;

    [Header("NEAR Shot Settings")]
    public float hitPower = 3.5f;
    public float maxBallSpeed = 14f;

    [Header("MID Shot Settings")]
    public float midHitPower = 4.5f;
    public float midMaxBallSpeed = 17f;

    [Header("FAR Shot Settings")]
    public float farHitPower = 5f;
    public float farMaxBallSpeed = 20f;

    [Header("Airborne Shot Settings")]
    [Min(0f)]
    public float midLiftSpeed = 2f;

    [Min(0f)]
    public float farLiftSpeed = 3f;

    [Header("Shared Shot Settings")]
    public float minSwingSpeed = 0.5f;
    public float hitCooldown = 1f;

    [Header("Grab Protection")]
    public float grabProtectionSeconds = 0.75f;

    [Header("Club Reset")]
    public float resetDelay = 0.5f;

    private Rigidbody clubBody;
    private XRGrabInteractable grabInteractable;

    private Vector3 clubVelocity;
    private Vector3 previousHeadPosition;

    private Vector3 startingPosition;
    private Quaternion startingRotation;

    private bool isHeld = false;
    private bool resetPending = false;

    private float shotsAllowedAfter;
    private float nextHitTime;

    private Coroutine resetRoutine;

    private void Awake()
    {
        clubBody = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        startingPosition = clubBody.position;
        startingRotation = clubBody.rotation;

        if (clubHead != null && headCollider == null)
        {
            headCollider = clubHead.GetComponent<Collider>();
        }

        if (grabInteractable == null)
        {
            Debug.LogError(
                "[CLUB] XR Grab Interactable is missing.",
                this
            );
        }

        clubBody.useGravity = false;
        clubBody.linearVelocity = Vector3.zero;
        clubBody.angularVelocity = Vector3.zero;
        clubBody.isKinematic = true;

        if (clubHead != null)
        {
            previousHeadPosition = clubHead.position;
        }

        Debug.Log(
            "[CLUB] Ready beside the hitting mat.",
            this
        );
    }

    public void OnClubGrabbed()
    {
        if (resetRoutine != null)
        {
            StopCoroutine(resetRoutine);
            resetRoutine = null;
        }

        resetPending = false;
        isHeld = true;
        clubVelocity = Vector3.zero;

        if (clubHead != null)
        {
            previousHeadPosition = clubHead.position;
        }

        shotsAllowedAfter =
            Time.time + grabProtectionSeconds;

        Debug.Log("[CLUB] Club grabbed.", this);
    }

    public void OnClubReleased()
    {
        isHeld = false;
        clubVelocity = Vector3.zero;

        if (clubBody != null && !clubBody.isKinematic)
        {
            clubBody.linearVelocity = Vector3.zero;
            clubBody.angularVelocity = Vector3.zero;
        }

        Debug.Log("[CLUB] Club released.", this);

        ResetClub();
    }

    private void OnDisable()
    {
        isHeld = false;
        resetPending = false;
        clubVelocity = Vector3.zero;

        if (resetRoutine != null)
        {
            StopCoroutine(resetRoutine);
            resetRoutine = null;
        }
    }

    private void FixedUpdate()
    {
        if (clubHead == null)
        {
            return;
        }

        Vector3 currentPosition = clubHead.position;

        Vector3 measuredVelocity =
            (currentPosition - previousHeadPosition)
            / Time.fixedDeltaTime;

        previousHeadPosition = currentPosition;

        if (!isHeld ||
            resetPending ||
            Time.time < shotsAllowedAfter)
        {
            clubVelocity = Vector3.zero;
            return;
        }

        // Preserve the original horizontal swing detection.
        clubVelocity = Vector3.ProjectOnPlane(
            measuredVelocity,
            Vector3.up
        );
    }

    // Choose shot power according to the active level.
    private void GetShotSettings(
        out float power,
        out float maximumSpeed,
        out float upwardSpeed)
    {
        int level = gameManager != null
            ? gameManager.CurrentTargetIndex
            : 0;

        switch (level)
        {
            case 1:
                // MID: a low airborne chip.
                power = midHitPower;
                maximumSpeed = midMaxBallSpeed;
                upwardSpeed = midLiftSpeed;
                break;

            case 2:
                // FAR: a slightly higher airborne chip.
                power = farHitPower;
                maximumSpeed = farMaxBallSpeed;
                upwardSpeed = farLiftSpeed;
                break;

            default:
                // NEAR: preserve the original rolling shot.
                power = hitPower;
                maximumSpeed = maxBallSpeed;
                upwardSpeed = 0f;
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isActiveAndEnabled ||
            !isHeld ||
            resetPending)
        {
            return;
        }

        if (Time.timeScale == 0f ||
            Time.time < shotsAllowedAfter ||
            Time.time < nextHitTime)
        {
            return;
        }

        if (clubHead == null || headCollider == null)
        {
            return;
        }

        Rigidbody ballBody = collision.rigidbody;

        if (ballBody == null || ballBody.isKinematic)
        {
            return;
        }

        GolfBallController ball =
            ballBody.GetComponent<GolfBallController>();

        if (ball == null || ball.HasBeenHit)
        {
            return;
        }

        // Confirm that the club head touched the ball.
        bool headTouchedBall = false;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint contact = collision.GetContact(i);

            if (contact.thisCollider == headCollider)
            {
                headTouchedBall = true;
                break;
            }
        }

        if (!headTouchedBall)
        {
            return;
        }

        Vector3 horizontalVelocity =
            Vector3.ProjectOnPlane(
                clubVelocity,
                Vector3.up
            );

        float swingSpeed = horizontalVelocity.magnitude;

        if (swingSpeed < minSwingSpeed)
        {
            Debug.Log(
                "[CLUB] Swing was too slow.",
                this
            );

            return;
        }

        Vector3 direction = horizontalVelocity.normalized;

        // Get power and lift for the currently active target.
        GetShotSettings(
            out float selectedPower,
            out float selectedMaxSpeed,
            out float selectedLift
        );

        float launchSpeed = Mathf.Min(
            swingSpeed * selectedPower,
            selectedMaxSpeed
        );

        // NEAR stays horizontal.
        // MID and FAR gain controlled upward velocity.
        Vector3 launchVelocity =
            direction * launchSpeed +
            Vector3.up * selectedLift;

        ballBody.linearVelocity = launchVelocity;
        ballBody.angularVelocity = Vector3.zero;

        // Preserve the original valid-hit registration.
        ball.RegisterClubHit();

        nextHitTime = Time.time + hitCooldown;

        string targetName = "NEAR";

        if (gameManager != null)
        {
            switch (gameManager.CurrentTargetIndex)
            {
                case 1:
                    targetName = "MID";
                    break;

                case 2:
                    targetName = "FAR";
                    break;
            }
        }

        Debug.Log(
            "[CLUB] " + targetName +
            " shot. Forward speed: " +
            launchSpeed.ToString("F2") +
            ", Lift: " +
            selectedLift.ToString("F2"),
            this
        );
    }

    // Returns the club after release or ball reset.
    public void ResetClub()
    {
        if (!isActiveAndEnabled || clubBody == null)
        {
            return;
        }

        if (resetRoutine != null)
        {
            StopCoroutine(resetRoutine);
            resetRoutine = null;
        }

        resetPending = true;
        resetRoutine = StartCoroutine(ResetClubRoutine());
    }

    private IEnumerator ResetClubRoutine()
    {
        yield return new WaitForSeconds(resetDelay);

        // Do not reset while the player is still holding it.
        while (grabInteractable != null &&
               grabInteractable.isSelected)
        {
            yield return null;
        }

        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        isHeld = false;
        clubVelocity = Vector3.zero;

        if (!clubBody.isKinematic)
        {
            clubBody.linearVelocity = Vector3.zero;
            clubBody.angularVelocity = Vector3.zero;
        }

        clubBody.useGravity = false;
        clubBody.isKinematic = true;

        clubBody.detectCollisions = false;

        clubBody.position = startingPosition;
        clubBody.rotation = startingRotation;

        Physics.SyncTransforms();

        clubBody.detectCollisions = true;

        if (clubHead != null)
        {
            previousHeadPosition = clubHead.position;
        }

        shotsAllowedAfter =
            Time.time + grabProtectionSeconds;

        nextHitTime =
            Time.time + hitCooldown;

        resetPending = false;
        resetRoutine = null;

        Debug.Log(
            "[CLUB] Returned to the hitting mat.",
            this
        );
    }
}
