using UnityEngine;
public static class PolygonUtils
{
    public static bool IsPointInsidePolygon(Vector2[] polygon, Vector2 point, float offset = 0f)
    {
        Vector2 center = GetPolygonCenter(polygon);
        int crossings = 0;

        for (int i = 0; i < polygon.Length; i++)
        {
            Vector2 a = Vector2.Lerp(polygon[i], center, offset);
            Vector2 b = Vector2.Lerp(polygon[(i + 1) % polygon.Length], center, offset);

            if (((a.y > point.y) != (b.y > point.y)) &&
                (point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y + 0.0001f) + a.x))
            {
                crossings++;
            }
        }

        return (crossings % 2) == 1;
    }

    public static Vector2 GetPolygonCenter(Vector2[] polygon)
    {
        Vector2 center = Vector2.zero;
        foreach (var p in polygon)
            center += p;
        return center / polygon.Length;
    }
}
