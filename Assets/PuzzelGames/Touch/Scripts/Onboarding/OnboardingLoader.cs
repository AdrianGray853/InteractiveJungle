using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.Touch
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class OnboardingLoader : MonoBehaviour
    {
        public GameObject OnboardingPhone;
        public GameObject OnboardingTablet;

    	private void Awake()
    	{
    		if (UtilsTouch.IsTablet()) 
    		{
    			Instantiate(OnboardingTablet);
    		}
    		else
    		{
    			Instantiate(OnboardingPhone);
    		}
    	}
    }


}