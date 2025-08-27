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


    // This is a bad name! This only takes care of one planet! See MixPlanetDescription class for general usage!
    public class MixCasePlanetController : MonoBehaviour
    {
        public CharacterPulseController[] Letters;

        public void HideLetters()
        {
            foreach (var letter in Letters)
            {
                letter.Stop();
                DOTween.Sequence()
                    .AppendInterval(0.75f + Random.Range(0f, 0.3f))
                    .Append(letter.transform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack));
            }
        }
    }


}