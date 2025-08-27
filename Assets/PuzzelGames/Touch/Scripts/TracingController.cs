using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.Touch
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class TracingController : MonoBehaviour
    {
        [System.Serializable]
        public class Path
        {
            public Vector3[] points;
            public float[] radiuses;

            // Cached
            [HideInInspector]
            public float[] cachedDistances;
            [HideInInspector]
            public float length; 

            public void CachedDistances()
            {
                length = 0f;
                cachedDistances = new float[points.Length - 1];
                for (int i = 0; i < points.Length - 1; i++)
                {
                    float distance = points[i].Distance(points[i + 1]);
                    cachedDistances[i] = distance;
                    length += distance;
                }
            }

            public void TransformToWorld(Transform transform)
            {
                for (int i = 0; i < points.Length; i++)
                    points[i] = transform.TransformPoint(points[i]);
            }
        }

        //public Vector3[][] points = new Vector3[][] { new Vector3[] { } };
        public Path[] paths; // = new Path[] { new Path() { points = new Vector3[] { } } };
        public bool worldSpace = false;

        public void Init()
        {
            if (!worldSpace)
            {
                foreach (var path in paths)
                {
                    worldSpace = true;
                    path.TransformToWorld(transform);
                }
            }

            foreach (var path in paths)
                path.CachedDistances();
        }
    }


}