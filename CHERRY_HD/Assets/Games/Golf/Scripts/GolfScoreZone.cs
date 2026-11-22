using UnityEngine;

public class GolfScoreZone : MonoBehaviour
{
    public GolfTargetController target;

    // target connected to scoring zone
    private void OnTriggerEnter(Collider other)
    {
        if (target != null)
        {
            target.HandleBallEntered(other);
        }
    }

    // checks when ball enters scoring zone around the hole
    private void OnTriggerStay(Collider other)
    {
        if (target != null)
        {
            target.HandleBallEntered(other);
        }
    }
}