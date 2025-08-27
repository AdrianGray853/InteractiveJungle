using UnityEditor;
using UnityEngine;

namespace Interactive.Touch
{
using UnityEditor;
using UnityEngine;

    [CustomEditor(typeof(TracingController))]
    public class YourScriptEditor : Editor
    {
    	private TracingController script;
    	private bool editPoints;
    	private bool addPoint;
    	private bool removePoint;
    	[SerializeField]
    	private int pathIndex = -1;
    	[SerializeField]
    	private bool showRadius = false;
    	[SerializeField]
    	private bool snapToEdge = true;
    	[SerializeField]
    	private float snapDistance = 0.1f;
    	[SerializeField]
    	private int selectedPointIndex = -1;
    	[SerializeField]
    	private float defaultRadius = 1.0f;

    	private void OnEnable()
    	{
    		script = (TracingController)target;
    		SceneView.duringSceneGui += OnSceneGUI;
    	}

    	private void OnDisable()
    	{
    		SceneView.duringSceneGui -= OnSceneGUI;
    	}

    	public override void OnInspectorGUI()
    	{
    		EditorGUILayout.BeginHorizontal();

    		bool oldEditPoint = editPoints;
    		bool oldAddPoint = addPoint;
    		bool oldRemovePoint = removePoint;

    		GUI.enabled = (script.paths != null && script.paths.Length > 0);

    		editPoints = GUILayout.Toggle(editPoints, "Edit Points", "Button");
    		addPoint = GUILayout.Toggle(addPoint, "+ Point", "Button");
    		removePoint = GUILayout.Toggle(removePoint, "- Point", "Button");

    		GUI.enabled = true;

    		if (!oldEditPoint && editPoints)
    		{
    			addPoint = false;
    			removePoint = false;
    		}

    		if (!oldAddPoint && addPoint)
    		{
    			editPoints = false;
    			removePoint = false;
    		}

    		if (!oldRemovePoint && removePoint)
    		{
    			addPoint = false;
    			editPoints = false;
    		}

    		if (addPoint != oldAddPoint || removePoint != oldRemovePoint || editPoints != oldEditPoint)
    			SceneView.RepaintAll();

    		if (GUILayout.Button("+ Path"))
    		{
    			Undo.SetCurrentGroupName("Undo Add Path");
    			Undo.RecordObject(script, "Undo Add Path");
    			Undo.RecordObject(this, "Undo Set Path");
    			int undoGroup = Undo.GetCurrentGroup();

    			if (script.paths == null)
    				script.paths = new TracingController.Path[] { };
    			ArrayUtility.Add(ref script.paths, new TracingController.Path() { points = new Vector3[] { }, radiuses = new float[] { } });
    			pathIndex = script.paths.Length - 1;
    			addPoint = false;
    			editPoints = false;
    			removePoint = false;
    			SceneView.RepaintAll();

    			Undo.CollapseUndoOperations(undoGroup);
    		}
    		GUI.enabled = (script.paths != null && script.paths.Length > 0);
    		if (GUILayout.Button("- Path"))
    		{
    			Undo.SetCurrentGroupName("Undo Remove Path");
    			Undo.RecordObject(script, "Undo Remove Path");
    			Undo.RecordObject(this, "Undo Set Path");
    			int undoGroup = Undo.GetCurrentGroup();

    			ArrayUtility.RemoveAt(ref script.paths, pathIndex);
    			pathIndex = script.paths.Length - 1;
    			addPoint = false;
    			editPoints = false;
    			removePoint = false;
    			SceneView.RepaintAll();

    			Undo.CollapseUndoOperations(undoGroup);
    		}
    		GUI.enabled = true;

    		EditorGUILayout.EndHorizontal();

    		if (addPoint || editPoints)
    		{
    			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

    			if (editPoints)
    			{ 
    				if (script.paths != null && script.paths.Length > 0 && selectedPointIndex >= 0)
    				{
    					float newRadius = script.paths[pathIndex].radiuses[selectedPointIndex];
    					newRadius =	EditorGUILayout.FloatField("Point Radius: ", newRadius);
    					if (newRadius != script.paths[pathIndex].radiuses[selectedPointIndex])
    					{
    						Undo.RecordObject(script, "Undo Change Radius");
    						script.paths[pathIndex].radiuses[selectedPointIndex] = newRadius;
    						SceneView.RepaintAll();
    					}
    				}
    			}

    			bool newSnapToEdge = snapToEdge;
    			newSnapToEdge = EditorGUILayout.Toggle("Create on edge", newSnapToEdge);
    			if (newSnapToEdge != snapToEdge)
                {
    				Undo.RecordObject(this, "Undo Snap To Edge");
    				snapToEdge = newSnapToEdge;
                }

    			float newSnapDistance = snapDistance;
    			newSnapDistance = EditorGUILayout.FloatField("Snap Distance: ", newSnapDistance);
    			if (newSnapDistance != snapDistance)
                {
    				Undo.RecordObject(this, "Undo Snap Distance");
    				snapDistance = newSnapDistance;
                }
    		}

    		EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

    		GUI.enabled = (script.paths != null && script.paths.Length > 0);

    		bool newShowRadius = showRadius;
    		newShowRadius = EditorGUILayout.Toggle("Show Radius", newShowRadius);
    		if (newShowRadius != showRadius)
    		{
    			Undo.RecordObject(this, "Undo Show Radius");
    			showRadius = newShowRadius;
    			SceneView.RepaintAll();
    		}

    		float newDefaultRadius = defaultRadius;
    		newDefaultRadius = EditorGUILayout.FloatField("Radius", newDefaultRadius);
    		if (newDefaultRadius != defaultRadius)
    		{
    			Undo.RecordObject(this, "Undo Radius");
    			defaultRadius = newDefaultRadius;
    		}

    		int newIndex = pathIndex;
    		if (script.paths != null && script.paths.Length > 0)
    			newIndex = EditorGUILayout.IntSlider("Path Index: ", newIndex, 0, script.paths.Length - 1);
    		else
    			newIndex = -1;
    		if (newIndex != pathIndex)
    		{
    			Undo.RecordObject(this, "Undo Path Index");
    			pathIndex = newIndex;
    			selectedPointIndex = -1;
    			SceneView.RepaintAll();
    		}
    		/*
    		if (pathIndex > script.paths.Length)
    		{
    			int prevLen = script.paths.Length;
    			System.Array.Resize(ref script.paths, pathIndex + 1);
    			for (int i = prevLen; i <= pathIndex; i++)
    				script.paths[i].points = new Vector3[] { };
    		}
    		*/
    		EditorGUILayout.BeginHorizontal();
    		if (GUILayout.Button("To World") && script.worldSpace == false)
    		{
    			Undo.RecordObject(script, "To World");
    			for (int j = 0; j < script.paths.Length; j++)
    			{
    				for (int i = 0; i < script.paths[j].points.Length; i++)
    				{
    					script.paths[j].points[i] = script.transform.TransformPoint(script.paths[j].points[i]);
    				}
    			}
    			script.worldSpace = true;
    			SceneView.RepaintAll();
    		}
    		if (GUILayout.Button("To Local") && script.worldSpace == true)
    		{
    			Undo.RecordObject(script, "To Local");
    			for (int j = 0; j < script.paths.Length; j++)
    			{
    				for (int i = 0; i < script.paths[j].points.Length; i++)
    				{
    					script.paths[j].points[i] = script.transform.InverseTransformPoint(script.paths[j].points[i]);
    				}
    			}
    			script.worldSpace = false;
    			SceneView.RepaintAll();
    		}
    		EditorGUILayout.EndHorizontal();

    		GUI.enabled = true;

    		EditorGUILayout.Space();

    		EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

    		DrawDefaultInspector();
    	}

