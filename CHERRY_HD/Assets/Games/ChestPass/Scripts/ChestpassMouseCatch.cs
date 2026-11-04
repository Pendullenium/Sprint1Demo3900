
using UnityEngine;
using UnityEngine.InputSystem;

public class ChestPassMouseCatch : MonoBehaviour
{
    [Header("Catch Settings")]
    [SerializeField] private float catchRadius = 0.65f;
    [SerializeField]
    private Vector3 catchOffset =
        new Vector3(0f, -0.3f, 0f);

    [Header("Debug")]
    [SerializeField] private bool showCatchArea = true;

    private void Update()
    {
        if (Mouse.current == null ||
            !Mouse.current.leftButton.isPressed)
        {
            return;
        }

        Vector3 catchCentre =
            transform.position +
            transform.TransformDirection(catchOffset);

        Collider[] hits = Physics.OverlapSphere(
            catchCentre,
            catchRadius,
            ~0,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hit in hits)
        {
            ChestPassBall ball =
                hit.GetComponentInParent<ChestPassBall>();

            if (ball != null)
            {
                ball.Catch();
                break;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showCatchArea)
            return;

        Gizmos.color = Color.green;

        Vector3 catchCentre =
            transform.position +
            transform.TransformDirection(catchOffset);

        Gizmos.DrawWireSphere(
            catchCentre,
            catchRadius
        );
    }
}
