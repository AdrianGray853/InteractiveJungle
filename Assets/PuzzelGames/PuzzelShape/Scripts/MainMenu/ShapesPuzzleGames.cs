using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Interactive.PuzzelShape
{
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

    public class ShapesPuzzleGames : MonoBehaviour, IGameModule
    {
    	public Sprite[] PuzzleIcons;
    	public RectTransform ContentRoot;
    	public GameObject MenuItemGO;
    	public string GameScene;
    	public Color FadeColor = Color.white;

    	MainMenuItem fluturiTarget;
    	Tween shakeSequence;


    	public GameDataShape.eGameType Type => GameDataShape.eGameType.ShapePuzzle;
    	public bool Enabled { set => gameObject.SetActive(value); }
    	public int LevelCount => PuzzleIcons.Length;
    	public void UpdateFluturiTarget() => FluturiController.Instance.SetTargetItem(fluturiTarget);


    	void Start()
    	{
    		for (int i = 0; i < PuzzleIcons.Length; i++)
    		{
    			GameObject go = Instantiate(MenuItemGO, ContentRoot);
    			go.name = "ShapesPuzzleMenuItem" + i;
    			go.SetActive(true);
    			MainMenuItem item = go.GetComponent<MainMenuItem>();
    			int idx = i;
    			item.Button.onClick.AddListener(() => ButtonClicked(idx));
    			item.Image.sprite = PuzzleIcons[i];

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
    		SoundManagerShape.Instance.PlaySFX("MenuGameClick");
    		int unlockLevel = ProgressManagerShape.Instance.GetUnlockLevel(Type);
    		if (index <= unlockLevel)
    		{
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
    			shakeSequence = UnityEngine.EventSystems.EventSystem.current?.currentSelectedGameObject?.transform.DOShakeRotation(0.5f, Vector3.forward * 15.0f, 20, 90.0f, true/*, ShakeRandomnessMode.Harmonic*/);
    			Debug.Log("Locked!");
    		}
    	}
    }


}