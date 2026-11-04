
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BallStartMarker : MonoBehaviour
{
    public float radius = 0.23f;
    public int segments = 64;

    private void Awake()
    {
        LineRenderer line = GetComponent<LineRenderer>();

        // Create a flat circular outline.
        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = segments;
        line.startWidth = 0.035f;
        line.endWidth = 0.035f;



        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;

            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            line.SetPosition(i, new Vector3(x, 0f, z));
        }
    }
}
