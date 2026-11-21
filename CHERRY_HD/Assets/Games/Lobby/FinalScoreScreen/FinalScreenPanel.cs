using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class FinalScorePanel : MonoBehaviour
{
    [SerializeField]
    private TMP_Text finalScoreText;

    [SerializeField]
    private PanelPopUp worldSpacePanel;

    // Show the final score panel with the provided text
    public void ShowFinalScore(string finalText)
    {
        finalScoreText.text = finalText;
        worldSpacePanel.ShowPanel();
    }
    
    public void HideFinalScore()
    {
        worldSpacePanel.HidePanel();
    }
    
}