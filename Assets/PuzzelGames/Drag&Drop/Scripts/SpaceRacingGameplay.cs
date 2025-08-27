using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Interactive.DRagDrop
{
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

    public class SpaceRacingGameplay : MonoBehaviour
    {
    	public Collider2D Player;
    	public Transform ShipBooster;
    	public int Health = 3;
    	int LastHealth; // Used for animations
    	public float MaxInvicibilityTimer = 1.0f;
    	public float Speed = 5.0f;
    	public float HitSpeed = -10.0f;
    	public float RecoveryTimeFromHit = 0.2f; // Seconds
    	public float LineChangeSpeed = 1.0f;
    	public float LineHeight = 3.5f;

    	public GameObject[] Obstacles;
    	public GameObject[] Letters;
    	public GameObject[] Words;
    	public float WordsScale = 0.25f;
    	public string[] WordStrings;
    	public Transform[] Backgrounds;
    	public float MinBackgroundX = -30.0f;
    	public float LetterScaleMultiplier = 0.5f;
    	public Vector3 WordSpawnPosition = new Vector3(0f, 4.03f, 0f);

    	public Image[] ActiveStars;
    	public Image[] InactiveStars;

    	public RawImage Fader;
    	public GameObject PopUp;
    	public GameObject WinPanel;
    	public GameObject LosePanel;

    	[Header("Tutorial")]
    	public RawImage TutorialBackdrop;
    	public Image TutorialFocus;

    	public float ObstacleMinFrequency = 5f;
    	public float ObstacleMaxFrequency = 10f;
    	public float LetterMinFrequency = 10f;
    	public float LetterMaxFrequency = 20f;

    	float obstacleNextDistance;
    	float letterNextDistance;

    	List<Collider2D> SpawnedObstacles = new List<Collider2D>();
    	List<LetterDefinition> SpawnedLetters = new List<LetterDefinition>();
    	string wantedWord;
    	int wordPosition = 0;

    	float scrollSpeed;
    	float invicibilityTimer;
    	bool isDead = false;
    	bool isTutorial = false;
    	bool tutorialDamaged = false;
    	int tutorialStep = 0;
    	bool isPaused = false;
    	float distance = 0f;
    	int currentLine = 0;
    	Vector3 originalPosition;

    	List<SpriteRenderer> WordLetters = new List<SpriteRenderer>();

    	const float SPAWN_X = 12.0f;
    	const float KILL_X = -12.0f;

    	// Start is called before the first frame update
    	void Start()
    	{
    #if false && UNITY_EDITOR
    		isDead = true;
    		PerformTest();
    		return;
    #endif

    		SoundManager.Instance.CrossFadeMusic("SpaceshipGameBgMusic", 1.0f);

    		originalPosition = Player.transform.position;
    		obstacleNextDistance = Random.Range(ObstacleMinFrequency, ObstacleMaxFrequency);
    		letterNextDistance = Random.Range(LetterMinFrequency, LetterMaxFrequency);
    		int wordIdx = Random.Range(0, WordStrings.Length);
    		wantedWord = WordStrings[wordIdx];
    		wordPosition = 0;
    		scrollSpeed = Speed;

    		LastHealth = Health;
    		UpdateHealthGraphics();
    		CreateWord(wordIdx);

    		isTutorial = !ProgressManager.Instance.IsTutorialShown(0);
    		//isTutorial = true; Debug.LogError("REMOVE ME!");
    		if (isTutorial)
    		{
    			StartCoroutine(TutorialCoroutine());
    		}
    	}

    	// Update is called once per frame
    	void Update()
    	{
    		if (isDead)
    			return;
    		if (isPaused)
    			return;

    		// Check Distances
    		if (distance > obstacleNextDistance)
    		{
    			SpawnLetterOrObstacle();
    			if (isTutorial)
    			{
    				if (distance < 59.0f)
    					obstacleNextDistance = 60.0f;
    				else
    					obstacleNextDistance = distance + 25.0f;
    			}
    			else
    			{
    				obstacleNextDistance = distance + Random.Range(ObstacleMinFrequency, ObstacleMaxFrequency);
    			}
    		}

    		// Update positions
    		UpdatePlayer();

    		List<Collider2D> deleteObjects = new List<Collider2D>();
    		foreach (var obstacle in SpawnedObstacles)
    		{
    			obstacle.transform.position += Vector3.left * (scrollSpeed * Time.deltaTime);
    			if (obstacle.transform.position.x < KILL_X)
    			{
    				deleteObjects.Add(obstacle);
    				Destroy(obstacle.gameObject);
    			}
    		}

    		List<LetterDefinition> deleteLetters = new List<LetterDefinition>();
    		foreach (var letter in SpawnedLetters)
    		{
    			letter.transform.position += Vector3.left * (scrollSpeed * Time.deltaTime);
    			if (letter.transform.position.x < KILL_X)
    			{
    				deleteLetters.Add(letter);
    				Destroy(letter.gameObject);
    			}
    		}
	
    		distance += scrollSpeed * Time.deltaTime;

    		// Update Backgrounds
    		foreach (var background in Backgrounds)
    		{
    			background.position += Vector3.left * (scrollSpeed * Time.deltaTime);
    			if (background.position.x < MinBackgroundX)
    				background.position += Vector3.right * (36.395f * 2.0f);
    		}

    		// Check Collisions
    		if (invicibilityTimer > 0)
    		{
    			invicibilityTimer -= Time.deltaTime;
    		}
    		else
    		{
    			foreach (var obstacle in SpawnedObstacles)
    			{
    				if (Player.IsTouching(obstacle))
    				{ // Hit!
    					invicibilityTimer = MaxInvicibilityTimer;
    					Health--;
    					UpdateHealthGraphics();
    					tutorialDamaged = true;
    					//Debug.Log("AIAIAIAIAIAIiiiiii");
    					scrollSpeed = HitSpeed;
    					//velocity = Vector3.up * HitImpulse;
    					obstacle.enabled = false;
    					SoundManager.Instance.PlaySFX("RocketHitMiniGame");
    					if (Health == 0)
    					{
    						ShowDeathUI();
    						isDead = true;
    						return;
    					}
    					SpriteRenderer sr = Player.GetComponentInChildren<SpriteRenderer>();
    					//sr.DOFade(0f, MaxInvicibilityTimer).SetEase(Ease.Flash, 4.0f);
    					sr.DOColor(new Color(1.0f, 0f, 0f, 0.5f), MaxInvicibilityTimer).SetEase(Ease.Flash, 12.0f);
    					break;
    				}
    			}
    		}

    		foreach (var letter in SpawnedLetters)
    		{
    			if (Player.IsTouching(letter.Collider))
    			{ // Got Letter
    			  //Debug.Log("Ohh Mamaa");
    				SoundManager.Instance.PlaySFX("LetterMiniGameColect");
    				var letterRef = letter; // Used for timed references
    				int wordPosCopy = wordPosition;
    				SpriteRenderer sr = letter.Collider.GetComponent<SpriteRenderer>();
    				sr.sortingLayerName = "Foreground";
    				sr.sortingOrder = 5;
    				SoundManager.Instance.PlaySFX(char.ToLower(letterRef.Letter).ToString());
    				letter.transform.DOMove(WordLetters[wordPosition].transform.position, 1.0f);
    				letter.transform.DOScale(WordLetters[wordPosition].transform.localScale * WordsScale, 1.0f).OnComplete(() =>
    				{
    					Destroy(letterRef.gameObject);
    					WordLetters[wordPosCopy].color = Color.white;
    				});
    				wordPosition++;
    				if (wordPosition < wantedWord.Length && wantedWord[wordPosition] != letter.Letter)
    				{
    					// If we're touching one letter we need to remove all letters as we'll get new ones from the word
    					foreach (var let in SpawnedLetters)
    					{
    						if (let == letter)
    							continue;
    						Destroy(let.gameObject);
    					}
    					SpawnedLetters.Clear();
    					deleteLetters.Clear();
    				}
    				else
    				{
    					//Destroy(letter.gameObject);
    					deleteLetters.Add(letter);
    				}

    				if (wordPosition >= wantedWord.Length)
    				{
    					isDead = true;
    					ShowWinUI();
    					return;
                    }
    				break;
    			}
    		}

    		// Delayed removal
    		foreach (var del in deleteObjects)
    			SpawnedObstacles.Remove(del);
    		foreach (var del in deleteLetters)
    			SpawnedLetters.Remove(del);

    		scrollSpeed = Mathf.MoveTowards(scrollSpeed, Speed, Time.deltaTime / RecoveryTimeFromHit);
    	}

    	void SpawnLetterOrObstacle()
        {
    		if (distance > letterNextDistance)
    		{
    			SpawnLetter();
    			letterNextDistance = distance + Random.Range(LetterMinFrequency, LetterMaxFrequency);
    			if (isTutorial)
    				letterNextDistance = 1000.0f;
    		} else
            {
    			SpawnObstacle();
            }
    	}

    	void SpawnObstacle()
    	{	
    		GameObject go = Instantiate(Obstacles[Random.Range(0, Obstacles.Length)]);
    		go.SetActive(true);
    		float spawnY = originalPosition.y + (Random.Range(0, 3) - 1) * LineHeight;
    		if (isTutorial)
    		{
    			if (distance < 59.0f)
    				spawnY = originalPosition.y;
    			else
    				spawnY = originalPosition.y - LineHeight;
    		}
    		go.transform.position = new Vector3(SPAWN_X, spawnY, 0f);
    		SpawnedObstacles.Add(go.GetComponent<Collider2D>());
    	}

    	void SpawnLetter()
    	{
		
    		if (wordPosition >= wantedWord.Length)
    			return;

    		char wantedChar = wantedWord[wordPosition];
    		int idx = wantedChar - 'A';
    		GameObject go = Instantiate(Letters[idx]);
    		go.SetActive(true);
    		LetterDefinition letterDef = go.GetComponent<LetterDefinition>();
    		float yOffset = letterDef.Collider.transform.position.y - letterDef.Collider.bounds.center.y;
    		float spawnY = originalPosition.y + (Random.Range(0, 3) - 1) * LineHeight + yOffset * LetterScaleMultiplier;
    		if (isTutorial)
    			spawnY = originalPosition.y + -1 * LineHeight + yOffset * LetterScaleMultiplier;
    		go.transform.position = new Vector3(SPAWN_X, spawnY, 0f);
    		go.transform.localScale *= LetterScaleMultiplier;
    		letterDef.Collider.gameObject.AddComponent<SetSpriteColorTintOffset>();
    		SpawnedLetters.Add(letterDef);
    	}

    	Vector3 lastTouchPos;
    	const float TOUCH_THRESHOLD = 0.1f; // units

    	bool IsTutorialBlocked()
        {
    		if (isTutorial && !Utils.In(tutorialStep, 1, 3, 5, 7))
    			return true;
    		return false;
        }

    	void UpdatePlayerInput()
        {
    		if (!IsTutorialBlocked() && Input.touchCount > 0)
    		{
    			Touch touch = Input.GetTouch(0);
    			Vector3 worldPos = DragManager.GetWorldSpacePos(touch.position);
    			// Old Style
    			/*
    			if (touch.phase == TouchPhase.Began)
    			{
    				lastTouchPos = worldPos;
    			}
    			else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
    			{
    				Vector2 diff = worldPos - lastTouchPos;
    				if (diff.magnitude < TOUCH_THRESHOLD)
    				{ // Touch
    					float localDiffY = worldPos.y - originalPosition.y;
    					if (Mathf.Abs(localDiffY) < LineHeight * 0.5f)
    					{
    						currentLine = 0;
    					}
    					else if (localDiffY < 0)
    					{
    						if (tutorialStep == 7)
    							tutorialStep++;
    						currentLine = -1;
    					}
    					else
    					{
    						if (tutorialStep == 5)
    							tutorialStep++;
    						currentLine = 1;
    					}
    				}
    				else
    				{ // Slide 
    					if (diff.y < 0)
    					{
    						if (tutorialStep == 3)
    							tutorialStep++;
    						currentLine = Mathf.Max(-1, currentLine - 1);
    					}
    					else
    					{
    						if (tutorialStep == 1)
    							tutorialStep++;
    						currentLine = Mathf.Min(1, currentLine + 1);
    					}
    				}
    			}
    			*/

    			// New Style
    			float localDiffY = worldPos.y - originalPosition.y;
    			if (Mathf.Abs(localDiffY) < LineHeight * 0.5f)
    			{
    				if (tutorialStep == 3)
    					tutorialStep++;
    				currentLine = 0;
    			}
    			else if (localDiffY < 0)
    			{
    				if (tutorialStep == 7)
    					tutorialStep++;
    				currentLine = -1;
    			}
    			else
    			{
    				if (tutorialStep == 1 || tutorialStep == 5)
    					tutorialStep++;
    				currentLine = 1;
    			}
    		}
    	}

    	void UpdatePlayer()
    	{
    		int lastLine = currentLine;
    		// We need to detect both taps and slides
    		UpdatePlayerInput();

    		if (lastLine != currentLine)
    			SoundManager.Instance.PlaySFX("MoveRocketMiniGame1");

    		float targetY = originalPosition.y + currentLine * LineHeight;
    		Vector3 position = Player.transform.position;
    		position.y = Mathf.MoveTowards(position.y, targetY, LineChangeSpeed * Time.deltaTime);
    		Player.transform.position = position;

    		Quaternion oldRotation = Player.transform.rotation;

    		float yDiff = Mathf.Abs(targetY - position.y);
    		float sign = (targetY > position.y) ? 1.0f : -1.0f;
    		float maxRotation = 30.0f * sign;
    		if (yDiff < 1.0f) // Expose?
            {
    			float interpolation = yDiff / 1.0f;
    			Player.transform.rotation = Quaternion.Euler(0f, 0f, maxRotation * interpolation);
            } 
    		else
            {
    			Player.transform.rotation = Quaternion.RotateTowards(Player.transform.rotation, Quaternion.Euler(0f, 0f, maxRotation), Time.deltaTime * 360.0f);
            }

    		float angleDiff = Mathf.DeltaAngle(oldRotation.eulerAngles.z, Player.transform.rotation.eulerAngles.z);
    		//Debug.Log("Angles: " + oldRotation.eulerAngles.z + " : " + Player.transform.rotation.eulerAngles.z + " : " + angleDiff);
    		Vector3 boosterRotation = ShipBooster.localEulerAngles;
    		boosterRotation.z -= angleDiff;
    		boosterRotation.z += Mathf.DeltaAngle(boosterRotation.z, 0f) * 15.0f * Time.deltaTime;
    		ShipBooster.localRotation = Quaternion.Euler(boosterRotation);
    		//Debug.Log(boosterRotation.z);

    		/*
    		// Apply forces
    		Player.transform.position += velocity * Time.deltaTime;
    		velocity.y -= Gravity * Time.deltaTime;

    		// Check ground collision
    		if (Player.transform.position.y < FLOOR_HEIGHT)
    		{
    			Player.transform.position = Player.transform.position.SetY(FLOOR_HEIGHT);
    			velocity.y = 0f;
    			isJumping = false;
    		}

    		if (Input.touchCount > 0)
    		{
    			if (Input.GetTouch(0).phase == TouchPhase.Began)
    			{
    				jumpHold = true;
    			}
    			else if (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled)
    			{
    				jumpHold = false;
    			}
    		}
    		*/
    	}

    	void UpdateHealthGraphics()
    	{
    		Health = Mathf.Max(0, Health);
    		for (int i = 0; i < Health; i++)
    		{
    			ActiveStars[i].gameObject.SetActive(true);
    			InactiveStars[i].gameObject.SetActive(false);
    		}
    		for (int i = Health; i < ActiveStars.Length; i++)
    		{
    			InactiveStars[i].gameObject.SetActive(true);
    			//ActiveStars[i].gameObject.SetActive(false);
    		}

    		if (LastHealth != Health)
    		{
    			for (int i = Health; i < LastHealth; i++)
    			{
    				int idx = i;
    				Sequence s = DOTween.Sequence()
    					.Append(ActiveStars[idx].transform.DOMove(ActiveStars[idx].transform.position + Vector3.right * 50.0f + Vector3.up * 50.0f, 0.5f).SetEase(Ease.Linear))
    					.Append(ActiveStars[idx].transform.DOMove(ActiveStars[idx].transform.position + Vector3.right * 100.0f + Vector3.down * 2000.0f, 3.0f).SetEase(Ease.InSine))
    					.Insert(0f, ActiveStars[idx].transform.DORotate(new Vector3(0f, 0f, -360.0f * 3.0f), 3.0f, RotateMode.FastBeyond360).SetEase(Ease.InSine))
    					.OnComplete(() => ActiveStars[idx].gameObject.SetActive(false));
    			}
    		}

    		LastHealth = Health;
    	}

    	public void Restart()
    	{
    		SoundManager.Instance.PlaySFX("ClickButton");
    		SceneLoader.Instance.ReloadScene();
    	}

    	public void GoBack()
    	{
    		SoundManager.Instance.PlaySFX("ClickButton");
    		TransitionManager.Instance.ShowFade(2.0f, () => SceneLoader.Instance.LoadScene("MiniGames"));
    	}

    	public void ShowDeathUI()
    	{
    #if UNITY_IOS
    		if (!ProgressManager.Instance.IsReviewShown(0))
    		{
    			Debug.Log("Asking for review!");
    			UnityEngine.iOS.Device.RequestStoreReview();
    			ProgressManager.Instance.SetReviewShow(0);
    		}
    #endif

    		string[] sfx = new string[] { "fine_try_again", "try_again", "come_play_again" };
    		DOTween.Sequence()
    		.AppendInterval(1.5f)
    		.AppendCallback(() => SoundManager.Instance.PlaySFX(sfx.GetRandomElement()));

    		SoundManager.Instance.PlaySFX("GameOverMiniGames");
    		WinPanel.SetActive(false);
    		LosePanel.SetActive(true);
    		Fader.gameObject.SetActive(true);
    		DOTween.Sequence()
    			.Append(Fader.DOFade(0.8f, 1.0f))
    			.AppendCallback(() => PopUp.SetActive(true))
    			.Append(PopUp.transform.DOScale(Vector3.zero, 0.4f).From().SetEase(Ease.OutBack));
    	}

    	public void ShowWinUI()
    	{
    #if UNITY_IOS
    		if (!ProgressManager.Instance.IsReviewShown(0))
    		{
    			Debug.Log("Asking for review!");
    			UnityEngine.iOS.Device.RequestStoreReview();
    			ProgressManager.Instance.SetReviewShow(0);
    		}
    #endif
    		SoundManager.Instance.SetMusicVolume(1.0f, 0.1f);
    		SoundManager.Instance.AddSFXToQueue("FinishMiniGame_1");
    		WinPanel.SetActive(true);
    		LosePanel.SetActive(false);
    		Fader.gameObject.SetActive(true);
    		DOTween.Sequence()
    			.Append(Fader.DOFade(0.8f, 1.0f))
    			.AppendCallback(() => PopUp.SetActive(true))
    			.Append(PopUp.transform.DOScale(Vector3.zero, 0.4f).From().SetEase(Ease.OutBack));

    		string[] sfx = new string[] { "good_job", "do_again", "did_great", "did_all", "play_again" };

    		//DOTween.Sequence()
    		//	.AppendInterval(2.0f)
    		//	.AppendCallback(() =>
    		//	{
    				SoundManager.Instance.AddSFXToQueue(wantedWord.ToLower());
    				SoundManager.Instance.AddSFXToQueue(sfx.GetRandomElement());
    		//	});
    	}

    	void CreateWord(int idx)
    	{
    		GameObject word = Instantiate(Words[idx]);
    		word.transform.position = WordSpawnPosition;
    		word.transform.localScale *= WordsScale;
    		Transform letterRoot = word.transform.GetChild(0);
    		for (int i = 0; i < letterRoot.childCount; i++)
    		{
    			SpriteRenderer sr = letterRoot.GetChild(i).GetComponent<SpriteRenderer>();
    			sr.color = new Color(0f, 0f, 0f, 0.5f);
    			WordLetters.Add(sr);
    		}
    	}

    	IEnumerator TutorialCoroutine()
    	{
    		tutorialStep = 0;
    		obstacleNextDistance = 40.0f;
    		letterNextDistance = 60.0f;

    		// Show Controls Tutorial
    		yield return new WaitUntil(() => distance > 15.0f);
    		// Show Up
    		isPaused = true;
    		float hintCooldown = -1.0f;
    		tutorialStep = 1;
    		SoundManager.Instance.AddSFXToQueue("direct_up_down");
    		while (tutorialStep == 1)
    		{
    			hintCooldown -= Time.deltaTime;
    			if (hintCooldown < 0)
    			{
    				//FingerHintController.Instance.ShowDownSlideHint(Vector3.zero); // For Up movement!
    				FingerHintController.Instance.ShowTapHint(Vector3.up * LineHeight);
    				hintCooldown = 5.0f;
    			}
    			UpdatePlayerInput();
    			yield return null;
    		}
    		FingerHintController.Instance.Hide();
    		isPaused = false;

    		yield return new WaitUntil(() => distance > 25.0f);
    		// Show Down
    		isPaused = true;
    		hintCooldown = -1.0f;
    		tutorialStep = 3;
    		while (tutorialStep == 3)
    		{
    			hintCooldown -= Time.deltaTime;
    			if (hintCooldown < 0)
    			{
    				//FingerHintController.Instance.ShowUpSlideHint(Vector3.zero); // For Down movement!
    				FingerHintController.Instance.ShowTapHint(Vector3.zero);
    				hintCooldown = 5.0f;
    			}
    			UpdatePlayerInput();
    			yield return null;
    		}
    		FingerHintController.Instance.Hide();
    		isPaused = false;

    		// Show Obstacle Tutorial
    		yield return new WaitUntil(() => distance > 52.0f);
    		isPaused = true;
    		TutorialFocus.gameObject.SetActive(true);
    		TutorialFocus.transform.position = Camera.main.WorldToScreenPoint(SpawnedObstacles[0].transform.position);
    		TutorialBackdrop.gameObject.SetActive(true);
    		float initialAlpha = TutorialBackdrop.color.a;
    		TutorialBackdrop.color = Color.clear;
    		SetSpriteColorTintOffset spriteOffset = SpawnedObstacles[0].GetComponent<SetSpriteColorTintOffset>();

    		SoundManager.Instance.AddSFXToQueue("avoid_rocks"); 

    		Sequence fadeSequence = DOTween.Sequence()
    			.Append(TutorialBackdrop.DOFade(initialAlpha, 2.0f))
    			.Join(DOTween.To(spriteOffset.GetOffsetAlpha, spriteOffset.SetOffsetAlpha, 0.75f, 2.0f).SetEase(Ease.Flash, 8.0f))
    			.AppendInterval(0.5f)
    			.Append(TutorialBackdrop.DOFade(0f, 1.0f));
    		yield return new WaitUntil(() => !fadeSequence.IsActive() || Input.touchCount > 0);

    		if (fadeSequence.IsActive())
    			fadeSequence.Kill(true);

    		//yield return new WaitUntil(() => distance > 52.0f);
    		// Show Tap Up
    		isPaused = true;
    		hintCooldown = -1.0f;
    		tutorialStep = 5;
    		while (tutorialStep == 5)
    		{
    			hintCooldown -= Time.deltaTime;
    			if (hintCooldown < 0)
    			{
    				FingerHintController.Instance.ShowTapHint(Vector3.up * LineHeight);
    				hintCooldown = 5.0f;
    			}
    			UpdatePlayerInput();
    			yield return null;
    		}
    		FingerHintController.Instance.Hide();
    		isPaused = false;

    		// Show Letter Tutorial
    		yield return new WaitUntil(() => distance > 72.0f);
    		isPaused = true;
    		TutorialFocus.transform.position = Camera.main.WorldToScreenPoint(SpawnedLetters[0].transform.position + Vector3.up * 1.0f);
    		TutorialBackdrop.color = Color.clear;
    		spriteOffset = SpawnedLetters[0].Collider.GetComponent<SetSpriteColorTintOffset>();

    		SoundManager.Instance.AddSFXToQueue("collect_to_form");

    		fadeSequence = DOTween.Sequence()
    			.Append(TutorialBackdrop.DOFade(initialAlpha, 2.0f))
    			.Join(DOTween.To(spriteOffset.GetOffsetAlpha, spriteOffset.SetOffsetAlpha, 0.75f, 2.0f).SetEase(Ease.Flash, 8.0f))
    			.Append(TutorialBackdrop.DOFade(0f, 1.0f));
    		yield return new WaitUntil(() => !fadeSequence.IsActive() || Input.touchCount > 0);

    		if (fadeSequence.IsActive())
    			fadeSequence.Kill(true);

    		TutorialFocus.transform.position = Camera.main.WorldToScreenPoint(WordLetters[0].transform.position + Vector3.up * 0.75f);
    		TutorialFocus.transform.localScale *= 0.5f;
    		TutorialBackdrop.color = Color.clear;

    		fadeSequence = DOTween.Sequence()
    			.Append(TutorialBackdrop.DOFade(initialAlpha, 2.0f))
    			.Join(WordLetters[0].DOFade(0f, 2.0f).SetEase(Ease.Flash, 8.0f))
    			.AppendInterval(0.5f)
    			.Append(TutorialBackdrop.DOFade(0f, 1.0f));
    		yield return new WaitUntil(() => !fadeSequence.IsActive() || Input.touchCount > 0);

    		if (fadeSequence.IsActive())
    			fadeSequence.Kill(true);

    		//yield return new WaitUntil(() => distance > 72.0f);
    		// Show Tap Down
    		isPaused = true;
    		hintCooldown = -1.0f;
    		tutorialStep = 7;
    		while (tutorialStep == 7)
    		{
    			hintCooldown -= Time.deltaTime;
    			if (hintCooldown < 0)
    			{
    				FingerHintController.Instance.ShowTapHint(Vector3.down * LineHeight);
    				hintCooldown = 5.0f;
    			}
    			UpdatePlayerInput();
    			yield return null;
    		}
    		FingerHintController.Instance.Hide();
    		isPaused = false;

    		tutorialStep = 10;

    		// Show Hit
    		yield return new WaitUntil(() => tutorialDamaged);
    		yield return new WaitForSeconds(0.5f);

    		isPaused = true;
    		TutorialFocus.transform.position = ActiveStars[1].transform.position;
    		TutorialFocus.transform.localScale *= 2.0f;
    		TutorialBackdrop.color = Color.clear;

    		SoundManager.Instance.AddSFXToQueue("save_avoid");

    		fadeSequence = DOTween.Sequence()
    			.Append(TutorialBackdrop.DOFade(initialAlpha, 2.0f))
    			.AppendInterval(0.5f)
    			.Append(TutorialBackdrop.DOFade(0f, 1.0f));
    		yield return new WaitUntil(() => !fadeSequence.IsActive() || Input.touchCount > 0);

    		if (fadeSequence.IsActive())
    			fadeSequence.Kill(true);

    		isPaused = false;

    		TutorialFocus.gameObject.SetActive(false);
    		TutorialBackdrop.gameObject.SetActive(false);

    		isTutorial = false;
    		ProgressManager.Instance.SetTutorialShown(0);

    		TransitionManager.Instance.ShowFade(2.0f, () => Restart());
    	}

    #if UNITY_EDITOR
    	void PerformTest()
        {
    		for (int i = 0; i < WordStrings.Length; i++)
    			SoundManager.Instance.AddSFXToQueue(WordStrings[i].ToLower());
    	}
    #endif
    }


}