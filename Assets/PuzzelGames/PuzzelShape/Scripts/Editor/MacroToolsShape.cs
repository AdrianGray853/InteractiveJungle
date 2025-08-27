using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Linq;
using System.Collections.Generic;

namespace Interactive.PuzzelShape
{
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Linq;
using System.Collections.Generic;

    public class MacroToolsShape
    {
    	[MenuItem("Macro/SetupTouchAndFill")]
    	private static void SetupTouchAndFill()
    	{
    		GameObject go = Selection.activeGameObject;
    		if (go != null && go.scene.IsValid() && go.transform.parent != null)
    		{
    			List<PolygonCollider2D> colliders = new List<PolygonCollider2D>();
    			Transform parent = go.transform.parent;
    			Undo.RegisterCompleteObjectUndo(parent.gameObject, "Undo Setup Touch and Fill Macro");
    			Undo.SetCurrentGroupName("Undo Setup Touch and Fill Macro");
    			if (PrefabUtility.IsPartOfAnyPrefab(parent.gameObject))
    			{ // Unpack
    				PrefabUtility.UnpackPrefabInstance(parent.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
    			}
    			for (int i = 0; i < parent.childCount; i++)
    			{
    				GameObject child = parent.GetChild(i).gameObject;
    				if (child == go)
    					continue; // Base

    				PolygonCollider2D collider = Undo.AddComponent<PolygonCollider2D>(child);
    				SortingGroup sortingGroup = Undo.AddComponent<SortingGroup>(child);
    				SpriteMask spriteMask = Undo.AddComponent<SpriteMask>(child);
    				SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
    				spriteMask.sprite = spriteRenderer.sprite;
    				Undo.DestroyObjectImmediate(spriteRenderer);
    				sortingGroup.sortingOrder = 3 + i;

    				CreateCircle(child.transform);
    				colliders.Add(collider);
    			}

    			FillColorOnTouch fill = Undo.AddComponent<FillColorOnTouch>(parent.gameObject);
    			fill.ShapeMasks = colliders.ToArray();
    			fill.Base = go.GetComponent<SpriteRenderer>();

    			SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
    			sr.sortingOrder = 100;
    			Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SpriteMultiply.mat");
    			sr.sharedMaterial = mat;
    		}
    		else
    		{
    			Debug.LogWarning("Bad selection! Please select the Base layer!");
    		}
    	}

    	private static void CreateCircle(Transform parent)
    	{
    		GameObject go = new GameObject(parent.name + "Circle");
    		Undo.RegisterCreatedObjectUndo(go, "Create Circle");
    		Undo.SetTransformParent(go.transform, parent, "Modify parent");
    		SpriteRenderer sr = Undo.AddComponent<SpriteRenderer>(go);
    		Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/Circle.png");		
    		Undo.RecordObject(go, "Change sprite");
    		Color randomColor = Random.ColorHSV(0f, 1.0f, 0.5f, 1.0f, 0.3f, 0.9f);
    		randomColor.a = 1.0f;
    		sr.color = randomColor;
    		sr.sprite = sprite;
    		sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
    		sr.transform.localScale = Vector3.one * 10.0f;
    		//Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SpriteMultiply.mat");
    		//sr.sharedMaterial = mat;
    	}

    	[MenuItem("Macro/SetupPuzzle")]
    	private static void SetupPuzzle()
        {
    		GameObject go = Selection.activeGameObject;
    		if (go != null && go.scene.IsValid() && go.transform.parent != null && go.transform.childCount > 0)
    		{
    			List<PolygonCollider2D> colliders = new List<PolygonCollider2D>();
    			Transform parent = go.transform.parent;
    			Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SpriteOutline.mat");
    			Undo.RegisterCompleteObjectUndo(parent.gameObject, "Undo Setup Puzzle");
    			Undo.SetCurrentGroupName("Undo Setup Puzzle");
    			for (int i = 0; i < go.transform.childCount; i++)
    			{
    				GameObject child = go.transform.GetChild(i).gameObject;
    				PolygonCollider2D collider = Undo.AddComponent<PolygonCollider2D>(child);
    				OutlineControllerShape outline = Undo.AddComponent<OutlineControllerShape>(child);
    				SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
    				spriteRenderer.sharedMaterial = mat;
    				outline.OutlineThickness = 0f;
    				colliders.Add(collider);
    			}

    			SortingGroup sorting = Undo.AddComponent<SortingGroup>(go);
    			sorting.sortingOrder = 100;
    			PuzzleController puzzle = Undo.AddComponent<PuzzleController>(parent.gameObject);
    			puzzle.PuzzlePieces = colliders.ToArray();
    			puzzle.Target = (parent.GetChild(0) == go.transform) ? parent.GetChild(1).gameObject : parent.GetChild(0).gameObject;

    			GameObject outlineGO = puzzle.Target;
    			outlineGO.GetComponent<SpriteRenderer>().sharedMaterial = mat;
    			Undo.AddComponent<OutlineControllerShape>(outlineGO);
    		}
    		else
    		{
    			Debug.LogWarning("Bad selection! Please select the Puzzle root layer!");
    		}
    	}

    	[MenuItem("Macro/SetupEnvironment")]
    	private static void SetupEnvironment()
    	{
    		GameObject go = Selection.activeGameObject;
    		if (go != null && go.scene.IsValid() && go.transform.parent != null)
    		{
    			List<PolygonCollider2D> colliders = new List<PolygonCollider2D>();
    			List<GameObject> outlines = new List<GameObject>();
    			Transform parent = go.transform.parent;
    			Undo.RegisterCompleteObjectUndo(parent.gameObject, "Undo Setup Environment");
    			Undo.SetCurrentGroupName("Undo Setup Environment");

    			Transform colorGO = null;
    			Transform outlineGO = null;
    			Transform backgroundGO = null;
    			for (int i = 0; i < parent.childCount; i++)
    			{
    				if (parent.GetChild(i).name == "Color")
    					colorGO = parent.GetChild(i);
    				else if (parent.GetChild(i).name == "Outline")
    					outlineGO = parent.GetChild(i);
    				else
    					backgroundGO = parent.GetChild(i);
    			}

    			if (colorGO == null)
    			{
    				Debug.LogWarning("Color game object root is missing!");
    				return;
    			}

    			if (outlineGO == null)
    			{
    				Debug.LogWarning("Outline game object root is missing!");
    				return;
    			}

    			if (backgroundGO == null)
    			{
    				Debug.LogWarning("Background game object is missing!");
    				return;
    			}

    			backgroundGO.GetComponent<SpriteRenderer>().sortingLayerName = "Background";
    			backgroundGO.GetComponent<SpriteRenderer>().sortingOrder = 0;

    			Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SpriteOutline.mat");
    			for (int i = 0; i < colorGO.childCount; i++)
    			{
    				GameObject child = colorGO.GetChild(i).gameObject;

    				PolygonCollider2D collider = Undo.AddComponent<PolygonCollider2D>(child);
    				OutlineControllerShape outline = Undo.AddComponent<OutlineControllerShape>(child);
    				outline.Antialiased = true;
    				SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
    				spriteRenderer.sharedMaterial = mat;
    				outline.OutlineThickness = 0f;
    				colliders.Add(collider);
    			}

    			for (int i = 0; i < outlineGO.childCount; i++)
    			{
    				GameObject child = outlineGO.GetChild(i).gameObject;

    				OutlineControllerShape outline = Undo.AddComponent<OutlineControllerShape>(child);
    				outline.Antialiased = true;
    				SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
    				spriteRenderer.sharedMaterial = mat;
    				outlines.Add(child);
    			}

    			//SortingGroup colorSorting = Undo.AddComponent<SortingGroup>(colorGO.gameObject);
    			//colorSorting.sortingOrder = 1;

    			SortingGroup outlineSorting = Undo.AddComponent<SortingGroup>(outlineGO.gameObject);
    			outlineSorting.sortingOrder = 0;

    			EnvironmentController envController = Undo.AddComponent<EnvironmentController>(parent.gameObject);
    			envController.ColorObjects = colliders.ToArray();
    			envController.OutlineObjects = outlines.ToArray();

    		}
    		else
    		{
    			Debug.LogWarning("Bad selection! Please select one of the Environment children (Background, Color or Outline game object)!");
    		}
    	}

    	[MenuItem("Macro/SetupShapePuzzle")]
    	private static void SetupShapePuzzle()
    	{
    		GameObject go = Selection.activeGameObject;
    		if (go != null && go.scene.IsValid() && go.transform.parent != null && go.transform.childCount > 0)
    		{
    			List<PolygonCollider2D> colliders = new List<PolygonCollider2D>();
    			Transform parent = go.transform.parent;
    			Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SpriteOutline.mat");
    			Undo.RegisterCompleteObjectUndo(parent.gameObject, "Undo Setup Shape Puzzle");
    			Undo.SetCurrentGroupName("Undo Setup Shape Puzzle");
    			for (int i = 0; i < go.transform.childCount; i++)
    			{
    				GameObject child = go.transform.GetChild(i).gameObject;
    				PolygonCollider2D collider = Undo.AddComponent<PolygonCollider2D>(child);
    				OutlineControllerShape outline = Undo.AddComponent<OutlineControllerShape>(child);
    				SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
    				spriteRenderer.sharedMaterial = mat;
    				outline.Antialiased = true;
    				colliders.Add(collider);
    			}

    			SortingGroup sorting = Undo.AddComponent<SortingGroup>(go);
    			sorting.sortingOrder = 100;
    			ShapePuzzleController puzzle = Undo.AddComponent<ShapePuzzleController>(parent.gameObject);
    			puzzle.PuzzlePieces = colliders.ToArray();
    			puzzle.Target = (parent.GetChild(0) == go.transform) ? parent.GetChild(1).gameObject : parent.GetChild(0).gameObject;

    			GameObject outlineGO = puzzle.Target;
    			mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SpriteMultiply.mat");
    			outlineGO.GetComponent<SpriteRenderer>().sharedMaterial = mat;
    		}
    		else
    		{
    			Debug.LogWarning("Bad selection! Please select the Puzzle root layer!");
    		}
    	}

    	[MenuItem("Macro/UpdatePolygonColliderFromMask")]
    	private static void UpdatePolygonColliderFromMask()
    	{
    		GameObject go = Selection.activeGameObject;
    		if (go != null && go.scene.IsValid() && go.GetComponent<SpriteMask>() != null && go.GetComponent<PolygonCollider2D>() != null)
    		{
    			PolygonCollider2D collider = go.GetComponent<PolygonCollider2D>();
    			Undo.RecordObject(collider, "Undo Update Path");
    			Sprite sprite = go.GetComponent<SpriteMask>().sprite;
    			int shapes = sprite.GetPhysicsShapeCount();
    			collider.pathCount = shapes;
    			List<Vector2> points = new List<Vector2>();
    			for (int i = 0; i < shapes; i++)
    			{
    				points.Clear();
    				sprite.GetPhysicsShape(i, points);
    				collider.SetPath(i, points);
    			}
    			//Undo.FlushUndoRecordObjects();
    		}
    		else
    		{
    			Debug.LogWarning("Bad selection! Please select a game object with a PolygonCollider2D and Sprite Mask!");
    		}
    	}

    	[MenuItem("Macro/CheckForDuplicatedComponents")]
    	private static void CheckForDuplicatedComponents()
        {
    		GameObject[] selectedObjects = Selection.gameObjects;

    		foreach (GameObject selectedObject in selectedObjects)
    		{
    			if (PrefabUtility.IsPartOfPrefabAsset(selectedObject))
    			{
    				string prefabPath = AssetDatabase.GetAssetPath(selectedObject);
    				GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

    				if (prefab != null)
    				{
    					if (CheckForDuplicatedComponentsRecursive(prefab.transform))
                        {
    						Debug.LogWarning("Found duplicate at root: " + prefab.name);
                        }
    				}
    			}
    		}
    	}

    	private static bool HasDuplicatedComponents(Transform transform)
        {
    		Component[] components = transform.GetComponents<Component>();
    		bool hasDuplicates = false;

    		for (int i = 0; i < components.Length; i++)
    		{
    			Component currentComponent = components[i];

    			for (int j = i + 1; j < components.Length; j++)
    			{
    				Component otherComponent = components[j];

    				if (currentComponent != null && otherComponent != null && currentComponent.GetType() == otherComponent.GetType())
    				{
    					hasDuplicates = true;
    					Debug.Log($"Duplicate component found: {currentComponent.GetType().Name}");
    				}
    			}
    		}

    		return hasDuplicates;
    	}

    	private static bool CheckForDuplicatedComponentsRecursive(Transform transform)
        {
    		bool hasDuplicates = false;
    		if (HasDuplicatedComponents(transform))
    		{
    			hasDuplicates = true;
    			Debug.LogWarning("Found duplicated components in object: " + transform.name);
    		}
    		for (int i = 0; i < transform.childCount; i++)
            {
    			hasDuplicates |= CheckForDuplicatedComponentsRecursive(transform.GetChild(i));
            }
    		return hasDuplicates;
    	}

    	[MenuItem("Macro/RemoveDuplicatedSounds")]
    	private static void RemoveDuplicatedSounds()
    	{
    		var assetPath = "Assets/Resources/Managers.prefab";

    		using (var editingScope = new PrefabUtility.EditPrefabContentsScope(assetPath))
    		{
    			var prefabRoot = editingScope.prefabContentsRoot;
    			SoundManagerShape soundManager = prefabRoot.GetComponent<SoundManagerShape>();
    			Undo.RecordObject(soundManager, "Undo Sound Manager changes");

    			HashSet<SoundManagerShape.AudioDesc> soundsToRemove = new HashSet<SoundManagerShape.AudioDesc>();

    			for (int i = 0; i < soundManager.Sounds.Length; i++)
    			{
    				if (soundManager.Sounds[i].Clip == null)
    				{
    					Debug.LogWarning("Found null clip: " + soundManager.Sounds[i].Name + " i: " + i);
    					soundsToRemove.Add(soundManager.Sounds[i]);
    					continue;
    				}

    				for (int j = i + 1; j < soundManager.Sounds.Length; j++)
    				{
    					if (soundManager.Sounds[i].Name == soundManager.Sounds[j].Name)
    					{
    						Debug.LogWarning("Found Duplicate " + soundManager.Sounds[i].Name + " clip1: " + soundManager.Sounds[i].Clip.name + " clip2: " + soundManager.Sounds[j].Clip.name + " i: " + i + " j: " + j);
    						soundsToRemove.Add(soundManager.Sounds[j]);
    					}
    				}
    			}

    			foreach (var sound in soundsToRemove)
    				soundManager.Sounds = soundManager.Sounds.Where(x => !soundsToRemove.Contains(x)).ToArray();

    			soundsToRemove.Clear();

    			for (int i = 0; i < soundManager.Sounds.Length; i++)
    			{
    				for (int j = i + 1; j < soundManager.Sounds.Length; j++)
    				{
    					if (soundManager.Sounds[i].Clip.name == soundManager.Sounds[j].Clip.name)
    					{
    						Debug.LogWarning("Found Duplicate Clip " + soundManager.Sounds[i].Clip.name + " clip1: " + soundManager.Sounds[i].Name + " clip2: " + soundManager.Sounds[j].Name + " i: " + i + " j: " + j);
    						soundsToRemove.Add(soundManager.Sounds[j]);
    					}
    				}
    			}

    			foreach (var sound in soundsToRemove)
    				soundManager.Sounds = soundManager.Sounds.Where(x => !soundsToRemove.Contains(x)).ToArray();
    		}
    	}
    }


}