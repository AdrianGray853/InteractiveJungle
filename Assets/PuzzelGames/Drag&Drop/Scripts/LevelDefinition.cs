using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class LevelDefinition : MonoBehaviour
    {
        public Collider2D Collider;
        public Animator Bubble;

        bool isEnabled = false;

        public bool IsEnabled => isEnabled;

        public void SetEnabled(bool enabled)
        {
            if (enabled)
                Bubble.gameObject.SetActive(false);

            isEnabled = enabled;
        }

        public void PlayUnlockAnim()
        {
            Bubble.Play("BuleExplosion");
            isEnabled = true;
        }
    }


}