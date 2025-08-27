using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

namespace Interactive.PuzzelShape
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

    public class FingerHint : MonoBehaviour
    {

        public float ShowTime = 2.0f;
        public float FadeInOutTime = 0.5f;

        Sequence s;

        public float ShowHint()
        {
            if (s != null)
                s.Kill(true);

            Image image = GetComponent<Image>();
            Color c = image.color;
            c.a = 0.0f;
            image.color = c;
            s = DOTween.Sequence();
            s.AppendCallback(() => gameObject.SetActive(true));
            s.Append(image.DOFade(1.0f, FadeInOutTime));
            s.AppendInterval(ShowTime);
            s.Append(image.DOFade(0.0f, FadeInOutTime));
            s.AppendCallback(() => gameObject.SetActive(false));
            return s.Duration();
        }
    }


}