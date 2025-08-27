using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Linq;
using System.Collections.Generic;

namespace Interactive.DRagDrop
{
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Linq;
using System.Collections.Generic;

    public class MacroTools
    {
    	[MenuItem("Macro/SetupLetterForAnimation")]
    	private static void SetupLetterForAnimation()
    	{
    		GameObject go = Selection.activeGameObject;
    		if (go != null && go.scene.IsValid())
    		{
    			string rootBone = "bone_1";
    			string letterName = go.name;
    			string outlineName = "Outline" + letterName;
    			string[] validChildren = { "GuraDeschisa", "GuraInchisa", "OchiDreptDeschis",
    										"OchiStangDeschis", "OchiStangInchis", "OchiDreptInchis",
    										"LeftArm", "RightArm" };

    			string[] faceChildren = { "GuraDeschisa", "GuraInchisa", "OchiDreptDeschis",
    										"OchiStangDeschis", "OchiStangInchis", "OchiDreptInchis" };

    			List<Transform> children = new List<Transform>();
    			bool good = true;
    			for (int i = 0; i < go.transform.childCount; i++)
                {
    				string childName = go.transform.GetChild(i).name;
    				if (!validChildren.Contains(childName) && childName != letterName && childName != outlineName && childName != rootBone)
                    {
    					Debug.Log("Bad name: " + childName);
    					good = false;
    					break;
                    }

    				children.Add(go.transform.GetChild(i));
                }

    			if (!good)
                {
    				Debug.Log("Bad Structure or names! Valid names: " + letterName + ", " + outlineName + ", " + string.Join(", ", validChildren));
    				return;
                }

    			Undo.RegisterCompleteObjectUndo(go, "Undo Setup Letters For Animation");
    			Undo.SetCurrentGroupName("Undo Setup Letters For Animation");
    			if (PrefabUtility.IsPartOfAnyPrefab(go))
    			{ // Unpack
    				PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
    			}

    			// Let's create the new parents
    			GameObject letterParent = new GameObject("Letter");
    			Undo.RegisterCreatedObjectUndo(letterParent, "Undo create Letter");
    			Undo.AddComponent<Animator>(letterParent);

    			GameObject faceParent = new GameObject("FaceTransform");
    			Undo.RegisterCreatedObjectUndo(letterParent, "Undo create FaceTransform");

    			// Reparent
    			foreach (Transform child in children)
    			{
    				if (faceChildren.Contains(child.name))
    					Undo.SetTransformParent(child, faceParent.transform, "Undo face parenting"); // child.parent = faceParent.transform;
    				else
    					Undo.SetTransformParent(child, letterParent.transform, "Undo letter parenting");  //child.parent = letterParent.transform;

    				if (child.name == rootBone)
    					Undo.SetTransformParent(faceParent.transform, child, "Undo bone parenting");  //faceParent.transform.parent = child;

    				if (child.name == letterName)
    					child.name = "Letter";
    			}

    			Undo.SetTransformParent(letterParent.transform, go.transform, "Undo letter reparenting");
    		}
    		else
    		{
    			Debug.LogWarning("Bad selection! Please select the Base layer!");
    		}
    	}

