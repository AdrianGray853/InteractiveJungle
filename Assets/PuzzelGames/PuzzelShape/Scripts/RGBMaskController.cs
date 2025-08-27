using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.PuzzelShape
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class RGBMaskController : MonoBehaviour
    {
        public Color TintColor = Color.white;
        public Color RedColor = Color.red;
        public Color GreenColor = Color.green;
        public Color BlueColor = Color.blue;

        SpriteRenderer spriteRenderer = null;
        MaterialPropertyBlock pblock = null;


        private void Awake()
        {
            UpdateMaterial();
        }

        public void SetTintColor(Color tintColor)
        {
            spriteRenderer.GetPropertyBlock(pblock);
            TintColor = tintColor;
            pblock.SetColor("_Color", tintColor);
            spriteRenderer.SetPropertyBlock(pblock);
        }

        public void SetRedColor(Color redColor)
        {
            spriteRenderer.GetPropertyBlock(pblock);
            RedColor = redColor;
            pblock.SetColor("_RedColor", redColor);
            spriteRenderer.SetPropertyBlock(pblock);
        }

        public void SetGreenColor(Color greenColor)
        {
            spriteRenderer.GetPropertyBlock(pblock);
            GreenColor = greenColor;
            pblock.SetColor("_GreenColor", greenColor);
            spriteRenderer.SetPropertyBlock(pblock);
        }

        public void SetBlueColor(Color blueColor)
        {
            spriteRenderer.GetPropertyBlock(pblock);
            BlueColor = blueColor;
            pblock.SetColor("_BlueColor", blueColor);
            spriteRenderer.SetPropertyBlock(pblock);
        }

        public void SetRGBColor(Color redColor, Color greenColor, Color blueColor)
        {
            spriteRenderer.GetPropertyBlock(pblock);
            RedColor = redColor;
            GreenColor = greenColor;
            BlueColor = blueColor;
            pblock.SetColor("_RedColor", redColor);
            pblock.SetColor("_GreenColor", greenColor);
            pblock.SetColor("_BlueColor", blueColor);
            spriteRenderer.SetPropertyBlock(pblock);
        }

        public void UpdateMaterial()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            if (pblock == null)
                pblock = new MaterialPropertyBlock();

            spriteRenderer.GetPropertyBlock(pblock);
            pblock.SetColor("_Color", TintColor);
            pblock.SetColor("_RedColor", RedColor);
            pblock.SetColor("_GreenColor", GreenColor);
            pblock.SetColor("_BlueColor", BlueColor);
            spriteRenderer.SetPropertyBlock(pblock);
        }

    #if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateMaterial();
        }
    #endif
    }


}