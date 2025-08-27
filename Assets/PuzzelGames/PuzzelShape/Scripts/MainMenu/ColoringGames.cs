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

    public class ColoringGames : MonoBehaviour, IGameModule
    {
        public Sprite[] LevelSprites;
        public RectTransform ContentRoot;
        public GameObject MenuItemPrefab;
        public string GameScene = "ColoringModule";
    	public Color FadeColor = Color.white;

    	MainMenuItem fluturiTarget;
    	Tween shakeSequence;


    	public GameDataShape.eGameType Type => GameDataShape.eGameType.Coloring;
    	public bool Enabled { set => gameObject.SetActive(value); }
    	public int LevelCount => LevelSprites.Length;
    	public void UpdateFluturiTarget() => FluturiController.Instance.SetTargetItem(fluturiTarget);


    	void Start()
        {
            for (int i = 0; i < LevelSprites.Length; i++)
    		{
    			GameObject go = Instantiate(MenuItemPrefab, ContentRoot);
    			go.name = "ColorMenuItem" + i;
                go.SetActive(true);
                MainMenuItem item = go.GetComponent<MainMenuItem>();
                int idx = i;
                Sprite doneSprite = GetExistingLevelSprite(i + 1);
                if (doneSprite != null)
                    item.Image.sprite = doneSprite;
                else
                    item.Image.sprite = LevelSprites[i];
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

        Sprite GetExistingLevelSprite(int levelIdx)
    	{
            string path = Application.persistentDataPath + "/GeneratedLevelIcons/Coloring/Level" + levelIdx + ".png";
            if (System.IO.File.Exists(path))
    		{
    			byte[] fileData = System.IO.File.ReadAllBytes(path);
                Texture2D tex2D = new Texture2D(2, 2); // Temp texture
                if (tex2D.LoadImage(fileData))
    			{
    				tex2D.filterMode = FilterMode.Bilinear;
    				tex2D.wrapMode = TextureWrapMode.Clamp;
    				//tex2D.alphaIsTransparency = true;
                    tex2D.hideFlags = HideFlags.DontSave;
    				const float PixelsPerUnit = 100.0f;
                    Sprite sprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), new Vector2(0, 0), PixelsPerUnit, 0, SpriteMeshType.FullRect);
                    return sprite;
    			}
    			Destroy(tex2D);
    		}

            return null;
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
    			shakeSequence = UnityEngine.EventSystems.EventSystem.current?.currentSelectedGameObject?.transform.DOShakeRotation(0.5f, Vector3.forward * 15.0f, 20, 90.0f, true/*, ShakeRandomnessMode.Harmonic*/);
    			Debug.Log("Locked!");
    		}
    	}
    }


}