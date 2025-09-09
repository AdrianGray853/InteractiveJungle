using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Interactive.DRagDrop
{
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Timeline;

    public class GameManager : MonoBehaviour
    {
    	public static GameManager Instance { get; private set; }

    	public float WordLeftMargin = 1.0f;
    	public float WordTopMargin = 1.0f;

    	public PlayableDirector IntroTimeline;
    	public Transform LetterRefTransform;
    	public Transform AnimalRefTransform;

    	public PlayableDirector OutroTimeline;
    	public Transform OutLetterRefTransform;
    	public Transform OutAnimalRefTransform;

    	public SelectLetterGameplay SelectLetter;

    	public GameObject[] Letters;
    	public GameObject[] Texts;
    	public GameObject[] Animals;
    	public GameObject[] Tracings;

    	public Transform IntroAndWordLevel;
    	public Transform SelectLetterLevel;
    	public Transform TracingLevel;
    	public Transform OutroLevel;

    	public GameObject[] Rewards;

    	// FOR DEBUG! DELETE
    	[Header("Testing!")]
    	public bool TESTING = false;
    	public bool TEST_OUTRO = false;
    	public int TEST_LEVEL = 0;
    	public GameData.eGameType TEST_TYPE = GameData.eGameType.UpperCase;

    	int levelIndex;

    	int[] upperSecondaryAnimLevels =
    		{
    			0, // Alligator
    			23, // X-Ray Fish
    			4, //Elephant
    			9, //Jellyfish
            };

    	int[] lowerSecondaryAnimLevels =
    		{
    			0, // astronaut
    			4, //envelope
    			11, //ladybug
    			12,  //mushroom
    			13, //notebook
    			17, //rainbow
    			20, //umbrella
    			21, //vulcano
    			22, //watermellon
    			23, //xylophone
    			25, //zepelin
            };

    	private void Awake()
    	{
    		Instance = this;
    	}

    	// Start is called before the first frame update
    	void Start()
    	{
    		if (GameData.Instance.GameType == GameData.eGameType.UpperCase)
    			SoundManager.Instance.CrossFadeMusic("LevelFirstPlanet", 0.5f);
    		else
    			SoundManager.Instance.CrossFadeMusic("LevelSecondPlanet", 0.5f);
    		// Reset bubble explossion levels so they don't always explode when you enter the game
    		ProgressManager.Instance.LastUnlockedLevelBigLetter = -1;
    		ProgressManager.Instance.LastUnlockedLevelSmallLetter = -1;
    		LoadLevels();
    	}

    	void LoadLevels()
    	{
    		levelIndex = GameData.Instance.SelectedLevel;
    #if UNITY_EDITOR
    		if (TESTING)
    		{
    			GameData.Instance.SelectedLevel = levelIndex = TEST_LEVEL;
    			GameData.Instance.GameType = TEST_TYPE;
    		}
    #endif
    		SetupIntroTimeline();
    		SetupOutroTimeline();
    #if UNITY_EDITOR
    		if (TEST_OUTRO)
    			ShowOutro(); // debug
    #endif
    	}

    	void Process(Playable playable, int muteIdx, int depth = 0)
    	{
    		if (playable.GetHandle() == PlayableHandle.Null)
    			return;
		
    		if (playable.GetPlayableType() == typeof(AnimationLayerMixerPlayable) && playable.GetInputCount() > 1)
    		{
    			AnimationLayerMixerPlayable mixer = (AnimationLayerMixerPlayable)playable;
    			//Debug.Log("Found Mixer!" + mixer.GetInputCount());
    			//Debug.Log(mixer.CanSetWeights());
    			mixer.GetInput(muteIdx).GetInput(0).SetInputWeight(0, 0f);

    			return;
    		}

    		//Debug.Log(new string('>', depth + 1) + playable.GetPlayableType().ToString());
    		for (int i = 0; i < playable.GetInputCount(); i++)
    		{
    			Process(playable.GetInput(i), muteIdx, depth + 1);
    		}
    	}

    	void SetupIntroTimeline()
        {
    		Object track = IntroTimeline.playableAsset.outputs.Where(x => x.streamName == "LetterAnimTrack").First().sourceObject;
    		if (track != null)
    		{
    			GameObject go = Instantiate(Letters[levelIndex], IntroAndWordLevel);
    			go.transform.localPosition += LetterRefTransform.localPosition;
    			Animator animator = go.transform.GetChild(0).GetComponent<Animator>();

    			IntroTimeline.SetGenericBinding(track, animator);
    		}


    		track = IntroTimeline.playableAsset.outputs.Where(x => x.streamName == "AnimalAnimTrack").First().sourceObject;
    		if (track != null)
    		{
    			GameObject go = Instantiate(Animals[levelIndex], IntroAndWordLevel);
    			go.transform.localPosition += AnimalRefTransform.localPosition;
    			Animator animator = go.transform.GetChild(0).GetComponent<Animator>();

    			IntroTimeline.SetGenericBinding(track, animator);
    		}

    		bool muteOverride;
    		if (GameData.Instance.GameType == GameData.eGameType.UpperCase)
    			muteOverride = !upperSecondaryAnimLevels.Contains(levelIndex);
    		else
    			muteOverride = !lowerSecondaryAnimLevels.Contains(levelIndex);
    		/*
    		// First Approach, Changes the asset!
            {
    			track = IntroTimeline.playableAsset.outputs.Where(x => x.streamName == "AnimalAnimTrack").First().sourceObject;
    			AnimationTrack animTrack = track as AnimationTrack;
    			if (animTrack != null)
                {
    				foreach (TrackAsset childTrack in animTrack.GetChildTracks())
    				{
    					Debug.Log(childTrack.name);
    					childTrack.muted = muteOverride;
    					IntroTimeline.RebuildGraph();
    				}
    			}
    		}
    		*/

    		// Second approach, runtime using graph
    		for (int i = 0; i < IntroTimeline.playableGraph.GetRootPlayableCount(); i++)
    			Process(IntroTimeline.playableGraph.GetRootPlayable(i), muteOverride ? 1 : 0);

    		IntroTimeline.stopped += OnIntroFinished;
    	}

    	void SetupOutroTimeline()
    	{
    		Object track = OutroTimeline.playableAsset.outputs.Where(x => x.streamName == "LetterAnimTrack").First().sourceObject;
    		if (track != null)
    		{
    			GameObject go = Instantiate(Letters[levelIndex], OutroLevel);
    			go.transform.localPosition += OutLetterRefTransform.localPosition;
    			AdditionalOffset offset = go.GetComponent<AdditionalOffset>();
    			if (offset != null)
    				go.transform.localPosition += offset.PositionOffset;
    			Animator animator = go.transform.GetChild(0).GetComponent<Animator>();

    			OutroTimeline.SetGenericBinding(track, animator);
    		}


    		track = OutroTimeline.playableAsset.outputs.Where(x => x.streamName == "AnimalAnimTrack").First().sourceObject;
    		if (track != null)
    		{
    			GameObject go = Instantiate(Animals[levelIndex], OutroLevel);
    			go.transform.localPosition += OutAnimalRefTransform.localPosition;
    			AdditionalOffset offset = go.GetComponent<AdditionalOffset>();
    			if (offset != null)
    				go.transform.localPosition += offset.PositionOffset;
    			Animator animator = go.transform.GetChild(0).GetComponent<Animator>();

    			OutroTimeline.SetGenericBinding(track, animator);
    		}

    		OutroTimeline.stopped += OnOutroFinished;
    	}

    	TextLetterDragController dragController;

    	private void OnIntroFinished(PlayableDirector director)
        {
    		Debug.Log("Intro done!");
        }

    	private void OnOutroFinished(PlayableDirector director)
        {
    		Debug.Log("Outro finished!");
    		TransitionManager.Instance.ShowFade(2.0f, () => SceneLoader.Instance.LoadScene(SceneLoader.Instance.LastScene, true));
    	}

    	public void ShowWord()
        {
    		// Show Text
    		GameObject text = Instantiate(Texts[levelIndex], IntroAndWordLevel);
    		dragController = text.GetComponent<TextLetterDragController>();
    		dragController.OnDoneEvent += DragLettersDone;

    		// Position
    		Camera camera = Camera.main;
    		float camHeight = camera.orthographicSize; //  half
    		float camWidth = camHeight * camera.aspect; // half
    		int[] checkLvl = (GameData.Instance.GameType == GameData.eGameType.UpperCase) ? upperSecondaryAnimLevels : lowerSecondaryAnimLevels;
    		if (checkLvl.Contains(levelIndex))
    		{
    			float offset = text.transform.position.y - dragController.Letters[0].Collider.bounds.max.y;
    			dragController.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y + camHeight - WordTopMargin + offset, 0f);
    		} 
    		else
            {
    			float offset = text.transform.position.x - dragController.Letters[0].Collider.bounds.min.x;
    			dragController.transform.position = new Vector3(camera.transform.position.x - camWidth + WordLeftMargin + offset, camera.transform.position.y, 0f);
    		}

    	}

        private void DragLettersDone()
        {
    #if UNITY_IOS
    		if (!ProgressManager.Instance.IsReviewShown(9))
    		{
    			Debug.Log("Asking for review!");
    			//UnityEngine.iOS.Device.RequestStoreReview();
    			ProgressManager.Instance.SetReviewShow(9);
    		}
    #endif
    		SoundManager.Instance.AddSFXToQueue("good_job");
    		Debug.Log("Done First Level!");
    		Sequence s = dragController.HideLetters();	
    		s.AppendCallback(SelectLetter.SpawnLetters);
        }

    	public void ShowTracingLevel()
        {
    		TracingLevel.gameObject.SetActive(true);
    		GameObject go = Instantiate(Tracings[levelIndex], TracingLevel);
    		TracingController controller = go.GetComponentInChildren<TracingController>();
    		TracingManager manager = go.GetComponentInChildren<TracingManager>();
    		manager.FinishCallback += ShowOutro;
    		Vector3 camPos = Camera.main.transform.position;
    		camPos.z = 0f;
    		go.transform.position = camPos + Vector3.left * 20.0f;
    		go.transform.DOMove(camPos, 1.0f).SetEase(Ease.OutQuint).OnComplete(() =>
    			{
    				manager.Init();
    			});

    		SoundManager.SFXStack stack;
    		if (GameData.Instance.GameType == GameData.eGameType.UpperCase)
    			stack = SoundManager.Instance.PlaySFXStack("startTrace_uppercase", 1.0f, "voiceover", 1);
    		else
    			stack = SoundManager.Instance.PlaySFXStack("startTrace_lowercase", 1.0f, "voiceover", 1);
    		char letter = (char)('a' + levelIndex);
    		stack.AddSFXStack(letter.ToString());
    	}

    	public void ShowOutro()
        {
    		string[] sfx = new string[] { "awesome", "good_job", "bravo", "amazing", "did_great", "did_all" };
    		SoundManager.Instance.AddSFXToQueue(sfx.GetRandomElement());

    		IntroAndWordLevel.gameObject.SetActive(false);
    		TracingLevel.gameObject.SetActive(false);
    		OutroLevel.gameObject.SetActive(true);

    		int lastUnlockedLvl = ProgressManager.Instance.GetUnlockLevel(GameData.Instance.GameType);
    		if (lastUnlockedLvl <= levelIndex)
    		{
    			ProgressManager.Instance.UnlockLevel(GameData.Instance.GameType, levelIndex + 1);
    			if (GameData.Instance.GameType == GameData.eGameType.UpperCase)
    				ProgressManager.Instance.LastUnlockedLevelBigLetter = levelIndex + 1;
    			else
    				ProgressManager.Instance.LastUnlockedLevelSmallLetter = levelIndex + 1;
    		}

    		if (GameData.Instance.GameType == GameData.eGameType.UpperCase)
    			ProgressManager.Instance.LastPlayedLevelBigLetter = levelIndex;
    		else
    			ProgressManager.Instance.LastPlayedLevelSmallLetter = levelIndex;
    	}

    	public void ShowReward()
        {
    		SoundManager.Instance.PlaySFX("CrowdCheer");
    		ProgressManager.Instance.ReviewFinishedLetterCounter++; // We don't care if it's upper or lower
    #if UNITY_IOS
    		if (ProgressManager.Instance.ReviewFinishedLetterCounter >= 2 && !ProgressManager.Instance.IsReviewShown(10))
    		{
    			Debug.Log("Asking for review!");
    			//UnityEngine.iOS.Device.RequestStoreReview();
    			ProgressManager.Instance.SetReviewShow(10);
    		}
    #endif

    		GameObject reward = Instantiate(Rewards.GetRandomElement());
        }

    	public void PlaySFXHereIsTheLetter()
        {
    		SoundManager.Instance.AddSFXToQueue("here_is_A");
    		if (GameData.Instance.GameType == GameData.eGameType.UpperCase)
    			SoundManager.Instance.AddSFXToQueue("uppercase");
    		else
    			SoundManager.Instance.AddSFXToQueue("lowercase");
    		SoundManager.Instance.AddSFXToQueue(((char)('a' + levelIndex)).ToString());
        }

    	public void PlaySFXHereIsTheWord()
        {
    		SoundManager.Instance.AddSFXToQueue("here_is_the_word");
    		SoundManager.Instance.AddSFXToQueue(Texts[levelIndex].GetComponent<TextLetterDragController>().GetWord());
    	}

        private void OnDrawGizmos()
        {
    		if (Camera.main == null)
    			return;

    		Gizmos.color = Color.red;
    		float height = 2f * Camera.main.orthographicSize;
    		float width = height * Camera.main.aspect;
    		Gizmos.DrawWireCube(Camera.main.transform.position.SetZ(0), new Vector3(width, height, 0f));
        }
    }


}