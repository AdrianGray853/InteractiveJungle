using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Interactive.DRagDrop
{
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

    public static class Utils
    {
    	public static Vector3 ToVector3(this Vector2 me, float offset = 0.0f)
    	{
    		return new Vector3(me.x, me.y, offset);
    	}
    	public static Vector2 ToVector2(this Vector3 me)
    	{
    		return new Vector2(me.x, me.y);
    	}
    	public static Vector3 SetX(this Vector3 me, float x)
    	{
    		return new Vector3(x, me.y, me.z);
    	}
    	public static Vector3 SetY(this Vector3 me, float y)
    	{
    		return new Vector3(me.x, y, me.z);
    	}
    	public static Vector3 SetZ(this Vector3 me, float z)
    	{
    		return new Vector3(me.x, me.y, z);
    	}
    	public static Vector2 SetX(this Vector2 me, float x)
    	{
    		return new Vector2(x, me.y);
    	}
    	public static Vector2 SetY(this Vector2 me, float y)
    	{
    		return new Vector2(me.x, y);
    	}

    	public static float Distance(this Vector3 me, Vector3 other)
    	{
    		return (me - other).magnitude;
    	}

    	public static float DistanceSq(this Vector3 me, Vector3 other)
    	{
    		return (me - other).sqrMagnitude;
    	}

    	public static float Distance(this Vector2 me, Vector2 other)
    	{
    		return (me - other).magnitude;
    	}

    	public static float DistanceSq(this Vector2 me, Vector2 other)
    	{
    		return (me - other).sqrMagnitude;
    	}

    	public static void Shuffle<T>(T[] array)
    	{
    		int n = array.Length;
    		while (n > 1)
    		{
    			int k = Random.Range(0, n--);
    			T temp = array[n];
    			array[n] = array[k];
    			array[k] = temp;
    		}
    	}

    	public static void Shuffle<T>(IList<T> list)
    	{
    		int n = list.Count;
    		while (n > 1)
    		{
    			int k = Random.Range(0, n--);
    			T temp = list[n];
    			list[n] = list[k];
    			list[k] = temp;
    		}
    	}

    	public static bool In<T>(T element, params T[] set)
    	{
    		return set.Contains(element);
    	}

    	public static T GetRandomElement<T>(this IList<T> list)
        {
    		if (list == null || list.Count == 0)
    			return default(T);
    		return list[Random.Range(0, list.Count)];
        }

    	public static T GetRandomElement<T>(this T[] array)
    	{
    		if (array == null || array.Length == 0)
    			return default(T);
    		return array[Random.Range(0, array.Length)];
    	}

    	public static Vector3 GetRandomPosition(this Bounds bounds)
        {
    		return new Vector3(
    			Random.Range(bounds.min.x, bounds.max.x),
    			Random.Range(bounds.min.y, bounds.max.y),
    			Random.Range(bounds.min.z, bounds.max.z)
    			);
        }
    	public static Vector3 ProjectPointSegmentClamped(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    	{
    		Vector3 rhs = point - lineStart;
    		Vector3 vector = lineEnd - lineStart;
    		float magnitude = vector.magnitude;
    		Vector3 vector2 = vector;
    		if (magnitude > 1E-06f)
    		{
    			vector2 /= magnitude;
    		}

    		float value = Vector3.Dot(vector2, rhs);
    		value = Mathf.Clamp(value, 0f, magnitude);
    		return lineStart + vector2 * value;
    	}

    	public static float DistancePointSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    	{
    		return Vector3.Magnitude(ProjectPointSegmentClamped(point, lineStart, lineEnd) - point);
    	}

    	public static float GetTFromProjectPointSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    	{
    		Vector3 rhs = point - lineStart;
    		Vector3 vector = lineEnd - lineStart;
    		float value = Vector3.Dot(vector.normalized, rhs);
    		return value / vector.magnitude;
    	}

    	public static Vector2 AngleToVector2(float angle)
    	{
    		float radians = angle * Mathf.Deg2Rad;
    		return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)); 
    	}
    	public static Vector3 AngleToVector3(float angle)
    	{
    		float radians = angle * Mathf.Deg2Rad;
    		return new Vector3(Mathf.Cos(radians), Mathf.Sin(radians));
    	}

    	public static float NormalizeAngle360(float angle)
    	{
    		angle = angle % 360.0f;
    		if (angle < 0)
    		{
    			angle += 360.0f;
    		}
    		return angle;
    	}

    	public static bool IsPointInBox3D(Vector3 point, Vector3 boxCenter, Vector3 boxSize)
        {
    		boxSize *= 0.5f; // Reduce box size to half
    		if (point.x < boxCenter.x - boxSize.x || point.x > boxCenter.x + boxSize.x)
    			return false;
    		if (point.y < boxCenter.y - boxSize.y || point.y > boxCenter.y + boxSize.y)
    			return false;
    		if (point.z < boxCenter.z - boxSize.z || point.z > boxCenter.z + boxSize.z)
    			return false;
    		return true;
        }

    	public static bool IsPointInBox2D(Vector3 point, Vector3 boxCenter, Vector3 boxSize)
    	{
    		boxSize *= 0.5f; // Reduce box size to half
    		if (point.x < boxCenter.x - boxSize.x || point.x > boxCenter.x + boxSize.x)
    			return false;
    		if (point.y < boxCenter.y - boxSize.y || point.y > boxCenter.y + boxSize.y)
    			return false;
    		return true;
    	}

    	public static float FitToSizeScale(Vector2 content, Vector2 size)
    	{
    		float scale;
    		float contentRatio = content.x / content.y;
    		float boxRatio = size.x / size.y;
    		if (contentRatio > boxRatio)
    		{
    			scale = size.x / content.x;
    		}
    		else
    		{
    			scale = size.y / content.y;
    		}
    		return scale;
    	}

    	public static float GetAngle(Vector2 start, Vector2 end)
        {
    		return Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;
        }

    	public static float GetAngle(Vector2 vec)
    	{
    		return Mathf.Atan2(vec.y, vec.x) * Mathf.Rad2Deg;
    	}

    	// I hate Unity for this!
    	public static float RotationTo180(float rotation360)
    	{
    		if (rotation360 > 180f)
    		{
    			return rotation360 - 360f;
    		}
    		else
    		{
    			return rotation360;
    		}
    	}

    	public static void SetAlpha(this RawImage image, float alpha)
        {
    		Color color = image.color;
    		color.a = alpha;
    		image.color = color;
        }

    	public static void SetAlpha(this Image image, float alpha)
    	{
    		Color color = image.color;
    		color.a = alpha;
    		image.color = color;
    	}

    	public static void SetAlpha(this SpriteRenderer sprite, float alpha)
    	{
    		Color color = sprite.color;
    		color.a = alpha;
    		sprite.color = color;
    	}
    }


}