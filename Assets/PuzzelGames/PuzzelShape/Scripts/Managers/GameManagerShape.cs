using System;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Interactive.PuzzelShape
{
using System;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

    public class GameManagerShape : MonoBehaviour
    {
        [System.Serializable]
        public class CardDesc
        {
            public Sprite CardSprite;
            [Range(0, 6)]
            public int Difficulty;
            public float Scale = 1f;
        }
        [System.Serializable]
        public class CardCollection
        {
            public MemoryGameSetup.eCategory Category;
            public CardDesc[] Cards;
        }


        public static GameManagerShape Instance { get; private set; }

        [Header("| General")]
        public GameObject[] Levels;
        public ParticleSystem DoneFX;
        public float HintTimer = 10.0f;
        public NavigationController Navigation;
        public int ReviewLevel = 2;
        [Header("| For Shapes")]
        public GameObject[] Geometries;
        public GameObject[] GeometryHoles;
        public CustomRGBColors RGBColors;
        public ParticleSystem SuckInFX;
        [Header("| For Cards")]
        public GameObject CardPrefab;
        public CardCollection[] Cards;
        [Header("| For Coloring")]
        public ColorWheel ColorWheel;
        public Material ColoringIconMaterial;
        public GameObject[] ExcludeFromPhoto;
        public GameObject[] ExcludeFromIcon;
        public SpriteRenderer PhotoFrame;
        public GameObject PhotoIcon;
        [Header("| Additional")]
        public ScrollPanelShape ScrollPanelShape;

        public GameObject CurrentLevel { get; private set; }
        public int CurrentLevelIdx { get; private set; }


        bool ShouldAskForPhoto = false;
        float ShowNextLevelHintTimer = 0f;

        private enum eDelayedAction
        {
            NextLevel,
            GoHome,
            GameDone
        }

        private void Awake()
        {
            Instance = this;
        }

        void Start()
        {
    #if UNITY_EDITOR
            var dropzone = GameObject.FindObjectsOfType<MonoBehaviour>().OfType<IDropZone>().FirstOrDefault();
            if (SceneLoaderShape.Instance.LastScene == "Loader" && dropzone != null)
            {
                CurrentLevelIdx = 0;
                CurrentLevel = (dropzone as MonoBehaviour).gameObject;
            }
            else
            {
    #endif
                CurrentLevelIdx = GameDataShape.Instance.SelectedLevel;
                CurrentLevel = Instantiate(Levels[CurrentLevelIdx]);
    #if UNITY_EDITOR
            }
    #endif
            Navigation.NextLevelButton.gameObject.SetActive(false);
            NotifyLevelChanged();

            // Photo stuff
            if (GameDataShape.Instance.GameType == GameDataShape.eGameType.Coloring)
            {
                SoundManagerShape.Instance.PlaySFX("TouchToFeelTheColor");

                var permission = NativeGallery.CheckPermission(NativeGallery.PermissionType.Write, NativeGallery.MediaType.Image);
                if (permission == NativeGallery.Permission.Denied)
                    PhotoIcon.gameObject.SetActive(false);
                else if (permission == NativeGallery.Permission.ShouldAsk)
                    ShouldAskForPhoto = true;
            }
            else if (GameDataShape.Instance.GameType == GameDataShape.eGameType.Memory)
            {
                SoundManagerShape.Instance.PlaySFX("CanYouMemorise");
            }
            else if (GameDataShape.Instance.GameType == GameDataShape.eGameType.Environment)
            {
                SoundManagerShape.Instance.PlaySFX("MoveWhereBelongs");
            }

            SoundManagerShape.Instance.CrossFadeMusic(GetLevelMusic(GameDataShape.Instance.GameType), 2.0f);
        }

        private void Update()
        {
            if (ShowNextLevelHintTimer > 0)
            {
                ShowNextLevelHintTimer -= Time.deltaTime;
                if (ShowNextLevelHintTimer <= 0)
                {
                    float duration = Navigation.FingerHintRef.ShowHint();
                    ShowNextLevelHintTimer = HintTimer + duration;
                }
            }
        }

        void OnDisable()
        {
            //        //        AnalyticsManager.Instance.LevelEnded(false);
        }

        string GetLevelMusic(GameDataShape.eGameType gameType)
        {
            switch (gameType)
            {
                case GameDataShape.eGameType.Shapes:
                    return "ShapesMusic";
                case GameDataShape.eGameType.Coloring:
                    return "ColoringMusic";
                case GameDataShape.eGameType.Puzzle:
                    return "PuzzleMusic";
                case GameDataShape.eGameType.Environment:
                    return "EnvironmentMusic";
                case GameDataShape.eGameType.Memory:
                    return "MemoryMusic";
                case GameDataShape.eGameType.ShapePuzzle:
                    return "ShapePuzzleMusic";
                default:
                    return "ShapesMusic";
            }
        }

        public IDropZone GetCurrentLeveDropZonel()
        {
            return CurrentLevel.GetComponent<IDropZone>();
        }

        Sequence nextScale;

        public void ShowNextLevel()
        {
            Navigation.NextLevelButton.gameObject.SetActive(true);
            if (nextScale != null)
                nextScale.Kill(true);
            //Color color = NextLevelButton.color;
            //color.a = 0f;
            //NextLevelButton.color = color;
            //NextLevelButton.DOFade(1.0f, 0.5f);
            SoundManagerShape.Instance.PlaySFX("NextLevelAppear", 0.5f);

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Navigation.NextLevelButton.transform.position);
            worldPos.z = 0f;
            Navigation.NextLevelFX.transform.position = worldPos;

            Vector3 targetScale = Navigation.NextLevelButton.transform.localScale;
            Navigation.NextLevelButton.transform.localScale = Vector3.zero;

            nextScale = DOTween.Sequence()
                .Append(Navigation.NextLevelButton.transform.DOScale(targetScale, 0.3f).SetEase(Ease.OutBack))
                .InsertCallback(0f, () =>
                {
                    Navigation.NextLevelFX.Clear();
                    Navigation.NextLevelFX.Play();
                });

            if (CurrentLevelIdx < 3)
                ShowNextLevelHintTimer = HintTimer;
            ProgressManagerShape.Instance.UnlockLevel(GameDataShape.Instance.GameType, CurrentLevelIdx + 1); // Mark next level as unfinished

            SoundManagerShape.Instance.PlayCongratsVoice(true);
        }

        bool gameDone = false;

        public void NextLevel()
        {
            ShowNextLevelHintTimer = -1.0f;

            if (gameDone)
                return;

    #if UNITY_IOS
              if (!ProgressManagerShape.Instance.IsReviewShown(GameDataShape.Instance.GameType) && (CurrentLevelIdx == ReviewLevel))
              {
                  Debug.Log("Asking for review!");
                  UnityEngine.iOS.Device.RequestStoreReview();
                  ProgressManagerShape.Instance.SetReviewShow(GameDataShape.Instance.GameType);
              }
    #endif

            //        AnalyticsManager.Instance.LevelEnded(true);
            if (CurrentLevelIdx >= Levels.Length - 1)
            {
                ProgressManagerShape.Instance.UnlockLevel(GameDataShape.Instance.GameType, CurrentLevelIdx + 1); // We need to do it here as we don't call Notify Level Change...
                gameDone = true;
                ProgressManagerShape.Instance.SetGameDone(GameDataShape.Instance.GameType);
                DOTween.Sequence().AppendInterval(1.0f)
                    .AppendCallback(() => SoundManagerShape.Instance.PlaySFX("FinishedLevels"));
                if (GameDataShape.Instance.GameType == GameDataShape.eGameType.Coloring && CurrentLevel.GetComponent<FillColorOnTouch>().IsDone())
                    StartCoroutine(DelayedIcon(eDelayedAction.GameDone));
                else
                    ShowDone();
                return;
            }

            if (GameDataShape.Instance.GameType == GameDataShape.eGameType.Coloring && CurrentLevel.GetComponent<FillColorOnTouch>().IsDone())
            { // Make an icon and postpone next switch
                StartCoroutine(DelayedIcon(eDelayedAction.NextLevel));
            }
            else
            {
                AdvanceLevel(1);

                /*
                if (GameDataShape.Instance.GameType != GameDataShape.eGameType.Puzzle && GameDataShape.Instance.GameType != GameDataShape.eGameType.ShapePuzzle)
                {
                    string[] sounds = { "LetsPlayAgain", "LetsDoItAgain", "AnotherOne" };
                    if (Random.value < 0.3f) 
                        SoundManagerShape.Instance.PlaySFX(sounds[Random.Range(0, sounds.Length)]);
                }
                */
            }
        }

        private void ShowDone()
        {
            //DoneFX.Play();
            SoundManagerShape.Instance.PlaySFX("LevelDone");
            TransitionManagerShape.Instance.SetDefaultFadeColor();
            /*
            DOTween.Sequence()
                .AppendInterval(5.0f)
                .Append(TransitionManagerShape.Instance.ShowFade(1.0f, () =>
                {
                    Cleanup();
                    SceneLoaderShape.Instance.LoadScene("MainMenu");
                }));
            */

            TransitionManagerShape.Instance.ShowFade(1.0f, () =>
            {
                Cleanup();
                SceneLoaderShape.Instance.LoadScene("MainMenu");
            }).PrependInterval(0.0f);
        }

        private void AdvanceLevel(int Direction)
        {
            Cleanup();
            Destroy(CurrentLevel);
            CurrentLevelIdx += Direction;
            CurrentLevel = Instantiate(Levels[CurrentLevelIdx]);
            Navigation.NextLevelButton.gameObject.SetActive(false);
            NotifyLevelChanged();
        }

        public void PrevLevel()
        {
            if (CurrentLevelIdx <= 0)
                return;

            // For DEBUG ONLY!
            goingHome = false;
            gameDone = false;


            AdvanceLevel(-1);
        }

        bool goingHome = false;

        public void GoHome()
        {
            if (goingHome)
                return;

            SoundManagerShape.Instance.PlaySFX("MenuItemClick");
            goingHome = true;
            if (GameDataShape.Instance.GameType == GameDataShape.eGameType.Coloring && CurrentLevel.GetComponent<FillColorOnTouch>().IsDone())
            { // Make an icon and postpone next switch
                StartCoroutine(DelayedIcon(eDelayedAction.GoHome));
            }
            else
            {
                Cleanup();
                TransitionManagerShape.Instance.SetDefaultFadeColor();
                TransitionManagerShape.Instance.ShowFade(1.0f, () => SceneLoaderShape.Instance.LoadScene("MainMenu"));
            }
        }

        bool ScreenshotIsRunning = false;
        public void Screenshot()
        {
            if (ScreenshotIsRunning)
                return;

            if (ShouldAskForPhoto)
            {
                var permission = NativeGallery.RequestPermission(NativeGallery.PermissionType.Write, NativeGallery.MediaType.Image);
                if (permission == NativeGallery.Permission.Denied)
                    return;
            }

            StartCoroutine(DelayedScreenshot());
            //StartCoroutine(DelayedIcon(eDelayedAction.NextLevel));
        }

        Sequence photoFrameSequence = null;

        IEnumerator DelayedScreenshot()
        {
            ScreenshotIsRunning = true;
            yield return null;
            List<GameObject> returnActive = new List<GameObject>();
            for (int i = 0; i < ExcludeFromPhoto.Length; i++)
            {
                if (!ExcludeFromPhoto[i].activeInHierarchy)
                    continue;
                ExcludeFromPhoto[i].SetActive(false);
                returnActive.Add(ExcludeFromPhoto[i]);
            }
            yield return new WaitForEndOfFrame();
            var texture = ScreenCapture.CaptureScreenshotAsTexture();
            //byte[] bytes = texture.EncodeToPNG();
            //System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/Screenshots/Coloring");
            //System.IO.File.WriteAllBytes(Application.persistentDataPath + "/Screenshots/Coloring/Screenshot" + ProgressManagerShape.Instance.GetScreenShotId() + ".png", bytes);
            //ProgressManagerShape.Instance.SetScreenshotId(ProgressManagerShape.Instance.GetScreenShotId() + 1);
            // cleanup
            //Destroy(texture);
            foreach (var go in returnActive)
                go.SetActive(true);

            Camera.main.Render();
            yield return null;

            string screenshotFileName = "Screenshot" + ProgressManagerShape.Instance.GetScreenShotId() + System.DateTime.Now.ToString("yyyyMMddhhmmss") + ".png";
            NativeGallery.SaveImageToGallery(texture, Application.productName, screenshotFileName);
            ProgressManagerShape.Instance.SetScreenshotId(ProgressManagerShape.Instance.GetScreenShotId() + 1);

            if (photoFrameSequence != null)
                photoFrameSequence.Kill(true);
            Sprite releaseSprite = GameManagerShape.Instance.PhotoFrame.sprite; // Fix Memory leaks
            Sprite screenSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f, 100.0f);
            GameManagerShape.Instance.PhotoFrame.sprite = screenSprite;
            GameManagerShape.Instance.PhotoFrame.transform.localScale = Vector3.one * (100.0f * Camera.main.orthographicSize * 2.0f / Screen.height);
            GameManagerShape.Instance.PhotoFrame.transform.position = Vector3.zero;
            GameManagerShape.Instance.PhotoFrame.gameObject.SetActive(true);
            if (releaseSprite != null)
            {
                Destroy(releaseSprite.texture);
                Destroy(releaseSprite);
            }
            OutlineControllerShape controller = GameManagerShape.Instance.PhotoFrame.GetComponent<OutlineControllerShape>();
            Vector3 iconPosition = Camera.main.ScreenToWorldPoint(PhotoIcon.transform.position).SetZ(0);
            photoFrameSequence = DOTween.Sequence()
                .Append(DOTween.ToAlpha(() => controller.OffsetColor, (color) => controller.SetOffsetColor(color), 1.0f, 0.5f).SetEase(Ease.Flash, 2))
                .Join(GameManagerShape.Instance.PhotoFrame.transform.DOScale(0.5f, 0.5f).SetEase(Ease.OutSine))
                .AppendInterval(0.5f)
                .Append(GameManagerShape.Instance.PhotoFrame.transform.DOScale(0f, 0.5f).SetEase(Ease.InSine))
                .Join(GameManagerShape.Instance.PhotoFrame.transform.DOMove(iconPosition, 0.5f).SetEase(Ease.InSine))
                .AppendCallback(() => GameManagerShape.Instance.PhotoFrame.gameObject.SetActive(false));

            SoundManagerShape.Instance.PlaySFX("CameraCapture");
            ScreenshotIsRunning = false;
        }

        IEnumerator DelayedIcon(eDelayedAction NextAction)
        {
            ScreenshotIsRunning = true;
            yield return null;
            List<GameObject> returnActive = new List<GameObject>();
            for (int i = 0; i < ExcludeFromPhoto.Length; i++)
            {
                if (!ExcludeFromPhoto[i].activeInHierarchy)
                    continue;
                ExcludeFromPhoto[i].SetActive(false);
                returnActive.Add(ExcludeFromPhoto[i]);
            }
            for (int i = 0; i < ExcludeFromIcon.Length; i++)
            {
                if (!ExcludeFromIcon[i].activeInHierarchy)
                    continue;
                ExcludeFromIcon[i].SetActive(false);
                returnActive.Add(ExcludeFromIcon[i]);
            }
            yield return new WaitForEndOfFrame();
            //var texture_tmp = ScreenCapture.CaptureScreenshotAsTexture();

            SpriteRenderer sr = CurrentLevel.GetComponent<FillColorOnTouch>().Base;
            Vector3 bMin = Camera.main.WorldToScreenPoint(sr.bounds.min);
            Vector3 bMax = Camera.main.WorldToScreenPoint(sr.bounds.max);
            Vector3 size = bMax - bMin;
            Texture2D screenshotTexture = new Texture2D(Mathf.CeilToInt(size.x), Mathf.CeilToInt(size.y), TextureFormat.ARGB32, false);
            screenshotTexture.wrapMode = TextureWrapMode.Clamp;
            screenshotTexture.ReadPixels(new Rect(Mathf.Round(bMin.x), Mathf.Round(bMin.y), screenshotTexture.width, screenshotTexture.height), 0, 0);
            screenshotTexture.Apply();
            //Vector2 screenshotSize = new Vector2(screenshotTexture.width, screenshotTexture.height);
            Texture frame = ColoringIconMaterial.GetTexture("_FrameTex");
            //screenshotSize *= UtilsShape.FitToSizeScale(screenshotSize, new Vector2(frame.width, frame.height));
            //var resizedTexture = ScaleTexture(screenshotTexture, Mathf.CeilToInt(screenshotSize.x), Mathf.CeilToInt(screenshotSize.y));
            //Destroy(screenshotTexture);

            RenderTexture rt = RenderTexture.GetTemporary(frame.width, frame.height);
            rt.filterMode = FilterMode.Bilinear;
            RenderTexture last = RenderTexture.active;
            RenderTexture.active = rt; // No need to set as Blit sets it for us, Adrian from the future: No we need it because iOS is fucked up and it doesn't clear the texture right...
            GL.Clear(true, true, Color.clear);

            Graphics.Blit(screenshotTexture, rt, ColoringIconMaterial);
            Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
            texture.ReadPixels(new Rect(0f, 0f, rt.width, rt.height), 0, 0);

            RenderTexture.active = last;
            RenderTexture.ReleaseTemporary(rt);

            //var texture = ScaleTexture(texture_tmp, 100, 100);
            // do something with texture
            byte[] bytes = texture.EncodeToPNG();
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/GeneratedLevelIcons/Coloring");
            System.IO.File.WriteAllBytes(Application.persistentDataPath + "/GeneratedLevelIcons/Coloring/Level" + (CurrentLevelIdx + 1) + ".png", bytes);
            // cleanup
            Destroy(texture);
            Destroy(screenshotTexture);
            foreach (var go in returnActive)
                go.SetActive(true);

            Camera.main.Render();
            ScreenshotIsRunning = false;

            // Postponed level change
            if (NextAction == eDelayedAction.NextLevel)
            {
                AdvanceLevel(1);
            }
            else if (NextAction == eDelayedAction.GoHome)
            {
                Cleanup();
                TransitionManagerShape.Instance.SetDefaultFadeColor();
                TransitionManagerShape.Instance.ShowFade(1.0f, () => SceneLoaderShape.Instance.LoadScene("MainMenu"));
            }
            else if (NextAction == eDelayedAction.GameDone)
            {
                ShowDone();
            }
        }

        /*
    	private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
            Color[] rpixels = result.GetPixels(0);
            float incX = (1.0f / (float)targetWidth);
            float incY = (1.0f / (float)targetHeight);
            for (int px = 0; px < rpixels.Length; px++)
            {
                rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth), incY * ((float)Mathf.Floor(px / targetWidth)));
            }
            result.SetPixels(rpixels, 0);
            result.Apply();
            return result;
        }
        */

        void Cleanup()
        {
            DOTween.KillAll(true, "Transition");
            if (ScrollPanelShape != null)
                ScrollPanelShape.Clear();
        }

        void NotifyLevelChanged()
        {
            ProgressManagerShape.Instance.UnlockLevel(GameDataShape.Instance.GameType, CurrentLevelIdx); // Mark current level as unfinished
                                                                                               //  AnalyticsManager.Instance.LevelStarted(CurrentLevelIdx);

            if (ScrollPanelShape != null)
                ScrollPanelShape.NotifyLevelChange();
            if (ColorWheel != null)
                ColorWheel.NotifyLevelChange();
            SoundManagerShape.Instance.ClearQueue();
        }
    }


}