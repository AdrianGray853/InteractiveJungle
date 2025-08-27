using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class RocketBoosterRandomizer : MonoBehaviour
    {
        public float minAddScaleY = 0.9f;
        public float maxAddScaleY = 1.2f;
        public float minAlpha = 0.75f;
        public float maxAlpha = 1.0f;

        SpriteRenderer spriteRenderer;
        Vector3 initialScale;

        // Start is called before the first frame update
        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            initialScale = transform.localScale;
        }

        // Update is called once per frame
        void Update()
        {
            Color color = spriteRenderer.color;
            color.a = Random.Range(minAlpha, maxAlpha);
            spriteRenderer.color = color;

            Vector3 scale = transform.localScale;
            scale.y = initialScale.y * Random.Range(minAddScaleY, maxAddScaleY);
            transform.localScale = scale;
        }
    }


}