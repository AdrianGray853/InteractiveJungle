using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class SpinLoading : MonoBehaviour
    {
        public enum eLoadingType
        {
            RotateAround = 0,
            Spin = 1,
            Pulse = 2
        }

        public eLoadingType LoadingType = eLoadingType.RotateAround;
        public float Distance = 100.0f;
        public float Speed = 120.0f;
        public float ScaleMagnitude = 0.2f;

        Vector3 initialPosition;
        Vector3 initialScale;
        float angle;

        // Start is called before the first frame update
        void Start()
        {
            initialPosition = transform.position;
            initialScale = transform.localScale;
        }

        // Update is called once per frame
        void Update()
        {
            angle += Speed * Time.deltaTime;
            if (LoadingType == eLoadingType.Spin)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
            else if (LoadingType == eLoadingType.RotateAround)
            {
                transform.position = initialPosition + Utils.AngleToVector3(angle) * Distance;
            }
            else if (LoadingType == eLoadingType.Pulse)
            {
                transform.localScale = initialScale * (1.0f + Mathf.Sin(Speed * Time.time) * ScaleMagnitude);
            }
        }
    }


}