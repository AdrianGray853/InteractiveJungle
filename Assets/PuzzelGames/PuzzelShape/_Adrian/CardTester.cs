using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.PuzzelShape
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class CardTester : MonoBehaviour
    {
    	[System.Serializable]
    	public class CardDesc
    	{
    		public Sprite CardSprite;
    		[Range(0, 5)]
    		public int Difficulty;
    		public float Scale = 1f;
    	}
    	[System.Serializable]
    	public class CardCollection
    	{
    		public MemoryGameSetup.eCategory Category;
    		public CardDesc[] Cards;
    	}

    	public MemoryCard CardPrefab;
    	public GameManagerShape.CardCollection[] Cards;

    	int CurrentCattegory = 0;
    	int CurrentCard = 0;

    	// Start is called before the first frame update
    	void Start()
        {
    		/*
    		for (int i = 0; i < Cards.Length; i++)
    		{
    			for (int j = 0; j < Cards[i].Cards.Length; j++)
    				Cards[i].Cards[j].Scale = 1.0f;
    		}
    		*/
    		UpdateCard();
    		CardPrefab.AnimateState(true);
        }

        // Update is called once per frame
        void Update()
        {
        
        }

    	public void NextCard()
    	{
    		CurrentCard++;
    		if (CurrentCard >= Cards[CurrentCattegory].Cards.Length)
    		{
    			if (CurrentCattegory + 1 < Cards.Length)
    			{
    				CurrentCard = 0;
    				CurrentCattegory++;
    			} 
    			else
    			{
    				CurrentCard = Cards[CurrentCattegory].Cards.Length - 1;
    			}
    		}
    		UpdateCard();
    	}

    	public void PrevCard()
    	{
    		CurrentCard--;
    		if (CurrentCard < 0)
    		{
    			if (CurrentCattegory - 1 >= 0)
    			{
    				CurrentCattegory--;
    				CurrentCard = Cards[CurrentCattegory].Cards.Length - 1;
    			}
    			else
    			{
    				CurrentCard = 0;
    			}
    		}
    		UpdateCard();
    	}

    	void UpdateCard()
    	{
    		sliderValue = Cards[CurrentCattegory].Cards[CurrentCard].Scale;
    		CardPrefab.SetContent(Cards[CurrentCattegory].Cards[CurrentCard].CardSprite, Cards[CurrentCattegory].Cards[CurrentCard].Scale);
    	}

    	private void OnValidate()
    	{
    		CardPrefab.SetContent(Cards[CurrentCattegory].Cards[CurrentCard].CardSprite, Cards[CurrentCattegory].Cards[CurrentCard].Scale);
    	}

    	float sliderValue = 0f;

    	private void OnGUI()
    	{
    		GUI.skin.horizontalSlider.fixedHeight = 100;
    		GUI.skin.horizontalSliderThumb.fixedHeight = 100;
    		GUI.skin.horizontalSliderThumb.fixedWidth = 100;

    		float oldValue = Cards[CurrentCattegory].Cards[CurrentCard].Scale;
    		sliderValue = GUI.HorizontalSlider(new Rect(Screen.width * 0.5f - 250, Screen.height - 150, 500, 100), sliderValue, 0.0F, 2.0F);
    		if (oldValue != sliderValue)
    		{
    			Cards[CurrentCattegory].Cards[CurrentCard].Scale = sliderValue;
    			UpdateCard();
    		}
    	}
    }


}