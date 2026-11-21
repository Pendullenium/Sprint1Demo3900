
using UnityEngine;
using UnityEngine.UI;

public class UISelectedManager : MonoBehaviour
{
    public float unselectedAlpha = 0.4f;

    public void SelectButton(GameObject[] buttons, GameObject selectedButton)
    {
        foreach (GameObject button in buttons)
        {
            if (button == null) continue;

            bool isSelected = button == selectedButton;

            // Set button transparency
            Image image = button.GetComponent<Image>();

            if (image != null)
            {
                // Adjust the alpha based on selection
                Color color = image.color;
                color.a = isSelected ? 1f : unselectedAlpha;
                image.color = color;
            }

            // Set outline visibility
            Outline outline = button.GetComponent<Outline>();

            if (outline != null)
                outline.enabled = isSelected;
        }
    }

    public void ResetButtons(GameObject[] buttons)
{
    foreach (GameObject button in buttons)
    {
        if (button == null) continue;
        
        // Reset outline visibility
        Outline outline = button.GetComponent<Outline>();

        if (outline != null)
            outline.enabled = false;

        // Reset button transparency
        Image image = button.GetComponent<Image>();

        if (image != null)
        {
            Color color = image.color;
            color.a = 1f;
            image.color = color;
        }
    }
}
}