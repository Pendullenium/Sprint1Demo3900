using UnityEngine;

// Place this trigger on a child of the existing virtual hand/controller.
// A valid incoming ball enters the hand's catch region and is counted once.
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
        if (ball != null) ball.Catch();
    }
}