    	[MenuItem("Macro/SetupAnimalForAnimation")]
    	private static void SetupAnimalForAnimation()
    	{
    		GameObject go = Selection.activeGameObject;
    		if (go != null && go.scene.IsValid())
    		{
    			string animalName = go.name;
    			string outlineName = "Outline";
    			string[] ochiChildren = { "OchiStangInchis", "OchiDreptInchis", "OchiStangDeschis", "OchiDreptDeschis" };

    			List<Transform> children = new List<Transform>();
    			bool good = true;
    			for (int i = 0; i < go.transform.childCount; i++)
    			{
    				string childName = go.transform.GetChild(i).name;
    				if (!ochiChildren.Contains(childName) && childName != animalName && childName != outlineName)
    				{
    					Debug.Log("Bad name: " + childName);
    					good = false;
    					break;
    				}

    				children.Add(go.transform.GetChild(i));
    			}

    			if (!good)
    			{
    				Debug.Log("Bad Structure or names, please rename or fix!\nValid names: " + animalName + ", " + outlineName + ", " + string.Join(", ", ochiChildren));
    				return;
    			}

    			Undo.RegisterCompleteObjectUndo(go, "Undo Setup Animala For Animation");
    			Undo.SetCurrentGroupName("Undo Setup Animala For Animation");
    			if (PrefabUtility.IsPartOfAnyPrefab(go))
    			{ // Unpack
    				PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
    			}

    			// Let's create the new parents
    			GameObject animalParent = new GameObject("Animal");
    			Undo.RegisterCreatedObjectUndo(animalParent, "Undo create Animal");
    			Undo.AddComponent<Animator>(animalParent);

    			GameObject animalStickerParent = new GameObject("AnimalSticker");
    			Undo.RegisterCreatedObjectUndo(animalParent, "Undo create AnimalSticker");
    			Undo.SetTransformParent(animalStickerParent.transform, animalParent.transform, "Undo set Animal Parent");

    			GameObject ochiParent = new GameObject("Ochi");
    			Undo.RegisterCreatedObjectUndo(animalParent, "Undo create Ochi");
    			Undo.SetTransformParent(ochiParent.transform, animalStickerParent.transform, "Undo set Animal Parent");


    			List<string> ochiTransNames = new List<string>();
    			List<Transform> ochiTransParents = new List<Transform>();
    			for (int i = 0; i < ochiChildren.Length; i++)
                {
    				string newTransName = ochiChildren[i] + "Tr";
    				ochiTransNames.Add(newTransName);
    				GameObject transParent = new GameObject(newTransName);
    				Undo.RegisterCreatedObjectUndo(animalParent, "Undo create Trans Ochi");
    				Undo.SetTransformParent(transParent.transform, ochiParent.transform, "Undo ochi parenting");
    				ochiTransParents.Add(transParent.transform);
                }

    			// Reparent
    			foreach (Transform child in children)
    			{
    				if (ochiChildren.Contains(child.name))
    				{
    					for (int i = 0; i < ochiTransNames.Count; i++)
                        {
    						if (child.name + "Tr" == ochiTransNames[i])
                            {
    							Undo.SetTransformParent(child, ochiTransParents[i], "Undo ochi parenting"); 
    							break;
                            }
                        }
    				}
    				else
    					Undo.SetTransformParent(child, animalStickerParent.transform, "Undo animal sticker parenting");  
    			}

    			Undo.SetTransformParent(animalParent.transform, go.transform, "Undo animal reparenting");


    			// Create Stick
    			GameObject stick = new GameObject("Stick");
    			Undo.RegisterCreatedObjectUndo(animalParent, "Undo create stick");
    			SpriteRenderer sr = Undo.AddComponent<SpriteRenderer>(stick);
    			Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/Square.png");
    			sr.sprite = sprite;
    			stick.transform.position = new Vector3(-0.2f, -9.1f, 0.8343682f);
    			stick.transform.rotation = Quaternion.Euler(0f, 0f, 0.43f);
    			stick.transform.localScale = new Vector3(0.8585801f, 18.75274f, 0.8585801f);
    			Undo.SetTransformParent(stick.transform, animalParent.transform, "Undo animal reparenting");
    		}
    		else
    		{
    			Debug.LogWarning("Bad selection! Please select the Base layer!");
    		}
    	}

