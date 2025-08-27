using UnityEditor;
using UnityEngine;

public class CreateBoneConstraint : EditorWindow
{
	private Transform transformReference;
	private bool useParentTransform;

	[MenuItem("Macro/Create Bone Constraint")]
	public static void ShowWindow()
	{
		EditorWindow window = GetWindow<CreateBoneConstraint>("Create Bone Constraint");
		Rect winRect = window.position;
		winRect.width = 300.0f;
		winRect.height = 100.0f;
		window.position = winRect;
	}

	private void OnGUI()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label("Use Parent", EditorStyles.boldLabel);
		useParentTransform = EditorGUILayout.Toggle(useParentTransform);
		GUILayout.EndHorizontal();

		if (!useParentTransform)
		{
			GUILayout.Label("Bone Reference", EditorStyles.boldLabel);
			transformReference = EditorGUILayout.ObjectField(transformReference, typeof(Transform), true) as Transform;
		}

		GUILayout.Space(10);

		if (GUILayout.Button("Create Constraint"))
		{
			if (Selection.activeGameObject != null && Selection.activeGameObject.scene.IsValid() &&
				(transformReference != null || useParentTransform && Selection.activeGameObject.transform.parent != null))
			{
				GameObject bone = Selection.activeGameObject;
				Undo.RecordObject(bone, "Undo apply bone constraint");
				BoneRotationConstraint constraint = Undo.AddComponent<BoneRotationConstraint>(bone);

				constraint.PositionOffset = bone.transform.localPosition;
				constraint.RotationOffset = bone.transform.localRotation;
				constraint.ScaleOffset = bone.transform.localScale;

				Transform refBone = transformReference;
				if (useParentTransform)
					refBone = bone.transform.parent;

				constraint.ReferencePosition = refBone.localPosition;
				constraint.ReferenceRotation = refBone.localRotation;
				constraint.ReferenceScale = refBone.localScale;

				constraint.ReferenceBone = refBone;

				EditorUtility.SetDirty(bone);
			} 
			else
            {
				Debug.LogWarning("Please select the bone you want to constraint!");
            }
		}
	}
}
