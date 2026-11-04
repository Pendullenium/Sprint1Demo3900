using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TennisRacketVisual : MonoBehaviour
{
    public Material frameMaterial;
    public Material stringMaterial;

    private const float RadiusX = 0.14f;
    private const float RadiusY = 0.18f;

    private void Start()
    {
        LineRenderer frame = GetComponent<LineRenderer>();

        frame.useWorldSpace = false;
        frame.loop = true;
        frame.positionCount = 64;
        frame.startWidth = 0.012f;
        frame.endWidth = 0.012f;
        frame.alignment = LineAlignment.TransformZ;
        frame.numCornerVertices = 3;

        if (frameMaterial != null)
            frame.sharedMaterial = frameMaterial;

        for (int i = 0; i < frame.positionCount; i++)
        {
            float angle =
                i * Mathf.PI * 2f / frame.positionCount;

            frame.SetPosition(i, new Vector3(
                Mathf.Cos(angle) * RadiusX,
                Mathf.Sin(angle) * RadiusY,
                0f));
        }

        for (int i = -6; i <= 6; i++)
        {
            float x = i * 0.02f;

            float height = RadiusY * Mathf.Sqrt(
                Mathf.Max(0f, 1f - x * x / (RadiusX * RadiusX)));

            CreateString(
                new Vector3(x, -height, 0f),
                new Vector3(x, height, 0f));
        }

        for (int i = -8; i <= 8; i++)
        {
            float y = i * 0.02f;

            float width = RadiusX * Mathf.Sqrt(
                Mathf.Max(0f, 1f - y * y / (RadiusY * RadiusY)));

            CreateString(
                new Vector3(-width, y, 0f),
                new Vector3(width, y, 0f));
        }
    }

    private void CreateString(Vector3 start, Vector3 end)
    {
        GameObject stringObject = new GameObject("RacketString");
        stringObject.transform.SetParent(transform, false);

        LineRenderer line =
            stringObject.AddComponent<LineRenderer>();

        line.useWorldSpace = false;
        line.positionCount = 2;
        line.startWidth = 0.002f;
        line.endWidth = 0.002f;
        line.alignment = LineAlignment.TransformZ;

        line.sharedMaterial = stringMaterial != null
            ? stringMaterial
            : frameMaterial;

        line.SetPosition(0, start);
        line.SetPosition(1, end);

        line.shadowCastingMode =
            UnityEngine.Rendering.ShadowCastingMode.Off;

        line.receiveShadows = false;
    }
}