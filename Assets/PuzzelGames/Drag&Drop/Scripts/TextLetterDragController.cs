using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

    public class TextLetterDragController : MonoBehaviour
    {
    	public LetterDefinition[] Letters;
    	public LetterDefinition[] TargetLetters;
    	public float TargetRange = 0.5f;
    	public float MaxHintCooldown = 7.0f;
    	float hintCooldownTimer;

    	public System.Action OnDoneEvent;
    	[HideInInspector]
    	public bool ShowIntro = true;

    	int lettersToTarget = 0;
    	int sortOrder = 2;

    	// Start is called before the first frame update
    	void Start()
    	{
    		hintCooldownTimer = -1.0f; // Wait for the spread first
    		if (SystemInfo.deviceModel.Contains("iPad"))
    			transform.localScale *= 0.75f;
    		Rebase();
    		foreach (var letter in Letters)
    		{
    			Vector3 originalScale = letter.transform.localScale;
    			letter.transform.localScale = Vector3.zero;
    			letter.transform.DOScale(originalScale, 2.0f)
    				.SetDelay(Random.Range(0f, 0.75f))
    				.SetEase(Ease.OutElastic, 1.1f, 0.5f)
    				.OnPlay(() => SoundManager.Instance.PlaySFX("BubblePop"));
    		}

    		// This is UGLY TOO, use gametype maybe?
    		if (ShowIntro)
    		{
    			// UGLY JUST LIKE YOU MOTHER!
    			Vector3 camPos = GameManager.Instance.IntroAndWordLevel.GetComponentInChildren<Camera>().transform.position;

    			DOTween.Sequence()
    				.AppendInterval(1.75f)
    				.Append(transform.DOMove(camPos.SetZ(0f), 1f))
    				.AppendCallback(Spread);
    		} 
    		else
            {
    			DOTween.Sequence()
    				.AppendInterval(2.75f)
    				.AppendCallback(Spread);

    			SoundManager.Instance.PlaySFXStack("here_is_the_word", 1.0f, "voiceover", 2)
    				.AddSFXStack(GetWord());
    		}
    	}

    	public string GetWord()
        {
    		string word = "";
    		for (int i = 0; i < Letters.Length; i++)
    		{
    			if (Letters[i].Letter == '-' || Letters[i].Letter == ' ' || Letters[i].Letter == '_')
    				continue;
    			word += char.ToLower(Letters[i].Letter);
    		}
    		return word;
    	}

    	void Rebase()
        {
    		for (int i = 0; i < Letters.Length; i++)
            {
    			var letter = Letters[i];
                letter.GetComponent<SpriteRenderer>().sortingOrder = 1;
                Vector3 originalPosition = letter.transform.position;
    			//Vector3 centerOffset = letter.Collider.bounds.center - transform.position;
    			bool activeState = letter.gameObject.activeSelf;
    			letter.gameObject.SetActive(true);
    			GameObject letterParent = new GameObject(letter.name + "TR");
    			letterParent.transform.SetParent(letter.transform.parent);

    			// TODO: Check for a better fix! This refreshes the collider based on new offsets
    			letter.Collider.enabled = false;
    			letter.Collider.enabled = true;

    			(letter.Collider as BoxCollider2D).edgeRadius = 0.5f;

    			letterParent.transform.position = letter.Collider.bounds.center;
    			//letterParent.transform.position = transform.position + centerOffset;
    			letter.transform.SetParent(letterParent.transform);
    			letterParent.SetActive(activeState);
    			Letters[i] = SwapComponents(letter, letterParent);

    			if (letter.TargetPosition != null)
    				letter.TargetPosition.position += letter.transform.position - originalPosition;
            }
    		for (int i = 0; i < TargetLetters.Length; i++)
    		{
    			var letter = TargetLetters[i];
    			letter.GetComponent<SpriteRenderer>().sortingOrder = 0;
    			bool activeState = letter.gameObject.activeSelf;
    			letter.gameObject.SetActive(true);
    			GameObject letterParent = new GameObject(letter.name + "TR");
    			letterParent.transform.SetParent(letter.transform.parent);

    			letter.Collider.enabled = false;
    			letter.Collider.enabled = true;

    			letterParent.transform.position = letter.Collider.bounds.center;
    			letter.transform.SetParent(letterParent.transform);
    			letterParent.SetActive(activeState);
    			TargetLetters[i] = SwapComponents(letter, letterParent);
    		}
    	}

    	LetterDefinition SwapComponents(LetterDefinition from, GameObject to)
        {
    		LetterDefinition def = to.AddComponent<LetterDefinition>();
    		def.Collider = from.Collider;
    		def.IsActive = from.IsActive;
    		def.Letter = from.Letter;
    		def.TargetPosition = from.TargetPosition;
    		def.ReadyForDrag = from.ReadyForDrag;
    		def.OriginalPosition = from.OriginalPosition;
    		Destroy(from);
    		return def;
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
    		LetterDefinition def = helper.DraggingTransform.GetComponent<LetterDefinition>();
    		char letter = def.Letter;

    		foreach (var target in TargetLetters)
    		{
    			if (target.IsActive && target.Letter == letter && target.transform.position.Distance(def.transform.position) < TargetRange)
    			{
    				MoveToTarget(def, target);
    				def.Collider.enabled = false;
    				target.IsActive = false;
    				break;
    			}
    		}
        }

        private void OnTouch(Touch touch)
    	{
    		foreach (var letter in Letters)
    		{
    			if (letter.Collider == null)
    				continue;
    			if (!letter.Collider.enabled)
    				continue;
    			if (!letter.ReadyForDrag)
    				continue;

    			Vector3 worldPos = DragManager.GetWorldSpacePos(touch.position);
    			if (letter.Collider.OverlapPoint(worldPos))
    			{
    				hintCooldownTimer = -1.0f;
    				FingerHintController.Instance?.Hide();
    				SoundManager.Instance.PlaySFX("MovingIslands");
    				letter.Collider.GetComponent<SpriteRenderer>().sortingOrder = sortOrder++;
    				DragManager.Instance.AddDrag(touch.fingerId, letter.transform, worldPos, Vector3.zero);
    				if (!ShowIntro)
    					SoundManager.Instance.PlaySFX(char.ToLower(letter.Letter).ToString(), 1.0f, "letter", 1);

    				break;
    			}
    		}
    	}

    	private void MoveToTarget(LetterDefinition source, LetterDefinition target)
    	{
    		SoundManager.Instance.PlaySFX("LettersToTargetPoint");
    		// aici tot trebuie sunet
    		DOTween.Sequence()
    			.Append(source.transform.DOMove(target.transform.position, 0.2f))
    			.OnComplete(() =>
    			{
    				target.gameObject.SetActive(false);
    				lettersToTarget++;
    				CheckIfDone();
    			});
    	}

    	public void Spread()
    	{
    		foreach (var target in TargetLetters)
    			target.gameObject.SetActive(true);

    		hintCooldownTimer = MaxHintCooldown;
    		SoundManager.Instance.PlaySFX("LettersSpread");
    		// aici trebuie sunet
    		foreach (var letter in Letters)
    		{
    			if (letter.TargetPosition == null)
    				continue;

    			Vector3 targetDiff = letter.TargetPosition.position - transform.position;
    			Vector2 scale = Vector2.one;
    			if (SystemInfo.deviceModel.Contains("iPad"))
    				scale = new Vector2(0.5f, 1.5f);
    			targetDiff.x *= scale.x;
    			targetDiff.y *= scale.y;
    			Vector3 targetPos = transform.position + targetDiff;
    			letter.Collider.GetComponent<SpriteRenderer>().sortingOrder = sortOrder++;

    			DOTween.Sequence()
    				.Append(letter.transform.DOMove(targetPos, 0.5f).SetEase(Ease.InOutSine))
    				.Join(letter.transform.DORotate(new Vector3(0f, 0f, 360.0f), 0.5f, RotateMode.LocalAxisAdd).SetEase(Ease.InOutSine))
    				.OnComplete(() => letter.ReadyForDrag = true);
    		}

    		if (ShowIntro)
            {
    			string word = GetWord();
    			SoundManager.Instance.AddSFXToQueue(char.ToLower(word[0]) + "_from_" + word);
    		}
    	}

    	private void CheckIfDone()
        {
    		if (lettersToTarget == TargetLetters.Length)
            {
    			OnDoneEvent();
            }
        }

    	public Sequence HideLetters()
        {
    		Sequence s = DOTween.Sequence();
    		foreach (var letter in Letters)
    		{
    			s.Join(letter.transform.DOScale(Vector3.zero, 0.3f).SetDelay(Random.Range(0f, 0.3f)).SetEase(Ease.InBack));
    		}
    		s.AppendCallback(() => Destroy(gameObject));
    		return s;
    	}

        private void Update()
        {
            if (hintCooldownTimer > 0)
            {
    			hintCooldownTimer -= Time.deltaTime;
    			if (hintCooldownTimer < 0 && FingerHintController.Instance != null)
                {
    				hintCooldownTimer = MaxHintCooldown;
    				// Reused list for hint and for target!
    				List<LetterDefinition> hintLetters = new List<LetterDefinition>();
    				foreach (var letter in Letters)
    				{
    					if (letter.ReadyForDrag)
    						hintLetters.Add(letter);
    				}
    				if (hintLetters.Count > 0)
                    {
    					LetterDefinition letter = hintLetters.GetRandomElement();
    					hintLetters.Clear();
    					foreach (var targetLetter in TargetLetters)
    					{
    						if (targetLetter.Letter == letter.Letter)
    							hintLetters.Add(targetLetter);
    					}
    					if (hintLetters.Count > 0)
    						FingerHintController.Instance?.ShowDrag(letter.transform.position, hintLetters.GetRandomElement().transform.position, 2.0f);
    						SoundManager.Instance.PlaySFXStack("add_letter", 1.0f, "voiceover", 1)
    								.AddSFXStack(char.ToLower(hintLetters[0].Letter).ToString())
    								.AddSFXStack("where_belongs_word");


    				}
    			}
            }
        }

    #if UNITY_EDITOR
        private void OnGUI()
    	{
    		if (GUI.Button(new Rect(0, 0, 100, 100), "Restart"))
    			Start();

    		if (GUI.Button(new Rect(200, 200, 400, 400), "Spread"))
    			Spread();
    	}
    #endif
    }


}