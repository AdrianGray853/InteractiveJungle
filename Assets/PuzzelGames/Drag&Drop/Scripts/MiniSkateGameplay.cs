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

    public class MiniSkateGameplay : MonoBehaviour
    {
    	public Collider2D Player;
    	public Animator PlayerAnimator;
    	public int Health = 3;
    	int LastHealth; // Used for animations
    	public float MaxInvicibilityTimer = 1.0f;
    	public float RunSpeed = 5.0f;
    	public float JumpSpeed = 10.0f;
    	public float HitSpeed = -10.0f;
    	public float HitImpulse = 10.0f;
    	public float RunRecoveryTimeFromHit = 0.2f; // Seconds
    	public float RunRecoverTimeFromJump = 1.0f; // Seconds
    	public float JumpImpulse = 100.0f;
    	public float Gravity = 9.8f;

    	public float LetterScaleMultiplier = 0.5f;
	
    	public float ObstacleMinFrequency = 0.5f;
    	public float ObstacleMaxFrequency = 1.0f;
    	public float LetterMinFrequency = 3.0f;
    	public float LetterMaxFrequency = 5.0f;

    	public GameObject[] Obstacles;
    	public GameObject[] Letters; // A-Z
    	public GameObject[] Words;
    	public float WordsScale = 0.75f;
    	public string[] WordStrings;
    	public Transform[] Grounds;
    	public float MinGroundX = -2.0f;
    	public Transform[] Backgrounds;
    	public float MinBackgroundX = -30.0f;
    	public float BackgroundParallax = 0.5f;

    	public Image[] ActiveStars;
    	public Image[] InactiveStars;

    	public RawImage Fader;
    	public GameObject PopUp;
    	public GameObject WinPanel;
    	public GameObject LosePanel;

    	[Header("Tutorial")]
    	public RawImage TutorialBackdrop;
    	public Image    TutorialFocus;

    	float obstacleNextDistance;
    	float letterNextDistance;

    	List<Collider2D> SpawnedObstacles = new List<Collider2D>();
    	List<LetterDefinition> SpawnedLetters = new List<LetterDefinition>();
    	string wantedWord;
    	int wordPosition = 0;

    	bool isJumping = false;
    	Vector3 velocity = Vector3.zero;
    	float scrollSpeed;
    	bool jumpHold = false;
    	float invicibilityTimer;
    	bool isDead = false;
    	bool isTutorial = false;
    	bool tutorialDamaged = false;
    	bool isPaused = false;
    	float distance = 0f;

    	const float FLOOR_HEIGHT = -2.69f;
    	const float LETTER_HEIGHT = 2.0f;
    	const float SPAWN_X = 12.0f;
    	const float KILL_X = -12.0f;

    	List<SpriteRenderer> WordLetters = new List<SpriteRenderer>();

    	// Start is called before the first frame update
    	void Start()
    	{
    		SoundManager.Instance.CrossFadeMusic("SkateMiniGameBgSound", 1.0f);

    		obstacleNextDistance = Random.Range(ObstacleMinFrequency, ObstacleMaxFrequency);
    		letterNextDistance = Random.Range(LetterMinFrequency, LetterMaxFrequency);
    		int wordIdx = Random.Range(0, WordStrings.Length);
    		wantedWord = WordStrings[wordIdx];
    		wordPosition = 0;
    		scrollSpeed = RunSpeed;

    		LastHealth = Health;
    		UpdateHealthGraphics();
    		CreateWord(wordIdx);

    		isTutorial = !ProgressManager.Instance.IsTutorialShown(1);
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
    			SpawnObstacle();
    			if (isTutorial)
    			{
    				if (distance < 40.0f)
    					obstacleNextDistance = 60.0f;
    				else
    					obstacleNextDistance = 100.0f;
    			}
    			else
    			{
    				obstacleNextDistance = distance + Random.Range(ObstacleMinFrequency, ObstacleMaxFrequency);
    			}

    		}

    		if (distance > letterNextDistance)
    		{
    			SpawnLetter();
    			letterNextDistance = distance + Random.Range(LetterMinFrequency, LetterMaxFrequency);
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

    		// Update Ground
    		foreach (var ground in Grounds)
    		{
    			ground.position += Vector3.left * (scrollSpeed * Time.deltaTime);
    			if (ground.position.x < MinGroundX)
    				ground.position += Vector3.right * (ground.GetComponent<SpriteRenderer>().size.x * ground.localScale.x * 2.0f);
    		}

    		distance += scrollSpeed * Time.deltaTime;

    		// Update Backgrounds
    		foreach (var background in Backgrounds)
    		{
    			background.position += Vector3.left * (scrollSpeed * Time.deltaTime * BackgroundParallax);
    			if (background.position.x < MinBackgroundX)
    				background.position += Vector3.right * (27.8f * 2.0f);
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
    					tutorialDamaged = true;
    					SoundManager.Instance.PlaySFX("HitSound");
    					UpdateHealthGraphics();
    					//Debug.Log("AIAIAIAIAIAIiiiiii");
    					scrollSpeed = HitSpeed;
    					velocity = Vector3.up * HitImpulse;
    					obstacle.enabled = false;

    					if (Health == 0)
    					{
    						ShowDeathUI();

    						isDead = true;
    						PlayerAnimator.Play("Lose");
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

    		if (invicibilityTimer > 0)
    			scrollSpeed = Mathf.MoveTowards(scrollSpeed, RunSpeed, Time.deltaTime / RunRecoveryTimeFromHit); 
    		else
    			scrollSpeed = Mathf.MoveTowards(scrollSpeed, RunSpeed, Time.deltaTime / RunRecoverTimeFromJump);


    		if (isJumping)
    		{
    			if (velocity.magnitude < 10.0f)
    			{
    				PlayerAnimator.Play("InAir");
    			} 
    			else if (velocity.y > 0)
    			{
    				PlayerAnimator.Play("JumpUp");
    			}
    			else
    			{
    				PlayerAnimator.Play("JumpDown");
    			}
    		} 
    		else
    		{
    			if (invicibilityTimer > 0)
    				PlayerAnimator.Play("Hit");
    			else
    				PlayerAnimator.Play("Idle");
    		}
    	}

    	void SpawnObstacle()
    	{
    		GameObject go = Instantiate(Obstacles[Random.Range(0, Obstacles.Length)]);
    		go.SetActive(true);
    		go.transform.position = new Vector3(SPAWN_X, FLOOR_HEIGHT, 0f);
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
    		go.transform.position = new Vector3(SPAWN_X, LETTER_HEIGHT, 0f);
    		go.transform.localScale *= LetterScaleMultiplier;
    		LetterDefinition letterDef = go.GetComponent<LetterDefinition>();
    		letterDef.Collider.gameObject.AddComponent<SetSpriteColorTintOffset>();
    		SpawnedLetters.Add(letterDef);
    	}

    	void UpdatePlayer()
    	{
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

    		if (!isTutorial && Input.touchCount > 0)
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

    		if (jumpHold && !isJumping)
    		{
    			// Apply impulse and Jump!
    			velocity = Vector3.up * JumpImpulse;
    			Debug.Log("Jumping " + velocity.ToString() + Time.frameCount);
    			isJumping = true;
    			scrollSpeed = JumpSpeed;
    			jumpHold = false;
    			SoundManager.Instance.PlaySFX("JumpCatMiniGame");
    		}
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
    					.Append(ActiveStars[idx].transform.DOMove(ActiveStars[idx].transform.position + Vector3.left * 50.0f + Vector3.up * 50.0f, 0.5f).SetEase(Ease.Linear))
    					.Append(ActiveStars[idx].transform.DOMove(ActiveStars[idx].transform.position + Vector3.left * 100.0f + Vector3.down * 2000.0f, 3.0f).SetEase(Ease.InSine))
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
    		if (!ProgressManager.Instance.IsReviewShown(1))
    		{
    			Debug.Log("Asking for review!");
    			//UnityEngine.iOS.Device.RequestStoreReview();
    			ProgressManager.Instance.SetReviewShow(1);
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
    		if (!ProgressManager.Instance.IsReviewShown(1))
    		{
    			Debug.Log("Asking for review!");
    			//UnityEngine.iOS.Device.RequestStoreReview();
    			ProgressManager.Instance.SetReviewShow(1);
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

    		SoundManager.Instance.AddSFXToQueue(wantedWord.ToLower());
    		string[] sfx = new string[] { "so_fast", "professional_rider", "good_job", "doing_great", "feel_rhythm" };
    		SoundManager.Instance.AddSFXToQueue(sfx.GetRandomElement());
    	}

    	void CreateWord(int idx)
    	{
    		GameObject word = Instantiate(Words[idx]);
    		word.transform.position = new Vector3(0f, -4.08f, 0f);
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
    		obstacleNextDistance = 20.0f;
    		letterNextDistance = 40.0f;
    		bool gotInput = false;

    		SoundManager.Instance.AddSFXToQueue("skateride_collect");

    		// Show Obstacle Tutorial
    		yield return new WaitUntil(() => distance > 32.0f);
    		isPaused = true;
    		TutorialFocus.gameObject.SetActive(true);
    		TutorialFocus.transform.position = Camera.main.WorldToScreenPoint(SpawnedObstacles[0].transform.position + Vector3.up * 1.0f);
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

    		float hintCooldown = -1.0f;
    		while (!gotInput)
    		{
    			hintCooldown -= Time.deltaTime;
    			if (hintCooldown < 0)
    			{
    				FingerHintController.Instance.ShowTapHint(Vector3.zero);
    				DOTween.Sequence()
    					.AppendInterval(1.0f)
    					.AppendCallback(() =>
    					{
    						SoundManager.Instance.PlaySFX("jump");
    					});
    				hintCooldown = 5.0f;
    			}
    			//yield return new WaitForSeconds(2.0f);
    			gotInput = (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
    			yield return null;
    		}
    		FingerHintController.Instance.Hide();

    		isPaused = false;
    		jumpHold = true;

    		// Show Letter Tutorial
    		yield return new WaitUntil(() => distance > 52.0f);
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

    		SoundManager.Instance.AddSFXToQueue("jump_collect"); 

    		fadeSequence = DOTween.Sequence()
    			.Append(TutorialBackdrop.DOFade(initialAlpha, 2.0f))
    			.Join(WordLetters[0].DOFade(0f, 2.0f).SetEase(Ease.Flash, 8.0f))
    			.AppendInterval(0.5f)
    			.Append(TutorialBackdrop.DOFade(0f, 1.0f));
    		yield return new WaitUntil(() => !fadeSequence.IsActive() || Input.touchCount > 0);

    		if (fadeSequence.IsActive())
    			fadeSequence.Kill(true);

    		gotInput = false;
    		hintCooldown = -1.0f;
    		while (!gotInput)
    		{
    			hintCooldown -= Time.deltaTime;
    			if (hintCooldown < 0)
    			{
    				FingerHintController.Instance.ShowTapHint(Vector3.zero);
    				DOTween.Sequence()
    					.AppendInterval(1.0f)
    					.AppendCallback(() =>
    					{
    						SoundManager.Instance.AddSFXToQueue("jump");
    					});
    				hintCooldown = 5.0f;
    			}
    			//yield return new WaitForSeconds(2.0f);
    			gotInput = (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
    			yield return null;
    		}
    		FingerHintController.Instance.Hide();
    		isPaused = false;
    		jumpHold = true;

    		// Show Hit
    		yield return new WaitUntil(() => tutorialDamaged);
    		yield return new WaitForSeconds(0.5f);

    		isPaused = true;
    		TutorialFocus.transform.position = ActiveStars[1].transform.position;
    		TutorialFocus.transform.localScale *= 2.0f;
    		TutorialBackdrop.color = Color.clear;

    		SoundManager.Instance.AddSFXToQueue("try_avoid_rocks"); 

    		fadeSequence = DOTween.Sequence()
    			.Append(TutorialBackdrop.DOFade(initialAlpha, 2.0f))
    			.Append(TutorialBackdrop.DOFade(0f, 1.0f));
    		yield return new WaitUntil(() => !fadeSequence.IsActive() || Input.touchCount > 0);

    		if (fadeSequence.IsActive())
    			fadeSequence.Kill(true);

    		//isPaused = false;
    		yield return new WaitForSeconds(3.0f);

    		TutorialFocus.gameObject.SetActive(false);
    		TutorialBackdrop.gameObject.SetActive(false);

    		isTutorial = false;
    		ProgressManager.Instance.SetTutorialShown(1);

    		TransitionManager.Instance.ShowFade(2.0f, () => Restart());
    	}
    }


}