    	[MenuItem("Macro/SetupObjectForAnimation")]
    	private static void SetupObjectForAnimation()
    	{
    		GameObject go = Selection.activeGameObject;
    		if (go != null && go.scene.IsValid())
    		{
    			string objectName = go.name;
    			string outlineName = "Outline";
    			string[] ochiChildren = { "OchiStangInchis", "OchiDreptInchis", "OchiStangDeschis", "OchiDreptDeschis" };

    			List<Transform> children = new List<Transform>();
    			bool good = true;
    			for (int i = 0; i < go.transform.childCount; i++)
    			{
    				string childName = go.transform.GetChild(i).name;
    				if (!ochiChildren.Contains(childName) && childName != objectName && childName != outlineName)
    				{
    					Debug.Log("Bad name: " + childName);
    					good = false;
    					break;
    				}

    				children.Add(go.transform.GetChild(i));
    			}

    			if (!good)
    			{
    				Debug.Log("Bad Structure or names, please rename or fix!\nValid names: " + objectName + ", " + outlineName + ", " + string.Join(", ", ochiChildren));
    				return;
    			}

    			Undo.RegisterCompleteObjectUndo(go, "Undo Setup Object For Animation");
    			Undo.SetCurrentGroupName("Undo Setup Object For Animation");
    			if (PrefabUtility.IsPartOfAnyPrefab(go))
    			{ // Unpack
    				PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
    			}

    			// Let's create the new parents
    			GameObject objectParent = new GameObject("Object");
    			Undo.RegisterCreatedObjectUndo(objectParent, "Undo create Object");
    			Undo.AddComponent<Animator>(objectParent);

    			GameObject objectStickerParent = new GameObject("ObjectSticker");
    			Undo.RegisterCreatedObjectUndo(objectParent, "Undo create ObjectSticker");
    			Undo.SetTransformParent(objectStickerParent.transform, objectParent.transform, "Undo set Object Parent");

    			if (go.transform.childCount == 0)
                { // Ball like objects, psd
    				GameObject newRoot = new GameObject(go.name);
    				Undo.RegisterCreatedObjectUndo(objectParent, "Undo create Root");
    				Undo.SetTransformParent(objectParent.transform, newRoot.transform, "Undo reparent root");

    				Undo.SetTransformParent(go.transform, objectStickerParent.transform, "Undo sprite reparenting");
    			}

    			GameObject ochiParent = new GameObject("Ochi");
    			Undo.RegisterCreatedObjectUndo(objectParent, "Undo create Ochi");
    			Undo.SetTransformParent(ochiParent.transform, objectStickerParent.transform, "Undo set Object Parent");


    			List<string> ochiTransNames = new List<string>();
    			List<Transform> ochiTransParents = new List<Transform>();
    			for (int i = 0; i < ochiChildren.Length; i++)
    			{
    				string newTransName = ochiChildren[i] + "Tr";
    				ochiTransNames.Add(newTransName);
    				GameObject transParent = new GameObject(newTransName);
    				Undo.RegisterCreatedObjectUndo(objectParent, "Undo create Trans Ochi");
    				Undo.SetTransformParent(transParent.transform, ochiParent.transform, "Undo ochi parenting");
    				ochiTransParents.Add(transParent.transform);
    			}

    			// Reparent
    			foreach (Transform child in children)
    			{
    				if (ochiChildren.Contains(child.name))
    				{
    					for (int i = 0; i < ochiTransNames.Count; i++)
    					{
    						if (child.name + "Tr" == ochiTransNames[i])
    						{
    							Undo.SetTransformParent(child, ochiTransParents[i], "Undo ochi parenting");
    							break;
    						}
    					}
    				}
    				else
    					Undo.SetTransformParent(child, objectStickerParent.transform, "Undo object sticker parenting");
    			}

    			Undo.SetTransformParent(objectParent.transform, go.transform, "Undo object reparenting");


    			objectParent.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

    			// Create Stick
    			GameObject stick = new GameObject("Stick");
    			Undo.RegisterCreatedObjectUndo(objectParent, "Undo create stick");
    			SpriteRenderer sr = Undo.AddComponent<SpriteRenderer>(stick);
    			Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/Square.png");
    			sr.sprite = sprite;
    			Undo.SetTransformParent(stick.transform, objectParent.transform, "Undo object reparenting");
    			stick.transform.localPosition = new Vector3(0f, 10.52f, 1.112491f);
    			stick.transform.localRotation = Quaternion.identity;
    			stick.transform.localScale = new Vector3(1.3f, 25.00365f, 1.3f);


    			// Setup new positions for object
    			objectStickerParent.transform.localPosition = new Vector3(0f, 22.52f, 0f);
    			objectStickerParent.transform.localScale = Vector3.one * 1.3f;


    		}
    		else
    		{
    			Debug.LogWarning("Bad selection! Please select the Base layer!");
    		}
    	}
    	/*
    		GameObject go = Selection.activeGameObject;
    		if (go != null && go.scene.IsValid() && go.GetComponent<SpriteRenderer>() != null)
    		{
    			string objectName = go.name;
    			Undo.RegisterCompleteObjectUndo(go, "Undo Setup Object For Animation");
    			Undo.SetCurrentGroupName("Undo Setup Object For Animation");

    			// Let's create the new parents
    			GameObject objectParent = new GameObject(go.name);
    			Undo.RegisterCreatedObjectUndo(objectParent, "Undo create Object parent");
    			Undo.AddComponent<Animator>(objectParent);
    			if (go.transform.parent != null)
                {
    				Undo.SetTransformParent(objectParent.transform, go.transform.parent, "Undo reparenting");
                }
    			Undo.SetTransformParent(go.transform, objectParent.transform, "Undo object parenting");
    			go.transform.position = new Vector3(-0.23f, 12.38f, 0.7422149f);
    			go.transform.rotation = Quaternion.Euler(0f, 0f, 9.726f);
    			go.transform.localScale = Vector3.one * 0.7511588f;

    			go.GetComponent<SpriteRenderer>().sortingOrder = 2;

    			// Create Stick
    			GameObject stick = new GameObject("Square");
    			Undo.RegisterCreatedObjectUndo(objectParent, "Undo create stick");
    			SpriteRenderer sr = Undo.AddComponent<SpriteRenderer>(stick);
    			Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/Square.png");
    			sr.sprite = sprite;
    			stick.transform.position = new Vector3(-0.1827818f, 4.914396f, 0.7422149f);
    			stick.transform.rotation = Quaternion.Euler(0f, 0f, -0.373f);
    			stick.transform.localScale = new Vector3(0.7875001f, 13.05651f, 1f);
    			Undo.SetTransformParent(stick.transform, objectParent.transform, "Undo letter reparenting");
    		}
    		else
    		{
    			Debug.LogWarning("Bad selection! Please select the Base layer!");
    		}
    	}
    	*/

