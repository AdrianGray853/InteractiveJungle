using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

namespace Interactive.Touch
{
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

    public class MacroToolsTouch
    {
    	[MenuItem("Macro/RemoveDuplicatedSounds")]
    	private static void RemoveDuplicatedSounds()
        {
    		var assetPath = "Assets/Resources/Managers.prefab";

    		using (var editingScope = new PrefabUtility.EditPrefabContentsScope(assetPath))
    		{
    			var prefabRoot = editingScope.prefabContentsRoot;
    			SoundManagerTouch soundManager = prefabRoot.GetComponent<SoundManagerTouch>();
    			Undo.RecordObject(soundManager, "Undo Sound Manager changes");

    			HashSet<SoundManagerTouch.AudioDesc> soundsToRemove = new HashSet<SoundManagerTouch.AudioDesc>();

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

    	[MenuItem("Macro/RebaseAnimation")]
    	private static void RebaseAnimation()
        {
    		GameObject go = Selection.activeGameObject;
    		if (go != null && go.scene.IsValid())
    		{
    			Animator animator = go.transform.GetComponentInParent<Animator>();
    			foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
    			{
    				if (!clip.isLooping)
    					continue;

    				Debug.Log("Adjusting clip: " + clip.name);
    				/*
    				string path = AnimationUtility.CalculateTransformPath(go.transform, animator.transform);
    				var curveBindings = AnimationUtility.GetCurveBindings(clip);
    				foreach (var curve in curveBindings)
                    {
    					if (curve.path != path)
    						continue;
    					Debug.Log(curve.propertyName);
                    }
    				*/
    				Undo.RegisterCompleteObjectUndo(clip, "Clip " + clip.name + " Edit");


    				EditorCurveBinding curveBinding = new EditorCurveBinding();
    				curveBinding.path = AnimationUtility.CalculateTransformPath(go.transform, animator.transform);
    				curveBinding.type = typeof(Transform);
    				bool updated = UpdateCurveBinding(clip, curveBinding, "m_LocalPosition.x", go.transform.localPosition.x);
    				updated |= UpdateCurveBinding(clip, curveBinding, "m_LocalPosition.y", go.transform.localPosition.y);
    				updated |= UpdateCurveBinding(clip, curveBinding, "m_LocalPosition.z", go.transform.localPosition.z);

    				updated |= UpdateCurveBinding(clip, curveBinding, "localEulerAnglesRaw.x", go.transform.localEulerAngles.x);
    				updated |= UpdateCurveBinding(clip, curveBinding, "localEulerAnglesRaw.y", go.transform.localEulerAngles.y);
    				updated |= UpdateCurveBinding(clip, curveBinding, "localEulerAnglesRaw.z", go.transform.localEulerAngles.z);

    				if (updated)
    					EditorUtility.SetDirty(clip);
    			}
    		}
    		else
            {
    			Debug.LogWarning("Please select the object you want to rebase with an Animator parent and inside the scene!");
            }
        }

    	private static bool UpdateCurveBinding(AnimationClip clip, EditorCurveBinding curveBinding, string propertyPath, float value)
        {
    		curveBinding.propertyName = propertyPath;

    		AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, curveBinding);
    		if (curve != null && curve.length > 0)
    		{
    			Debug.Log("Found and adjusted curve " + propertyPath);

    			float offset = value - curve[0].value;
    			for (int i = 0; i < curve.length; i++)
    			{
    				Keyframe keyframe = curve[i];
    				keyframe.value += offset;
    				curve.MoveKey(i, keyframe);
    			}

    			AnimationUtility.SetEditorCurve(clip, curveBinding, curve);

    			return true;
    		}
    		else
            {
    			Debug.Log("Didn't find curve " + propertyPath);
            }

    		return false;
    	}

    	[MenuItem("Macro/SetupForTouchAndFill")]
    	private static void SetupLetterForAnimation()
    	{
    		GameObject[] gos = Selection.gameObjects;
    		if (gos != null && gos.Length > 0)
    		{
    			foreach (GameObject go in gos)
    			{
    				if (!go.scene.IsValid() || go.GetComponent<SpriteRenderer>() == null)
                    {
    					Debug.LogWarning("You have selected invalid object, please select only the SpriteRenderer elements of the prefab! And make sure it's in the scene!");
    					return;
                    }
    			}

    			Animator parentAnimator = gos[0].transform.GetComponentInParent<Animator>();
    			if (parentAnimator == null)
    			{
    				Debug.LogWarning("Couldn't find an animator in the parents!");
    				return;
    			}

    			Transform prefabRoot = parentAnimator.transform.parent;
    			if (prefabRoot == null)
                {
    				Debug.LogWarning("Couldn't find the parent of the animator (aka prefab root)! Check hierarchy!");
    				return;
    			}

    			Undo.RegisterFullObjectHierarchyUndo(prefabRoot, "Undo Touch&Fill setup");
    			Undo.SetCurrentGroupName("Undo Touch&Fill setup");

    			Material DrawLayerMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/DrawLayerSprite.mat");

    			TouchAndFillController touchAndFill = Undo.AddComponent<TouchAndFillController>(prefabRoot.gameObject);
    			touchAndFill.BlitMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/BlitTouchAndFill.mat");
    			touchAndFill.Scale = 500.0f;
    			touchAndFill.ScalePower = 0.01f;
    			touchAndFill.Hardness = 0.9f;
    			touchAndFill.FillSpeed = 2.0f;

    			SpriteGroups spriteGroups = Undo.AddComponent<SpriteGroups>(prefabRoot.gameObject);
    			spriteGroups.RendererGroups = new List<SpriteGroups.SpriteGroup>();

    			foreach (GameObject go in gos)
    			{
    				SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
    				sr.sharedMaterial = DrawLayerMaterial;

    				Undo.AddComponent<PolygonCollider2D>(go);

    				spriteGroups.RendererGroups.Add(new SpriteGroups.SpriteGroup() { Group = new List<SpriteRenderer>() { sr } });
    			}

    			GameObject rootGO = PrefabUtility.GetOutermostPrefabInstanceRoot(prefabRoot.gameObject);

    			if (rootGO == null)
    			{
    				Debug.LogWarning(string.Format("Selected GameObject ({0}) is not part of a prefab", prefabRoot.gameObject), prefabRoot.gameObject);
    				return;
    			}

    			PrefabInstanceStatus prefabInstanceStatus = PrefabUtility.GetPrefabInstanceStatus(rootGO);
    			if (prefabInstanceStatus != PrefabInstanceStatus.Connected)
    			{
    				Debug.LogWarning(string.Format("Selected Prefab Root of {0} ({1}) has invalid status of {2}", prefabRoot.gameObject, rootGO, prefabInstanceStatus), rootGO);
    				return;
    			}

    			if (!PrefabUtility.HasPrefabInstanceAnyOverrides(rootGO, false))
    			{
    				Debug.LogWarning(string.Format("Selected Prefab Root of {0} ({1}) doesn't have any overrides", prefabRoot.gameObject, rootGO), rootGO);
    				return;
    			}

    			PrefabUtility.ApplyPrefabInstance(rootGO, InteractionMode.UserAction);
    			AssetDatabase.SaveAssets();

    			Debug.Log(string.Format("Changes on {0} applied to {1}", prefabRoot.gameObject, rootGO), rootGO);
    		}
    		else
    		{
    			Debug.LogWarning("Bad selection! Please select the SpriteRenderer game objects!");
    		}
    	}

    	[MenuItem("Macro/SetupForColorInEnvironment")]
    	private static void SetupForColorInEnvironment()
    	{
    		GameObject[] gos = Selection.gameObjects;
    		if (gos != null && gos.Length > 0)
    		{
    			foreach (GameObject go in gos)
    			{
    				if (!go.scene.IsValid() || go.GetComponent<SpriteRenderer>() == null)
    				{
    					Debug.LogWarning("You have selected invalid object, please select only the SpriteRenderer elements of the prefab! And make sure it's in the scene! " + go.name);
    					return;
    				}
    			}

    			Animator parentAnimator = gos[0].transform.GetComponentInParent<Animator>();
    			if (parentAnimator == null)
    			{
    				Debug.LogWarning("Couldn't find an animator in the parents!");
    				return;
    			}

    			Transform prefabRoot = parentAnimator.transform.parent;
    			if (prefabRoot == null)
    			{
    				Debug.LogWarning("Couldn't find the parent of the animator (aka prefab root)! Check hierarchy!");
    				return;
    			}

    			Undo.RegisterFullObjectHierarchyUndo(prefabRoot, "Undo Color in Env setup");
    			Undo.SetCurrentGroupName("Undo Color in Env setup");

    			Material DrawLayerMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/DrawLayerSprite.mat");

    			ColorEnvironmentController colorEnv = Undo.AddComponent<ColorEnvironmentController>(prefabRoot.gameObject);
    			colorEnv.BlitMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/BlitDraw.mat");
    			colorEnv.Scale = 0.075f;
    			colorEnv.Hardness = 0.9f;

    			SpriteGroups spriteGroups = Undo.AddComponent<SpriteGroups>(prefabRoot.gameObject);
    			spriteGroups.RendererGroups = new List<SpriteGroups.SpriteGroup>();

    			foreach (GameObject go in gos)
    			{
    				SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
    				sr.sharedMaterial = DrawLayerMaterial;

    				Undo.AddComponent<PolygonCollider2D>(go);

    				spriteGroups.RendererGroups.Add(new SpriteGroups.SpriteGroup() { Group = new List<SpriteRenderer>() { sr } });
    			}

    			GameObject rootGO = PrefabUtility.GetOutermostPrefabInstanceRoot(prefabRoot.gameObject);

    			if (rootGO == null)
    			{
    				Debug.LogWarning(string.Format("Selected GameObject ({0}) is not part of a prefab", prefabRoot.gameObject), prefabRoot.gameObject);
    				return;
    			}

    			if (!EditorSceneManager.IsPreviewScene(rootGO.scene))
    			{
    				PrefabInstanceStatus prefabInstanceStatus = PrefabUtility.GetPrefabInstanceStatus(rootGO);
    				if (prefabInstanceStatus != PrefabInstanceStatus.Connected)
    				{
    					Debug.LogWarning(string.Format("Selected Prefab Root of {0} ({1}) has invalid status of {2}", prefabRoot.gameObject, rootGO, prefabInstanceStatus), rootGO);
    					return;
    				}

    				if (!PrefabUtility.HasPrefabInstanceAnyOverrides(rootGO, false))
    				{
    					Debug.LogWarning(string.Format("Selected Prefab Root of {0} ({1}) doesn't have any overrides", prefabRoot.gameObject, rootGO), rootGO);
    					return;
    				}

    				PrefabUtility.ApplyPrefabInstance(rootGO, InteractionMode.UserAction);
    				AssetDatabase.SaveAssets();

    				Debug.Log(string.Format("Done!\nChanges on {0} applied to {1}", prefabRoot.gameObject, rootGO), rootGO);
    			}
    			else
                {
    				Debug.Log("Done!");
                }
    		}
    		else
    		{
    			Debug.LogWarning("Bad selection! Please select the SpriteRenderer game objects!");
    		}
    	}

    	[MenuItem("Macro/SetupForOutline")]
    	private static void SetupForOutline()
    	{
    		GameObject[] gos = Selection.gameObjects;
    		if (gos != null && gos.Length > 0)
    		{
    			foreach (GameObject go in gos)
    			{
    				if (!go.scene.IsValid() || go.GetComponent<SpriteRenderer>() == null)
    				{
    					Debug.LogWarning("You have selected invalid object, please select only the SpriteRenderer elements of the prefab! And make sure it's in the scene! " + go.name);
    					return;
    				}
    			}

    			Animator parentAnimator = gos[0].transform.GetComponentInParent<Animator>();
    			if (parentAnimator == null)
    			{
    				Debug.LogWarning("Couldn't find an animator in the parents!");
    				return;
    			}

    			Transform prefabRoot = parentAnimator.transform.parent;
    			if (prefabRoot == null)
    			{
    				Debug.LogWarning("Couldn't find the parent of the animator (aka prefab root)! Check hierarchy!");
    				return;
    			}

    			Undo.RegisterFullObjectHierarchyUndo(prefabRoot, "Undo Color in Outline");
    			Undo.SetCurrentGroupName("Undo Color in Outline");

    			Material DrawLayerMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/DrawLayerSpriteOutline.mat");

    			OutlineControllerTouch outlineController = Undo.AddComponent<OutlineControllerTouch>(prefabRoot.gameObject);
    			outlineController.BlitMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/BlitOutline.mat");
    			outlineController.Scale = 0.05f;
    			outlineController.Hardness = 0.9f;

    			Undo.AddComponent<TracingController>(prefabRoot.gameObject);

    			SpriteGroups spriteGroups = Undo.AddComponent<SpriteGroups>(prefabRoot.gameObject);
    			spriteGroups.RendererGroups = new List<SpriteGroups.SpriteGroup>();

    			foreach (GameObject go in gos)
    			{
    				SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
    				sr.sharedMaterial = DrawLayerMaterial;

    				Undo.AddComponent<PolygonCollider2D>(go);

    				spriteGroups.RendererGroups.Add(new SpriteGroups.SpriteGroup() { Group = new List<SpriteRenderer>() { sr } });
    			}

    			Debug.Log("Done setting up! Now trying to save the prefab...");

    			GameObject rootGO = PrefabUtility.GetOutermostPrefabInstanceRoot(prefabRoot.gameObject);

    			if (rootGO == null)
    			{
    				Debug.LogWarning(string.Format("Selected GameObject ({0}) is not part of a prefab", prefabRoot.gameObject), prefabRoot.gameObject);
    				return;
    			}

    			if (!EditorSceneManager.IsPreviewScene(rootGO.scene))
    			{
    				PrefabInstanceStatus prefabInstanceStatus = PrefabUtility.GetPrefabInstanceStatus(rootGO);
    				if (prefabInstanceStatus != PrefabInstanceStatus.Connected)
    				{
    					Debug.LogWarning(string.Format("Selected Prefab Root of {0} ({1}) has invalid status of {2}", prefabRoot.gameObject, rootGO, prefabInstanceStatus), rootGO);
    					return;
    				}

    				if (!PrefabUtility.HasPrefabInstanceAnyOverrides(rootGO, false))
    				{
    					Debug.LogWarning(string.Format("Selected Prefab Root of {0} ({1}) doesn't have any overrides", prefabRoot.gameObject, rootGO), rootGO);
    					return;
    				}

    				PrefabUtility.ApplyPrefabInstance(rootGO, InteractionMode.UserAction);
    				AssetDatabase.SaveAssets();

    				Debug.Log(string.Format("Done!\nChanges on {0} applied to {1}", prefabRoot.gameObject, rootGO), rootGO);
    			}
    			else
    			{
    				Debug.Log("Done!");
    			}
    		}
    		else
    		{
    			Debug.LogWarning("Bad selection! Please select the SpriteRenderer game objects!");
    		}
    	}

    	[MenuItem("Macro/SetupForStampAndPattern")]
    	private static void SetupForStampAndPattern()
    	{
    		GameObject[] gos = Selection.gameObjects;
    		if (gos != null && gos.Length > 0)
    		{
    			foreach (GameObject go in gos)
    			{
    				if (!go.scene.IsValid() || go.GetComponent<SpriteRenderer>() == null)
    				{
    					Debug.LogWarning("You have selected invalid object, please select only the SpriteRenderer elements of the prefab! And make sure it's in the scene! " + go.name);
    					return;
    				}
    			}

    			Animator parentAnimator = gos[0].transform.GetComponentInParent<Animator>();
    			if (parentAnimator == null)
    			{
    				Debug.LogWarning("Couldn't find an animator in the parents!");
    				return;
    			}

    			Transform prefabRoot = parentAnimator.transform.parent;
    			if (prefabRoot == null)
    			{
    				Debug.LogWarning("Couldn't find the parent of the animator (aka prefab root)! Check hierarchy!");
    				return;
    			}

    			Undo.RegisterFullObjectHierarchyUndo(prefabRoot, "Undo Stamp and Pattern setup");
    			Undo.SetCurrentGroupName("Undo Stamp and Pattern setup");

    			Material DrawLayerMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/DrawLayerSprite.mat");

    			StampAndPatternController controller = Undo.AddComponent<StampAndPatternController>(prefabRoot.gameObject);
    			controller.BlitPatternMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/BlitPattern.mat");
    			controller.BlitStampMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/BlitStamp.mat");
    			controller.Scale = 0.075f;
    			controller.Hardness = 0.9f;

    			SpriteGroups spriteGroups = Undo.AddComponent<SpriteGroups>(prefabRoot.gameObject);
    			spriteGroups.RendererGroups = new List<SpriteGroups.SpriteGroup>();

    			foreach (GameObject go in gos)
    			{
    				SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
    				sr.sharedMaterial = DrawLayerMaterial;

    				Undo.AddComponent<PolygonCollider2D>(go);

    				spriteGroups.RendererGroups.Add(new SpriteGroups.SpriteGroup() { Group = new List<SpriteRenderer>() { sr } });
    			}

    			GameObject rootGO = PrefabUtility.GetOutermostPrefabInstanceRoot(prefabRoot.gameObject);

    			if (rootGO == null)
    			{
    				Debug.LogWarning(string.Format("Selected GameObject ({0}) is not part of a prefab", prefabRoot.gameObject), prefabRoot.gameObject);
    				return;
    			}

    			if (!EditorSceneManager.IsPreviewScene(rootGO.scene))
    			{
    				PrefabInstanceStatus prefabInstanceStatus = PrefabUtility.GetPrefabInstanceStatus(rootGO);
    				if (prefabInstanceStatus != PrefabInstanceStatus.Connected)
    				{
    					Debug.LogWarning(string.Format("Selected Prefab Root of {0} ({1}) has invalid status of {2}", prefabRoot.gameObject, rootGO, prefabInstanceStatus), rootGO);
    					return;
    				}

    				if (!PrefabUtility.HasPrefabInstanceAnyOverrides(rootGO, false))
    				{
    					Debug.LogWarning(string.Format("Selected Prefab Root of {0} ({1}) doesn't have any overrides", prefabRoot.gameObject, rootGO), rootGO);
    					return;
    				}

    				PrefabUtility.ApplyPrefabInstance(rootGO, InteractionMode.UserAction);
    				AssetDatabase.SaveAssets();

    				Debug.Log(string.Format("Done!\nChanges on {0} applied to {1}", prefabRoot.gameObject, rootGO), rootGO);
    			}
    			else
    			{
    				Debug.Log("Done!");
    			}
    		}
    		else
    		{
    			Debug.LogWarning("Bad selection! Please select the SpriteRenderer game objects!");
    		}
    	}

    	[MenuItem("Macro/SetupForTracing")]
    	private static void SetupForTracing()
        {
    		GameObject[] gos = Selection.objects.OfType<GameObject>().ToArray(); //  Selection.gameObjects; <--- unordered
    		if (gos != null && gos.Length == 3)
    		{
    			foreach (GameObject go in gos)
    			{
    				if (!go.scene.IsValid() || go.GetComponent<SpriteRenderer>() == null)
    				{
    					Debug.LogWarning("You have selected invalid object, please select only the SpriteRenderer elements! And make sure it's in the scene! " + go.name);
    					return;
    				}
    			}

    			Undo.SetCurrentGroupName("Undo Setup");
    			int undoGroup = Undo.GetCurrentGroup();

    			GameObject startObject = gos[0];
    			GameObject lineObject = gos[1];
    			GameObject endObject = gos[2];

    			GameObject newRoot = new GameObject(startObject.name + "Root");
    			Undo.RegisterCreatedObjectUndo(newRoot, "Undo New Root");
    			newRoot.transform.parent = startObject.transform.parent;
    			Undo.SetTransformParent(startObject.transform, newRoot.transform, "Undo Reparent Start Object");
    			Undo.SetTransformParent(endObject.transform, newRoot.transform, "Undo Reparent End Object");

    			SpriteRenderer startRenderer = startObject.GetComponent<SpriteRenderer>();
    			Undo.RegisterCompleteObjectUndo(startRenderer, "Undo Start Order Layer");
    			startRenderer.sortingOrder = 3;

    			SpriteRenderer endRenderer = endObject.GetComponent<SpriteRenderer>();
    			Undo.RegisterCompleteObjectUndo(endRenderer, "Undo Start Order Layer");
    			endRenderer.sortingOrder = 2;

    			GameObject tracingGO = new GameObject("Tracing");
    			Undo.RegisterCreatedObjectUndo(tracingGO, "Undo Tracing Object");
    			Undo.SetTransformParent(tracingGO.transform, newRoot.transform, "Undo Reparent Tracing Object");

    			GameObject lineFill = GameObject.Instantiate(lineObject);
    			Undo.RegisterCreatedObjectUndo(lineFill, "Undo LineFill");
    			lineFill.name = lineObject.name + "Fill";

    			Undo.SetTransformParent(lineObject.transform, tracingGO.transform, "Undo Line Reparent");
    			Undo.SetTransformParent(lineFill.transform, tracingGO.transform, "Undo LineFill Reparent");

    			GameObject controllerGO = new GameObject("Controller");
    			Undo.RegisterCreatedObjectUndo(controllerGO, "Undo Create Controller");
    			Undo.SetTransformParent(controllerGO.transform, tracingGO.transform, "Undo Controller Reparent");

    			SpriteRenderer lineSR = lineObject.GetComponent<SpriteRenderer>();
    			Undo.RegisterCompleteObjectUndo(lineSR, "Undo Line Settings");
    			lineSR.sortingOrder = 0;
    			lineSR.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Tracing/TracingBase.mat");

    			SpriteRenderer lineFillSR = lineFill.GetComponent<SpriteRenderer>();
    			lineFillSR.sortingOrder = 1;
    			lineFillSR.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Tracing/TracingReveal.mat");

    			SetSpriteColorTintOffset spriteOffset = Undo.AddComponent<SetSpriteColorTintOffset>(lineFill);
    			spriteOffset.SetColor(Color.white, Color.red);

    			TracingManagerTouch manager = Undo.AddComponent<TracingManagerTouch>(controllerGO);
    			Undo.AddComponent<TracingController>(controllerGO);
    			Undo.AddComponent<TracingMesh>(controllerGO);

    			manager.StartObject = startObject.transform;

    			MeshRenderer meshRenderer = controllerGO.GetComponent<MeshRenderer>();
    			meshRenderer.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Tracing/TracingMask.mat");

    			Undo.CollapseUndoOperations(undoGroup);
    		}
    		else
            {
    			Debug.LogWarning("Bad selection! Please select the Start Object, Line and the End Object, all with SpriteRenderer!");
    		}
        }

    	/*
    	[MenuItem("Macro/ResetRootScale")]
    	private static void ResetRootScale()
    	{
    		GameObject[] gos = Selection.gameObjects;
    		if (gos != null && gos.Length > 0)
    		{
    			foreach (GameObject go in gos)
    			{
    				if (!go.scene.IsValid())
    				{
    					Debug.LogWarning("You have selected invalid object, please select only scene objects! " + go.name);
    					return;
    				}
    			}

    			Undo.SetCurrentGroupName("Undo Reset Root Scale");
    			int group = Undo.GetCurrentGroup();

    			foreach (GameObject go in gos)
    			{
    				Undo.RecordObject(go, "Undo Parent Transform");
    				//Vector3 originalScale = go.transform.localScale;

    				List<Transform> children = new List<Transform>();
    				for (int i = 0; i < go.transform.childCount; i++)
    				{
    					Transform child = go.transform.GetChild(i);
    					children.Add(child);
    					Undo.RecordObject(go, "Undo Child Transform");
    					//child.localScale = Vector3.Scale(child.localScale, originalScale);
    					child.parent = go.transform.parent;
    				}

    				foreach (children in children)

    				go.transform.localScale = Vector3.one;

    				Undo.CollapseUndoOperations(group);
    			}
    		}
    	}
    	*/
    }


}