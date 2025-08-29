using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Interactive.PuzzelShape
{
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

    public class FindObjectGames : MonoBehaviour, IGameModule
    {
    	public Sprite[] EnvironmentIcons;
    	public RectTransform ContentRoot;
    	public GameObject MenuItemPrefab;
    	public string GameScene = "PlaceObjectModule";
    	public Color FadeColor = Color.white;

    	JungleItem fluturiTarget;
    	Tween shakeSequence;


    	public GameDataShape.eGameType Type => GameDataShape.eGameType.Environment;
    	public bool Enabled { set => gameObject.SetActive(value); }
    	public int LevelCount => EnvironmentIcons.Length;
    	public void UpdateFluturiTarget() => FluturiController.Instance.SetTargetItem(fluturiTarget);


    	void Start()
    	{
    		for (int i = 0; i < EnvironmentIcons.Length; i++)
    		{
    			GameObject go = Instantiate(MenuItemPrefab, ContentRoot);
    			go.name = "EnvMenuItem" + i;
    			go.SetActive(true);
    			JungleItem item = go.GetComponent<JungleItem>();
    			int idx = i;
    			item.Image.sprite = EnvironmentIcons[i];
    			item.Button.onClick.AddListener(() => ButtonClicked(idx));

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