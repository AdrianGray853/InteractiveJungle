using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class TargetPositionController : MonoBehaviour
    {
        public enum eType
    	{
            Circle,
            Box
    	}

        public eType Type = eType.Circle;

        public float Radius;
        public Vector2 BoxSize = Vector2.one;
        public Color GizmoColor = Color.red;

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public bool InRange(Vector3 position)
        {
            if (Type == eType.Circle)
            {
                return (position.DistanceSq(transform.position) <= Radius * Radius);
            } 
            else if (Type == eType.Box)
    		{
                if (position.x < transform.position.x - BoxSize.x) return false;
                if (position.x > transform.position.x + BoxSize.x) return false;
                if (position.y < transform.position.y - BoxSize.y) return false;
                if (position.y > transform.position.y + BoxSize.y) return false;
                return true;
            }
            return false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = GizmoColor;
            if (Type == eType.Circle)
                Gizmos.DrawWireSphere(transform.position, Radius);
            else
                Gizmos.DrawWireCube(transform.position, BoxSize * 2.0f);
        }
    }


}