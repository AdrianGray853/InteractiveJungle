using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.PuzzelShape
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    [CreateAssetMenu(fileName = "RGBColorList", menuName = "ScriptableObjects/CustomRGBColors", order = 3)]
    public class CustomRGBColors : ScriptableObject
    {
    	[System.Serializable]
    	public class ColorDef
    	{
    		public Color RedColor = Color.red;
    		public Color GreenColor = Color.green;
    		public Color BlueColor = Color.blue;

    		public ColorDef()
            {
    			RedColor = Color.red;
    			GreenColor = Color.green;
    			BlueColor = Color.blue;
    		}
    	}

    	public ColorDef[] Colors;

    	void Reset()
        {
    		Colors = new ColorDef[] { new ColorDef() };
        }

    	public class ColorSampler
    	{
    		public ColorDef[] RandomColors;
    		int currentColorIdx = 0;

    		public enum eSampleMode
    		{
    			Wrap,
    			Clamp,
    			Random
    		}
    		public eSampleMode SampleMode = eSampleMode.Wrap;

    		public ColorSampler(CustomRGBColors colorList)
    		{
    			RandomColors = colorList.GetRandomizedColors();
    		}

    		public ColorDef GetNextColor()
    		{
    			ColorDef color = null;
    			if (SampleMode == eSampleMode.Wrap)
    			{
    				color = RandomColors[currentColorIdx++];
    				currentColorIdx = currentColorIdx % RandomColors.Length;
    			}
    			else if (SampleMode == eSampleMode.Clamp)
    			{
    				color = RandomColors[currentColorIdx++];
    				currentColorIdx = Mathf.Min(currentColorIdx, RandomColors.Length - 1);
    			}
    			else if (SampleMode == eSampleMode.Random)
    			{
    				currentColorIdx = Random.Range(0, RandomColors.Length);
    				color = RandomColors[currentColorIdx];
    			}
    			return color;
    		}

    		public int CurrentColorIndex => currentColorIdx;
    	}

    	public ColorSampler GetNewSampler()
    	{
    		return new ColorSampler(this);
    	}

    	public ColorDef[] GetRandomizedColors()
    	{
    		ColorDef[] RandColors = new ColorDef[Colors.Length];
    		System.Array.Copy(Colors, RandColors, Colors.Length);
    		UtilsShape.Shuffle(RandColors);
    		return RandColors;
    	}
    }

}