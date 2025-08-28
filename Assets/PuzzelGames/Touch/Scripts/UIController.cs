using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.Touch
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class UIController : MonoBehaviour
    {
        public void GoToJungle()
    	{
            TransitionManagerTouch.Instance.ShowFade(new Color(0.53f, 0.84f, 1.0f), 0.5f, () => SceneLoader.Instance.LoadScene("Jungle"));

            SoundManagerTouch.Instance.PlaySFX("Click");
        }

        public void Screenshot()
    	{
            GameManagerTouch.Instance.Screenshot();
        }

        public void DEBUG_GOTO_TESTING_SCENE()
        {
           SceneLoader.Instance.LoadScene("TESTING");
        }
    }


}