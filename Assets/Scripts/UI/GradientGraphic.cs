using UnityEngine;
using UnityEngine.UI;

public class GradientGraphic : MaskableGraphic
{
    public enum Orientation { Horizontal, Vertical }
    public Orientation orientation = Orientation.Horizontal;
    public Color startColor = new Color(0f, 0f, 0f, 0.7f);
    public Color endColor = new Color(0f, 0f, 0f, 0f);

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        var r = GetPixelAdjustedRect();
        var v0 = UIVertex.simpleVert;
        var v1 = UIVertex.simpleVert;
        var v2 = UIVertex.simpleVert;
        var v3 = UIVertex.simpleVert;
        v0.position = new Vector2(r.xMin, r.yMin);
        v1.position = new Vector2(r.xMax, r.yMin);
        v2.position = new Vector2(r.xMax, r.yMax);
        v3.position = new Vector2(r.xMin, r.yMax);
        if (orientation == Orientation.Horizontal)
        {
            v0.color = startColor;
            v3.color = startColor;
            v1.color = endColor;
            v2.color = endColor;
        }
        else
        {
            v0.color = startColor;
            v1.color = startColor;
            v2.color = endColor;
            v3.color = endColor;
        }
        vh.AddVert(v0);
        vh.AddVert(v1);
        vh.AddVert(v2);
        vh.AddVert(v3);
        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }

    public void SetColors(Color a, Color b)
    {
        startColor = a;
        endColor = b;
        SetVerticesDirty();
    }
}
