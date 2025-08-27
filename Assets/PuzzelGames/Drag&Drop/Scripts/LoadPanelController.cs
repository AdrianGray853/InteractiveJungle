using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class LoadPanelController : MonoBehaviour
    {
        public OnBoardingController BoardingController;
        public Transform Spaceship;
        public float Radius = 5.0f;
        public float Speed = 720.0f;
        public float LoadingTime = 5.0f;

        float angle = 0f;
        Vector3 center;
        float loadCooldown;

        private void Awake()
        {
            center = Spaceship.position;
        }

        // Start is called before the first frame update
        void OnEnable()
        {
            loadCooldown = LoadingTime;
            Spaceship.position = center + Utils.AngleToVector3(angle) * Radius;
        }

        // Update is called once per frame
        void Update()
        {
            loadCooldown -= Time.deltaTime;
            if (loadCooldown < 0f)
            {
                BoardingController.ShowStartTrial(true);
            }

            angle += Speed * Time.deltaTime;
            Spaceship.position = center + Utils.AngleToVector3(angle) * Radius;
        }
    }


}