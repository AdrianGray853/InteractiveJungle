using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Interactive.Touch
{
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

    public class JungleActions : MonoBehaviour
    {
    	public GameObject Jungle;

    	public GameObject ColoringMenu;
    	public GameObject DrawingMenu;

    	public GameObject Modules;
    	public GameObject Categories;
    	public GameObject TouchAndFillMenu;
    	public GameObject ColoringInEnvironmentMenu;
    	public GameObject StampsAndPatternMenu;
    	public GameObject OutlineMenu;
    	public GameObject TracingMenu;

    	public GameObject StickerBook;

    	public GameObject BookButton;
    	public GameObject BackButton;

    	public ParticleSystem BubblesPS;

    	private GameObject lastMenu;

    	bool inputActive = false;

    	private void OnEnable()
    	{
    		ProductManagerTouch.Instance.OnStoreResults += StoreResult;
    	}

    	private void OnDisable()
    	{
    		ProductManagerTouch.Instance.OnStoreResults -= StoreResult;
    	}

    	private void StoreResult(bool Success)
    	{
    		CheckUnlocks();
    	}

    	private void Start()
    	{
    		TransitionManagerTouch.Instance.SetFadeColor(new Color(0.53f, 0.84f, 1.0f));
    		InitPlatformTransformations();

    		if (SoundManagerTouch.Instance.GetCurrentMusicName() != "Jungle")
    		{
    			SoundManagerTouch.Instance.CrossFadeMusic("Jungle", 1.0f);
    		}

    		if (SceneLoader.Instance.LastScene == "" || SceneLoader.Instance.LastScene == "Loader" || SceneLoader.Instance.LastScene == "Jungle")
    		{
    #if PROJECT_X
    				//PlayInitVoice();
    				InitForProjectX();
    #else
    			PlayInitVoice();
    			ShowJungle();
    #endif
    		}
    		else
    		{
    			InitJungleFromMask();
    			PlayReturnVoice();
    		}

    		CheckUnlocks();
    	}

    	private void Update()
    	{
    		if (ProductManagerTouch.Instance.State == ProductManagerTouch.eState.Initiating)
    			return;

    		if (TransitionManagerTouch.Instance.InTransition)
    			return;

    		if (ProductManagerTouch.Instance.State == ProductManagerTouch.eState.Initialized && !ProductManagerTouch.Instance.IsSubscribed && !ProgressManagerTouch.Instance.IsOnboardingShown()) // TODO: Check the save file for only one onboarding show!
    		{
    			ProgressManagerTouch.Instance.SetOnboardingShown(true);
    			//OnBoardingControllerTouch.Instance.ShowInGame();
    			OnBoardingControllerTouch.Instance.ShowOnBoarding();
    		}

    		if (OnBoardingControllerTouch.Instance.IsOnBoardingActive)
    			return; // Don't do anything bello from here, like hints etc...

    		inputActive = true;
    	}

    	void PlayReturnVoice()
    	{
    		SoundManagerTouch.Instance.PlaySFX("next_activity", 1.0f, "voiceover", 1);
    	}

    	void PlayInitVoice()
    	{
    		string[] initVoices = { "discover_beyond_water_world", "choose_activity", "hey_artist_get_ready_to_draw" };
    		SoundManagerTouch.Instance.PlaySFX(initVoices[Random.Range(0, initVoices.Length)], 1.0f, "voiceover", 1);
    	}

    	void HideAllExcept(params GameObject[] gameObjects)
    	{
    		Jungle.SetActive(gameObjects.Contains(Jungle));

    		ColoringMenu.SetActive(gameObjects.Contains(ColoringMenu));
    		DrawingMenu.SetActive(gameObjects.Contains(DrawingMenu));

    		//Categories.SetActive(gameObjects.Contains(Categories));
    		bool categoriesActive = gameObjects.Contains(Categories);
    		categoriesActive |= gameObjects.Contains(TouchAndFillMenu);
    		categoriesActive |= gameObjects.Contains(OutlineMenu);
    		Categories.SetActive(categoriesActive);

    		CheckTabletButtonSwap();

    		Modules.SetActive(UtilsTouch.In(gameObjects, TouchAndFillMenu, ColoringInEnvironmentMenu, StampsAndPatternMenu, OutlineMenu, TracingMenu));
    		TouchAndFillMenu.SetActive(gameObjects.Contains(TouchAndFillMenu));
    		ColoringInEnvironmentMenu.SetActive(gameObjects.Contains(ColoringInEnvironmentMenu));
    		StampsAndPatternMenu.SetActive(gameObjects.Contains(StampsAndPatternMenu));
    		OutlineMenu.SetActive(gameObjects.Contains(OutlineMenu));
    		TracingMenu.SetActive(gameObjects.Contains(TracingMenu));

    		StickerBook.SetActive(gameObjects.Contains(StickerBook));

    		if (Jungle.activeSelf || ColoringMenu.activeSelf || DrawingMenu.activeSelf || StickerBook.activeSelf)
    			BookButton.SetActive(false);
    		else
    			BookButton.SetActive(true);

    		BackButton.SetActive(!Jungle.activeSelf);

    		foreach (var go in gameObjects)
    		{
    			var controller = go.GetComponent<ModuleController>();
    			controller?.ResetAll();
    		}
    	}

    	void CheckTabletButtonSwap()
    	{
    		// Tablet hack...
    		if (Categories.activeSelf && Camera.main.aspect < 2.1f)
    		{ // Aspect too small move the buttons down
    			RectTransform bookRect = BookButton.GetComponent<RectTransform>();
    			Vector2 pos = bookRect.anchoredPosition;
    			bookRect.anchorMin = Vector2.right;
    			bookRect.anchorMax = Vector2.right;
    			pos.y = Mathf.Abs(pos.y); // Positive at the bottom
    			bookRect.anchoredPosition = pos;

    			RectTransform backRect = BackButton.GetComponent<RectTransform>();
    			pos = backRect.anchoredPosition;
    			backRect.anchorMin = Vector2.zero;
    			backRect.anchorMax = Vector2.zero;
    			pos.y = Mathf.Abs(pos.y); // Positive at the bottom
    			backRect.anchoredPosition = pos;
    		}
    		else
    		{
    			RectTransform bookRect = BookButton.GetComponent<RectTransform>();
    			Vector2 pos = bookRect.anchoredPosition;
    			bookRect.anchorMin = Vector2.one;
    			bookRect.anchorMax = Vector2.one;
    			pos.y = -Mathf.Abs(pos.y); // Positive at the bottom
    			bookRect.anchoredPosition = pos;

    			RectTransform backRect = BackButton.GetComponent<RectTransform>();
    			pos = backRect.anchoredPosition;
    			backRect.anchorMin = Vector2.up;
    			backRect.anchorMax = Vector2.up;
    			pos.y = -Mathf.Abs(pos.y); // Positive at the bottom
    			backRect.anchoredPosition = pos;
    		}
    	}
    	public void ShowJungle()
    	{
    		HideAllExcept(Jungle);
    	}

    	// Main Menu
    	public void JunglePressColoring()
    	{
    		if (!inputActive)
    			return;

    		lastMenu = Jungle;
    		HideAllExcept(ColoringMenu);

    		SoundManagerTouch.Instance.PlaySFX("BubbleClick");
    	}

    	public void JunglePressDrawing()
    	{
    		if (!inputActive)
    			return;

    		lastMenu = Jungle;
    		HideAllExcept(DrawingMenu);

    		SoundManagerTouch.Instance.PlaySFX("BubbleClick");
    	}

    	// SubMenu

    	public void TouchAndFillPress()
    	{
    		if (!inputActive)
    			return;

    		lastMenu = ColoringMenu;
    		GameDataTouch.Instance.GameType = GameDataTouch.eGameType.TouchAndFill;
    		HideAllExcept(TouchAndFillMenu, Categories);

    		SoundManagerTouch.Instance.PlaySFX("BubbleClick");
    	}

    	public void StampsAndPatternsPress()
    	{
    		if (!inputActive)
    			return;

    		lastMenu = ColoringMenu;
    		GameDataTouch.Instance.GameType = GameDataTouch.eGameType.PatternAndStamps;
    		HideAllExcept(StampsAndPatternMenu);

    		SoundManagerTouch.Instance.PlaySFX("BubbleClick");
    	}

    	public void ColoringInEnvironment()
    	{
    		if (!inputActive)
    			return;

    		lastMenu = ColoringMenu;
    		GameDataTouch.Instance.GameType = GameDataTouch.eGameType.ColorEnvironment;
    		HideAllExcept(ColoringInEnvironmentMenu);

    		SoundManagerTouch.Instance.PlaySFX("BubbleClick");
    	}

    	public void AIPress()
    	{
    		if (!inputActive)
    			return;

    		CreateJungleMask();

    		GameDataTouch.Instance.GameType = GameDataTouch.eGameType.AI;
    		TransitionManagerTouch.Instance.ShowFade(new Color(0.53f, 0.84f, 1.0f), 0.5f, () => SceneLoader.Instance.LoadScene("AI"));
    		//HideAllExcept(TouchAndFillMenu, ModulesMenu);

    		SoundManagerTouch.Instance.PlaySFX("BubbleClick");
    	}

    	public void SymmetryPress()
    	{
    		if (!inputActive)
    			return;

    		CreateJungleMask();

    		GameDataTouch.Instance.GameType = GameDataTouch.eGameType.Symmetry;
    		TransitionManagerTouch.Instance.ShowFade(new Color(0.53f, 0.84f, 1.0f), 0.5f, () => SceneLoader.Instance.LoadScene("Symmetry"));
    		//HideAllExcept(Symme, ModulesMenu);

    		SoundManagerTouch.Instance.PlaySFX("BubbleClick");
    	}

    	public void OutlinePress()
    	{
    		if (!inputActive)
    			return;

    		lastMenu = DrawingMenu;
    		GameDataTouch.Instance.GameType = GameDataTouch.eGameType.Outline;
    		HideAllExcept(OutlineMenu, Categories);

    		SoundManagerTouch.Instance.PlaySFX("BubbleClick");
    	}

    	public void TracingPress()
    	{
    		if (!inputActive)
    			return;

    		lastMenu = DrawingMenu;
    		GameDataTouch.Instance.GameType = GameDataTouch.eGameType.Tracing;
    		HideAllExcept(TracingMenu);

    		SoundManagerTouch.Instance.PlaySFX("BubbleClick");
    	}

    	public void StickerBookPress()
    	{
    		if (!inputActive)
    			return;

    		GameDataTouch.eGameType gameType = GameDataTouch.eGameType.NumModules;

    		if (TouchAndFillMenu.activeSelf)
    		{
    			lastMenu = TouchAndFillMenu;
    			gameType = GameDataTouch.eGameType.TouchAndFill;
    		}
    		else if (ColoringInEnvironmentMenu.activeSelf)
    		{
    			lastMenu = ColoringInEnvironmentMenu;
    			gameType = GameDataTouch.eGameType.ColorEnvironment;
    		}
    		else if (StampsAndPatternMenu.activeSelf)
    		{
    			lastMenu = StampsAndPatternMenu;
    			gameType = GameDataTouch.eGameType.PatternAndStamps;
    		}
    		else if (OutlineMenu.activeSelf)
    		{
    			lastMenu = OutlineMenu;
    			gameType = GameDataTouch.eGameType.Outline;
    		}
    		else if (TracingMenu.activeSelf)
    		{
    			lastMenu = TracingMenu;
    			gameType = GameDataTouch.eGameType.Tracing;
    		}

    		if (gameType != GameDataTouch.eGameType.NumModules)
    			ShowStickerBook(gameType);

    		SoundManagerTouch.Instance.PlaySFX("Click");
    	}

    	public void ShowStickerBook(GameDataTouch.eGameType gameType)
    	{
    		// Do something with stickerBook Controller and stickers!
    		HideAllExcept(StickerBook);
    		StickerBook.GetComponent<Animator>().Play("StickerBook_Show", -1, 0f);

    		StickerBook.GetComponent<StickerBookController>().Init(gameType);
    	}

    	public void CreateJungleMask(int category = 3, float scrollOffset = 0f)
    	{
    		int mask = 0;

    		if (Jungle.activeSelf)
    			mask |= 1 << 0;

    		if (ColoringMenu.activeSelf)
    			mask |= 1 << 1;
    		if (DrawingMenu.activeSelf)
    			mask |= 1 << 2;

    		if (Categories.activeSelf)
    			mask |= 1 << 3;

    		if (TouchAndFillMenu.activeSelf)
    			mask |= 1 << 4;
    		if (ColoringInEnvironmentMenu.activeSelf)
    			mask |= 1 << 5;
    		if (StampsAndPatternMenu.activeSelf)
    			mask |= 1 << 6;
    		if (OutlineMenu.activeSelf)
    			mask |= 1 << 7;
    		if (TracingMenu.activeSelf)
    			mask |= 1 << 8;

    		if (BookButton.activeSelf)
    			mask |= 1 << 9;
    		if (BackButton.activeSelf)
    			mask |= 1 << 10;

    		GameDataTouch.Instance.JungleEnableMask = mask;
    		GameDataTouch.Instance.JungleScrollOffset = scrollOffset;
    		GameDataTouch.Instance.JungleCategory = category;
    	}

    #if PROJECT_X
    	private void InitForProjectX()
    	{
    		Jungle.SetActive(false);
    		ColoringMenu.SetActive(false);
    		DrawingMenu.SetActive(false);

    		TouchAndFillMenu.SetActive(global::GameDataTouch.Instance.GameTarget == "TouchAndFill");
    		ColoringInEnvironmentMenu.SetActive(global::GameDataTouch.Instance.GameTarget == "ColorInEnvironment");
    		StampsAndPatternMenu.SetActive(global::GameDataTouch.Instance.GameTarget == "StampsAndPatterns");
    		OutlineMenu.SetActive(global::GameDataTouch.Instance.GameTarget == "Outline");
    		TracingMenu.SetActive(global::GameDataTouch.Instance.GameTarget == "Tracing");

    		Categories.SetActive(TouchAndFillMenu.activeSelf || OutlineMenu.activeSelf);

    		BookButton.SetActive(true);
    		BackButton.SetActive(true);

    		ModuleController controller = null;
    		if (TouchAndFillMenu.activeSelf)
    			controller = TouchAndFillMenu.GetComponent<ModuleController>();
    		if (ColoringInEnvironmentMenu.activeSelf)
    			controller = ColoringInEnvironmentMenu.GetComponent<ModuleController>();
    		if (StampsAndPatternMenu.activeSelf)
    			controller = StampsAndPatternMenu.GetComponent<ModuleController>();
    		if (OutlineMenu.activeSelf)
    			controller = OutlineMenu.GetComponent<ModuleController>();
    		if (TracingMenu.activeSelf)
    			controller = TracingMenu.GetComponent<ModuleController>();

    		if (controller != null)
    		{
    			controller.CategorySelectorRef.SelectCategory(GameDataTouch.Instance.JungleCategory, true);
    		}
    		Modules.SetActive(true);

    		if (TouchAndFillMenu.activeSelf || ColoringInEnvironmentMenu.activeSelf || StampsAndPatternMenu.activeSelf)
    			lastMenu = ColoringMenu;
    		else if (OutlineMenu.activeSelf || TracingMenu.activeSelf)
    			lastMenu = DrawingMenu;
    		else
    			lastMenu = Jungle;

    		StickerBook.SetActive(false);

    		CheckTabletButtonSwap();
    	}
    #endif

    	public void InitJungleFromMask()
    	{
    		int mask = GameDataTouch.Instance.JungleEnableMask;
    		Jungle.SetActive((mask & (1 << 0)) != 0);

    		ColoringMenu.SetActive((mask & (1 << 1)) != 0);
    		DrawingMenu.SetActive((mask & (1 << 2)) != 0);

    		Categories.SetActive((mask & (1 << 3)) != 0);

    		TouchAndFillMenu.SetActive((mask & (1 << 4)) != 0);
    		ColoringInEnvironmentMenu.SetActive((mask & (1 << 5)) != 0);
    		StampsAndPatternMenu.SetActive((mask & (1 << 6)) != 0);
    		OutlineMenu.SetActive((mask & (1 << 7)) != 0);
    		TracingMenu.SetActive((mask & (1 << 8)) != 0);

    		BookButton.SetActive((mask & (1 << 9)) != 0);
    		BackButton.SetActive((mask & (1 << 10)) != 0);

    		ModuleController controller = null;
    		if (TouchAndFillMenu.activeSelf)
    			controller = TouchAndFillMenu.GetComponent<ModuleController>();
    		if (ColoringInEnvironmentMenu.activeSelf)
    			controller = ColoringInEnvironmentMenu.GetComponent<ModuleController>();
    		if (StampsAndPatternMenu.activeSelf)
    			controller = StampsAndPatternMenu.GetComponent<ModuleController>();
    		if (OutlineMenu.activeSelf)
    			controller = OutlineMenu.GetComponent<ModuleController>();
    		if (TracingMenu.activeSelf)
    			controller = TracingMenu.GetComponent<ModuleController>();

    		if (controller != null)
    		{
    			controller.CategorySelectorRef.SelectCategory(GameDataTouch.Instance.JungleCategory, true);
    			controller.ScrollToOffset(GameDataTouch.Instance.JungleScrollOffset);

    			Modules.SetActive(true);
    		}
    		else
    		{
    			Modules.SetActive(false);
    		}
			
    		if (TouchAndFillMenu.activeSelf || ColoringInEnvironmentMenu.activeSelf || StampsAndPatternMenu.activeSelf)
    			lastMenu = ColoringMenu;
    		else if (OutlineMenu.activeSelf || TracingMenu.activeSelf)
    			lastMenu = DrawingMenu;
    		else
    			lastMenu = Jungle;

    		StickerBook.SetActive(false);

    		CheckTabletButtonSwap();
    	}

    	void InitPlatformTransformations()
    	{
    		Vector3 pos = Categories.transform.position;
    		pos.y = Camera.main.transform.position.y + Camera.main.orthographicSize;
    		Categories.transform.position = pos;
    	}

    	// UI

    	public void GoBack()
    	{
    		if (!inputActive)
    			return;

    #if PROJECT_X
    			if (!StickerBook.activeSelf)
    			{
    				global::SceneLoader.Instance.LoadDrawingScene("Jungle", "JungleDrawing");
    			}
    #endif

    		if (lastMenu.activeSelf)
    		{
    			ShowJungle();
    			return;
    		}

    		//if (UtilsTouch.In(lastMenu, TouchAndFillMenu, ColoringInEnvironmentMenu, StampsAndPatternMenu, OutlineMenu, TracingMenu))
    		//{
    		//	HideAllExcept(lastMenu, Categories);
    		//}
    		//else
    		//{
    			HideAllExcept(lastMenu);
    		//}

    		SoundManagerTouch.Instance.PlaySFX("Click");
    	}

    	public void CheckUnlocks()
    	{
    		ModuleController[] controllers = Modules.GetComponentsInChildren<ModuleController>();

    		foreach (var controller in controllers)
    			controller.UpdateLockStatus();
    	}
    }


}