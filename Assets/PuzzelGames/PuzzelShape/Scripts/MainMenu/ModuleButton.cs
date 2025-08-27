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

    public class ModuleButton : MonoBehaviour
    {
        public RawImage Highlight;
        public Transform Target;
        public float Speed = 1200.0f;
        public float MinInterval = 3.0f;
        public float MaxInterval = 5.0f;

        float interval;
        float interpolation;
        Vector3 originalPosition;
        Vector3 originalScale;

        bool animating = false;

        // Start is called before the first frame update
        void Start()
        {
            interval = Random.Range(MinInterval, MaxInterval);
            originalPosition = Highlight.transform.localPosition;
            originalScale = Highlight.transform.localScale;
            transform.DOScale(Vector3.zero, 0.5f).From().SetEase(Ease.OutBack).SetDelay(Random.Range(0f, 0.5f));
        }

        // Update is called once per frame
        void Update()
        {
            if (animating)
            {
                interpolation = Mathf.MoveTowards(interpolation, 1.0f, Speed * Time.deltaTime);
                Highlight.transform.localPosition = Vector3.Lerp(Highlight.transform.localPosition, Target.localPosition, interpolation);
                Highlight.transform.localScale = Vector3.Lerp(Highlight.transform.localScale, Target.localScale, interpolation);
                if (interpolation == 1.0f)
                {
                    animating = false;
                }
            }

            interval -= Time.deltaTime;
            if (interval < 0)
            {
                animating = true;
                Highlight.transform.localPosition = originalPosition;
                Highlight.transform.localScale = originalScale;
                interval = Random.Range(MinInterval, MaxInterval);
                interpolation = 0f;
            }
        }
    }


}