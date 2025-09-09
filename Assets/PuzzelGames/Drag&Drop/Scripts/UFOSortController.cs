using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using UnityEngine.UI;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using UnityEngine.UI;

    public class UFOSortController : MonoBehaviour
    {
    	public float LetterScale = 0.4f;
    	public float MaxHintCooldown = 7.0f;
    	float hintCooldown;

    	public Animator OrangeUFO;
    	public Animator BlueUFO;

    	public TargetPositionController BigDropTarget;
    	public TargetPositionController SmallDropTarget;

    	[System.Serializable]
    	public class LetterSpawnConfig {
    		public Transform[] BigLettersPositions;
    		public Transform[] SmallLettersPositions;
    	}

    	public LetterSpawnConfig[] LetterSpawners;

    	public Sprite[] BigLetters;
    	public Sprite[] SmallLetters;

    	public RawImage Fader;
    	public GameObject PopUp;

    	[Header(".:Colors")]
    	public ColorPalleteSO ColorPallete;
    	public Material LetterMaterial;
    	public Color LowerCaseColor;
    	public Color UpperCaseColor;
    	public float OutlineMultiplier = 0.5f;

    	private List<Collider2D> SpawnedBigLetters = new List<Collider2D>();
    	private List<Collider2D> SpawnedSmallLetters = new List<Collider2D>();

    	private int SmallLettersAtDestination = 0;
    	private int BigLettersAtDestination = 0;

    	ColorPalleteSO.ColorSampler colorSampler;

    	int currentLevel = -1;
    	int maxLevels = 7;
    	//int maxDifficultyAtLevel = 5;

    	private void Start()
    	{
    		GameProgressController.Instance.SetMaxProgressSteps(maxLevels);
    		hintCooldown = MaxHintCooldown * 2.0f;
    		SoundManager.Instance.CrossFadeMusic("SortingLettersBgSoundModule", 1.0f);
    		colorSampler = ColorPallete.GetNewSampler();
		
    		SoundManager.Instance.PlaySFXStack("2_spaceships", 1.0f, "voiceover", 1)
    			.AddSFXStack("move_lower_to_blue")
    			.AddSFXStack("move_upper_to_orange");
    		/*
    		SoundManager.Instance.AddSFXToQueue("2_spaceships", 1.0f, "voiceover", 1);
    		SoundManager.Instance.AddSFXToQueue("move_lower_to_blue", 1.0f, "voiceover", 1);
    		SoundManager.Instance.AddSFXToQueue("move_upper_to_orange", 1.0f, "voiceover", 1);
    		*/
    	}

    	private void OnEnable()
    	{
    		DragManager.Instance.AddOnTouchCallback(OnTouch);
    		DragManager.Instance.AddOnReleaseCallback(OnRelease);
    	}

    	private void OnDisable()
    	{
    		if (DragManager.Instance == null)
    			return;
    		DragManager.Instance.RemoveOnTouchCallback(OnTouch);
    		DragManager.Instance.RemoveOnReleaseCallback(OnRelease);
    	}

    	private void OnRelease(Touch touch, DragHelper helper)
    	{
    		Collider2D collider = helper.DraggingTransform.GetComponent<Collider2D>();
    		DragLetterInfo info = collider.GetComponent<DragLetterInfo>();

    		if (info.IsUpperCase)
    		{
    			if (BigDropTarget.InRange(collider.transform.position))
    			{
    				hintCooldown = -1.0f;
    				FingerHintController.Instance?.Hide();
    				SoundManager.Instance.PlaySFX("Click");
    				Vector3 randomPos = BigDropTarget.GetComponent<Collider2D>().bounds.GetRandomPosition();
    				MoveToPosition(info, randomPos);
    				collider.enabled = false;
    				return;
    			}
    			else if (SmallDropTarget.InRange(collider.transform.position))
    			{
    				SoundManager.Instance.PlaySFX("WrongSound");
    				string[] sfx = new string[] { "attention_details", "dont_worry", "decide", "try" };
    				if (Random.value < 0.25f)
    					SoundManager.Instance.PlaySFX(sfx.GetRandomElement(), 1.0f, "voiceover", 1);
    			}
    		}
    		else
    		{
    			if (SmallDropTarget.InRange(collider.transform.position))
    			{
    				hintCooldown = -1.0f;
    				FingerHintController.Instance?.Hide();
    				SoundManager.Instance.PlaySFX("Click");
    				Vector3 randomPos = SmallDropTarget.GetComponent<Collider2D>().bounds.GetRandomPosition();
    				MoveToPosition(info, randomPos);
    				collider.enabled = false;
    				return;
    			}
    			else if (BigDropTarget.InRange(collider.transform.position))
    			{
    				SoundManager.Instance.PlaySFX("WrongSound");
    				string[] sfx = new string[] { "attention_details", "dont_worry", "decide", "try" };
    				if (Random.value < 0.25f)
    					SoundManager.Instance.PlaySFX(sfx.GetRandomElement(), 1.0f, "voiceover", 1);
    			}
    		}

    		// Didn't find any target, return to base;
    		info.IsActive = false;
    		info.transform.DOMove(info.OriginalPosition, 0.4f).SetEase(Ease.OutBack).OnComplete(() => info.IsActive = true);
    	}

    	private void OnTouch(Touch touch)
    	{
    		foreach (var collider in SpawnedBigLetters)
    		{
    			if (!collider.enabled)
    				continue;
    			if (!collider.GetComponent<DragLetterInfo>().IsActive)
    				continue;

    			Vector3 worldPos = DragManager.GetWorldSpacePos(touch.position);
    			if (collider.OverlapPoint(worldPos))
    			{
    				FingerHintController.Instance?.Hide();
    				SoundManager.Instance.PlaySFX("MovingIslands");
    				DragManager.Instance.AddDrag(touch.fingerId, collider.transform, worldPos, Vector3.zero);
    				SoundManager.Instance.PlaySFX(char.ToLower(collider.GetComponent<DragLetterInfo>().Letter).ToString());
    				return;
    			}
    		}

    		foreach (var collider in SpawnedSmallLetters)
    		{
    			if (!collider.enabled)
    				continue;

    			Vector3 worldPos = DragManager.GetWorldSpacePos(touch.position);
    			if (collider.OverlapPoint(worldPos))
    			{
    				FingerHintController.Instance?.Hide();
    				SoundManager.Instance.PlaySFX("MovingIslands");
    				DragManager.Instance.AddDrag(touch.fingerId, collider.transform, worldPos, Vector3.zero);
    				SoundManager.Instance.PlaySFX(char.ToLower(collider.GetComponent<DragLetterInfo>().Letter).ToString());
    				return;
    			}
    		}
    	}

    	void MoveToPosition(DragLetterInfo info, Vector3 position)
    	{
    		/*
    		 s.Append(stea.DOMove(secondPosition, 0.5f).SetEase(TargetTweenEase));		
    		s.AppendCallback(() => stea.GetComponent<SpriteRenderer>().sortingOrder = 3);
    		s.Append(stea.DOMove(target.position, 0.5f).SetEase(TargetTweenEase));
    		s.Insert(0f, stea.DORotate(new Vector3(0f, 0f, targetRotation), 1.0f, RotateMode.FastBeyond360).SetEase(TargetTweenEase));
    		 */

    		DOTween.Sequence()
    			.Append(info.transform.DOMove((position + info.transform.position) * 0.5f + Vector3.up * 3.0f, 0.5f).SetEase(Ease.OutSine))
    			.AppendCallback(() => info.GetComponent<SpriteRenderer>().sortingOrder = 0)
    			.Append(info.transform.DOMove(position, 0.5f).SetEase(Ease.InSine))
    			.Insert(0f, info.transform.DORotate(new Vector3(0f, 0f, 360.0f), 1f, RotateMode.FastBeyond360).SetEase(Ease.InOutSine))
    			.AppendCallback(() => FinishedAnimation(info));
    	}

    	void FinishedAnimation(DragLetterInfo info)
    	{
    		if (info.IsUpperCase)
    		{
    			BigLettersAtDestination++;
    		}
    		else
    		{
    			SmallLettersAtDestination++;
    		}

    		if (BigLettersAtDestination == SpawnedBigLetters.Count && SmallLettersAtDestination == SpawnedSmallLetters.Count)
    		{
    			SoundManager.Instance.PlaySFX("UFOCloseLid");
    			DOTween.Sequence().
    				AppendInterval(0.5f)
    				.AppendCallback(() => SoundManager.Instance.PlaySFX("SpaceshipSellectLeter"));
    			string[] sfx = new string[] { "good_job", "okay", "doing_great", "did_this_too", "great", "have_fun", "did_it" };
    			SoundManager.Instance.PlaySFX(sfx.GetRandomElement(), 1.0f, "voiceover", 2);
    			// Done;
    			BlueUFO.Play("Closing");
    			OrangeUFO.Play("Closing");
    		}
    	}

    	public void RemoveLetters()
    	{ // is called 2 times, check if it's needed?
    		foreach (var collider in SpawnedBigLetters)
    			Destroy(collider.gameObject);
    		foreach (var collider in SpawnedSmallLetters)
    			Destroy(collider.gameObject);

    		SpawnedBigLetters.Clear();
    		SpawnedSmallLetters.Clear();

    		BigLettersAtDestination = 0;
    		SmallLettersAtDestination = 0;
    	}

    	public void SpawnLetters(bool isBig)
    	{
    		/* // Old buggy way
    		LetterSpawnConfig config = LetterSpawners.GetRandomElement();
    		int idx = 0;
    		if (isBig)
    		{
    			level++;
    			Utils.Shuffle(BigLetters);
    			Utils.Shuffle(config.BigLettersPositions);
    			foreach (var t in config.BigLettersPositions)
    			{
    				SpawnLetter(true, t.position, OrangeUFO.transform, idx++);
    			}
    		}
    		else
    		{
    			Utils.Shuffle(SmallLetters);
    			Utils.Shuffle(config.SmallLettersPositions);
    			foreach (var t in config.SmallLettersPositions)
    			{
    				SpawnLetter(false, t.position, BlueUFO.transform, idx++);
    			}
    		}
    		*/

    		// New Way, ignore the stupid double call
    		if (isBig)
    		{
    			currentLevel++;
    			if (currentLevel > 0) // We start from -1 as this is triggered on very first level that isn't yet done!
    				GameProgressController.Instance.AddProgressSteps();
    			if (currentLevel >= maxLevels)
    			{
    				ShowWinUI();
    				return;
    			}

    			LetterSpawnConfig config = LetterSpawners.GetRandomElement();

    			int idx = 0;
    			Utils.Shuffle(BigLetters);
    			Utils.Shuffle(config.BigLettersPositions);
    			foreach (var t in config.BigLettersPositions)
    			{
    				SpawnLetter(true, t.position, OrangeUFO.transform, idx++);
    			}

    			idx = 0;
    			Utils.Shuffle(SmallLetters);
    			Utils.Shuffle(config.SmallLettersPositions);
    			foreach (var t in config.SmallLettersPositions)
    			{
    				SpawnLetter(false, t.position, BlueUFO.transform, idx++);
    			}
    		}
    	}

    	private void SpawnLetter(bool isBig, Vector3 position, Transform parent, int idx)
    	{
    		Sprite letterSprite = isBig ? BigLetters[idx] : SmallLetters[idx];
    		GameObject go = new GameObject("Letter_" + letterSprite.name);
    		SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
    		sr.sprite = letterSprite;
    		sr.sortingOrder = 4;
    		sr.sharedMaterial = LetterMaterial;
    		go.transform.parent = parent;
    		go.transform.position = position;
    		go.transform.localScale = Vector3.one * LetterScale;

    		// Add Collider
    		BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
    		collider.isTrigger = true;
    		collider.size = letterSprite.bounds.size;
    		collider.edgeRadius = 0.5f;
    		//collider.offset = new Vector2(0f, collider.size.y * 0.5f);
    		if (isBig)
    			SpawnedBigLetters.Add(collider);
    		else
    			SpawnedSmallLetters.Add(collider);

    		DragLetterInfo info = go.AddComponent<DragLetterInfo>();
    		info.Collider = collider;
    		info.OriginalPosition = go.transform.position;
    		info.Renderer = sr;
    		info.IsUpperCase = isBig;
    		info.Letter = letterSprite.name[0];

    		SetSpriteColor colorController = go.AddComponent<SetSpriteColor>();
    		Color baseColor;
    		//Color rndColor = colorSampler.GetNextColor();
    		//float interpolation = Mathf.Clamp01((float)(level - 1) / maxDifficultyAtLevel);
    		if (isBig)
    		{
    			baseColor = UpperCaseColor; // Random.value < interpolation ? rndColor : UpperCaseColor;
    		}
    		else
    		{
    			baseColor = LowerCaseColor; // Random.value < interpolation ? rndColor : LowerCaseColor;
    		}
    		Color outlineColor = baseColor * OutlineMultiplier;
    		outlineColor.a = 1.0f;
    		colorController.SetColor(baseColor, outlineColor);

    		// Animate apparition
    		Vector3 localScale = go.transform.localScale;
    		go.transform.localScale = Vector3.zero;
    		go.transform.DOScale(localScale, 0.3f).SetEase(Ease.OutBack).SetDelay(Random.Range(0f, 0.3f));
    	}

    	private void OnDrawGizmos()
    	{
    		if (LetterSpawners != null)
    		{
    			foreach (var spawner in LetterSpawners)
    			{
    				Gizmos.color = new Color(1.0f, 0.65f, 0f); // Orange
    				if (spawner.BigLettersPositions != null)
    				{
    					foreach (var trans in spawner.BigLettersPositions)
    					{
    						Gizmos.DrawCube(trans.position, Vector3.one * 0.1f);
    					}
    				}
    				Gizmos.color = Color.blue;
    				if (spawner.SmallLettersPositions != null)
    				{
    					foreach (var trans in spawner.SmallLettersPositions)
    					{
    						Gizmos.DrawCube(trans.position, Vector3.one * 0.1f);
    					}
    				}
    			}
    		}
    	}

    	private void Update()
    	{
    		if (hintCooldown > 0)
    		{
    			hintCooldown -= Time.deltaTime;
    			if (hintCooldown < 0)
    			{
    				hintCooldown = MaxHintCooldown;

    				List<DragLetterInfo> infos;
    				if (Random.value < 0.5f)
                    {
    					infos = SpawnedBigLetters.Where(x => x.enabled).Select(x => x.GetComponent<DragLetterInfo>()).ToList();
    				}
    				else
                    {
    					infos = SpawnedSmallLetters.Where(x => x.enabled).Select(x => x.GetComponent<DragLetterInfo>()).ToList();
    				}
    				if (infos.Count > 0)
    				{
    					DragLetterInfo info = infos.GetRandomElement();
    					if (info.IsUpperCase)
    					{
    						FingerHintController.Instance?.ShowDrag(info.transform.position, BigDropTarget.transform.position, 1.0f);
    						//if(Random.value < 0.5f)
    						//	SoundManager.Instance.PlaySFX("letter_back");
    					}
    					else
                        {
    						FingerHintController.Instance?.ShowDrag(info.transform.position, SmallDropTarget.transform.position, 1.0f);
    						//if (Random.value < 0.5f)
    						//	SoundManager.Instance.PlaySFX("letter_back");
    					}
    				}
    			}
    		}
    	}

    	public void ShowWinUI()
    	{
    #if UNITY_IOS
    		if (!ProgressManager.Instance.IsReviewShown(7))
    		{
    			Debug.Log("Asking for review!");
    			//UnityEngine.iOS.Device.RequestStoreReview();
    			ProgressManager.Instance.SetReviewShow(7);
    		}
    #endif

    		SoundManager.Instance.PlaySFX("FinishMiniGame_3");
    		string[] sfxx = new string[] { "good_job", "do_again", "did_great", "did_all", "play_again" };
    		SoundManager.Instance.PlaySFX(sfxx.GetRandomElement());
    		DOTween.Sequence()
    			.AppendInterval(1.0f)
    			.AppendCallback(() => Fader.gameObject.SetActive(true))
    			.Append(Fader.DOFade(0.8f, 1.0f))
    			.AppendCallback(() => PopUp.SetActive(true))
    			.Append(PopUp.transform.DOScale(Vector3.zero, 0.4f).From().SetEase(Ease.OutBack));
    	}
    }


}