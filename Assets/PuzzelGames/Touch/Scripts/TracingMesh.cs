using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.Touch
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class TracingMesh : MonoBehaviour
    {
    	public TracingController controller;
    	//[HideInInspector]
    	public float[] Fills;
    	public Color color = Color.red;
    	public float size = 1.0f;
    	public int circleResolution = 10;
    	public int endCircleResolution = 20;

    	MeshRenderer meshRenderer;
    	MeshFilter meshFilter;

    	float[] lastFill;


    	private void Awake()
    	{
    		if (controller == null)
    			controller = GetComponent<TracingController>();
    		Fills = new float[controller.paths.Length];
    		lastFill = new float[Fills.Length];
    	}

    	void Start()
    	{
    		meshRenderer = GetComponent<MeshRenderer>();
    		meshFilter = GetComponent<MeshFilter>();
    	}

    	List<Vector3> vertices = new List<Vector3>();
    	List<int> triangles = new List<int>();


    	// Update is called once per frame
    	void Update()
    	{
    		//if (lastFill == fill)
    		//    return;

    		vertices.Clear();
    		triangles.Clear();

    		bool changed = false;
    		// Check for changes!
    		for (int pathIdx = 0; pathIdx < controller.paths.Length; pathIdx++)
            {
    			if (lastFill[pathIdx] != Fills[pathIdx])
                {
    				changed = true;
    				break;
                }
            }
    		if (!changed)
    			return;

    		for (int pathIdx = 0; pathIdx < controller.paths.Length; pathIdx++)
    		{
    			TracingController.Path path = controller.paths[pathIdx];
    			if (Fills[pathIdx] > 0f)
    			{
    				float length = path.length;
    				float fillLength = length * Fills[pathIdx];
    				int lowerPt = 0;
    				int highPt = 0;
    				float distance = 0f;
    				float interpolation = 0f;
    				for (int i = 0; i < path.cachedDistances.Length; i++)
                    {
    					float oldDistance = distance;
    					distance += path.cachedDistances[i];
    					if (fillLength <= distance + Mathf.Epsilon)
    					{
    						lowerPt = i;
    						highPt = i + 1; // cached distances is less than one than max points
    						interpolation = (fillLength - oldDistance) / path.cachedDistances[i];
    						break;
    					}
                    }
				
    				float startRadius, endRadius;

    				// Fill the meshes
    				for (int i = 0; i < lowerPt; i++)
    				{
    					Vector3 startPt = transform.InverseTransformPoint(path.points[i]);
    					startRadius = path.radiuses[i];
    					Vector3 endPt = transform.InverseTransformPoint(path.points[i + 1]);
    					endRadius = path.radiuses[i + 1];
    					AddQuad(startPt, endPt, startRadius, endRadius);
    				}
    				for (int i = 0; i <= lowerPt; i++)
    				{
    					Vector3 pos = transform.InverseTransformPoint(path.points[i]);
    					float radius = path.radiuses[i];
    					AddCircle(pos, circleResolution, radius);
    				}

    				Vector3 start = transform.InverseTransformPoint(path.points[lowerPt]);
    				startRadius = path.radiuses[lowerPt];
    				Vector3 end = transform.InverseTransformPoint(path.points[highPt]);
    				endRadius = path.radiuses[highPt];
    				Vector3 positionInterpolation = Vector3.Lerp(start, end, interpolation);
    				float interpolationRadius = Mathf.Lerp(startRadius, endRadius, interpolation);
    				AddQuad(start, positionInterpolation, startRadius, interpolationRadius);
    				AddCircle(positionInterpolation, endCircleResolution, interpolationRadius);
    			}
    			lastFill[pathIdx] = Fills[pathIdx];
    		}

    		Mesh mesh = meshFilter.mesh;
    		if (mesh == null)
    		{
    			mesh = new Mesh();
    			//meshFilter.mesh = mesh;
    		} else
    		{
    			mesh.Clear();
    		}
    		Vector3[] normals = new Vector3[vertices.Count];
    		Color[] colors = new Color[vertices.Count];
    		for (int i = 0; i < vertices.Count; i++)
    		{
    			normals[i] = Vector3.forward;
    			colors[i] = color;
    		}
    		mesh.vertices = vertices.ToArray();
    		mesh.triangles = triangles.ToArray();
    		mesh.normals = normals;
    		mesh.colors = colors;
    		mesh.RecalculateBounds();
    		meshFilter.mesh = mesh;
    	}

    	void AddQuad(Vector3 startPt, Vector3 endPt, float sizeMultStart = 1.0f, float sizeMultEnd = 1.0f)
    	{
    		Vector3 diff = endPt - startPt;
    		Vector3 diffNorm = diff.normalized;
    		Vector3 perp = new Vector3(diffNorm.y, -diffNorm.x, 0f) * size;
    		Vector3 pt1 = startPt + perp * sizeMultStart;
    		Vector3 pt2 = startPt - perp * sizeMultStart;
    		Vector3 pt3 = endPt + perp * sizeMultEnd;
    		Vector3 pt4 = endPt - perp * sizeMultEnd;

    		int triangleStart = vertices.Count;
	 
    		vertices.Add(pt1);
    		vertices.Add(pt2);
    		vertices.Add(pt3);
    		vertices.Add(pt4);

    		triangles.Add(triangleStart);
    		triangles.Add(triangleStart + 1);
    		triangles.Add(triangleStart + 2);
									   
    		triangles.Add(triangleStart + 2);
    		triangles.Add(triangleStart + 1);
    		triangles.Add(triangleStart + 3);
    	}
	
    	void AddCircle(Vector3 center, int resolution, float radiusMultiplier = 1.0f)
    	{
    		int triangleStart = vertices.Count;
    		float increment = 2.0f * Mathf.PI / resolution;
    		float multSize = size * radiusMultiplier;
    		for (int i = 0; i < resolution; i++)
    		{
    			Vector3 ptPos = new Vector3(center.x + Mathf.Sin(i * increment) * multSize, center.y + Mathf.Cos(i * increment) * multSize, center.z);
    			vertices.Add(ptPos);
    		}
    		for (int i = 0; i < (resolution - 2); i++)
    		{
    			triangles.Add(triangleStart);
    			triangles.Add(triangleStart + i + 1);
    			triangles.Add(triangleStart + i + 2);
    		}
    	}
    }


}