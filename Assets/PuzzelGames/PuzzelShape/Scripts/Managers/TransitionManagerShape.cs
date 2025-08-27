using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Interactive.PuzzelShape
{
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

    public class TransitionManagerShape : MonoBehaviour
    {
        public static TransitionManagerShape Instance { get; private set; }


        public RawImage FadeImage;
        public Color DefaultFadeColor = Color.white;
        public bool InTransition;

        Color FadeColor = Color.white;

        Sequence fadeSequence;

        bool waitCondition;
        System.Func<bool> waitConditionFunc;
        float waitContinueDuration = 1.0f;
        TweenCallback waitDoneCallback;



        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                DontDestroyOnLoad(this);
                Instance = this;
            }
        }

        void Update()
        {
            if (waitCondition)
            {
                if (waitConditionFunc == null || !waitConditionFunc())
                {
                    waitConditionFunc = null;
                    waitCondition = false;

                    // Finish the fade
                    if (fadeSequence != null)
                        fadeSequence.Kill(true);
                    fadeSequence = DOTween.Sequence();
                    fadeSequence.Append(FadeImage.DOFade(0f, waitContinueDuration * 0.5f));
                    if (waitDoneCallback != null)
                        fadeSequence.AppendCallback(waitDoneCallback);
                    fadeSequence.AppendCallback(() =>
                    {
                        FadeImage.gameObject.SetActive(false);
                        InTransition = false;
                    });
                    fadeSequence.SetId("Transition");
                }
            }
        }

        public void SetFadeColor(Color color)
    	{
            FadeColor = color;
    	}

        public void SetDefaultFadeColor()
    	{
            FadeColor = DefaultFadeColor;
    	}

        public void TerminateFade()
        {
            if (fadeSequence != null)
                fadeSequence.Kill(true);
        }

        public Sequence ShowFade(Color fadeColor, float duration = 1.0f, TweenCallback fadeOutCallback = null, TweenCallback doneCallback = null, System.Func<bool> waitForCondition = null)
        {
            if (InTransition)
                return DOTween.Sequence();

            if (fadeSequence != null)
                fadeSequence.Kill(true);

            InTransition = true;
            FadeImage.gameObject.SetActive(true);
            fadeColor.a = 0f;
            FadeImage.color = fadeColor;
            fadeSequence = DOTween.Sequence();
            fadeSequence.Append(FadeImage.DOFade(1.0f, duration * 0.5f));
            if (fadeOutCallback != null)
                fadeSequence.AppendCallback(fadeOutCallback);

            if (waitForCondition != null)
            {
                waitContinueDuration = duration;
                waitDoneCallback = doneCallback;
                fadeSequence.AppendCallback(() => waitCondition = true);
            }
            else
            {
                fadeSequence.Append(FadeImage.DOFade(0f, duration * 0.5f));
                if (doneCallback != null)
                    fadeSequence.AppendCallback(doneCallback);
                fadeSequence.AppendCallback(() =>
                {
                    FadeImage.gameObject.SetActive(false);
                    InTransition = false;
                });
            }
            fadeSequence.SetId("Transition");
            return fadeSequence;
        }

        public Sequence ShowFade(float duration = 1.0f, TweenCallback fadeOutCallback = null, TweenCallback doneCallback = null)
        {
            return ShowFade(FadeColor, duration, fadeOutCallback, doneCallback);
        }
    }


}