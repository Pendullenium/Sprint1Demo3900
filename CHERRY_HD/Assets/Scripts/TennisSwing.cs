using UnityEngine;

#if UNITY_EDITOR
using UnityEngine.InputSystem;
#endif

public class DesktopTennisSwing : MonoBehaviour
{
    public bool desktopPractice = true;

    public Vector3 practicePosition =
        new Vector3(0.35f, 0.75f, -6.3f);

    public float swingDuration = 0.7f;

    [Header("Desktop sideways movement")]
    public float moveSpeed = 1.5f;
    public float sidewaysRange = 0.9f;

#if UNITY_EDITOR
    private bool swinging;
    private float elapsed;
    private float angle = -55f;
    private float sidewaysOffset;

    private void Update()
    {
        if (!desktopPractice || Time.timeScale == 0f)
            return;

        Keyboard keyboard = Keyboard.current;

        if (keyboard != null)
        {
            float direction = 0f;

            if (keyboard.aKey.isPressed ||
                keyboard.leftArrowKey.isPressed)
            {
                direction -= 1f;
            }

            if (keyboard.dKey.isPressed ||
                keyboard.rightArrowKey.isPressed)
            {
                direction += 1f;
            }

            sidewaysOffset +=
                direction * moveSpeed * Time.deltaTime;

            sidewaysOffset = Mathf.Clamp(
                sidewaysOffset,
                -sidewaysRange,
                sidewaysRange);
        }

        if (!swinging &&
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            swinging = true;
            elapsed = 0f;
        }

        if (!swinging)
            return;

        elapsed += Time.deltaTime;

        float progress =
            elapsed / Mathf.Max(0.1f, swingDuration);

        if (progress < 0.25f)
        {
            angle = Mathf.Lerp(
                -55f, -75f, progress / 0.25f);
        }
        else if (progress < 0.8f)
        {
            angle = Mathf.Lerp(
                -75f, 65f,
                (progress - 0.25f) / 0.55f);
        }
        else
        {
            angle = Mathf.Lerp(
                65f, -55f,
                (progress - 0.8f) / 0.2f);
        }

        if (progress >= 1f)
        {
            swinging = false;
            angle = -55f;
        }
    }

    private void LateUpdate()
    {
        if (!desktopPractice)
            return;

        Vector3 position = practicePosition;
        position.x += sidewaysOffset;

        transform.SetPositionAndRotation(
            position,
            Quaternion.Euler(angle, 0f, 0f));
    }
#endif
}