    	[MenuItem("Macro/CenterText ^w")]
    	private static void CenterText()
    	{
    		GameObject go = Selection.activeGameObject;
    		if (go != null && go.scene.IsValid() && go.transform.childCount == 2 && go.GetComponent<TextLetterDragController>() != null)
            {
    			Undo.RegisterFullObjectHierarchyUndo(go, "Undo center!");
    			TextLetterDragController dragController = go.GetComponent<TextLetterDragController>();
    			/*
    			bool initiatedBounds = false;
    			Vector3 midPoint = Vector3.zero;
    			Vector3 sum = Vector3.zero;
    			int nr = 0;
    			foreach (var letter in dragController.Letters)
                {
    				SpriteRenderer sr = letter.GetComponent<SpriteRenderer>();

    				if (!initiatedBounds)
    				{
    					midPoint = Vector3.Scale(sr.sprite.bounds.center, sr.transform.localScale);
    					initiatedBounds = true;
    				}

    				sum += sr.transform.localPosition;
    				nr++;
    			}

    			sum = sum * (1.0f / nr) + midPoint;
    			foreach (var letter in dragController.Letters)
    			{
    				letter.transform.position -= sum;
    			}
    			foreach (var letter in dragController.TargetLetters)
    			{
    				letter.transform.position -= sum;
    			}
    			*/

    			SpriteRenderer sr0 = dragController.Letters[0].GetComponent<SpriteRenderer>();
    			SpriteRenderer sr1 = dragController.Letters[dragController.Letters.Length - 1].GetComponent<SpriteRenderer>();
    			Vector3 midPoint = sr0.GetComponent<Collider2D>().bounds.center;
    			float xDiff = sr1.bounds.max.x - sr0.bounds.min.x;
    			float xMove = sr0.bounds.min.x - (go.transform.position.x - xDiff * 0.5f);
    			float yMove = midPoint.y - go.transform.position.y;
    			//Vector3 diff = go.transform.position - Vector3.left * (xDiff * 0.5f);
    			foreach (var letter in dragController.Letters)
    			{
    				letter.transform.position += Vector3.left * xMove + Vector3.up * yMove;
    			}

    			foreach (var letter in dragController.TargetLetters)
    			{
    				letter.transform.position += Vector3.left * xMove + Vector3.up * yMove;
    			}
    		} 
    		else
            {
    			Debug.LogWarning("Please select the text root object!");
            }
    	}

