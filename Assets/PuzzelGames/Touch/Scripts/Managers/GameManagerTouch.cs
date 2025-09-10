using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Interactive.Touch
{
    using DG.Tweening;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;
    public class GameManagerTouch : MonoBehaviour
    {
        public static GameManagerTouch Instance { get; private set; }


        private void Awake()
        {
            Instance = this;

#if UNITY_EDITOR
            if (TEST_GAME_TYPE != GameDataTouch.eGameType.NumModules)
                GameDataTouch.Instance.GameType = TEST_GAME_TYPE;
#endif
        }

        public GameObject[] Levels;
        public Sprite[] LevelsSprite;
        public GameObject tracingSolvedPanel;
        public Image tracingSprite;
        public string key;
        public string SoundName;
        public string[] boostSounds;
        public int sessionId;

        public CategoryRangeConfigSO CategoryConfig;
        //[System.NonSerialized]
        public int CurrentLevel;
        private GameObject SpawnedLevel;
        public GameObject DoneButton;
        public GameObject NextGroupButton;
        public RectTransform BookIcon;
        public SpriteRenderer StickerReward;
        public ParticleSystem StickerBubblesPS;
        [HideInInspector]
        public Bounds UIBounds = new Bounds();
        public RectTransform LeftBoundObject;
        public RectTransform RightBoundObject;

        // FOR SCREENSHOT
        public GameObject[] ScreenshotHideObjects;
        public SpriteRenderer PhotoFrame;
        public GameObject PhotoIcon;

        public AudioSource DrawingAudio;

        public bool IsFirstLevelLoaded { get; private set; } = true;

#if UNITY_EDITOR
        public GameDataTouch.eGameType TEST_GAME_TYPE = GameDataTouch.eGameType.NumModules;
#endif

        public enum eTool
        {
            Fill,
            Brush,
            Eraser,
            Stamp,
            Pattern,
            Symmetry2x,
            Symmetry6x
        }

        public eTool SelectedTool { get; private set; }
        public Color SelectedColor { get; private set; }
        public Sprite SelectedPattern { get; private set; }
        public Sprite SelectedStamp { get; private set; }
        public bool RainbowColor { get; private set; } = true;

        private bool StickerShown = false;
        private bool levelCompleted = false;

        // Start is called before the first frame update
        void Start()
        {
            Application.targetFrameRate = 60;
            // Calculate non UI Bounds
            if (LeftBoundObject != null && RightBoundObject != null)
            {
                Rect leftBound = LeftBoundObject.GetWorldRectGameSpace();
                Rect rightBound = RightBoundObject.GetWorldRectGameSpace();
                float height = Camera.main.orthographicSize * 2.0f;
                float width = rightBound.xMin - leftBound.xMax;
                //Debug.Log(rightBound.xMax + " " + rightBound.xMin + " " + leftBound.xMin + " " + leftBound.xMax);
                UIBounds.center = new Vector3(leftBound.xMax + width * 0.5f, 0f);
                UIBounds.size = new Vector3(width, height);
            }
            else
            {
                Debug.Log("Left and Right bounds not defined! Using default values!");
                UIBounds.center = Vector3.zero;
                UIBounds.size = new Vector3(Camera.main.orthographicSize * 2.0f * Camera.main.aspect, Camera.main.orthographicSize * 2.0f);
            }
            GameDataTouch.Instance.SelectedLevel = PlayerPrefs.GetInt(key, 0);
            if (GameDataTouch.Instance.SelectedLevel >= Levels.Length - 1)
            {
                GameDataTouch.Instance.SelectedLevel = Random.Range(0, Levels.Length - 1);
                levelCompleted = true;
            }
            CurrentLevel = GameDataTouch.Instance.SelectedLevel;
            if (Levels != null && Levels.Length > 0)
                SpawnLevel(Levels[CurrentLevel]);
            else
                Debug.LogWarning("No levels added to GameManagerTouch!");

            //SoundManagerTouch.Instance.CrossFadeMusic("Music" + Random.Range(1, 8), 0.5f);
            SoundManagerTouch.Instance.PlaySFX(SoundName);
            IsFirstLevelLoaded = true;
            //PlayIntroVoice();
        }

        bool playedIntroVoiceOver = false; // Make sure we're playing the intro just once per scene load
        public void PlayIntroVoice()
        {
            if (playedIntroVoiceOver)
                return;
            playedIntroVoiceOver = true;

            /*
    		Let's start coloring. [La inceput] "start_coloring"
    		Pick your favorite colors and let's begin. [La inceput] "pick_favourite_colors"
    		Pick a color and let's get creative! [La inceput] "pick_color_get_creative"
    		Draw time � you're in charge! [La inceput] "draw_time"

    		[la inceput la pattern]
    		Select the pattern that you like and fill the canvas. "select_pattern_fill_canvas" 
    		[touch and fill]
    		Select the color that you like and fill the sketch.   "select_color_fill_sketch"
    		[Symmetry + Color in Environment]
    		Select the color you like, draw and watch the magic happen!  

    		*/

            List<string> IntroGreetings = new List<string>();

            if (GameDataTouch.Instance.GameType != GameDataTouch.eGameType.PatternAndStamps && GameDataTouch.Instance.GameType != GameDataTouch.eGameType.Tracing)
            {
                IntroGreetings.AddRange(new string[] { "start_coloring", "pick_favourite_colors", "pick_color_get_creative", "draw_time" });
            }

            if (GameDataTouch.Instance.GameType == GameDataTouch.eGameType.PatternAndStamps)
                IntroGreetings.Add("select_pattern_fill_canvas");
            else if (GameDataTouch.Instance.GameType == GameDataTouch.eGameType.TouchAndFill)
                IntroGreetings.Add("select_color_fill_sketch");
            else if (GameDataTouch.Instance.GameType == GameDataTouch.eGameType.Symmetry || GameDataTouch.Instance.GameType == GameDataTouch.eGameType.ColorEnvironment)
                Debug.LogWarning("Missing sound, ask Igor!");

            if (IntroGreetings.Count > 0)
                SoundManagerTouch.Instance.AddSFXToQueue(IntroGreetings[Random.Range(0, IntroGreetings.Count)], 1.0f, "voiceover", 0, 0.5f);
        }
        public void PlayRandomSound()
        {
            if (Random.Range(0, 4) == 1)
            {

                //SoundManagerTouch.Instance.PlaySFX(boostSounds[Random.Range(0, boostSounds.Length)]);
            }
        }
        // ColorPicker actions
        public void OnColorSelection(Color color)
        {
            SelectedColor = color;
            RainbowColor = false;
        }
        public void OnToolSelection(eTool tool)
        {
            SelectedTool = tool;
        }
        public void OnPatternSelection(Sprite sprite)
        {
            SelectedPattern = sprite;

            var controller = SpawnedLevel?.GetComponent<StampAndPatternController>();
            controller?.SetPattern(sprite);
        }
        public void OnStampSelection(Sprite sprite)
        {
            SelectedStamp = sprite;

            var controller = SpawnedLevel?.GetComponent<StampAndPatternController>();
            controller?.SetStamp(sprite);
        }
        public void OnRainbowColorSelection()
        {
            RainbowColor = true;
        }
        void SpawnLevel(GameObject level)
        {
            StickerShown = false;
            Resources.UnloadUnusedAssets();
            DoneButton.SetActive(false);
            if (NextGroupButton != null)
                NextGroupButton.SetActive(false);
            if (SpawnedLevel != null)
                Destroy(SpawnedLevel);
            SpawnedLevel = Instantiate(level);

            var controller = SpawnedLevel?.GetComponent<StampAndPatternController>();
            if (SelectedStamp != null)
                controller?.SetStamp(SelectedStamp);
            if (SelectedPattern != null)
                controller?.SetPattern(SelectedPattern);
        }

        public void NextLevel()
        {
            Coroutine iconRoutine = CheckForIcons();
            if (iconRoutine != null)
            {
                StartCoroutine(DelayedNextLevel(iconRoutine));
            }
            else
            {
                SpawnNextLevel();
            }

        }
        public void GoHome()
        {
            SoundManagerTouch.Instance.PlaySFX("HomeButtonFromActivities", 1);
            SceneLoader.Instance.LoadScene("Jungle");
        }
        private void SpawnNextLevel()
        {
            CurrentLevel++;

            IsFirstLevelLoaded = false;

            int lastLevel = Levels.Length - 1;
#if !UNITY_EDITOR
    		if (CategoryConfig != null)
    		{
    			lastLevel = CategoryConfig.GetLastLevel(CurrentLevel - 1);
    		}
#endif

            int currentLevelRelative = CurrentLevel;
            if (CategoryConfig != null)
            {
                int startLevel = CategoryConfig.GetStartLevel(CurrentLevel);
                if (startLevel >= 0)
                    currentLevelRelative = CurrentLevel - startLevel;
            }

#if UNITY_IOS
            //if (currentLevelRelative % 2 == 0)
            //{
            //    Debug.Log("Asking for review!");
            //    UnityEngine.iOS.Device.RequestStoreReview();
            //}
#endif

            if (CurrentLevel > lastLevel  /*!ProductManagerTouch.Instance.IsSubscribed && currentLevelRelative >= 2*/)
            {
                TransitionManagerTouch.Instance.ShowFade(1.0f, () => SceneLoader.Instance.LoadScene("Jungle"));
                return;
            }

            GameDataTouch.Instance.SelectedLevel = CurrentLevel;
            //SpawnLevel(Levels[CurrentLevel]);
            NextLevel();

        }

        IEnumerator DelayedNextLevel(Coroutine coroutine)
        {
            yield return coroutine;
            SpawnNextLevel();
        }

        public void PreviousLevel()
        {
            if (CurrentLevel <= 0)
                return; // No more levels

            IsFirstLevelLoaded = false;

            CurrentLevel--;
            GameDataTouch.Instance.SelectedLevel = CurrentLevel;
            SpawnLevel(Levels[CurrentLevel]);
        }

        Sequence StickerToBookSequence;

        public void ShowDoneButton()
        {
            //SoundManagerTouch.Instance.PlaySFX("DoneActivity");

            //DoneButton.SetActive(true);
            ////SoundManagerTouch.Instance.PlaySFX("ThatsAmazing");
            ////SoundManagerTouch.Instance.PlaySFX("CompletedActivity", 1, "sfx", 1);
            //if (LevelsSprite.Length > PlayerPrefs.GetInt(key))
            //    tracingSprite.sprite = LevelsSprite[PlayerPrefs.GetInt(key)];

            //// set initial small scale
            //Transform panelChild = tracingSolvedPanel.transform.GetChild(1);
            //panelChild.localScale = new Vector3(0.15f, 0.15f, 0.15f);
            //PlayerPrefs.SetInt(key, ++GameDataTouch.Instance.SelectedLevel);
            //Debug.Log($"AdvanceLevel: {PlayerPrefs.GetInt(key)}");
            ////SoundManagerTouch.Instance.PlaySFX(boostSounds[Random.Range(0, boostSounds.Length)]);

            //tracingSolvedPanel.SetActive(true);
            //PlayerPrefs.SetInt("Session", sessionId);
            //// animate scale to 1 smoothly
            //panelChild.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            //SoundManagerTouch.Instance.PlaySFX("GiftFromCompletedActivity");

            //Invoke(nameof(GoHome), 3);
            ////ShowStickerReward();
            ///

            StartCoroutine(AdvanceLevelSequence());
        }
        private IEnumerator AdvanceLevelSequence()
        {
            SoundManagerTouch.Instance.PlaySFX("DoneActivity");
           
            if (!levelCompleted)
            {
                SoundManagerTouch.Instance.PlaySFX(boostSounds[Random.Range(0, boostSounds.Length)]);

                yield return new WaitForSeconds(0.3f);
                if (LevelsSprite.Length > PlayerPrefs.GetInt(key))
                    tracingSprite.sprite = LevelsSprite[PlayerPrefs.GetInt(key)];

                // set initial small scale
                Transform panelChild = tracingSolvedPanel.transform.GetChild(1);
                panelChild.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                PlayerPrefs.SetInt(key, ++GameDataTouch.Instance.SelectedLevel);
                Debug.Log($"AdvanceLevel: {PlayerPrefs.GetInt(key)}");
                //SoundManagerTouch.Instance.PlaySFX(boostSounds[Random.Range(0, boostSounds.Length)]);

                tracingSolvedPanel.SetActive(true);
                PlayerPrefs.SetInt("Session", sessionId);
                // animate scale to 1 smoothly
                panelChild.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
                SoundManagerTouch.Instance.PlaySFX("GiftFromCompletedActivity");
            }
            Invoke(nameof(GoHome), 3);
        }
        public void ShowStickerReward()
        {
            SoundManagerTouch.Instance.AddSFXToQueue(SoundManagerTouch.GetCheeringVoice(), 1.0f, "voiceover", 1);

            if (StickerShown)
                return;
            StickerShown = true;

            SoundManagerTouch.Instance.AddSFXToQueue("wow_earned_sticker", 1.0f, "voiceover", 2);
            if (CurrentLevel == 0)
                SoundManagerTouch.Instance.AddSFXToQueue("goto_sticker_album", 1.0f, "voiceover", 2);

            if (CategoryConfig != null && !ProgressManagerTouch.Instance.IsStickerUnlocked(GameDataTouch.Instance.GameType, CurrentLevel))
            {
                if (StickerToBookSequence != null)
                    StickerToBookSequence.Kill(true);

                StickerReward.gameObject.SetActive(true);
                BookIcon.gameObject.SetActive(true);

                StickerReward.sprite = CategoryConfig.GetStickerByGlobalIdx(CurrentLevel);
                int currentLevel = CurrentLevel; // Saving for Tweens 
                float scale = UtilsTouch.FitToSizeScale(StickerReward.sprite.bounds.size, new Vector2(5.0f, 5.0f));
                StickerReward.transform.position = Vector3.zero;
                StickerReward.transform.localScale = Vector3.zero;
                Vector3 BookPosition = Camera.main.ScreenToWorldPoint(BookIcon.position).SetZ(0f);
                StickerBubblesPS.Emit(40);
                SoundManagerTouch.Instance.PlaySFX("WinSticker");
                StickerToBookSequence = DOTween.Sequence()
                    .Append(StickerReward.transform.DOScale(Vector3.one * scale, 0.5f).SetEase(Ease.OutBack))
                    .Join(StickerReward.transform.DORotate(Vector3.forward * 360.0f, 0.5f, RotateMode.FastBeyond360).SetEase(Ease.InOutQuad))
                    //.AppendInterval(2.0f) // Maybe some particles?
                    //.AppendCallback(() => StickerBubblesPS.Emit(30))
                    .Append(StickerReward.transform.DOScale(Vector3.one * scale * 1.2f, 2.0f))
                    .Append(StickerReward.transform.DOMove(BookPosition, 1.0f).SetEase(Ease.InBack))
                    .Join(StickerReward.transform.DOScale(Vector3.zero, 1.0f).SetEase(Ease.InBack))
                    .OnComplete(() =>
                    {
                        StickerReward.gameObject.SetActive(false);
                        BookIcon.gameObject.SetActive(false);
                        ProgressManagerTouch.Instance.UnlockSticker(GameDataTouch.Instance.GameType, currentLevel);
                    });
            }
        }

        public void HideNextGroupButton()
        {
            if (NextGroupButton != null)
                NextGroupButton.SetActive(false);
        }

        public void ShowNextGroupButton()
        {
            if (NextGroupButton != null)
                NextGroupButton.SetActive(true);
        }

        public void ColorEnvironmentNextGroup()
        {
            var controller = SpawnedLevel?.GetComponent<ColorEnvironmentController>();

#if UNITY_EDITOR
            if (controller == null)
                controller = FindObjectOfType<ColorEnvironmentController>();
#endif

            if (controller != null)
            {
                controller.NextGroup();
            }

            HideNextGroupButton();
        }

        public void OutlineNextGroup()
        {
            var controller = SpawnedLevel?.GetComponent<OutlineControllerTouch>();

#if UNITY_EDITOR
            if (controller == null)
                controller = FindObjectOfType<OutlineControllerTouch>();
#endif

            if (controller != null)
            {
                controller.NextGroup();
            }

            HideNextGroupButton();
        }

        public void StampAndPatternNextGroup()
        {
            var controller = SpawnedLevel?.GetComponent<StampAndPatternController>();

#if UNITY_EDITOR
            if (controller == null)
                controller = FindObjectOfType<StampAndPatternController>();
#endif

            if (controller != null)
            {
                controller.NextGroup();
            }

            HideNextGroupButton();
        }

        private Coroutine CheckForIcons()
        {
            if (GameDataTouch.Instance.GameType == GameDataTouch.eGameType.TouchAndFill) // Only Touch and Fill has unknown ending (as it can show next button after 2nd fill)...
            {
                return SpawnedLevel?.GetComponent<TouchAndFillController>().CreateIcon();
            }
            return null;
            //else if (GameDataTouch.Instance.GameType == GameDataTouch.eGameType.PatternAndStamps)
            //    SpawnedLevel?.GetComponent<StampAndPatternController>().CreateIcon();
            //else if (GameDataTouch.Instance.GameType == GameDataTouch.eGameType.Outline)
            //    SpawnedLevel?.GetComponent<OutlineControllerTouch>().CreateIcon();
        }

        public void Screenshot()
        {
            //CaptureManager.Instance.Screenshot(ScreenshotHideObjects, null, PhotoFrame, PhotoIcon);
        }

        public void ClearSymmetryTexture()
        {
            if (SpawnedLevel == null)
                SpawnedLevel = FindObjectOfType<SymmetryController>().gameObject;
            var controller = SpawnedLevel?.GetComponent<SymmetryController>();
            controller?.ClearOutRenderTexture();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(UIBounds.center, UIBounds.size);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            //if (CaptureManager.Instance.ScreenshotIsRunning)
            //	return;

            //GUIStyle myButtonStyle = new GUIStyle(GUI.skin.button);
            //myButtonStyle.fontSize = 50;
            //if (GUI.Button(new Rect(200, 200, 300, 150), "Next", myButtonStyle))
            //{
            //	NextLevel();
            //}
            //if (GUI.Button(new Rect(200, 350, 300, 150), "Prev", myButtonStyle))
            //{
            //	PreviousLevel();
            //}
        }
#endif
    }


}