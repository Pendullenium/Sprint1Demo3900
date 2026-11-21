using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIGradient : BaseMeshEffect
{
    public Color topColor = Color.blue;
    public Color bottomColor = Color.magenta;

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive()) return;

        var rect = GetComponent<RectTransform>().rect;
        var vertex = new UIVertex();

        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);

            float height = Mathf.InverseLerp(
                rect.yMin,
                rect.yMax,
                vertex.position.y
            );

            Color gradientColor = Color.Lerp(
                bottomColor,
                topColor,
                height
            );

            vertex.color *= gradientColor;

            vh.SetUIVertex(vertex, i);
        }
    }
}