using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class FinalScorePanel : MonoBehaviour
{
    public TMP_Text finalScoreText;

    public Transform playerCamera;
    public float distance = 2f;
    public float heightOffset = -0.2f;

    public void ShowFinalScore(string finalText)
    {
        // Get the direction the player is looking
        Vector3 forward = playerCamera.forward;
        forward.y = 0;
        forward.Normalize();

        //Position the panel in front of the player
        transform.position = playerCamera.position
                           + forward * distance
                           + Vector3.up * heightOffset;

        // Make the panel face the player
        transform.rotation = Quaternion.LookRotation(forward);

        this.finalScoreText.text = finalText;
        gameObject.SetActive(true);
        
    }

    private void Update()
{
    if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
    {
        ShowFinalScore("hello");
    }
}
    
}
