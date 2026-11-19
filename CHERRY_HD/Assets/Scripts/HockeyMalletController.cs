using UnityEngine;

public class HockeyMalletController : MonoBehaviour
{
    [SerializeField] private Transform rightController;
    [SerializeField] private Transform rightHand;
    private Transform puck;
    private float fixedY;
    private float fixedZ;

    public void switchToHand()
    {
        Debug.Log("hand");
        puck = rightHand;
    }

    public void switchToController()
    {
        Debug.Log("controller");
        puck = rightController;
    }



    void Start()
    {
        fixedY = transform.position.y;
        fixedZ = transform.position.z;
    }

    void LateUpdate()
    {
        Vector3 position = puck.position;

        position.x = puck.position.x;
        position.y = fixedY;
        position.z = fixedZ;

        transform.position = position;
    }
}
