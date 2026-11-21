using TMPro;
using UnityEngine;

public class OpponentHockeyPaddleController : MonoBehaviour
{
    
    public Transform puck;
    

    private void FixedUpdate()
    {
        Vector3 moveToPosition = transform.position;
        moveToPosition.x = puck.position.x;

        transform.position = moveToPosition;
    }





}