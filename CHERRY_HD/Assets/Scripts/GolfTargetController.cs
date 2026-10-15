using System.Collections;
using UnityEngine;
using TMPro;

public class GolfTargetController : MonoBehaviour
{
    public TMP_Text scoreText;
    public TMP_Text successText;

    public int Score { get; private set; }

    private bool processingHit;

    private void Start()
    {
        UpdateScoreText();
        if (successText != null)
        {
            successText.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GolfBallController ball =
            other.GetComponent<GolfBallController>();

        if (ball == null || processingHit)
        {
            return;
        }

        StartCoroutine(HandleSuccess(ball));
    }

    private IEnumerator HandleSuccess(GolfBallController ball)
    {
        processingHit = true;

        Rigidbody ballBody = ball.GetComponent<Rigidbody>();

        // Stop the ball and disable keyboard controls temporarily.
        ball.enabled = false;
        ballBody.linearVelocity = Vector3.zero;
        ballBody.angularVelocity = Vector3.zero;
        ballBody.isKinematic = true;

        // Add a point and update the visible score.
        Score = Score + 1;
        UpdateScoreText();

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

        // Reset the ball and enable controls again.
        ballBody.isKinematic = false;
        ball.ResetBall();
        ball.enabled = true;

        processingHit = false;
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + Score;
        }
    }
}