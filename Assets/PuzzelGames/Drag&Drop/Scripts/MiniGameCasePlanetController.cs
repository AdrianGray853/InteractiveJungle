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

    public class MiniGameCasePlanetController : MonoBehaviour
    {
        public Transform Spaceship;

        public void ShowSelectedAnimation()
        {
            DOTween.Sequence()
                    .AppendInterval(0.75f + Random.Range(0f, 0.1f))
                    .Append(Spaceship.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack));
        }
    }


}