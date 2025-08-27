using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class LetterDefinition : MonoBehaviour
    {
        public char Letter;

        public Transform TargetPosition;
        public Collider2D Collider;
        [HideInInspector]
        public bool ReadyForDrag = false; // For Source
        [HideInInspector]
        public bool IsActive = true; // For Target
        [HideInInspector]
        public Vector3 OriginalPosition;
    }


}