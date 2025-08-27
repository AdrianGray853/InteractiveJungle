using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.PuzzelShape
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class SpawnZone : MonoBehaviour
    {
        public enum eType
        {
            Box,
            Circle
        }

        public eType Type = eType.Box;

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

        private void OnDrawGizmos()
        {
            Gizmos.color = GizmoColor;
            if (Type == eType.Circle)
                Gizmos.DrawWireSphere(transform.position, Radius);
            else
                Gizmos.DrawWireCube(transform.position, BoxSize * 2.0f);
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

        public Vector3 GetRandomPositionInside()
        {
            if (Type == eType.Circle)
            {
                return transform.position + Random.insideUnitCircle.ToVector3() * Radius;
            } 
            else if (Type == eType.Box)
            {
                return transform.position + new Vector3(Random.Range(-BoxSize.x, BoxSize.x), Random.Range(-BoxSize.y, BoxSize.y), 0f);
            }

            return transform.position;
        }
    }


}