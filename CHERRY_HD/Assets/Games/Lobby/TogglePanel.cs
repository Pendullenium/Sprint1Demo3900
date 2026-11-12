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

    public void Toggle()
    {
        isVisible = !isVisible;
        targetPanel.SetActive(isVisible);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (buttonText != null)
        {
            buttonText.text = isVisible ? hideText : showText;
        }
    }
}