    	[MenuItem("Macro/FixLineZ")]
    	private static void FixLineZPosition()
        {
    		GameObject go = Selection.activeGameObject;
    		if (go != null && go.scene.IsValid() && go.GetComponent<LineRenderer>() != null)
    		{
    			LineRenderer lr = go.GetComponent<LineRenderer>();
    			Undo.RegisterCompleteObjectUndo(lr, "Undo Change LineRenderer");
    			for (int i = 0; i < lr.positionCount; i++)
                {
    				Vector3 pos = lr.GetPosition(i);
    				pos.z = 0f;
    				lr.SetPosition(i, pos);
                }
    		} else 
    		{
    			Debug.LogWarning("Please select a GameObject with a LineRenderer component!");
            }
        }

    	[MenuItem("Macro/SmoothLine")]
    	private static void SmoothLine()
        {
    		GameObject go = Selection.activeGameObject;
    		if (go != null && go.scene.IsValid() && go.GetComponent<LineRenderer>() != null)
    		{
    			LineRenderer lr = go.GetComponent<LineRenderer>();
    			Undo.RegisterCompleteObjectUndo(lr, "Undo Change LineRenderer");
    			if (lr.positionCount < 4)
                {
    				Debug.LogWarning("Please have at least 4 points!");
    				return;
                }

    			Vector3[] positions = new Vector3[lr.positionCount];
    			lr.GetPositions(positions);
    			Vector3[] newPos = MakeSmoothCurve(positions, 0f);
    			lr.positionCount = newPos.Length;
    			lr.SetPositions(newPos);

    		}
    		else
    		{
    			Debug.LogWarning("Please select a GameObject with a LineRenderer component!");
    		}
    	}

    	public static Vector3[] MakeSmoothCurve(Vector3[] arrayToCurve, float smoothness)
    	{
    		List<Vector3> points;
    		List<Vector3> curvedPoints;
    		int pointsLength = 0;
    		int curvedLength = 0;

    		if (smoothness < 1.0f) smoothness = 1.0f;

    		pointsLength = arrayToCurve.Length;

    		curvedLength = (pointsLength * Mathf.RoundToInt(smoothness)) - 1;
    		curvedPoints = new List<Vector3>(curvedLength);

    		float t = 0.0f;
    		for (int pointInTimeOnCurve = 0; pointInTimeOnCurve < curvedLength + 1; pointInTimeOnCurve++)
    		{
    			t = Mathf.InverseLerp(0, curvedLength, pointInTimeOnCurve);

    			points = new List<Vector3>(arrayToCurve);

    			for (int j = pointsLength - 1; j > 0; j--)
    			{
    				for (int i = 0; i < j; i++)
    				{
    					points[i] = (1 - t) * points[i] + t * points[i + 1];
    				}
    			}

    			curvedPoints.Add(points[0]);
    		}

    		return (curvedPoints.ToArray());
    	}

    	[MenuItem("Macro/LineToLocalSpace")]
    	private static void LineToLocalSpace()
    	{
    		GameObject[] gos = Selection.gameObjects;
    		foreach (GameObject go in gos)
    		{
    			if (go != null && go.scene.IsValid() && go.GetComponent<LineRenderer>() != null)
    			{
    				LineRenderer lr = go.GetComponent<LineRenderer>();
    				Undo.RegisterCompleteObjectUndo(lr, "Undo Change LineRenderer");
    				Vector3[] linePositions = new Vector3[lr.positionCount];
    				lr.GetPositions(linePositions);
    				for (int i = 0; i < lr.positionCount; i++)
    				{
    					linePositions[i] = lr.transform.InverseTransformPoint(linePositions[i]) + lr.transform.position;
    				}
    				lr.useWorldSpace = false;
    				lr.SetPositions(linePositions);
    			}
    			else
    			{
    				Debug.LogWarning("Please select a GameObject with a LineRenderer component!");
    			}
    		}
    	}

