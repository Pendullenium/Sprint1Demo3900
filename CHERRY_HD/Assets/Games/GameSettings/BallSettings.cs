using UnityEngine;

public class BallSettings : MonoBehaviour
{
    [SerializeField] private Transform ball;

    private Vector3 originalScale;

    private void Awake()
    {
        // Store the original scale of the ball for later use
        originalScale = ball.localScale;
    }

    private void Start()
    {
        //Apply the saved ball size from PlayerPrefs, defaulting to 1 if not set
        float savedSize = PlayerPrefs.GetFloat("ballSize", 1f);
        ApplySize(savedSize);
    }

    private void ApplySize(float size)
    {
        ball.localScale = originalScale * size;
    }
}