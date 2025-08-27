using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class SatelliteBehaviour : MonoBehaviour
    {
        public Vector3 Offset;
        public Quaternion Rotation;
        public float Radius = 1.0f;
        public float Speed = 1.0f;
        public float PositionOffset;
        public float Scale = 1.0f;
        public float ScaleMultiplier;

        public float CutOutZPos;

        SpriteRenderer spriteRenderer;

        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            Vector2 circlePos = new Vector2
            (
                Offset.x + Mathf.Sin(Time.time * Speed + PositionOffset) * Radius,
                Offset.y + Mathf.Cos(Time.time * Speed + PositionOffset) * Radius
            );
            transform.localPosition = Offset + Rotation * circlePos;
            transform.localScale = Vector3.one * (Scale - transform.position.z * ScaleMultiplier);

            if (transform.position.z > CutOutZPos)
            {
                spriteRenderer.sortingOrder = -1;
            } 
            else
            {
                spriteRenderer.sortingOrder = 2;
            }
        }
    }


}