    	private void OnSceneGUI(SceneView sceneView)
    	{
    		if (script.paths == null || script.paths.Length <= 0)
    			return;

    		if (addPoint)
    		{
    			HandleAddPoint();
    		}

    		if (removePoint)
    		{
    			HandleRemovePoint();
    		}

    		if (editPoints)
    		{
    			HandleEditPoints();
    		} 
    		else
    		{
    			selectedPointIndex = -1;
    		}

    		if (Selection.activeInstanceID == script.gameObject.GetInstanceID())
    		{

    			for (int pathIdx = 0; pathIdx < script.paths.Length; pathIdx++)
    			{
    				Handles.color = (pathIdx == pathIndex) ? Color.green : Color.gray;
    				//if (!script.worldSpace)
    				//	Handles.matrix = script.transform.localToWorldMatrix;

    				if (!editPoints && !removePoint)
    				{
    					for (int i = 0; i < script.paths[pathIdx].points.Length; i++)
    					{
    						// Get the position of the point in world space
    						Vector3 point = script.paths[pathIdx].points[i];
    						if (!script.worldSpace)
    							point = script.transform.TransformPoint(point);

    						// Draw a wire sphere at the point position
    						//float size = HandleUtility.GetHandleSize(point) * 0.05f;

    						if (showRadius)
    							Handles.DrawWireDisc(point, Vector3.back, script.paths[pathIdx].radiuses[i]);
    						float size = Mathf.Min(0.1f, HandleUtility.GetHandleSize(point) * 0.1f);
    						Handles.CylinderHandleCap(0, point, Quaternion.identity, size, EventType.Repaint);
    					}
    				}

    				if (!script.worldSpace)
    					Handles.matrix = script.transform.localToWorldMatrix;
    				Handles.DrawPolyLine(script.paths[pathIdx].points);
    				Handles.matrix = Matrix4x4.identity;
    			}
    		}

    		// Repaint the scene view to update changes
    		//sceneView.Repaint();
    	}

