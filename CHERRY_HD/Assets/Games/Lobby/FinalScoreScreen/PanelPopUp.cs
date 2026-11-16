using UnityEngine;


public class PanelPopUp : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform playerCamera;

    [Header("Position Settings")]
    [SerializeField]
    private float distance = 2f;

    [SerializeField]
    private float heightOffset = -0.2f;

    public void ShowPanel()
    {
        // Get the direction the player is looking
        Vector3 forward = playerCamera.forward;
        forward.y = 0;
        forward.Normalize();

        // Position panel in front of player
        transform.position = playerCamera.position
                           + forward * distance
                           + Vector3.up * heightOffset;

        // Make panel face the player
        transform.rotation = Quaternion.LookRotation(forward);

        gameObject.SetActive(true);
    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
    }

}