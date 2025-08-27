
namespace Interactive.DRagDrop
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
		private int pathIndex;
		private bool showRadius = true;
		private bool snapToEdge = true;
		private float snapDistance = 0.1f;

		private int selectedPointIndex = -1;

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
				ArrayUtility.Add(ref script.paths, new TracingController.Path() { points = new Vector3[] { }, radiuses = new float[] { } });
				pathIndex = script.paths.Length - 1;
				addPoint = false;
				editPoints = false;
				removePoint = false;
				SceneView.RepaintAll();
			}
			GUI.enabled = (script.paths != null && script.paths.Length > 0);
			if (GUILayout.Button("- Path"))
			{
				ArrayUtility.RemoveAt(ref script.paths, pathIndex);
				addPoint = false;
				editPoints = false;
				removePoint = false;
				SceneView.RepaintAll();
			}
			GUI.enabled = true;

			EditorGUILayout.EndHorizontal();

			if (addPoint || editPoints)
			{
				EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

				if (editPoints)
				{
					if (selectedPointIndex >= 0)
					{
						float newRadius = script.paths[pathIndex].radiuses[selectedPointIndex];
						newRadius = EditorGUILayout.FloatField("Point Radius: ", newRadius);
						if (newRadius != script.paths[pathIndex].radiuses[selectedPointIndex])
						{
							script.paths[pathIndex].radiuses[selectedPointIndex] = newRadius;
							SceneView.RepaintAll();
						}
					}
					bool oldShowRadius = showRadius;
					showRadius = EditorGUILayout.Toggle("Show Radius", showRadius);
					if (oldShowRadius != showRadius)
						SceneView.RepaintAll();
				}

				snapToEdge = EditorGUILayout.Toggle("Create on edge", snapToEdge);
				snapDistance = EditorGUILayout.FloatField("Snap Distance: ", snapDistance);
			}

			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

			GUI.enabled = (script.paths != null && script.paths.Length > 0);
			int oldIndex = pathIndex;
			pathIndex = EditorGUILayout.IntSlider("Path Index: ", pathIndex, 0, script.paths.Length - 1);
			if (oldIndex != pathIndex)
			{
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
					if (!script.worldSpace)
						Handles.matrix = script.transform.localToWorldMatrix;

					if (!editPoints && !removePoint)
					{
						for (int i = 0; i < script.paths[pathIdx].points.Length; i++)
						{
							// Get the position of the point in world space
							Vector3 point = script.paths[pathIdx].points[i];

							// Draw a wire sphere at the point position
							//float size = HandleUtility.GetHandleSize(point) * 0.05f;
							Handles.DrawWireDisc(point, Vector3.back, script.paths[pathIdx].radiuses[i]);
							float size = HandleUtility.GetHandleSize(point) * 0.1f;
							Handles.SphereHandleCap(0, point, Quaternion.identity, size, EventType.Repaint);
						}
					}

					Handles.DrawPolyLine(script.paths[pathIdx].points);
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
						float dist = Utils.DistancePointSegment(newPoint, path[i], path[i + 1]);
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
					ArrayUtility.Insert(ref script.paths[pathIndex].radiuses, minIndex, 1.0f);
				}
				else
				{
					ArrayUtility.Add(ref script.paths[pathIndex].points, newPoint);
					ArrayUtility.Add(ref script.paths[pathIndex].radiuses, 1.0f);
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
							float dist = Utils.DistancePointSegment(newPoint, path[i], path[i + 1]);
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
						ArrayUtility.Insert(ref script.paths[pathIndex].radiuses, minIndex, 1.0f);
					}
					else
					{
						ArrayUtility.Add(ref script.paths[pathIndex].points, newPoint);
						ArrayUtility.Add(ref script.paths[pathIndex].radiuses, 1.0f);
					}

					Repaint();
				}


				selectedPointIndex = -1;
				Event.current.Use();
			}

			// If the escape key is pressed
			if (Event.current.type == EventType.KeyDown)
			{
				if (Event.current.keyCode == KeyCode.Escape)
				{
					// Deselect all points
					selectedPointIndex = -1;
					Repaint();
					// Consume the event to prevent further processing
					Event.current.Use();
				}
				else if (Event.current.keyCode == KeyCode.Delete && selectedPointIndex >= 0)
				{
					Undo.RecordObject(script, "Remove Point");
					ArrayUtility.RemoveAt(ref script.paths[pathIndex].points, selectedPointIndex);
					ArrayUtility.RemoveAt(ref script.paths[pathIndex].radiuses, selectedPointIndex);
					selectedPointIndex = -1;
					Repaint();
					// Consume the event to prevent further processing
					Event.current.Use();
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