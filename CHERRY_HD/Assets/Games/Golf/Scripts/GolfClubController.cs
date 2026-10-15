using UnityEngine;

public class GolfClubController : MonoBehaviour
{
   [Header("Club")]
    public Transform clubHead;

    [Header("Impact")]
    public float hitPower = 1.5f;
    public float loft = 0.15f;
    public float minSwingSpeed = 0.5f;

    private Vector3 previousPosition;
    private Vector3 clubVelocity;

    private void Start()
    {
        if (clubHead == null)
            clubHead = transform;

        previousPosition = clubHead.position;
    }

    private void Update()
    {
        // Measure how fast the club head moves.
        clubVelocity =
            (clubHead.position - previousPosition)
            / Time.deltaTime;

        previousPosition = clubHead.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("GolfBall"))
            return;

        Rigidbody ballRb =
            collision.gameObject.GetComponent<Rigidbody>();

        if (ballRb == null)
            return;

        float swingSpeed = clubVelocity.magnitude;

        // Ignore tiny accidental touches.
        if (swingSpeed < minSwingSpeed)
            return;

        // Use swing direction, with a little upward loft.
        Vector3 hitDirection = clubVelocity.normalized;
        hitDirection.y += loft;
        hitDirection.Normalize();

        // Faster swings produce stronger shots.
        Vector3 impulse =
            hitDirection * swingSpeed * hitPower;

        ballRb.AddForce(impulse, ForceMode.Impulse);
    }
}
