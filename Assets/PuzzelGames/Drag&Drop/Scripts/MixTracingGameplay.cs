using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

namespace Interactive.DRagDrop
{
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

    public class MixTracingGameplay : MonoBehaviour
    {
    	public float Separation = 1.0f;
    	public float Scale = 1.0f;

    	public GameObject[] BigLetters;
    	public GameObject[] SmallLetters;

    	public RawImage Fader;
    	public GameObject PopUp;

    	int letterCount = 0;
    	int currentIdx = 0;

    	int[] indices;

    	int maxLevels = 10;
    	int currentLevel = 0;

    	TracingManager smallManager;
    	TracingManager bigManager;

    	// Start is called before the first frame update
    	void Start()
    	{
    		GameProgressController.Instance.SetMaxProgressSteps(maxLevels);
    		SoundManager.Instance.CrossFadeMusic("TracingModuleBgMusic", 1.0f);

    		indices = Enumerable.Range(0, BigLetters.Length).ToArray();
    		Utils.Shuffle(indices);
    		SpawnLetters(true);

    		SoundManager.Instance.AddSFXToQueue("learn_write_trace", 1.0f, "voiceover", 2);
    		SoundManager.Instance.AddSFXToQueue("start_uppercase", 1.0f, "voiceover", 2);
    		SoundManager.Instance.AddSFXToQueue(char.ToLower(bigManager.Letter).ToString(), 1.0f, "voiceover", 2);
    	}

    	void SpawnLetters(bool firstTime)
    	{
    		letterCount = 0;

    		int idx = indices[currentIdx++];
    		if (currentIdx >= indices.Length)
    		{
    			Utils.Shuffle(indices);
    			currentIdx = 0;
    		}
    		GameObject bigLetter = Instantiate(BigLetters[idx]);
    		GameObject smallLetter = Instantiate(SmallLetters[idx]);

    		Vector3 screenCenter = Camera.main.transform.position;
    		screenCenter.x += 0.75f;
    		screenCenter.z = 0f;

    		bigLetter.transform.localScale *= Scale;
    		smallLetter.transform.localScale *= Scale * 0.8f;

    		SpriteRenderer sr = bigLetter.GetComponentInChildren<SpriteRenderer>();
    		float bigOffset = Mathf.Abs(sr.bounds.max.x - bigLetter.transform.position.x);
    		float bigBottom = Mathf.Abs(sr.bounds.min.y - bigLetter.transform.position.y);
    		sr = smallLetter.GetComponentInChildren<SpriteRenderer>();
    		float smallOffset = Mathf.Abs(sr.bounds.max.x - smallLetter.transform.position.x);
    		float smallBottom = Mathf.Abs(sr.bounds.min.y - smallLetter.transform.position.y);

    		bigLetter.transform.position = screenCenter + Vector3.left * (bigOffset + Separation * 0.5f);
    		smallLetter.transform.position = screenCenter + Vector3.right * (smallOffset + Separation * 0.5f) + Vector3.down * (bigBottom - smallBottom);

    		bigManager = bigLetter.GetComponentInChildren<TracingManager>();
    		bigManager.DoneCallback += ActivateNextLetter;
    		bigManager.FinishCallback += CheckForNextLevel;
    		smallManager = smallLetter.GetComponentInChildren<TracingManager>();
    		smallManager.DoneCallback += () =>
    		{
    			string[] sfx = new string[] { "awesome", "good_job", "bravo", "amazing" };
    			SoundManager.Instance.AddSFXToQueue(sfx.GetRandomElement());
    			bigManager.DoFinishAnimation();
    		};
    		smallManager.FinishCallback += CheckForNextLevel;

    		bigLetter.transform.DOMove(bigLetter.transform.position + Vector3.up * 20.0f, 0.5f).From().SetEase(Ease.OutBack).OnComplete(() => bigManager.Init(false));
    		smallLetter.transform.DOMove(smallLetter.transform.position + Vector3.up * 20.0f, 0.5f).From().SetEase(Ease.OutBack);//.OnComplete(smallManager.Init);


    		if (!firstTime)
            {
    			SoundManager.Instance.PlaySFXStack("startTrace_uppercase", 1.0f, "voiceover", 2)
    				.AddSFXStack(char.ToLower(bigManager.Letter).ToString());
    		}

    		//bigManager.Init();
    		//smallManager.Init();
    	}

    	void ActivateNextLetter()
    	{
    		int idx = indices[currentIdx++];
    		if (currentIdx >= indices.Length)
    		{
    			Utils.Shuffle(indices);
    			currentIdx = 0;
    		}
    		smallManager.Init();
    		//CheckForNextLevel();

    		SoundManager.Instance.AddSFXToQueue("now_lowercase_1", 1.0f, "voiceover", 2);
    		SoundManager.Instance.AddSFXToQueue(char.ToLower(smallManager.Letter).ToString(), 1.0f, "voiceover", 2);
    	}

    	void CheckForNextLevel()
    	{
    		letterCount++;
    		if (letterCount == 2)
    		{
    			currentLevel++;
    			GameProgressController.Instance.AddProgressSteps();
    			if (currentLevel < maxLevels)
    			{
    				SpawnLetters(false);
    			}
    			else
    			{
    				ShowWinUI();
    			}
    		}
    	}

    	public void ShowWinUI()
    	{
    #if UNITY_IOS
    		if (!ProgressManager.Instance.IsReviewShown(8))
    		{
    			Debug.Log("Asking for review!");
    			//UnityEngine.iOS.Device.RequestStoreReview();
    			ProgressManager.Instance.SetReviewShow(8);
    		}
    #endif

    		SoundManager.Instance.PlaySFX("FinishMiniGame_3");
    		string[] sfx = new string[] { "good_job", "did_great", "did_all" };
    		string[] playAgainSFX = new string[] { "play_again", "do_again" };
    		SoundManager.Instance.AddSFXToQueue(sfx.GetRandomElement(), 1.0f, "voiceover", 3);
    		SoundManager.Instance.AddSFXToQueue(playAgainSFX.GetRandomElement(), 1.0f, "voiceover", 3);
    		DOTween.Sequence()
    			.AppendInterval(1.0f)
    			.AppendCallback(() => Fader.gameObject.SetActive(true))
    			.Append(Fader.DOFade(0.8f, 1.0f))
    			.AppendCallback(() => PopUp.SetActive(true))
    			.Append(PopUp.transform.DOScale(Vector3.zero, 0.4f).From().SetEase(Ease.OutBack));
    	}
    }


}