    	[MenuItem("Macro/LinesFromLeftToRight")]
    	private static void LinesFromLeftToRight()
    	{
    		GameObject[] gos = Selection.gameObjects;
    		foreach (GameObject go in gos)
    		{
    			if (go != null && go.scene.IsValid() && go.GetComponent<LineRenderer>() != null)
    			{
    				LineRenderer lr = go.GetComponent<LineRenderer>();
    				Vector3 leftPoint = lr.GetPosition(0);
    				Vector3 rightPoint = lr.GetPosition(lr.positionCount - 1);
    				if (leftPoint.x > rightPoint.x)
    				{
    					Undo.IncrementCurrentGroup();
    					Undo.RegisterCompleteObjectUndo(lr, "Undo Change LineRenderer");
    					Vector3[] linePositions = new Vector3[lr.positionCount];
    					lr.GetPositions(linePositions);
    					linePositions = linePositions.Reverse().ToArray();
    					lr.SetPositions(linePositions);
    					Debug.Log("Found and fixed reverse line: " + go.name);
    				}
    			}
    			else
    			{
    				Debug.LogWarning("Please select a GameObject with a LineRenderer component!");
    			}
    		}
    	}

    	[MenuItem("Macro/FixLetterDragControllerCollider ^e")]
    	private static void FixLetterDragControllerCollider()
    	{
    		GameObject[] gos = Selection.gameObjects;
    		Undo.IncrementCurrentGroup();
    		Undo.SetCurrentGroupName("Undo Fix Drag Controller");
    		foreach (GameObject go in gos)
    		{
    			if (go != null && go.scene.IsValid() && go.GetComponent<TextLetterDragController>() != null)
    			{
    				TextLetterDragController def = go.GetComponent<TextLetterDragController>();
    				foreach (var letter in def.Letters)
    				{
    					if (letter.Collider != null)
    						continue;
    					Undo.RecordObject(letter, "Undo Change Controller");
    					letter.Collider = letter.GetComponent<Collider2D>();
    					if (letter.Collider == null)
                        {
    						BoxCollider2D boxCollider = Undo.AddComponent<BoxCollider2D>(letter.gameObject);
    						boxCollider.isTrigger = true;
    						boxCollider.size = letter.GetComponent<SpriteRenderer>().sprite.bounds.size;
    						letter.Collider = boxCollider;
                        }
    				}
    				foreach (var letter in def.TargetLetters)
    				{
    					if (letter.Collider != null)
    						continue;
    					Undo.RecordObject(letter, "Undo Change Controller");
    					letter.Collider = letter.GetComponent<Collider2D>();
    					if (letter.Collider == null)
    					{
    						BoxCollider2D boxCollider = Undo.AddComponent<BoxCollider2D>(letter.gameObject);
    						boxCollider.isTrigger = true;
    						boxCollider.size = letter.GetComponent<SpriteRenderer>().sprite.bounds.size;
    						letter.Collider = boxCollider;
    					}
    				}
    			}
    			else
    			{
    				Debug.LogWarning("Please select a GameObject with a TextLetterDragController component!");
    			}
    		}
    	}

    	[MenuItem("Macro/RemoveDuplicatedSounds")]
    	private static void RemoveDuplicatedSounds()
        {
    		var assetPath = "Assets/Resources/Managers.prefab";

    		using (var editingScope = new PrefabUtility.EditPrefabContentsScope(assetPath))
    		{
    			var prefabRoot = editingScope.prefabContentsRoot;
    			SoundManager soundManager = prefabRoot.GetComponent<SoundManager>();
    			Undo.RecordObject(soundManager, "Undo Sound Manager changes");

    			HashSet<SoundManager.AudioDesc> soundsToRemove = new HashSet<SoundManager.AudioDesc>();

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