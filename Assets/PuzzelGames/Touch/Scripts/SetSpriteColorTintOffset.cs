using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.Touch
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class SetSpriteColorTintOffset : MonoBehaviour
    {
    	public Color TintColor = Color.white;
    	public Color OffsetColor = new Color(1f, 1f, 1f, 0f);

    	private SpriteRenderer spriteRenderer;
    	private MaterialPropertyBlock propertyBlock;

    	// Start is called before the first frame update
    	void Awake()
    	{
    		UpdateColor();
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
    		}
    		spriteRenderer.GetPropertyBlock(propertyBlock);
    		propertyBlock.SetColor("_Color", TintColor);
    		propertyBlock.SetColor("_ColorOffset", OffsetColor);
    		spriteRenderer.SetPropertyBlock(propertyBlock);
    	}

    	public void SetColor(Color tintColor, Color offsetColor)
    	{
    		TintColor = tintColor;
    		OffsetColor = offsetColor;
    		UpdateColor();
    	}

    	public void SetColor(Color tintColor)
    	{
    		TintColor = tintColor;
    		UpdateColor();
    	}

    	public void SetOffsetColor(Color offsetColor)
    	{
    		OffsetColor = offsetColor;
    		UpdateColor();
    	}

    	// Useful for flashes
    	public void SetOffsetAlpha(float alpha)
    	{
    		OffsetColor.a = alpha;
    		UpdateColor();
    	}

    	public float GetOffsetAlpha()
    	{
    		return OffsetColor.a;
    	}

    #if UNITY_EDITOR
    	private void OnValidate()
    	{
    		UpdateColor();
    	}
    #endif
    }


}