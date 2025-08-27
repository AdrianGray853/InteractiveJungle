using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BoneRotationConstraint : MonoBehaviour
{

    public float InheritPosition;
    public float InheritRotation;
    public float InheritScale;
    
    [Header("Offset Settings")]
    public Vector3 PositionOffset;
    public Quaternion RotationOffset;
    public Vector3 ScaleOffset = Vector3.one;

    [HideInInspector]
    public Transform ReferenceBone;

    [HideInInspector]
    public Vector3 ReferencePosition;
    [HideInInspector]
    public Quaternion ReferenceRotation;
    [HideInInspector]
    public Vector3 ReferenceScale;

    // Update is called once per frame
    void LateUpdate()
    {
        if (ReferenceBone != null)
        {
            transform.localPosition = PositionOffset + (ReferenceBone.localPosition - ReferencePosition) * InheritPosition;
            transform.localRotation = Quaternion.LerpUnclamped(RotationOffset,
                    RotationOffset * (Quaternion.Inverse(ReferenceBone.localRotation) * ReferenceRotation),
                    InheritRotation);
            transform.localScale = ScaleOffset + (ReferenceBone.localScale - ReferenceScale) * InheritScale;
        }
    }
}
