using UnityEngine;

// includes mouse and keyboard practice in the Unity Editor
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

    [Header("Hand Attachments")]
    [SerializeField] private Transform leftHandAttach;
    [SerializeField] private Transform rightHandAttach;

    // attaches the racket to the hand selected in the player's settings
    private void Start()
    {
        bool leftHandMode =
            PlayerPrefs.GetInt("leftHandMode", 0) == 1;

        Transform targetHand;

        targetHand = leftHandMode
            ? leftHandAttach
            : rightHandAttach;

        transform.SetParent(targetHand, false);

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

#if UNITY_EDITOR
    private bool swinging;
    private float elapsed;
    private float angle = -55f;
    private float sidewaysOffset;

    // reads desktop input and updates sideways movement and the swing angle
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

            // updates sideways movement based on speed and time between frames
            sidewaysOffset +=
                direction * moveSpeed * Time.deltaTime;

            // keeps sideways movement within the allowed range
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
        
        // calculates how far the swing has progressed
        float progress =
            elapsed / Mathf.Max(0.1f, swingDuration);

        // moves the racket backwards during the first part of the swing
        if (progress < 0.25f)
        {
            angle = Mathf.Lerp(
                -55f, -75f, progress / 0.25f);
        }
        else if (progress < 0.8f)
        {
            // swings the racket forwards
            angle = Mathf.Lerp(
                -75f, 65f,
                (progress - 0.25f) / 0.55f);
        }
        else
        {
            // returns the racket towards its resting angle
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

    // applies the racket's practice position and swing rotation after Update()
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