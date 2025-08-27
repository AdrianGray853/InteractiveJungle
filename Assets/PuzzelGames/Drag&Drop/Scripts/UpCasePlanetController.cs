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

    public class UpCasePlanetController : MonoBehaviour
    {
        public CharacterPulseController[] Letters;

        public void HideLetters()
        {
            foreach (var letter in Letters)
            {
                letter.Stop();
                Quaternion targetRotation = letter.transform.rotation * Quaternion.Euler(0f, 0f, 360.0f);
                DOTween.Sequence()
                    .AppendInterval(0.75f + Random.Range(0f, 0.3f))
                    .Append(letter.transform.DOScale(Vector3.zero, 0.75f).SetEase(Ease.InBack))
                    .Join(letter.transform.DOMove(transform.position + (Random.insideUnitCircle * 1.0f).ToVector3(), 0.75f).SetEase(Ease.InBack))
                    .Join(letter.transform.DORotateQuaternion(targetRotation, 0.75f).SetEase(Ease.InBack));
            }
        }
    }


}