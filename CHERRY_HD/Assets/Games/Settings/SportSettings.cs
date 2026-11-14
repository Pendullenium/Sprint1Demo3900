using UnityEngine;

public class BallSettings : MonoBehaviour
{
    [SerializeField] private Transform ball;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = ball.localScale;
    }

    private void Start()
    {
        float savedSize = PlayerPrefs.GetFloat("ballSize", 1f);
        ApplySize(savedSize);
    }

    private void ApplySize(float size)
    {
        ball.localScale = originalScale * size;
    }
}