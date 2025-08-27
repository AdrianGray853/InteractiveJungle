using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class BackgroundNoriAnim : MonoBehaviour
    {
        [System.Serializable]
        public class NorDesc {
            public Transform Nor;
            public float MinSpeed;
            public float MaxSpeed;
            [HideInInspector]
            public float Speed;
            public float MaxX;
            public float RespawnX;
        }

        public NorDesc[] Nori;

        // Start is called before the first frame update
        void Start()
        {
            foreach (var nor in Nori)
            {
                nor.Speed = Random.Range(nor.MinSpeed, nor.MaxSpeed);
            }
        }

        // Update is called once per frame
        void Update()
        {
            foreach(var nor in Nori)
            {
                nor.Nor.position += Vector3.right * (nor.Speed * Time.deltaTime);
                if (nor.Nor.position.x > nor.MaxX)
                {
                    nor.Nor.position = new Vector3(nor.RespawnX, nor.Nor.position.y, nor.Nor.position.z);
                    nor.Speed = Random.Range(nor.MinSpeed, nor.MaxSpeed);
                }
            }
        }
    }


}