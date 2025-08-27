using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.Touch
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class SetSpriteHSL : MonoBehaviour
    {
    	public Color TintColor = Color.white;
    	public Color OffsetColor = new Color(1f, 1f, 1f, 0f);
    	public float Hue = 0f;
    	public float Saturation = 1.0f;
    	public float Brightness = 1.0f;

    	private SpriteRenderer spriteRenderer;
    	private MaterialPropertyBlock propertyBlock;

    	// For performance...
    	Color oldTintColor;
    	Color oldOffsetColor;
    	float oldHue;
    	float oldSaturation;
    	float oldBrightness;

    	// Start is called before the first frame update
    	void Awake()
    	{
    		oldTintColor = TintColor;
    		oldOffsetColor = OffsetColor;
    		oldHue = Hue;
    		oldSaturation = Saturation;
    		oldBrightness = Brightness;

    		UpdateColor();
    	}

    	private void Update()
    	{
    		if (oldTintColor != TintColor || oldOffsetColor != OffsetColor || oldHue != Hue || oldSaturation != Saturation || oldBrightness != Brightness)
    			UpdateColor();

    		oldTintColor = TintColor;
    		oldOffsetColor = OffsetColor;
    		oldHue = Hue;
    		oldSaturation = Saturation;
    		oldBrightness = Brightness;
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
    		propertyBlock.SetColor("_Color", TintColor);
    		propertyBlock.SetColor("_ColorOffset", OffsetColor);
    		propertyBlock.SetFloat("_Hue", Hue);
    		propertyBlock.SetFloat("_Saturation", Saturation);
    		propertyBlock.SetFloat("_Brightness", Brightness);
    		spriteRenderer.SetPropertyBlock(propertyBlock);
    	}

    	public Color GetColor() => TintColor;

    	public void SetColor(Color tintColor)
    	{
    		TintColor = tintColor;
    		UpdateColor();
    	}

    	public void SetColor(Color tintColor, Color offsetColor)
    	{
    		TintColor = tintColor;
    		OffsetColor = offsetColor;
    		UpdateColor();
    	}

    	public void SetColor(Color tintColor, Color offsetColor, float hue, float saturation, float brighness)
    	{
    		TintColor = tintColor;
    		OffsetColor = offsetColor;
    		Hue = hue;
    		Saturation = saturation;
    		Brightness = brighness;
    		UpdateColor();
    	}

    	public void SetColor(Color tintColor, float hue, float saturation, float brighness)
    	{
    		TintColor = tintColor;
    		Hue = hue;
    		Saturation = saturation;
    		Brightness = brighness;
    		UpdateColor();
    	}

    	public void SetColor(float hue, float saturation, float brighness)
    	{
    		Hue = hue;
    		Saturation = saturation;
    		Brightness = brighness;
    		UpdateColor();
    	}

    	public void SetAlpha(float alpha)
    	{
    		TintColor.a = alpha;
    		UpdateColor();
    	}

    	public float GetAlpha()
    	{
    		return TintColor.a;
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