    	private void HandleAddPoint()
    	{
    		// If the left mouse button is clicked
    		if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
    		{
    			// Create a ray from the mouse position
    			Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

    			// Add the point to the array
    			Undo.RecordObject(script, "Add Point");
    			Vector3 newPoint = new Vector3(ray.origin.x, ray.origin.y);
    			if (!script.worldSpace)
    				newPoint = script.transform.InverseTransformPoint(newPoint);

    			int minIndex = -1;
    			if (snapToEdge)
    			{ // Find the edge to insert
    				Vector3[] path = script.paths[pathIndex].points;
    				float minDist = snapDistance;
    				for (int i = 0; i < path.Length - 1; i++)
    				{
    					float dist = UtilsTouch.DistancePointSegment(newPoint, path[i], path[i + 1]);
    					if (dist < minDist)
    					{
    						minIndex = i + 1;
    						minDist = dist;
    					}
    				}
    			}
    			if (minIndex >= 0)
    			{
    				ArrayUtility.Insert(ref script.paths[pathIndex].points, minIndex, newPoint);
    				ArrayUtility.Insert(ref script.paths[pathIndex].radiuses, minIndex, defaultRadius);
    			}
    			else
    			{
    				ArrayUtility.Add(ref script.paths[pathIndex].points, newPoint);
    				ArrayUtility.Add(ref script.paths[pathIndex].radiuses, defaultRadius);
    			}

    			// Consume the event to prevent further processing
    			Event.current.Use();
    			Repaint();
    		}

    		if (Event.current.type == EventType.Layout)
    		{
    			HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));
    		}
    	}

    	private void HandleRemovePoint()
    	{
    		for (int i = 0; i < script.paths[pathIndex].points.Length; i++)
    		{
    			// Get the position of the point in world space
    			Vector3 point = script.paths[pathIndex].points[i];
    			if (!script.worldSpace)
    				point = script.transform.TransformPoint(point);

    			float size = HandleUtility.GetHandleSize(point) * 0.1f;
    			if (Handles.Button(point, Quaternion.identity, size, size, Handles.SphereHandleCap))
    			{
    				Undo.RecordObject(script, "Remove Point");
    				ArrayUtility.RemoveAt(ref script.paths[pathIndex].points, i);
    				ArrayUtility.RemoveAt(ref script.paths[pathIndex].radiuses, i);
    				Repaint();
    				break;
    			}
    		}

    		if (Event.current.type == EventType.Layout)
    		{
    			HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));
    		}
    	}
    	private void HandleEditPoints()
    	{
    		bool selectedPoint = false;
    		for (int i = 0; i < script.paths[pathIndex].points.Length; i++)
    		{
    			// Get the position of the point in world space
    			Vector3 point = script.paths[pathIndex].points[i];
    			if (!script.worldSpace)
    				point = script.transform.TransformPoint(point);

    			// Draw a wire sphere at the point position
    			Handles.color = (i == selectedPointIndex) ? Color.green : Color.gray;
    			float size = HandleUtility.GetHandleSize(point) * 0.1f;
    			if (Handles.Button(point, Quaternion.identity, size, size, Handles.SphereHandleCap))
    			{
    				Undo.RecordObject(this, "Undo Select Point");
    				selectedPointIndex = i;
    				selectedPoint = true;
    				Repaint();
    			}

    			// If the point is selected
    			if (selectedPointIndex == i)
    			{
    				// Draw a move handle at the point position
    				EditorGUI.BeginChangeCheck();
    				Vector3 newPosition = Handles.PositionHandle(point, Quaternion.identity);
    				if (EditorGUI.EndChangeCheck())
    				{
    					Undo.RecordObject(script, "Move Point");
    					if (!script.worldSpace)
    						newPosition = script.transform.InverseTransformPoint(newPosition);
    					script.paths[pathIndex].points[i] = newPosition;
    				}

    				if (showRadius)
    				{
    					EditorGUI.BeginChangeCheck();
    					float newRadius = Handles.RadiusHandle(Quaternion.identity, newPosition, script.paths[pathIndex].radiuses[i]);
    					if (EditorGUI.EndChangeCheck())
    					{
    						Undo.RecordObject(script, "Change Radius");
    						script.paths[pathIndex].radiuses[i] = newRadius;
    					}
    				}
    			}
    		}

    		// If the left mouse button is clicked
    		if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && !selectedPoint)
    		{
    			if (Event.current.control)
    			{
    				// Create a ray from the mouse position
    				Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
    				Vector3 newPoint = new Vector3(ray.origin.x, ray.origin.y);
    				if (!script.worldSpace)
    					newPoint = script.transform.InverseTransformPoint(newPoint);

    				// Add the point to the array
    				Undo.RecordObject(script, "Add Point");
    				int minIndex = -1;
    				if (snapToEdge)
    				{ // Find the edge to insert
    					Vector3[] path = script.paths[pathIndex].points;
    					float minDist = snapDistance;
    					for (int i = 0; i < path.Length - 1; i++)
    					{
    						float dist = UtilsTouch.DistancePointSegment(newPoint, path[i], path[i + 1]);
    						if (dist < minDist)
    						{
    							minIndex = i + 1;
    							minDist = dist;
    						}
    					}
    				}
    				if (minIndex >= 0)
    				{
    					ArrayUtility.Insert(ref script.paths[pathIndex].points, minIndex, newPoint);
    					ArrayUtility.Insert(ref script.paths[pathIndex].radiuses, minIndex, defaultRadius);
    				}
    				else
    				{
    					ArrayUtility.Add(ref script.paths[pathIndex].points, newPoint);
    					ArrayUtility.Add(ref script.paths[pathIndex].radiuses, defaultRadius);
    				}

    				Repaint();
    			}

    			Undo.RecordObject(this, "Undo Deselect Point");
    			selectedPointIndex = -1;
    			Event.current.Use();
    		}

    		// If the escape key is pressed
    		if (Event.current.type == EventType.KeyDown)
    		{
    			if (Event.current.keyCode == KeyCode.Escape)
    			{
    				// Deselect all points
    				Undo.RecordObject(this, "Undo Deselect Point");
    				selectedPointIndex = -1;
    				Repaint();
    				// Consume the event to prevent further processing
    				Event.current.Use();
    			} 
    			else if (Event.current.keyCode == KeyCode.Delete && selectedPointIndex >= 0)
    			{
    				Undo.SetCurrentGroupName("Undo Remove Point");
    				Undo.RecordObject(script, "Undo Remove Point");
    				Undo.RecordObject(this, "Undo Deselect Point");
    				int undoGroup = Undo.GetCurrentGroup();

    				ArrayUtility.RemoveAt(ref script.paths[pathIndex].points, selectedPointIndex);
    				ArrayUtility.RemoveAt(ref script.paths[pathIndex].radiuses, selectedPointIndex);
    				selectedPointIndex = -1;
    				Repaint();
    				// Consume the event to prevent further processing
    				Event.current.Use();

    				Undo.CollapseUndoOperations(undoGroup);
    			}
    		}

    		if (Event.current.type == EventType.Layout)
    		{
    			//HandleUtility.AddDefaultControl(0);
    			HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));
    		}
    	}
    }

}