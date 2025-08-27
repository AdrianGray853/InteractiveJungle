using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Interactive.DRagDrop
{
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

    public class UFOSelectController : MonoBehaviour
    {
    	[System.Serializable]
    	public class SpawnConfig
    	{
    		public Transform[] SpawnPoints;
    	}

    	public Animator UFOAnimator;

    	public float LetterScale = 0.3f;
    	public float UFOLetterScale = 0.3f;
    	public float UFOLetterSpacing = 0.2f;
    	public float MaxHintCooldown = 7.0f;
    	float hintCooldown;
    	public TargetPositionController DropZone;
    	public Transform MoveToTarget;

    	public Transform UFOSpawnMidPoint;

    	public SpawnConfig[] SpawnConfigs;

    	public GameObject[] BigLetters;
    	public GameObject[] SmallLetters;

    	public Sprite[] BigOutlineLetters;
    	public Sprite[] SmallOutlineLetters;

    	public RawImage Fader;
    	public GameObject PopUp;

    	/*
    	class CharPair
    	{
    		public LetterDefinition UpperCase;
    		public LetterDefinition LowerCase;
    	}
    	*/

    	class CharPair
    	{
    		public SpriteRenderer UpperCase;
    		public SpriteRenderer LowerCase;
    		public char Letter;
    		public bool UpperActive;
    		public bool LowerActive;
    	}

    	List<LetterDefinition> SpawnedLetters = new List<LetterDefinition>();
    	Queue<char> SpawnedPairs = new Queue<char>();
    	CharPair CurrentPair;

    	int maxLevels = 5;
    	int currentLevel = 0;

    	// Start is called before the first frame update
    	void Start()
    	{
    		GameProgressController.Instance.SetMaxProgressSteps(maxLevels);
    		hintCooldown = MaxHintCooldown * 2.0f;
    		SoundManager.Instance.CrossFadeMusic("SelectLettersModulesBgMusic", 1.0f);

    		SpawnLetters();
    		SelectPair();
    		SoundManager.Instance.AddSFXToQueue("arrived");
    		SoundManager.Instance.AddSFXToQueue("have_select");
    	}

    	private void Update()
    	{
    		bool found = false;
    		if (DragManager.Instance.IsDragging())
    		{
    			var dragHelpers = DragManager.Instance.GetDragHelpers();
    			foreach (var helper in dragHelpers)
    			{
    				if (DropZone.InRange(helper.Value.DraggingTransform.position))
    				{
    					LetterDefinition letter = helper.Value.ReferenceObject as LetterDefinition;
    					if (char.ToLower(letter.Letter) == char.ToLower(CurrentPair.Letter))
    					{
    						FingerHintController.Instance?.Hide();
    						hintCooldown = -1.0f;
    						found = true;
    						break;
    					}
    				}
    			}
    		}
    		UFOAnimator.SetBool("IsHover", found);

    		if (hintCooldown > 0)
            {
    			hintCooldown -= Time.deltaTime;
    			if (hintCooldown < 0)
                {
    				hintCooldown = MaxHintCooldown;
    				List<LetterDefinition> letters = new List<LetterDefinition>();
    				foreach (var letter in SpawnedLetters)
    				{
    					if (!letter.Collider.enabled)
    						continue;
    					if (!letter.IsActive)
    						continue;
    					if (DragManager.Instance.HasDrag(letter.transform))
    						continue;

    					if (char.ToLower(letter.Letter) == char.ToLower(CurrentPair.Letter))
    					{
    						letters.Add(letter);
    					}
    				}
    				if (letters.Count > 0)
                    {
    					FingerHintController.Instance?.ShowDrag(letters.GetRandomElement().transform.position, DropZone.transform.position, 1.5f);
    					if(Random.value < 0.5f)
                        {
    						SoundManager.Instance.AddSFXToQueue("drag_drop_letter");
    						SoundManager.Instance.AddSFXToQueue(char.ToLower(CurrentPair.Letter).ToString());
                        }
                        else
                        {
    						string[] sfx = new string[] { "move_upper_lower", "have_select" };
    						SoundManager.Instance.PlaySFX(sfx.GetRandomElement());
    					}
    				}
    			}
            }
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
    		LetterDefinition letter = helper.ReferenceObject as LetterDefinition;
    		if (char.ToLower(letter.Letter) == char.ToLower(CurrentPair.Letter) &&
    				DropZone.InRange(helper.DraggingTransform.position))
    		{
    			SoundManager.Instance.PlaySFX("WooshSound", 0.5f);
    			MoveToPosition(letter, MoveToTarget.position);
    			letter.Collider.enabled = false;
    		}
    		else
    		{
    			if (DropZone.InRange(helper.DraggingTransform.position))
    			{
    				SoundManager.Instance.PlaySFX("WrongSound");

    				string[] sfx = new string[] { "attention_details", "dont_worry", "decide", "try" };
    				SoundManager.Instance.PlaySFX(sfx.GetRandomElement(), 1.0f, "voiceover", 1);
    			}
    			// Didn't find any target, return to base;
    			letter.IsActive = false;
    			letter.transform.DOMove(letter.OriginalPosition, 0.4f).SetEase(Ease.OutBack).OnComplete(() => letter.IsActive = true);
    		}

    		/*
    		if (info.IsUpperCase)
    		{
    			if (BigDropTarget.InRange(collider.transform.position))
    			{
    				Vector3 randomPos = BigDropTarget.GetComponent<Collider2D>().bounds.GetRandomPosition();
    				MoveToPosition(info, randomPos);
    				collider.enabled = false;
    				return;
    			}
    		}
    		else
    		{
    			if (SmallDropTarget.InRange(collider.transform.position))
    			{
    				Vector3 randomPos = SmallDropTarget.GetComponent<Collider2D>().bounds.GetRandomPosition();
    				MoveToPosition(info, randomPos);
    				collider.enabled = false;
    				return;
    			}
    		}

    		// Didn't find any target, return to base;
    		info.IsActive = false;
    		info.transform.DOMove(info.OriginalPosition, 0.4f).SetEase(Ease.OutBack).OnComplete(() => info.IsActive = true);
    		*/
    	}

    	private void OnTouch(Touch touch)
    	{
    		foreach(var letter in SpawnedLetters)
    		{
    			if (!letter.Collider.enabled)
    				continue;
    			if (!letter.IsActive)
    				continue;

    			Vector3 worldPos = DragManager.GetWorldSpacePos(touch.position);
    			if (letter.Collider.OverlapPoint(worldPos))
    			{
    				SoundManager.Instance.PlaySFX("MovingIslands");
    				DragHelper helper = DragManager.Instance.AddDrag(touch.fingerId, letter.transform, worldPos, Vector3.zero);
    				helper.ReferenceObject = letter;
    				FingerHintController.Instance?.Hide();
    				SoundManager.Instance.PlaySFX(char.ToLower(letter.Letter).ToString());
    				return;
    			}
    		}
    	}

    	void MoveToPosition(LetterDefinition letter, Vector3 position)
    	{
    		DOTween.Sequence()
    			.Append(letter.transform.DOMove(position, 1.0f).SetEase(Ease.InSine))
    			.Join(letter.transform.DOScale(Vector3.zero, 1.0f).SetEase(Ease.InSine))
    			.Join(letter.transform.DORotate(new Vector3(0f, 0f, 30.0f), 1f).SetEase(Ease.InSine))
    			.AppendCallback(() => FinishedAnimation(letter));

    		/*
    		DOTween.Sequence()
    			.Append(info.transform.DOMove((position + info.transform.position) * 0.5f + Vector3.up * 3.0f, 0.5f).SetEase(Ease.OutSine))
    			.AppendCallback(() => info.GetComponent<SpriteRenderer>().sortingOrder = 0)
    			.Append(info.transform.DOMove(position, 0.5f).SetEase(Ease.InSine))
    			.Insert(0f, info.transform.DORotate(new Vector3(0f, 0f, 360.0f), 1f, RotateMode.FastBeyond360).SetEase(Ease.InOutSine))
    			.AppendCallback(() => FinishedAnimation(info));
    		*/
    	}

    	void FinishedAnimation(LetterDefinition letter)
    	{
    		Debug.Log("Done!");

    		if (letter.Letter == char.ToLower(CurrentPair.Letter))
    		{
    			SpriteRenderer sr = CurrentPair.LowerCase;
    			sr.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    			CurrentPair.LowerActive = false;
    		}
    		else if (letter.Letter == char.ToUpper(CurrentPair.Letter))
    		{
    			SpriteRenderer sr = CurrentPair.UpperCase;
    			sr.DOColor(new Color(0.5f, 0.5f, 0.5f, 0.5f), 0.3f);
    			CurrentPair.UpperActive = false;
    		}

    		if (!CurrentPair.UpperActive && !CurrentPair.LowerActive)
    		{ // Change the letters
    			SelectPair();
    		}

    		SpawnedLetters.Remove(letter);
    		Destroy(letter.gameObject);

    		/*
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
    			// Done;
    			BlueUFO.Play("Closing");
    			OrangeUFO.Play("Closing");
    		}
    		*/
    	}

    	SpriteRenderer CreateOutlineLetter(char letter, bool UpperCase = true)
    	{
    		int idx = char.ToLower(letter) - 'a';
    		Sprite sprite = UpperCase ? BigOutlineLetters[idx] : SmallOutlineLetters[idx];
    		GameObject go = new GameObject("Letter_" + letter);
    		SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
    		sr.sprite = sprite;       
    		return sr;
    	}

    	public void SelectPair()
    	{
    		if (SpawnedPairs.Count > 0)
    		{
    			char letter = SpawnedPairs.Dequeue();
    			SpriteRenderer upperDef = CreateOutlineLetter(letter, true);
    			SpriteRenderer lowerDef = CreateOutlineLetter(letter, false);            
    			upperDef.transform.localScale = Vector3.one * UFOLetterScale;
    			lowerDef.transform.localScale = Vector3.one * UFOLetterScale * 0.75f;
    			upperDef.transform.position = UFOSpawnMidPoint.position + new Vector3(-upperDef.bounds.extents.x - UFOLetterSpacing * 0.5f, upperDef.bounds.extents.y);
    			lowerDef.transform.position = UFOSpawnMidPoint.position + new Vector3(lowerDef.bounds.extents.x + UFOLetterSpacing * 0.5f, lowerDef.bounds.extents.y);
    			// Recenter
    			Vector3 midPoint = (upperDef.transform.position + lowerDef.transform.position) * 0.5f;
    			float xDiff = midPoint.x - UFOSpawnMidPoint.position.x;
    			lowerDef.transform.position += Vector3.left * xDiff;
    			upperDef.transform.position += Vector3.left * xDiff;

    			upperDef.sortingOrder = 12;
    			lowerDef.sortingOrder = 12;

    			CharPair nextPair = new CharPair() { LowerCase = lowerDef, UpperCase = upperDef, Letter = letter, LowerActive = true, UpperActive = true };
    			if (CurrentPair != null)
    			{
    				SpriteRenderer tmpLower = CurrentPair.LowerCase;
    				SpriteRenderer tmpUpper = CurrentPair.UpperCase;

    				CurrentPair = nextPair;

    				nextPair.LowerCase.transform.localScale = Vector3.zero;
    				nextPair.UpperCase.transform.localScale = Vector3.zero;

    				DOTween.Sequence()
    					.Append(tmpUpper.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack))
    					.Join(tmpLower.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack))
    					.Append(nextPair.UpperCase.transform.DOScale(Vector3.one * UFOLetterScale, 0.3f).SetEase(Ease.OutBack))
    					.Join(nextPair.LowerCase.transform.DOScale(Vector3.one * UFOLetterScale * 0.75f, 0.3f).SetEase(Ease.OutBack).SetDelay(Random.Range(0f, 0.3f)))
    					.OnComplete(() =>
    						{
    							Destroy(tmpLower.gameObject);
    							Destroy(tmpUpper.gameObject);
    						});

    			} 
    			else
    			{
    				nextPair.LowerCase.transform.localScale = Vector3.zero;
    				nextPair.UpperCase.transform.localScale = Vector3.zero;
    				DOTween.Sequence()
    				   .Append(nextPair.UpperCase.transform.DOScale(Vector3.one * UFOLetterScale, 0.3f).SetEase(Ease.OutBack))
    				   .Join(nextPair.LowerCase.transform.DOScale(Vector3.one * UFOLetterScale * 0.75f, 0.3f).SetEase(Ease.OutBack).SetDelay(Random.Range(0f, 0.3f)));
    				CurrentPair = nextPair;
    			}
    		} 
    		else // Fly Away
    		{
    			currentLevel++;
    			GameProgressController.Instance.AddProgressSteps();
    			if (currentLevel < maxLevels)
    			{
    				string[] sfxx = new string[] { "good_job", "so_smart", "next_pair", "doing_great", "did_this_too" };
    				SoundManager.Instance.PlaySFX(sfxx.GetRandomElement(), 1.0f, "voiceover", 4);

    				SoundManager.Instance.PlaySFX("SpaceshipSellectLeter");
    				UFOAnimator.Play("FlyAway");
    			}
    			else
    			{
    				ShowWinUI();
    			}

    			if (CurrentPair != null)
    			{
    				SpriteRenderer tmpLower = CurrentPair.LowerCase;
    				SpriteRenderer tmpUpper = CurrentPair.UpperCase;

    				tmpLower.transform
    					.DOScale(Vector3.zero, 0.3f)
    					.SetEase(Ease.OutBack)
    					.OnComplete(() => Destroy(tmpLower.gameObject));

    				tmpUpper.transform
    					.DOScale(Vector3.zero, 0.3f)
    					.SetEase(Ease.OutBack)
    					.OnComplete(() => Destroy(tmpUpper.gameObject));
    			}

    			CurrentPair = null;
    		}
    	}

    	public void SpawnLetters()
    	{
    		SpawnConfig config = SpawnConfigs.GetRandomElement();
    		Debug.Assert(config.SpawnPoints.Length % 2 == 0, "The number of points needs to be even!");

    		int[] indices = Enumerable.Range(0, BigLetters.Length).ToArray();
    		Utils.Shuffle(indices);

    		Utils.Shuffle(config.SpawnPoints);
    		int nrPairs = config.SpawnPoints.Length / 2;
    		for (int i = 0; i < nrPairs; i++)
    		{
    			Vector3 UpperPos = config.SpawnPoints[i].position;
    			Vector3 LowerPos = config.SpawnPoints[i + nrPairs].position;
    			LetterDefinition biggy = Instantiate(BigLetters[indices[i]], UpperPos, Quaternion.identity).GetComponent<LetterDefinition>();
    			LetterDefinition smally = Instantiate(SmallLetters[indices[i]], LowerPos, Quaternion.identity).GetComponent<LetterDefinition>();
    			biggy.OriginalPosition = biggy.transform.position;
    			smally.OriginalPosition = smally.transform.position;
    			(biggy.Collider as BoxCollider2D).edgeRadius = 0.5f;
    			(smally.Collider as BoxCollider2D).edgeRadius = 0.5f;
    			CenterPivot(biggy);
    			CenterPivot(smally);
    			biggy.transform.localScale = Vector3.zero;
    			smally.transform.localScale = Vector3.zero;
    			biggy.transform.DOScale(Vector3.one * LetterScale, 0.3f).SetEase(Ease.OutBack).SetDelay(Random.Range(0f, 0.3f));
    			smally.transform.DOScale(Vector3.one * LetterScale, 0.3f).SetEase(Ease.OutBack).SetDelay(Random.Range(0f, 0.3f));
    			SpawnedLetters.Add(biggy);
    			SpawnedLetters.Add(smally);
    			SpawnedPairs.Enqueue(smally.Letter);
    		}
    	}

    	void CenterPivot(LetterDefinition letter)
    	{
    		letter.Collider.transform.localPosition -= (letter.Collider.bounds.center - letter.Collider.transform.position);
    	}

    	private void OnDrawGizmos()
    	{
    		Gizmos.color = Color.red;
    		foreach(var spawn in SpawnConfigs)
    		{
    			foreach(var point in spawn.SpawnPoints)
    			{
    				Gizmos.DrawCube(point.position, Vector3.one * 0.1f);
    			}
    		}
    	}
    	public void ShowWinUI()
    	{
    #if UNITY_IOS
    		if (!ProgressManager.Instance.IsReviewShown(6))
    		{
    			Debug.Log("Asking for review!");
    			UnityEngine.iOS.Device.RequestStoreReview();
    			ProgressManager.Instance.SetReviewShow(6);
    		}
    #endif

    		SoundManager.Instance.PlaySFX("FinishMiniGame_3");
    		string[] sfxx = new string[] { "good_job", "do_again", "did_great" };
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