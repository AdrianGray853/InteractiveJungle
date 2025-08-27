using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.Touch
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class OnBoardingAgeInputTouch : MonoBehaviour
    {
    	[System.Serializable]
    	public class DigitsRef
    	{
    		public GameObject[] Digits;
    	}

    	public DigitsRef[] InputBoxes;
    	int currentBox = 0;
    	int[] age = new int[4];

    	// Start is called before the first frame update
    	void Start()
    	{
		
    	}

    	// Update is called once per frame
    	void Update()
    	{
		
    	}

    	public void Press1()
    	{
    		Press(1);
    	}

    	public void Press2()
    	{
    		Press(2);
    	}

    	public void Press3()
    	{
    		Press(3);
    	}

    	public void Press4()
    	{
    		Press(4);
    	}

    	public void Press5()
    	{
    		Press(5);
    	}

    	public void Press6()
    	{
    		Press(6);
    	}

    	public void Press7()
    	{
    		Press(7);
    	}

    	public void Press8()
    	{
    		Press(8);
    	}

    	public void Press9()
    	{
    		Press(9);
    	}

    	public void Press0()
    	{
    		Press(0);
    	}

    	public void PressDelete()
    	{
    		if (currentBox > 0)
    		{
    			currentBox--;
    			age[currentBox] = 0;
    			for (int i = 0; i < InputBoxes[currentBox].Digits.Length; i++)
    			{
    				InputBoxes[currentBox].Digits[i].SetActive(false);
    			}
    		}
    	}

    	public void PressEnter()
    	{
    		if (currentBox >= 4)
    		{
    			int iAge = age[0] * 1000 + age[1] * 100 + age[2] * 10 + age[3];
    			Debug.Log(iAge);
    			int ageDiff = System.DateTime.Now.Year - iAge;
    			if (ageDiff > 99 || ageDiff < 18)
    			{
    				ClearInput();
    			} 
    			else
    			{
    				OnBoardingControllerTouch.Instance.AcceptAge();
    			}
    		}
    	}

    	private void Press(int number)
    	{
    		if (currentBox < 4)
    		{
    			age[currentBox] = number;
    			InputBoxes[currentBox++].Digits[number].SetActive(true);

    			if (currentBox >= 4)
    				PressEnter();
    		}
    	}

    	void ClearInput()
    	{
    		for (int i = 0; i < InputBoxes.Length; i++)
    		{
    			for (int j = 0; j < InputBoxes[i].Digits.Length; j++)
    			{
    				InputBoxes[i].Digits[j].SetActive(false);
    			}
    		}

    		currentBox = 0;
    		age[0] = age[1] = age[2] = age[3] = 0;
    	}

        private void OnDisable()
        {
    		ClearInput();
        }
    }


}