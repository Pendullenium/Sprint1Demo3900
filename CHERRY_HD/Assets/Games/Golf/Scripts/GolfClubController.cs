using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody))]
public class GolfClubController : MonoBehaviour
{
    // club head for hitting the ball
    [Header("Club")]
    public Transform clubHead;
    public Collider headCollider;

   // link to main golf game manager for scoring and feedback
    [Header("Game Manager")]
    public GolfGameManager gameManager;

   // power settings to reach different target distances, and lift for airborne shots
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

    // prevents a hit immediately after grabbing
    [Header("Grab Protection")]
    public float grabProtectionSeconds = 0.75f;

    // delay before club returns to original position after a hit or release
    [Header("Club Reset")]
    public float resetDelay = 0.5f;

    // sound played when the club head hits the ball
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip ballHitSound;

    //club head movement state
    private Rigidbody clubBody;
    private XRGrabInteractable grabInteractable;

    private Vector3 clubVelocity;
    private Vector3 previousHeadPosition;

    // club  original state after reset
    private Vector3 startingPosition;
    private Quaternion startingRotation;

    // flags if club is curerently held or if a reset is pending
    private bool isHeld;
    private bool resetPending;

    // prevents hits from happening too quickly
    private float shotsAllowedAfter;
    private float nextHitTime;

    private Coroutine resetRoutine;

    private void Awake()
    {
        clubBody = GetComponent<Rigidbody>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        grabInteractable =
            GetComponent<XRGrabInteractable>();

        // save the club's original position and rotation
        startingPosition = clubBody.position;
        startingRotation = clubBody.rotation;

        // find the club-head collider if it was not assigned
        if (clubHead != null &&
            headCollider == null)
        {
            headCollider =
                clubHead.GetComponent<Collider>();
        }

        // keep the club stable while it is not being held
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

    // when player grabs the club , ignore  any accidental movement
    public void OnClubGrabbed()
    {
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

        shotsAllowedAfter =
            Time.time + grabProtectionSeconds;
    }

   // when player releases the club, reset for next swing
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

    // clears the club state when disabled
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

        // measure the speed and direction of the club head
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

        // only use horizontal movement for the golf shot
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

        // use stronger settings for more distant targets
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

        // ignore collisions while paused or during cooldown
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

        // each ball reset allows only one registered hit
        if (ball == null || ball.HasBeenHit)
        {
            return;
        }

        bool headTouchedBall = false;

        // make sure the club head caused the collision
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

        // require a meaningful swing instead of a small tap
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

        // launch ball using the measured swing direction
        ballBody.linearVelocity =
            launchVelocity;

        ballBody.angularVelocity =
            Vector3.zero;

        // marks ball as hit so it cannot score twice
        ball.RegisterClubHit();

        // play sound  for a valid club hit
        if (audioSource != null &&
            ballHitSound != null)
        {
            audioSource.PlayOneShot(
                ballHitSound
            );
        }

        // reward the completed rehabilitation movement
        if (gameManager != null)
        {
            gameManager.RegisterValidHit();
        }

        nextHitTime =
            Time.time + hitCooldown;

        // give helpful feedback without changing the score
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

        // wait until the player releases the club
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
        clubBody.detectCollisions = false;

        // return the club to its original position
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