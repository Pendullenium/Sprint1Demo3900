using UnityEngine;


[RequireComponent(typeof(Collider))]
public class ChestPassCatchZone : MonoBehaviour
{
    private void Reset()
    {
        Collider zone = GetComponent<Collider>();
        zone.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        ChestPassBall ball = other.GetComponentInParent<ChestPassBall>();
        if (ball != null)
        {
            ball.Catch();
        }
    }
}
