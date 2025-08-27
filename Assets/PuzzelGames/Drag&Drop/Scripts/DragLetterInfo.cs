using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class DragLetterInfo : MonoBehaviour
    {
        public char Letter;
        public Vector3 OriginalPosition;
        public Collider2D Collider;
        public SpriteRenderer Renderer;
        public bool IsUpperCase;

        public bool IsActive = true;
    }


}