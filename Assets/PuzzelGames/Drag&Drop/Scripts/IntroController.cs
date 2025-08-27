using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Interactive.DRagDrop
{
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.Video;

    public class IntroController : MonoBehaviour
    {
        public VideoPlayer videoPlayer;
        public Image skipButton;
        public float skipTimeout = 5.0f;

        // Start is called before the first frame update
        void Start()
        {
            videoPlayer.loopPointReached += EndReached;
        }

        //bool videoIsPlaying = false;

        // Update is called once per frame
        void Update()
        {
            /*
            if (!videoIsPlaying)
            {
                videoPlayer.Play();
                videoIsPlaying = true;
            }
            */

            if (skipTimeout > 0)
            {
                skipTimeout -= Time.deltaTime;
                if (skipTimeout < 0)
                {
                    skipButton.gameObject.SetActive(true);
                    skipButton.color = new Color(1.0f, 1.0f, 1.0f, 0f);
                    skipButton.DOFade(0.69f, 1.0f);
                }
            }
        }

        void EndReached(VideoPlayer vp)
        {
            GoToMainMenu();
        }

        public void GoToMainMenu()
        {
            TransitionManager.Instance.ShowFade(Color.black, 2.0f, () => SceneLoader.Instance.LoadScene("MainMenu"));
        }
    }


}