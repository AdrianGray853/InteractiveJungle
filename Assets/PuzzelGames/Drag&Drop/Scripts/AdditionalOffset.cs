using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class AdditionalOffset : MonoBehaviour
    {
        public Vector3 PositionOffset;
        public Vector4 PrevizSizeOffset = new Vector4(0.0f, 0.0f, 1f, 1f);

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(transform.GetChild(0).position + PositionOffset + new Vector3(PrevizSizeOffset.x, PrevizSizeOffset.y), 
                new Vector3(PrevizSizeOffset.z, PrevizSizeOffset.w));
        }
    }


}