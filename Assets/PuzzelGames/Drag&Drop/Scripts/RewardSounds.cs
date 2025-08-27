using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.DRagDrop
{
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class RewardSounds : MonoBehaviour
    {
        public string[] Sounds;
        public float SoundDelay = 1.0f;

        // Start is called before the first frame update
        void Start()
        {
            DOTween.Sequence()
                .AppendInterval(SoundDelay)
                .AppendCallback(() => SoundManager.Instance.PlaySFX(Sounds.GetRandomElement()));
        }
    }


}