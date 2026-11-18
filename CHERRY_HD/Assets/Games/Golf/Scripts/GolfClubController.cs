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

    private bool isHeld;
    private bool resetPending;

    private float shotsAllowedAfter;
    private float nextHitTime;

    private Coroutine resetRoutine;

    private void Awake()
    {
        clubBody = GetComponent<Rigidbody>();

        grabInteractable =
            GetComponent<XRGrabInteractable>();

        // Save the club's original position and rotation.
        startingPosition = clubBody.position;
        startingRotation = clubBody.rotation;

        // Find the club-head collider if it was not assigned.
        if (clubHead != null &&
            headCollider == null)
        {
            headCollider =
                clubHead.GetComponent<Collider>();
        }

        // Keep the club stable while it is not being held.
        clubBody.useGravity = false;
        clubBody.linearVelocity = Vector3.zero;
        clubBody.angularVelocity = Vector3.zero;
        clubBody.isKinematic = true;

        if (clubHead != null)
        {
            previousHeadPosition =
                clubHead.position;
        }
    }

    public void OnClubGrabbed()
    {
        // Do not allow new swings after the session ends.
        if (gameManager != null &&
            gameManager.SessionEnded)
        {
            return;
        }

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
            previousHeadPosition =
                clubHead.position;
        }

        // Ignore movement briefly after grabbing the club.
        shotsAllowedAfter =
            Time.time + grabProtectionSeconds;
    }

    public void OnClubReleased()
    {
        isHeld = false;
        clubVelocity = Vector3.zero;

        if (!clubBody.isKinematic)
        {
            clubBody.linearVelocity =
                Vector3.zero;

            clubBody.angularVelocity =
                Vector3.zero;
        }

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

        Vector3 currentPosition =
            clubHead.position;

        // Measure the speed and direction of the club head.
        Vector3 measuredVelocity =
            (currentPosition -
             previousHeadPosition) /
            Time.fixedDeltaTime;

        previousHeadPosition =
            currentPosition;

        if (!isHeld ||
            resetPending ||
            Time.time < shotsAllowedAfter)
        {
            clubVelocity = Vector3.zero;
            return;
        }

        // Only use horizontal movement for the golf shot.
        clubVelocity =
            Vector3.ProjectOnPlane(
                measuredVelocity,
                Vector3.up
            );
    }

    private void GetShotSettings(
        out float power,
        out float maximumSpeed,
        out float upwardSpeed)
    {
        int level = gameManager != null
            ? gameManager.CurrentTargetIndex
            : 0;

        // Use stronger settings for more distant targets.
        switch (level)
        {
            case 1:
                power = midHitPower;
                maximumSpeed = midMaxBallSpeed;
                upwardSpeed = midLiftSpeed;
                break;

            case 2:
                power = farHitPower;
                maximumSpeed = farMaxBallSpeed;
                upwardSpeed = farLiftSpeed;
                break;

            default:
                power = hitPower;
                maximumSpeed = maxBallSpeed;
                upwardSpeed = 0f;
                break;
        }
    }

    private void OnCollisionEnter(
        Collision collision)
    {
        if (!isActiveAndEnabled ||
            !isHeld ||
            resetPending ||
            (gameManager != null &&
             gameManager.SessionEnded))
        {
            return;
        }

        // Ignore collisions while paused or during cooldown.
        if (Time.timeScale == 0f ||
            Time.time < shotsAllowedAfter ||
            Time.time < nextHitTime)
        {
            return;
        }

        if (clubHead == null ||
            headCollider == null)
        {
            return;
        }

        Rigidbody ballBody =
            collision.rigidbody;

        if (ballBody == null ||
            ballBody.isKinematic)
        {
            return;
        }

        GolfBallController ball =
            ballBody.GetComponent<GolfBallController>();

        // Each ball reset allows only one registered hit.
        if (ball == null || ball.HasBeenHit)
        {
            return;
        }

        bool headTouchedBall = false;

        // Make sure the club head caused the collision.
        for (int i = 0;
             i < collision.contactCount;
             i++)
        {
            ContactPoint contact =
                collision.GetContact(i);

            if (contact.thisCollider ==
                headCollider)
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

        float swingSpeed =
            horizontalVelocity.magnitude;

        // Require a meaningful swing instead of a small tap.
        if (swingSpeed < minSwingSpeed)
        {
            if (gameManager != null)
            {
                gameManager.ShowGuidance(
                    "Swing a little stronger."
                );
            }

            return;
        }

        Vector3 direction =
            horizontalVelocity.normalized;

        GetShotSettings(
            out float selectedPower,
            out float selectedMaxSpeed,
            out float selectedLift
        );

        float launchSpeed = Mathf.Min(
            swingSpeed * selectedPower,
            selectedMaxSpeed
        );

        Vector3 launchVelocity =
            direction * launchSpeed +
            Vector3.up * selectedLift;

        // Launch the ball using the measured swing direction.
        ballBody.linearVelocity =
            launchVelocity;

        ballBody.angularVelocity =
            Vector3.zero;

        // Mark the ball as hit so it cannot score twice.
        ball.RegisterClubHit();

        // Reward the completed rehabilitation movement.
        if (gameManager != null)
        {
            gameManager.RegisterValidHit();
        }

        nextHitTime =
            Time.time + hitCooldown;

        // Give helpful feedback without changing the score.
        if (gameManager != null)
        {
            float speedRatio =
                launchSpeed /
                Mathf.Max(
                    0.01f,
                    selectedMaxSpeed
                );

            if (speedRatio >= 0.85f)
            {
                gameManager.ShowGuidance(
                    "Hit counted! Try a gentler swing."
                );
            }
            else if (speedRatio <= 0.3f)
            {
                gameManager.ShowGuidance(
                    "Hit counted! Try a little more power."
                );
            }
            else
            {
                gameManager.ShowGuidance(
                    "Good swing! Hit counted."
                );
            }
        }
    }

    public void ResetClub()
    {
        if (!isActiveAndEnabled ||
            clubBody == null)
        {
            return;
        }

        if (resetRoutine != null)
        {
            StopCoroutine(resetRoutine);
        }

        resetPending = true;

        resetRoutine =
            StartCoroutine(ResetClubRoutine());
    }

    private IEnumerator ResetClubRoutine()
    {
        yield return new WaitForSeconds(
            resetDelay
        );

        // Wait until the player releases the club.
        while (grabInteractable != null &&
               grabInteractable.isSelected)
        {
            yield return null;
        }

        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        isHeld = false;
        clubVelocity = Vector3.zero;

        clubBody.linearVelocity =
            Vector3.zero;

        clubBody.angularVelocity =
            Vector3.zero;

        clubBody.useGravity = false;
        clubBody.isKinematic = true;
        clubBody.detectCollisions = false;

        // Return the club to its original position.
        clubBody.position = startingPosition;
        clubBody.rotation = startingRotation;

        Physics.SyncTransforms();

        clubBody.detectCollisions = true;

        if (clubHead != null)
        {
            previousHeadPosition =
                clubHead.position;
        }

        shotsAllowedAfter =
            Time.time + grabProtectionSeconds;

        nextHitTime =
            Time.time + hitCooldown;

        resetPending = false;
        resetRoutine = null;
    }
}