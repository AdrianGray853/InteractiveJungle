using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class SetSpriteColor : MonoBehaviour
    {
        public Color BaseColor = Color.white;
        public Color OutlineColor = Color.black;

        private SpriteRenderer spriteRenderer;
        private MaterialPropertyBlock propertyBlock;

        // Start is called before the first frame update
        void Awake()
        {
            UpdateColor();       
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void UpdateColor()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
            if (propertyBlock == null)
            {
                propertyBlock = new MaterialPropertyBlock();
                spriteRenderer.GetPropertyBlock(propertyBlock);
            }
            propertyBlock.SetColor("_Color", BaseColor);
            propertyBlock.SetColor("_BaseColor", OutlineColor);
            spriteRenderer.SetPropertyBlock(propertyBlock);
        }

        public void SetColor(Color baseColor, Color outlineColor)
        {
            BaseColor = baseColor;
            OutlineColor = outlineColor;
            UpdateColor();
        }

    #if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateColor();
        }
    #endif
    }


}