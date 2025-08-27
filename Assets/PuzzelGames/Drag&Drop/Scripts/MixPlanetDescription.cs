using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class MixPlanetDescription : MonoBehaviour
    {
        public Collider2D Collider;
        public string TargetScene;
        public GameObject VisitedGFX;
        public GameObject NotVisitedGFX;
        public int PlanetIndex;

        // Start is called before the first frame update
        void Start()
        {
            if (ProgressManager.Instance.IsMixLevelVisited(PlanetIndex))
            {
                VisitedGFX.SetActive(true);
                NotVisitedGFX.SetActive(false);
            } 
            else
            {
                VisitedGFX.SetActive(false);
                NotVisitedGFX.SetActive(true);
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }


}