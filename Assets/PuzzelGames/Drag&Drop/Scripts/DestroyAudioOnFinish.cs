using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class DestroyAudioOnFinish : MonoBehaviour
    {
        float timeToDestruction = -1.0f;

        // Start is called before the first frame update
        void Start()
        {
            AudioSource source = GetComponent<AudioSource>();
            if (source == null || source.clip == null)
            {
                Destroy(gameObject);
                return;
            }

            timeToDestruction = source.clip.length + 0.2f;
        }

        // Update is called once per frame
        void Update()
        {
            if (timeToDestruction > 0f)
                timeToDestruction -= Time.deltaTime;

            if (timeToDestruction < 0)
                Destroy(gameObject);
        }
    }


}