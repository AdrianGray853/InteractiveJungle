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

    public class MixMatchUpDownGameplay : MonoBehaviour
    {
    	public float LetterScale = 0.3f;
    	public float BubbleScale = 1f;
    	public float MaxHintCooldown = 7.0f;
    	float hintCooldown;
    	public GameObject Bula;
    	public LineRenderer DragLine;

    	public LetterDefinition[] BigLetters;
    	public LetterDefinition[] SmallLetters;

    	public Transform[] BigLetterPositions; // 3
    	public Transform[] SmallLetterPositions; // 3

    	public RawImage Fader;
    	public GameObject PopUp;

    	int[] randomIndices;

    	List<LetterDefinition> SpawnedLetters = new List<LetterDefinition>();

    	List<Tweener> tweensToKill = new List<Tweener>();

    	int maxLevels = 20;
    	int currentLevel = 0;

    	private void KillTweens()
        {
    		foreach(var tween in tweensToKill)
            {
    			if (tween.IsActive())
    				tween.Kill(true);
            }
    		tweensToKill.Clear();
        }

    	private void OnEnable()
    	{
    		DragManager.Instance.AddOnTouchCallback(OnTouch);
    		DragManager.Instance.AddOnReleaseCallback(OnRelease);
    		DragManager.Instance.AddOnMoveCallback(OnMove);
    	}

    	private void OnDisable()
    	{
    		if (DragManager.Instance == null)
    			return;
    		DragManager.Instance.RemoveOnTouchCallback(OnTouch);
    		DragManager.Instance.RemoveOnReleaseCallback(OnRelease);
    		DragManager.Instance.RemoveOnMoveCallback(OnMove);
    	}

    	private void OnRelease(Touch touch, DragHelper helper)
    	{
    		LineRenderer line = helper.DraggingTransform.GetComponent<LineRenderer>();

    		Vector3 worldPos = DragManager.GetWorldSpacePos(touch.position);
    		LetterDefinition sourceLetter = helper.ReferenceObject as LetterDefinition;
    		foreach (var letter in SpawnedLetters)
    		{
    			if (letter != sourceLetter &&
    				letter.Collider.enabled && letter.Collider.OverlapPoint(worldPos))
    			{
    				KillTweens();
    				SoundManager.Instance.PlaySFX(char.ToLower(letter.Letter).ToString(), 1.0f, "letter", 1);
    				if (char.ToLower(letter.Letter) == char.ToLower(sourceLetter.Letter))
    				{
    					letter.Collider.enabled = false;
    					sourceLetter.Collider.enabled = false;
    					line.SetPosition(1, letter.Collider.bounds.center);

    					Color targetColor = line.startColor;
    					targetColor.a = 0f;

    					hintCooldown = -1.0f;
    					FingerHintController.Instance?.Hide();

    					DOTween.Sequence()
    						.Append(letter.transform.DOPunchScale(Vector3.one * 0.05f, 0.5f))
    						.AppendCallback(() => SoundManager.Instance.PlaySFX("BubblePop"))
    						.Append(letter.transform.DOScale(Vector3.zero, 0.3f))
    						.Join(sourceLetter.transform.DOScale(Vector3.zero, 0.3f))
    						.Join(line.DOColor(new Color2(line.startColor, line.endColor), new Color2(targetColor, targetColor), 0.3f))
    						.AppendCallback(() =>
    						{
    							SpawnedLetters.Remove(letter);
    							SpawnedLetters.Remove(sourceLetter);
    							Destroy(letter.gameObject);
    							Destroy(sourceLetter.gameObject);
    							Destroy(line.gameObject);
    							CheckForEnding();
    						});
    					return; // EARLY RETURN!
    				} 
    				else
                    { // Wrong bubble, Shake and Nemo baby
    					letter.ReadyForDrag = false;
    					sourceLetter.ReadyForDrag = false;
    					letter.transform.DOShakePosition(0.3f, 0.5f, 20).OnComplete(() => letter.ReadyForDrag = true);
    					sourceLetter.transform.DOShakePosition(0.3f, 0.5f, 20).OnComplete(() => sourceLetter.ReadyForDrag = true);
    					SoundManager.Instance.PlaySFX("WrongSound");
    					string[] sfx = new string[] { "attention_details", "dont_worry", "decide", "try" };
    					SoundManager.Instance.PlaySFX(sfx.GetRandomElement(), 1.0f, "voiceover", 1);
    				}
    			}
    		}

    		Destroy(line.gameObject);
    	}

    	private void OnTouch(Touch touch)
    	{
    		Vector3 worldPos = DragManager.GetWorldSpacePos(touch.position);
    		foreach (var letter in SpawnedLetters)
    		{
    			if (letter.Collider.enabled && letter.ReadyForDrag && letter.Collider.OverlapPoint(worldPos))
    			{
    				FingerHintController.Instance?.Hide();
    				letter.ReadyForDrag = false;
    				tweensToKill.Add( letter.transform.DOPunchScale(Vector3.one * 0.05f, 0.5f).OnComplete(() => letter.ReadyForDrag = true) );
    				CreateLineHelper(touch, letter.Collider.bounds.center, letter);
    				SoundManager.Instance.PlaySFX(char.ToLower(letter.Letter).ToString(), 1.0f, "letter", 1);
    				/*if (letter.transform.position.y == BigLetterPositions[0].position.y)
    				{
    					//SoundManager.Instance.PlaySFX("uppercase_bubbe");
    				}
    				else if (letter.transform.position.y == SmallLetterPositions[0].position.y)
    				{
    					//SoundManager.Instance.PlaySFX("lowercase_bubble");
    				} */
    				// match the uppercase letter .. with the lowercase letter ..
    			}
    		}/*
    		foreach (var letter in SmallLetters)
    		{
    			if (letter.Collider.enabled && letter.Collider.OverlapPoint(worldPos))
    			{
    				CreateLineHelper(touch, worldPos, letter);
    			}
    		}
    		*/
    	}

    	private void CreateLineHelper(Touch touch, Vector3 worldPos, LetterDefinition letter)
    	{
    		LineRenderer newLine = Instantiate(DragLine.gameObject).GetComponent<LineRenderer>();
    		newLine.gameObject.SetActive(true);
    		newLine.positionCount = 2;
    		newLine.SetPosition(0, worldPos);
    		newLine.SetPosition(1, worldPos);
    		DragHelper helper = DragManager.Instance.AddDrag(touch.fingerId, newLine.transform, worldPos);
    		helper.ReferenceObject = letter;
    	}

    	private void OnMove(Touch touch, DragHelper helper)
    	{
    		LineRenderer line = helper.DraggingTransform.GetComponent<LineRenderer>();
    		Vector3 endPos = DragManager.GetWorldSpacePos(touch.position);
    		line.SetPosition(1, endPos);
    	}





    	// Start is called before the first frame update
    	void Start()
    	{
    		GameProgressController.Instance.SetMaxProgressSteps(maxLevels);
    		SoundManager.Instance.CrossFadeMusic("MatchUpperCaseWithLowerCaseBgMusic", 1.0f);
    		randomIndices = Enumerable.Range(0, BigLetters.Length).ToArray();
    		SpawnLetters();
    		DOTween.Sequence()
    			.AppendInterval(1.5f)
    			.AppendCallback(() => SoundManager.Instance.PlaySFX("have_here", 1.0f, "voiceover", 2));
    	}

    	void SpawnLetters()
    	{
    		hintCooldown = MaxHintCooldown;

    		Utils.Shuffle(randomIndices);
    		Utils.Shuffle(SmallLetterPositions);

    		for (int i = 0; i < 3; i++)
    		{
    			int idx = randomIndices[i];
    			SpawnBubble(BigLetters[idx], BigLetterPositions[i]);
    			SpawnBubble(SmallLetters[idx], SmallLetterPositions[i]);

    		}
    	}

    	void SpawnBubble(LetterDefinition letter, Transform targetTransform)
    	{
    		LetterDefinition def = Instantiate(letter).GetComponent<LetterDefinition>();
    		def.Collider.transform.localPosition += def.Collider.transform.position - def.Collider.bounds.center;
    		def.transform.position = targetTransform.position;
    		def.transform.localScale *= LetterScale * BubbleScale;
    		GameObject bula = Instantiate(Bula, def.transform, true);
    		bula.transform.position = targetTransform.position;
    		bula.transform.localScale *= BubbleScale;
    		def.Collider = bula.GetComponent<Collider2D>();
    		def.ReadyForDrag = false;
    		def.transform.DOScale(Vector3.zero, 0.2f)
    			.From()
    			.SetEase(Ease.OutBack)
    			.SetDelay(Random.Range(0f, 0.3f))
    			.OnComplete(() => def.ReadyForDrag = true)
    			.OnPlay(() => SoundManager.Instance.PlaySFX("BubblePop")); 
    		SpawnedLetters.Add(def);
    	}

    	void CheckForEnding()
        {
    		if (SpawnedLetters.Count == 0)
            {
    			Debug.Log("Done");
    			currentLevel++;
    			GameProgressController.Instance.AddProgressSteps();
    			if (currentLevel < maxLevels)
    				SpawnLetters();
    			else
    				ShowWinUI();
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
    				char[] spawnedLetters = SpawnedLetters.Select(x => char.ToLower(x.Letter)).ToArray();
    				Debug.Log("Before Shuffle: " + string.Join(", ", spawnedLetters));
    				Utils.Shuffle(spawnedLetters);
    				Debug.Log("After Shuffle: " + string.Join(", ", spawnedLetters));
    				char hintLetter = spawnedLetters[0];
    				LetterDefinition start = null;
    				LetterDefinition end = null;
    				foreach (var letter in SpawnedLetters)
    				{
    					if (char.ToLower(letter.Letter) == hintLetter)
    					{
    						if (start == null)
    							start = letter;
    						else
    							end = letter;
    					}
    				}
    				if (start != null && end != null)
    				{
    					FingerHintController.Instance?.ShowDrag(start.transform.position, end.transform.position, 1.0f);
    					SoundManager.Instance.PlaySFX("connect_upper_lower", 1.0f, "voiceover", 3);
    				}
    			}
    		}
    	}
    	public void ShowWinUI()
    	{
    #if UNITY_IOS
    		if (!ProgressManager.Instance.IsReviewShown(5))
    		{
    			Debug.Log("Asking for review!");
    			//UnityEngine.iOS.Device.RequestStoreReview();
    			ProgressManager.Instance.SetReviewShow(5);
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