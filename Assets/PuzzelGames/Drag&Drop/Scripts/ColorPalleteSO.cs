using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ColorPallete", order = 3)]
    public class ColorPalleteSO : ScriptableObject
    {
    	public Color[] Colors;

    	public class ColorSampler
    	{
    		public Color[] RandomColors;
    		int CurrentColorIdx = 0;

    		public enum eSampleMode
    		{
    			Wrap,
    			Clamp,
    			Random
    		}
    		public eSampleMode SampleMode = eSampleMode.Wrap;

    		public ColorSampler(ColorPalleteSO colorList)
    		{
    			RandomColors = colorList.GetRandomizedColors();
    		}

    		public Color GetNextColor()
    		{
    			Color color = Color.white;
    			if (SampleMode == eSampleMode.Wrap)
    			{
    				color = RandomColors[CurrentColorIdx++];
    				CurrentColorIdx = CurrentColorIdx % RandomColors.Length;
    			}
    			else if (SampleMode == eSampleMode.Clamp)
    			{
    				color = RandomColors[CurrentColorIdx++];
    				CurrentColorIdx = Mathf.Min(CurrentColorIdx, RandomColors.Length - 1);
    			}
    			else if (SampleMode == eSampleMode.Random)
    			{
    				CurrentColorIdx = Random.Range(0, RandomColors.Length);
    				color = RandomColors[CurrentColorIdx];
    			}
    			return color;
    		}
    	}

    	public ColorSampler GetNewSampler()
    	{
    		return new ColorSampler(this);
    	}

    	public Color[] GetRandomizedColors()
    	{
    		Color[] RandColors = new Color[Colors.Length];
    		System.Array.Copy(Colors, RandColors, Colors.Length);
    		Utils.Shuffle(RandColors);
    		return RandColors;
    	}
    }

}