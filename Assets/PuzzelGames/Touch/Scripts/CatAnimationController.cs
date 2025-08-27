using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.Touch
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class CatAnimationController : MonoBehaviour
    {
        public Animator CatAnimator;
        public float ChangeInterval = 5.0f;

        private float counter;

        private readonly string[] animationTriggers = new string[] { /*"ToLove", */ "ToHumble" };

        // Start is called before the first frame update
        void Start()
        {
            if (CatAnimator == null)
                CatAnimator = GetComponent<Animator>();

            counter = ChangeInterval;
        }

        public void TriggerHearts()
    	{
            CatAnimator.SetTrigger("ToLove");
    	}

        // Update is called once per frame
        void Update()
        {
            if (counter > 0)
    		{
                counter -= Time.deltaTime;

                if (counter < 0)
    			{
                    if (Random.value < 0.5f)
                        CatAnimator.SetTrigger(animationTriggers[Random.Range(0, animationTriggers.Length)]);
                    counter = ChangeInterval;
    			}
    		}            
        }
    }


}