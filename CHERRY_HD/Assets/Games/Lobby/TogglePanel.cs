using UnityEngine;
using TMPro;

public class PanelToggle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject targetPanel;
    [SerializeField] private TMP_Text buttonText;

    [Header("Button Labels")]
    [SerializeField] private string showText = "Show";
    [SerializeField] private string hideText = "Hide";

    private bool isVisible;

    private void Start()
    {
        isVisible = targetPanel.activeSelf;
        UpdateUI();
    }
    // Toggle the visibility of the target panel and update the button text accordingly
    public void Toggle()
    {
        isVisible = !isVisible;
        targetPanel.SetActive(isVisible);
        UpdateUI();
    }

    // Update the button text based on the current visibility state of the target panel
    private void UpdateUI()
    {
        if (buttonText != null)
        {
            buttonText.text = isVisible ? hideText : showText;
        }
    }
}