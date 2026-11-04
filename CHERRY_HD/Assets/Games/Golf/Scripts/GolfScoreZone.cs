
using UnityEngine;

public class GolfScoreZone : MonoBehaviour
{
    public GolfTargetController target;

    private void OnTriggerEnter(Collider other)
    {
        if (target == null)
            return;

        target.HandleBallEntered(other);
    }
}
