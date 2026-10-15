using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody))]
public class GolfBallController : MonoBehaviour
{
    public float hitSpeed = 4f;

    private Rigidbody ballBody;
    private Vector3 startingPosition;
    private Quaternion startingRotation;
    private bool hitRequested;
    private bool resetRequested;

    private void Awake()
    {
        ballBody = GetComponent<Rigidbody>();
        startingPosition = transform.position;
        startingRotation = transform.rotation;
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
    {
        hitRequested = false;
        resetRequested = false;
        return;
    }
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                hitRequested = true;
            }

            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                resetRequested = true;
            }
        }
#elif ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Space))
        {
            hitRequested = true;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            resetRequested = true;
        }
#endif
        // Reset automatically if the ball falls off the ground.
        if (transform.position.y < -2f)
        {
            resetRequested = true;
        }
    }

    private void FixedUpdate()
    {
        if (resetRequested)
        {
            ResetBall();
            resetRequested = false;
            hitRequested = false;
        }

        if (hitRequested)
        {
            // Allow a hit only when the ball is nearly stationary.
            if (ballBody.linearVelocity.magnitude < 0.1f)
            {
                ballBody.AddForce(
                    Vector3.forward * hitSpeed,
                    ForceMode.VelocityChange);
            }

            hitRequested = false;
        }
    }

    public void ResetBall()
    {
        ballBody.linearVelocity = Vector3.zero;
        ballBody.angularVelocity = Vector3.zero;
        ballBody.position = startingPosition;
        ballBody.rotation = startingRotation;
    }
}