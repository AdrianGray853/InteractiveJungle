using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using DG.Tweening;

namespace Interactive.PuzzelShape
{
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using DG.Tweening;

    public class MemoryGames : MonoBehaviour, IGameModule
    {
    	public int Levels = 3;
    	public RectTransform ContentRoot;
    	public GameObject MenuItemGO;
    	public string GameScene;
    	public Color FadeColor = Color.white;
	
    	MainMenuItem fluturiTarget;
    	Tween shakeSequence;


    	public GameDataShape.eGameType Type => GameDataShape.eGameType.Memory;
    	public bool Enabled { set => gameObject.SetActive(value); }
    	public int LevelCount => Levels;
    	public void UpdateFluturiTarget() => FluturiController.Instance.SetTargetItem(fluturiTarget);


    	void Start()
    	{
    		for (int i = 0; i < Levels; i++)
    		{
    			GameObject go = Instantiate(MenuItemGO, ContentRoot);
    			go.name = "MemoryMenuItem" + i;
    			go.SetActive(true);
    			MainMenuItem item = go.GetComponent<MainMenuItem>();
    			int idx = i;
    			item.Button.onClick.AddListener(() => ButtonClicked(idx));
    			item.Text.text = (i + 1).ToString();

    			int unlockLevel = ProgressManagerShape.Instance.GetUnlockLevel(Type);
    			if (i == unlockLevel)
    			{
    				item.SetUncompleted();
    				fluturiTarget = item;
    			}
    			else
    			{
    				item.SetLockedState(i > unlockLevel);
    			}
    		}
    		GetComponentInChildren<ScrollRect>().verticalNormalizedPosition = 1.0f;
    		UpdateFluturiTarget();
    		ProgressManagerShape.Instance.SetModuleLevelsCount(Type, LevelCount);
    	}

    	void ButtonClicked(int index)
    	{
    		Debug.Log("Clicked " + index);

    		int unlockLevel = ProgressManagerShape.Instance.GetUnlockLevel(Type);
    		if (index <= unlockLevel)
    		{
    			SoundManagerShape.Instance.PlaySFX("MenuGameClick");
    			GameDataShape.Instance.SelectedLevel = index;
    			GameDataShape.Instance.GameType = Type;
    			TransitionManagerShape.Instance.SetFadeColor(FadeColor);
    			TransitionManagerShape.Instance.ShowFade(0.5f, () => SceneLoaderShape.Instance.LoadScene(GameScene));
    		}
    		else
    		{
    			if (unlockLevel == 0)
    				SoundManagerShape.Instance.PlaySFX("StartFromFirstLevel");
    			SoundManagerShape.Instance.PlaySFX("Denied");
    			if (shakeSequence != null)
    				shakeSequence.Kill(true);
    			shakeSequence = UnityEngine.EventSystems.EventSystem.current?.currentSelectedGameObject?.transform.DOShakeRotation(0.5f, Vector3.forward * 15.0f, 20, 90.0f, true);
    			Debug.Log("Locked!");
    		}
    	}
    }


}