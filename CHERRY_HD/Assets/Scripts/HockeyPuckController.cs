using TMPro;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class HockeyPuckController : MonoBehaviour
{
    public int Score { get; private set; }
    public TMP_Text scoreText;
    public TMP_Text successText;
    private void Start()
    {
        UpdateScoreText();
    }

    private bool playerCanHit = true;

    public float playerHitSpeed = 4f;
    public float opponentHitSpeed = 4f;
    public float maxSpeed = 6f;


    public Transform opponentSide;
    public float startingSpeed = 4f;
    private Rigidbody puckBody;
    private Vector3 spawnPosition;
    private void Awake()
    {
        if (successText != null)
        {
            successText.gameObject.SetActive(false);
        }
        puckBody = GetComponent<Rigidbody>();
        Vector3 startingPosition = transform.position;
        spawnPosition = opponentSide.position;
        spawnPosition.y = startingPosition.y;
        RecoverPuck();

    }

    public float fallHeight = -2f;
    private void FixedUpdate()
    {
        if (transform.position.y < fallHeight)
        {
            RecoverPuck();
            return;
        }
        Vector3 velocity = puckBody.linearVelocity;
        velocity.y = 0f;

        if (velocity.magnitude > maxSpeed)
        {
            velocity = velocity.normalized * maxSpeed;
        }

        puckBody.linearVelocity = velocity;
    }


    private void RecoverPuck()
    {
        puckBody.linearVelocity = Vector3.zero;
        puckBody.angularVelocity = Vector3.zero;
        puckBody.position = spawnPosition;
        puckBody.position = new Vector3(spawnPosition.x, spawnPosition.y, spawnPosition.z);
        playerCanHit = true;

        puckBody.linearVelocity = Vector3.back * startingSpeed;


        Debug.Log($"Puck reset. Velocity: {puckBody.linearVelocity}");


    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("PlayerBumper"))
        {
            RecoverPuck();
            return;
        }

        if (collision.collider.CompareTag("PlayerPaddle"))
        {
            if (playerCanHit)
            {
                Score++;
                UpdateScoreText();
                StartCoroutine(ShowScoreText());

                Vector3 offset = transform.position - collision.transform.position;
                Vector3 velocity = puckBody.linearVelocity;

                velocity.z = playerHitSpeed;

                velocity.x = Mathf.Clamp(offset.x * 3f, -playerHitSpeed, playerHitSpeed);

                puckBody.linearVelocity = velocity;
                playerCanHit = false;
            }
        }

        if (collision.collider.CompareTag("OpponentPaddle"))
        {
            Debug.Log("Opponent paddle detected");
            playerCanHit = true;

            float randomAngle = Random.Range(-15f, 15f) + Random.Range(-15f, 15f);
            float angleRadians = randomAngle * Mathf.Deg2Rad;

            Vector3 velocity = puckBody.linearVelocity;

            velocity.z = -opponentHitSpeed * Mathf.Cos(angleRadians);

            velocity.x = -opponentHitSpeed * Mathf.Sin(angleRadians);

            puckBody.position += new Vector3(0f, 0f, -0.1f);

            puckBody.linearVelocity = velocity;
        }
    }


    private IEnumerator ShowScoreText()
    {
        if (successText != null)
        {
            successText.gameObject.SetActive(true);
        }

        Debug.Log("Good job! Target reached. Score: " + Score);

        yield return new WaitForSeconds(1f);
        if (successText != null)
        {
            successText.gameObject.SetActive(false);
        }

    }






    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + Score;
        }
    }

}