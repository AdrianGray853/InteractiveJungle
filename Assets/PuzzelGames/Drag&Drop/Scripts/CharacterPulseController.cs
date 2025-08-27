using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

    public class CharacterPulseController : MonoBehaviour
    {
        public float PulseMinInterval = 3.0f;
        public float PulseMaxInterval = 5.0f;
        public float PulseTimeInOut = 0.3f;
        public Vector3 targetScale = Vector3.one;

        public Ease Ease1;
        public Ease Ease2;

        float timer;

        bool stopped = false;
        Sequence sequence;
        Vector3 initialScale;

        // Start is called before the first frame update
        void Start()
        {
            timer = Random.Range(PulseMinInterval, PulseMaxInterval);
            initialScale = transform.localScale;
        }

        // Update is called once per frame
        void Update()
        {
            if (stopped)
                return;

            timer -= Time.deltaTime;
            if (timer < 0)
            {
                if (sequence != null)
                    sequence.Kill(true);
                sequence = DOTween.Sequence()
                    .Append(transform.DOScale(new Vector3(initialScale.x * targetScale.x, initialScale.y * targetScale.y, initialScale.z * targetScale.z), 
                                            PulseTimeInOut).SetEase(Ease1))
                    .Append(transform.DOScale(initialScale, PulseTimeInOut).SetEase(Ease2));

                timer = Random.Range(PulseMinInterval, PulseMaxInterval);
            }
        }

        public void Stop()
        {
            if (sequence != null)
            {
                sequence.Kill(true);
                sequence = null;
            }
            stopped = true;
        }
    }


}