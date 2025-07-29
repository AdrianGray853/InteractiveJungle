using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class AquariumBoundary : MonoBehaviour
{
    private PolygonCollider2D polygon;

    private void Awake()
    {
        polygon = GetComponent<PolygonCollider2D>();
    }

    public bool IsInside(Vector2 point)
    {
        return polygon.OverlapPoint(point);
    }

    public Vector2 GetClosestPointInside(Vector2 point)
    {
        return polygon.ClosestPoint(point);
    }

    public Vector2 GetCenter()
    {
        return polygon.bounds.center;
    }
}
