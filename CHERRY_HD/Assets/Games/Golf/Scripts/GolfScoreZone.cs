using UnityEngine;

public class GolfScoreZone : MonoBehaviour
{
    public GolfTargetController target;

    private void OnTriggerEnter(Collider other)
    {
        if (target != null)
        {
            target.HandleBallEntered(other);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (target != null)
        {
            target.HandleBallEntered(other);
        }
    }
}