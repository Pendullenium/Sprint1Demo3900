using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InstructionPanel : MonoBehaviour
{
    public TMP_Text title;
    public Image instructionImage;
    public TMP_Text subtext;

    [Header("Golf")]
    public Sprite golfImage;
    [TextArea] public string golfText;

    [Header("Air Hockey")]
    public Sprite airHockeyImage;
    [TextArea] public string airHockeyText;

    [Header("Tennis")]
    public Sprite tennisImage;
    [TextArea] public string tennisText;

    [Header("Goalkeeping")]
    public Sprite goalkeepingImage;
    [TextArea] public string goalkeepingText;

    [Header("Chest Pass")]
    public Sprite chestPassImage;
    [TextArea] public string chestPassText;

    public void ChangeInstructions(string sport)
    {
        switch (sport)
        {
            case "Golf":
                SetInstructions("How to play Golf", golfImage, golfText);
                break;
            
            case "Air Hockey":
                SetInstructions("How to play Air Hockey", airHockeyImage, airHockeyText);
                break;

            case "Tennis":
                SetInstructions("How to play Tennis", tennisImage, tennisText);
                break;


            case "Goalkeeping":
                SetInstructions("How to play Goalkeeping", goalkeepingImage, goalkeepingText);
                break;

            case "Chest Pass":
                SetInstructions("How to play Chest Pass", chestPassImage, chestPassText);
                break;
        }
    }

    private void SetInstructions(string newTitle, Sprite newImage, string newText)
    {
        title.text = newTitle;
        instructionImage.sprite = newImage;
        subtext.text = newText;
    }
}