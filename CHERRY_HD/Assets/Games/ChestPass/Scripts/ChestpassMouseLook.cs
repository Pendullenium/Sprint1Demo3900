
using UnityEngine;

public class ChestPassMouseLook : MonoBehaviour
{
    [Header("Mouse Settings")]
    public float mouseSensitivity = 0.15f;
    public bool lockCursor = true;

    private float verticalRotation = 0f;

    private void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Turn left and right freely.
        transform.Rotate(Vector3.up * mouseX, Space.World);

        // Look up and down, without flipping the camera.
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -75f, 75f);

        Vector3 angles = transform.localEulerAngles;
        angles.x = verticalRotation;
        angles.z = 0f;
        transform.localEulerAngles = angles;

        // Press Escape to release the mouse.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Left-click the Game view to capture the mouse again.
        if (Input.GetMouseButtonDown(0) && lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
