using System;
using System.Linq;
using UnityEngine;

namespace Interactive.PuzzelShape
{
using System;
using System.Linq;
using UnityEngine;

    public class DragHelperShape
    {
        Vector3 offset; // this is the attach offset
        public Transform DraggingTransform { get; private set; }
        public int FingerId { get; private set; }
        Vector3 RestrictOffset; // this is the offset for limiting the movement on screen

        public bool IsRigidBody { get; private set; }

        Rigidbody2D rigidBody;

        public DragHelperShape(int fingerId, Transform transform, Vector3 attachPosition, Vector3 offset, bool isRigidBody = false)
        {
    		DraggingTransform = transform;
    		this.offset = Quaternion.Inverse(transform.rotation) * (transform.position - attachPosition);
    		FingerId = fingerId;
    		RestrictOffset = offset;
            IsRigidBody = isRigidBody;

            if (IsRigidBody)
                rigidBody = transform.GetComponent<Rigidbody2D>();
        }

        public void Update(Vector3 newPosition)
        {
            if (DraggingTransform != null)
            {
                if (IsRigidBody)
    			{
                    //rigidBody.velocity = (newPosition - DraggingTransform.position) * Time.deltaTime * Force;
                    Vector3 forcePos = DraggingTransform.position + DraggingTransform.rotation * -offset;
    				//rigidBody.AddForceAtPosition((newPosition - forcePos) * Force - MemoryGameSetup.GravMult * Physics.gravity / Time.deltaTime, forcePos);
    				Vector3 dist = newPosition - forcePos;
    				Vector2 tgtVel = Vector3.ClampMagnitude(DragManagerShape.Instance._toVel * dist, DragManagerShape.Instance._maxVel);
    				Vector2 error = tgtVel - rigidBody.GetPointVelocity(forcePos);
    				Vector2 force = Vector3.ClampMagnitude(DragManagerShape.Instance._gain * error, DragManagerShape.Instance._maxForce);
    				rigidBody.AddForceAtPosition(force, forcePos);
    			}
                else
    			{
                    newPosition = DragManagerShape.ClampPositionToSafeScreen(newPosition + DraggingTransform.rotation * offset + RestrictOffset) - RestrictOffset;
                    DraggingTransform.position = newPosition;
                }
            }
        }

        public void DrawGizmos()
    	{
            Gizmos.DrawSphere(DraggingTransform.position + DraggingTransform.rotation * -offset, 1.0f);
    	}
    }



}