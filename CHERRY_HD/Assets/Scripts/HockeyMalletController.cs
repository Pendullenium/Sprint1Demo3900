using UnityEngine;

public class HockeyMalletController : MonoBehaviour
{
    [SerializeField] private Transform rightController;
    [SerializeField] private Transform leftController;
    [SerializeField] private Transform rightHand;
    [SerializeField] private Transform leftHand;
    private Transform puck;
    private float fixedY;
    private float fixedZ;
    private bool leftHandMode = false;


    public void switchToHand()
    {
        puck = leftHandMode
            ? leftHand
            : rightHand;
    }

    public void switchToController()
    {
        
        
        puck = leftHandMode
            ? leftController
            : rightController;
    }

    

    void Start()
    {
        bool leftHandMode = PlayerPrefs.GetInt("leftHandMode", 0) == 1;
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
