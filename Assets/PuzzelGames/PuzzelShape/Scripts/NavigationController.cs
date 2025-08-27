using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Interactive.PuzzelShape
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

    public class NavigationController : MonoBehaviour
    {
    	public Image NextLevelButton;
    	public ParticleSystem NextLevelFX;
    	public FingerHint FingerHintRef;

    	public FingerHint ColorHint;
    	public float ColorHintTime = 5.0f;
    	public FingerHint PhotoHint;
    	public float PhotoHintTime = 10.0f;
    	public void NextLevel()
    	{
    		GameManagerShape.Instance.NextLevel();
    	}

    	public void PrevLevel()
    	{
    		GameManagerShape.Instance.PrevLevel();
    	}

    	public void GoHome()
    	{
    		GameManagerShape.Instance.GoHome();
    	}

    	public void Screenshot()
    	{
    		GameManagerShape.Instance.Screenshot();
    	}

        private void Update()
        {
    		if (ColorHintTime > 0 && ColorHint != null)
    		{
    			ColorHintTime -= Time.deltaTime;
    			FillColorOnTouch colorModule = GameManagerShape.Instance.CurrentLevel.GetComponent<FillColorOnTouch>();
    			if (ColorHintTime < 0 && GameManagerShape.Instance.CurrentLevelIdx == 0 && !colorModule.IsDone())
    			{
    				ColorHint.ShowHint();
    				SoundManagerShape.Instance.AddSFXToQueue("SelectColorChange");
    			}
    		}

    		if (PhotoHintTime > 0 && PhotoHint != null)
    		{
    			PhotoHintTime -= Time.deltaTime;
    			FillColorOnTouch colorModule = GameManagerShape.Instance.CurrentLevel.GetComponent<FillColorOnTouch>();
    			if (PhotoHintTime < 0 && GameManagerShape.Instance.CurrentLevelIdx == 0 && !colorModule.IsDone())
    			{
    				PhotoHint.ShowHint();
    				SoundManagerShape.Instance.AddSFXToQueue("ScreenshotButton");
    			}
    		}
    	}
    }


}