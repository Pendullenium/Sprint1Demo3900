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
    private bool leftHandMode;
    private bool usingHands;

    public void switchToHand()
    {
        usingHands = true;
        setTracker();
    }

    public void switchToController()
    {
        usingHands = false;
        setTracker();
    }

    public void setTracker()
    {
        if (usingHands)
        {
            puck = leftHandMode
            ? leftHand
            : rightHand;
        }
        else
        {
            puck = leftHandMode
            ? leftController
            : rightController;
        }

    }


    void Start()
    {
        leftHandMode = PlayerPrefs.GetInt("leftHandMode", 0) == 1;
        setTracker();
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
