using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public void doExitGame()
    {
        Debug.Log("End Game");
        Application.Quit();
    }
}
