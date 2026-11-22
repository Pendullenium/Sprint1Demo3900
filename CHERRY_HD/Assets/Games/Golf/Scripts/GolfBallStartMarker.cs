
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BallStartMarker : MonoBehaviour
{
    // size of circle  around ball to show starting position
    public float radius = 0.23f;
    public int segments = 64;

    private void Awake()
    {
        LineRenderer line = GetComponent<LineRenderer>();

        // set up the line renderer to draw a circle
        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = segments;
        line.startWidth = 0.035f;
        line.endWidth = 0.035f;


        // loop to create points around the circle
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;

            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            line.SetPosition(i, new Vector3(x, 0f, z));
        }
    }
}
