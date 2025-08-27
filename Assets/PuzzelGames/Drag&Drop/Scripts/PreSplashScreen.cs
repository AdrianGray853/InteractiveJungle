using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class PreSplashScreen : MonoBehaviour
    {
        static bool initiated = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void BeforeSplashScreenCallback()
        {
            if (!initiated)
                InitializeStuff();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void BeforeSceneLoadCallback()
        {
            if (!initiated)
                InitializeStuff();
        }
        static void InitializeStuff()
        {
            initiated = true;
            //Screen.orientation = ScreenOrientation.Portrait;
            //Debug.Log("Initializing before splash screen...");


            // Remove analytics for kid!
            UnityEngine.Analytics.Analytics.enabled = false;
            UnityEngine.Analytics.Analytics.deviceStatsEnabled = false;
            UnityEngine.Analytics.Analytics.limitUserTracking = true;
            UnityEngine.Analytics.Analytics.initializeOnStartup = false;

            try
            {
                UnityEngine.Analytics.PerformanceReporting.enabled = false;
            }
            catch (System.MissingMethodException)
            {
                // Do nothing, old unity version... 5.6 - 2017.2
            }
        }
    }


}