using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class FinalScorePanel : MonoBehaviour
{
    [SerializeField]
    private TMP_Text finalScoreText;

    [SerializeField]
    private PanelPopUp worldSpacePanel;

    public void ShowFinalScore(string finalText)
    {
        finalScoreText.text = finalText;
        worldSpacePanel.ShowPanel();
    }

    public void HideFinalScore()
    {
        worldSpacePanel.HidePanel();
    }

    private void Update()
{
    if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
    {
        ShowFinalScore("hello");
    }
}
}