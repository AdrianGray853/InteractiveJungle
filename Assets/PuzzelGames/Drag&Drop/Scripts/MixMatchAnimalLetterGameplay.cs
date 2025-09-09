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

    // TODO: Add particle sprinkler

    public class MixMatchAnimalLetterGameplay : MonoBehaviour
    {
    	public float AnimalMaxScale = 1.0f;
        public float LetterScale = 0.4f;
    	public float BubbleScale = 1f;
    	public float MaxHintCooldown = 7.0f;
    	float hintCooldown;
        public GameObject Bula;
        public LineRenderer DragLine;

        public LetterDefinition[] BigLetters;
        public LetterDefinition[] SmallLetters;

        public Sprite[] Animals;
        public Sprite[] Objects;

        public Transform[] AnimalsPositions; // 3
        public Transform[] LetterPositions; // 3

    	public RawImage Fader;
    	public GameObject PopUp;

    	List<LetterDefinition> SpawnedLetters = new List<LetterDefinition>();
    	List<Tweener> tweensToKill = new List<Tweener>();

    	int maxLevels = 20;
    	int currentLevel = 0;

    	bool spawnAnimal = false;

    	private void KillTweens()
    	{
    		foreach (var tween in tweensToKill)
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
    				if (char.ToLower(letter.Letter) == char.ToLower(sourceLetter.Letter))
    				{
    					letter.Collider.enabled = false;
    					sourceLetter.Collider.enabled = false;
    					line.SetPosition(1, letter.Collider.bounds.center);

    					Color targetColor = line.startColor;
    					targetColor.a = 0f;

    					hintCooldown = -1.0f;
    					FingerHintController.Instance?.Hide();

    					string animalObjectName = spawnAnimal ? InfoUtils.GetAnimalSoundNameFromLetter(letter.Letter) :
    										InfoUtils.GetObjectSoundNameFromLetter(letter.Letter);
    					string letterSound = char.ToLower(letter.Letter) + "_from_" + animalObjectName;
    					SoundManager.Instance.PlaySFX(letterSound, 1.0f, "voiceover", 5);

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
    					string[] sfx = new string[] { "attention_details", "dont_worry", "decide", "try"};
    					SoundManager.Instance.PlaySFX(sfx.GetRandomElement(), 1.0f, "voiceover", 2);
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
    				tweensToKill.Add(letter.transform.DOPunchScale(Vector3.one * 0.05f, 0.5f).OnComplete(() => letter.ReadyForDrag = true));
    				CreateLineHelper(touch, letter.Collider.bounds.center, letter);


    				bool isAnimal = letter.transform.childCount == 0;
    				if (isAnimal)
                    {
    					string animalObjectName = spawnAnimal ? InfoUtils.GetAnimalSoundNameFromLetter(letter.Letter) :
    														InfoUtils.GetObjectSoundNameFromLetter(letter.Letter);
    					SoundManager.Instance.PlaySFX(animalObjectName, 1.0f, "voiceover", 1);
    				}
    				else
                    {
    					SoundManager.Instance.PlaySFX(char.ToLower(letter.Letter).ToString(), 1.0f, "letter", 1); // help aici a_from_a a_from_alligator
    				}
    				Debug.Log(letter);
				
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
    		SoundManager.Instance.CrossFadeMusic("MatchIconsWithLettersBgSound", 1.0f);
    		SpawnAnimalsAndLetters();
    		DOTween.Sequence()
    			.AppendInterval(1.0f)
    			.AppendCallback(() => SoundManager.Instance.PlaySFX("recognize", 1.0f, "voiceover", 2));
        }

        void SpawnAnimalsAndLetters()
        {
    		hintCooldown = MaxHintCooldown;
    		Utils.Shuffle(LetterPositions);

    		spawnAnimal = Random.value < 0.5f;
    		Sprite[] animalObject = spawnAnimal ? Animals : Objects;
    		Utils.Shuffle(animalObject);

    		bool small = Random.value < 0.5f;
            for (int i = 0; i < 3; i++)
            {
    			char letter = char.ToLower(SpawnAnimalObject(animalObject[i], AnimalsPositions[i]));
    			int idx = letter - 'a';
                SpawnBubble(small ? SmallLetters[idx] : BigLetters[idx], LetterPositions[i]);

    		}
        }

    	char SpawnAnimalObject(Sprite sprite, Transform targetTransform)
        {
    		float width = sprite.bounds.size.x;
    		float height = sprite.bounds.size.y;
    		float size = width > height ? width : height;
    		float wantedScale = AnimalMaxScale / size;

    		GameObject animObject = new GameObject(sprite.name);
    		SpriteRenderer sr = animObject.AddComponent<SpriteRenderer>();
    		sr.sprite = sprite;
    		CircleCollider2D collider = animObject.AddComponent<CircleCollider2D>();
    		collider.radius = 4.0f;
    		animObject.transform.position = targetTransform.position;
    		animObject.transform.localScale = Vector3.one * wantedScale;
    		LetterDefinition def = animObject.AddComponent<LetterDefinition>();
    		def.Letter = sprite.name.ToLower()[0];
    		def.Collider = collider;
    		def.ReadyForDrag = false;
    		def.transform.DOScale(Vector3.zero, 0.2f)
    			.From()
    			.SetEase(Ease.OutBack)
    			.SetDelay(Random.Range(0f, 0.3f))
    			.OnComplete(() => def.ReadyForDrag = true)
    			.OnPlay(() =>
    			{
    				SoundManager.Instance.PlaySFX("BubblePop");
    			});
    		SpawnedLetters.Add(def);
    		return def.Letter;
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
    			{
    				//string[] sfxx = new string[] { "good_job", "practice_great", "one_more", "continue_that_way" };
    				//SoundManager.Instance.PlaySFX(sfxx.GetRandomElement(), 1.0f, "voiceover", 10);
    				SpawnAnimalsAndLetters();
    			}
    			else
    			{
    				ShowWinUI();
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
    					PlayConnectSound(start, end);
    				}
    			}
            }
        }

    	void PlayConnectSound(LetterDefinition start, LetterDefinition end)
        {
    		/*
    		if (start.transform.childCount == 0)
            { // It's an animal or object, swap as we want start to be the letter and end the object/animal
    			LetterDefinition temp = start;
    			start = end;
    			end = temp;
            }
    		*/

    		SoundManager.Instance.AddSFXToQueue("connect_with");

    		SoundManager.Instance.AddSFXToQueue(char.ToLower(end.Letter).ToString());
    	}

    	public void ShowWinUI()
    	{
    #if UNITY_IOS
    		if (!ProgressManager.Instance.IsReviewShown(4))
    		{
    			Debug.Log("Asking for review!");
    			//UnityEngine.iOS.Device.RequestStoreReview();
    			ProgressManager.Instance.SetReviewShow(4);
    		}
    #endif

    		SoundManager.Instance.PlaySFX("FinishMiniGame_3");
    		string[] sfxx = new string[] { "good_job", "do_again", "did_great", "did_all", "play_again"};
    		DOTween.Sequence()
    					.AppendInterval(2.0f)
    					.AppendCallback(() => SoundManager.Instance.PlaySFX(sfxx.GetRandomElement(), 1.0f, "voiceover", 10));
    		DOTween.Sequence()
    			.AppendInterval(1.0f)
    			.AppendCallback(() => Fader.gameObject.SetActive(true))
    			.Append(Fader.DOFade(0.8f, 1.0f))
    			.AppendCallback(() => PopUp.SetActive(true))
    			.Append(PopUp.transform.DOScale(Vector3.zero, 0.4f).From().SetEase(Ease.OutBack));
    	}
    }


}