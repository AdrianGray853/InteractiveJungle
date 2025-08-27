using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.PuzzelShape
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class OutlineControllerShape : MonoBehaviour
    {
        public bool GrayscaleImage = false;
        public Color TintColor = Color.white;
        public Color BaseColor = Color.black;
        public Color OffsetColor = new Color(1.0f, 1.0f, 1.0f, 0f);
        [Header("Outline")]
        public Color OutlineColor = Color.white;
        public float OutlineThickness = 0f;
        [Header("Other")]
        public bool Antialiased = false;

        SpriteRenderer spriteRenderer = null;
        MaterialPropertyBlock pblock = null;


        private void Awake()
        {
            UpdateMaterial();
        }

        public void SetGrayscaleColor(Color tintColor, Color baseColor)
        {
            CheckReferences();

            spriteRenderer.GetPropertyBlock(pblock);
            TintColor = tintColor;
            BaseColor = baseColor;
            GrayscaleImage = true;
            pblock.SetColor("_Color", tintColor);
            pblock.SetColor("_BaseColor", baseColor);
            pblock.SetFloat("_GrayscaleImage", 1f);
            spriteRenderer.SetPropertyBlock(pblock);
        }

        public void SetGrayscaleEnable(bool enable)
        {
            CheckReferences();

            spriteRenderer.GetPropertyBlock(pblock);
            GrayscaleImage = enable;
            pblock.SetFloat("_GrayscaleImage", enable ? 1f : 0f);
            spriteRenderer.SetPropertyBlock(pblock);
        }

        public void SetTintColor(Color tintColor)
        {
            CheckReferences();

            spriteRenderer.GetPropertyBlock(pblock);
            TintColor = tintColor;
            pblock.SetColor("_Color", tintColor);
            spriteRenderer.SetPropertyBlock(pblock);
        }

        public void SetBaseColor(Color baseColor)
        {
            CheckReferences();

            spriteRenderer.GetPropertyBlock(pblock);
            BaseColor = baseColor;
            pblock.SetColor("_BaseColor", baseColor);
            spriteRenderer.SetPropertyBlock(pblock);
        }

        public void SetOffsetColor(Color offsetColor)
        {
            CheckReferences();

            spriteRenderer.GetPropertyBlock(pblock);
            OffsetColor = offsetColor;
            pblock.SetColor("_ColorOffset", offsetColor);
            spriteRenderer.SetPropertyBlock(pblock);
        }

        public void SetOutlineColor(Color outlineColor)
        {
            CheckReferences();

            spriteRenderer.GetPropertyBlock(pblock);
            OutlineColor = outlineColor;
            pblock.SetColor("_OutlineColor", outlineColor);
            spriteRenderer.SetPropertyBlock(pblock);
        }

        public void SetOutlineThickness(float thickness)
        {
            CheckReferences();

            spriteRenderer.GetPropertyBlock(pblock);
            OutlineThickness = thickness;
            pblock.SetFloat("_Thickness", thickness);
            spriteRenderer.SetPropertyBlock(pblock);
        }

        public void SetOutline(Color outlineColor, float thickness)
        {
            CheckReferences();

            spriteRenderer.GetPropertyBlock(pblock);
            OutlineColor = outlineColor;
            OutlineThickness = thickness;
            pblock.SetColor("_OutlineColor", outlineColor);
            pblock.SetFloat("_Thickness", thickness);
            spriteRenderer.SetPropertyBlock(pblock);
        }

        public void SetAntialiased(bool value)
    	{
    		CheckReferences();

    		spriteRenderer.GetPropertyBlock(pblock);
            Antialiased = value;
            pblock.SetFloat("_Antialiased", value ? 1.0f : 0f);
            spriteRenderer.SetPropertyBlock(pblock);
        }

        public void UpdateMaterial()
        {
            CheckReferences();

            spriteRenderer.GetPropertyBlock(pblock);
            pblock.SetColor("_Color", TintColor);
            pblock.SetColor("_BaseColor", BaseColor);
            pblock.SetColor("_ColorOffset", OffsetColor);
            pblock.SetColor("_OutlineColor", OutlineColor);
            pblock.SetFloat("_Thickness", OutlineThickness);
            pblock.SetFloat("_GrayscaleImage", GrayscaleImage ? 1f : 0f);
            pblock.SetFloat("_Antialiased", Antialiased ? 1.0f : 0f);
            spriteRenderer.SetPropertyBlock(pblock);
        }

        void CheckReferences()
    	{
    		if (spriteRenderer == null)
    			spriteRenderer = GetComponent<SpriteRenderer>();
    		if (pblock == null)
    			pblock = new MaterialPropertyBlock();
    	}

    #if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateMaterial();
        }
    #endif
    }


}