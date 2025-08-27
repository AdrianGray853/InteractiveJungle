using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.DRagDrop
{
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class SelectLetterGameplay : MonoBehaviour
    {
    	public char WantedLetter = 'A';
    	public int NrGoodLetters = 2;
    	public float ScaleMultiplier = 0.75f;
    	public List<LetterDefinition> LetterPrefabs;
    	public Transform[] SpawnConfigs;

    	public float MaxHintCooldown = 7.0f;
    	float hintCooldown;

    	private List<LetterDefinition> GoodLetters = new List<LetterDefinition>();
    	private List<LetterDefinition> BadLetters = new List<LetterDefinition>();

    	int selectedLetters = 0;
    	int selectedOrder = 5;

    	private void OnEnable()
    	{
    		DragManager.Instance.AddOnTouchCallback(OnTouch);
    	}

    	private void OnDisable()
    	{
    		if (DragManager.Instance == null)
    			return;
    		DragManager.Instance.RemoveOnTouchCallback(OnTouch);
    	}

    	private void OnTouch(Touch touch)
    	{
    		foreach (var letter in GoodLetters)
    		{
    			if (letter.Collider == null)
    				continue;
    			if (!letter.Collider.enabled)
    				continue;

    			Vector3 worldPos = DragManager.GetWorldSpacePos(touch.position);
    			if (letter.Collider.OverlapPoint(worldPos))
    			{
    				hintCooldown = -1.0f;
    				FingerHintController.Instance?.Hide();
    				letter.Collider.GetComponent<SpriteRenderer>().sortingOrder = selectedOrder++;
    				letter.Collider.enabled = false;
    				selectedLetters++;
    				SoundManager.Instance.PlaySFX("LetterClick");
    				SoundManager.Instance.PlaySFX(char.ToLower(WantedLetter).ToString(), 1.0f, "voiceover", 1);
    				StartCoroutine(AnimateNumber(letter.gameObject));
    				break;
    			}
    		}

    		foreach (var letter in BadLetters)
    		{
    			if (letter.Collider == null)
    				continue;
    			if (!letter.Collider.enabled)
    				continue;

    			Vector3 worldPos = DragManager.GetWorldSpacePos(touch.position);
    			if (letter.Collider.OverlapPoint(worldPos))
    			{
    				SoundManager.Instance.PlaySFX("WrongSound");
    				string[] sfx = new string[] { "attention_details", "dont_worry", "decide", "try" };
    				SoundManager.Instance.PlaySFX(sfx.GetRandomElement(), 1.0f, "voiceover", 1);
    				break;
    			}
    		}
    	}

    	// This is used to avoid concurent checks for ending (2 numbers or more can be in air while the first one might check if the game is done!)
    	int AnimatingCount = 0;
    	IEnumerator AnimateNumber(GameObject numberGO)
    	{
    		/*
    		Animator animator = numberGO.GetComponent<Animator>();
    		if (!animator.GetCurrentAnimatorStateInfo(0).IsName("idle"))
    			animator.Play("idle");
    		*/

    		Vector3 velocity = new Vector3(Random.Range(-1.0f, 1.0f), 10.0f);
    		float rotationSpeed = velocity.x > 0f ? -720.0f : 720.0f;
    		const float friction = 1.0f;

    		AnimatingCount++;

    		while (numberGO.transform.position.y > -10.0f)
    		{
    			velocity.y -= 20.0f * Time.deltaTime;
    			numberGO.transform.position += velocity * Time.deltaTime;
    			numberGO.transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    			rotationSpeed *= 1.0f / (1.0f + (Time.deltaTime * friction));
    			yield return new WaitForEndOfFrame();
    		}

    		AnimatingCount--;

    		Destroy(numberGO);
    		CheckForEnding();
    	}

    	void CheckForEnding()
    	{
    		if (selectedLetters == NrGoodLetters && AnimatingCount == 0)
    		{
    			//LevelManager.Instance.Advance();
    			HideLetters();
    		}
    	}

    	// Start is called before the first frame update
    	void Start()
    	{
    		hintCooldown = -1.0f;
    	}

    	// Update is called once per frame
    	void Update()
    	{
    		if (hintCooldown > 0)
            {
    			hintCooldown -= Time.deltaTime;
    			if (hintCooldown < 0f)
                {
    				hintCooldown = MaxHintCooldown;
    				List<LetterDefinition> hintLetters = new List<LetterDefinition>();
    				foreach (var letter in GoodLetters)
    				{
    					if (letter.Collider == null)
    						continue;
    					if (!letter.Collider.enabled)
    						continue;

    					hintLetters.Add(letter);
    				}

    				FingerHintController.Instance?.ShowTapHint(hintLetters.GetRandomElement().transform.position);

    				//SoundManager.Instance.AddSFXToQueue("select_letters");
    				//SoundManager.Instance.AddSFXToQueue(WantedLetter.ToString());
    				SoundManager.Instance.PlaySFXStack("select_letters", 1.0f, "voiceover", 2)
    					.AddSFXStack(char.ToLower(WantedLetter).ToString());
    			}	
            }
    	}

    	public void HideLetters()
        {
    		foreach (var letter in BadLetters)
            {
    			letter.transform.DOScale(Vector3.zero, 0.3f).SetDelay(Random.Range(0f, 0.2f)).SetEase(Ease.InBack);
            }
    		Sequence s = DOTween.Sequence();
    		s.AppendInterval(0.5f);
    		s.AppendCallback(GameManager.Instance.ShowTracingLevel);
        }

    	public void SpawnLetters()
    	{
    		Transform spawnConfig = SpawnConfigs[Random.Range(0, SpawnConfigs.Length)];
    		hintCooldown = MaxHintCooldown;

    		List<Vector3> positions = new List<Vector3>();
    		for (int i = 0; i < spawnConfig.childCount; i++)
    		{
    			Vector3 pos = spawnConfig.GetChild(i).position;
    			positions.Add(pos);
    		}

    		Utils.Shuffle(positions);

    		/*
    		LetterDefinition letterPrefab = null;
    		// Find our letter
    		foreach (var letter in LetterPrefabs)
    		{
    			if (letter.Letter == WantedLetter)
    			{
    				letterPrefab = letter;
    				break;
    			}
    		}
    		*/
    		LetterDefinition letterPrefab = LetterPrefabs[GameData.Instance.SelectedLevel];
    		WantedLetter = letterPrefab.Letter;
    		LetterPrefabs.Remove(letterPrefab);

    		SoundManager.Instance.PlaySFXStack("select_letters", 1.0f, "voiceover", 2)
    					.AddSFXStack(char.ToLower(WantedLetter).ToString());

    		if (letterPrefab == null)
    		{
    			Debug.LogError("Couldn't found letter " + WantedLetter);
    			return;
    		}

    		for (int i = 0; i < positions.Count; i++)
    		{
    			LetterDefinition prefab = letterPrefab;
    			if (i >= NrGoodLetters)
    				prefab = LetterPrefabs[Random.Range(0, LetterPrefabs.Count)];

    			LetterDefinition letDef = Instantiate(prefab).GetComponent<LetterDefinition>();
    			letDef.transform.position = positions[i];
    			letDef.transform.localScale *= ScaleMultiplier;
    			letDef.transform.DOScale(Vector3.zero, 0.5f)
    				.From()
    				.SetDelay(Random.Range(0f, 0.3f))
    				.SetEase(Ease.OutElastic, 1.1f, 0.5f)
    				.OnPlay(() => SoundManager.Instance.PlaySFX("BubblePop")); 

    			// Rebase
    			Vector3 offset = letDef.Collider.offset.ToVector3();
    			letDef.Collider.transform.localPosition -= offset;
    			letDef.transform.position += Vector3.Scale(offset, letDef.transform.localScale);

    			if (i >= NrGoodLetters)
    				BadLetters.Add(letDef);
    			else
    				GoodLetters.Add(letDef);
    		}
    	}
    }


}