using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.Touch
{
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class ModuleController : MonoBehaviour
    {
        public JungleActions MainActions;

        public GameDataTouch.eGameType GameType;
        public CategorySelector CategorySelectorRef;
        public CategoryRangeConfigSO CategoryConfig;
        public ScrollMenuController ScrollController;
        public GameObject[] ItemRefs;
        public float XSpread = 4.0f;
        public float YSpread = 2.0f;
        public Vector3 Offset = new Vector3(-6.72f, -1.0f, 0f);

        public Sprite[] LevelIcons;

        private List<ScrollMenuController> CategoriesGOs = new List<ScrollMenuController>();

        int currentCategory = -1;
        Tween bubbleHideShowTween;

        float voiceHintCooldown = -1.0f;

        private void Awake()
        {
            CreateCategoriesGOs();
        }

        public void ResetAll()
        {
            CategorySelectorRef.SelectCategory(3, true);
        }

        private void OnEnable()
        {
            CategorySelectorRef.OnCategorySelect += OnCategorySelection;
        }

        private void OnDisable()
        {
            CategorySelectorRef.OnCategorySelect -= OnCategorySelection;
        }

        private void Start()
        {
            //int currentCategory = CategorySelectorRef.
            //for ()
        }

        void CreateCategoriesGOs()
        {
            if (CategoryConfig != null)
            {
                for (int i = 0; i < CategoryConfig.Ranges.Length; i++)
                {
                    GameObject catGO = Instantiate(ScrollController.gameObject, ScrollController.transform.parent);
                    var range = CategoryConfig.Ranges[i];
                    catGO.name = "Category" + range.name;
                    int nrItems = range.Count;
                    int poolIdx = 0;
                    ScrollMenuController scrollMenu = catGO.GetComponent<ScrollMenuController>();
                    for (int j = 0; j < nrItems; j++)
                    {
                        GameObject item = Instantiate(ItemRefs[poolIdx++ % ItemRefs.Length], catGO.transform);
                        item.transform.position = Offset + Vector3.down * (YSpread * (j % 2 * 2 - 1)) + Vector3.right * (j * XSpread);
                        int tmpI = i, tmpJ = j;
                        item.GetComponent<SpriteButton>().onClick.AddListener(() => OnItemClick(tmpI, tmpJ, range.Start + tmpJ));
                        if (LevelIcons != null && LevelIcons.Length > 0)
                        {
                            //Debug.Log(range.Start + tmpJ);
                            //item.transform.Find("Icon").GetComponent<SpriteRenderer>().sprite =
                            //    CaptureManager.Instance.GetExistingLevelSprite(GameType, range.Start + tmpJ, LevelIcons[range.Start + tmpJ]);
                        }
                        //item.transform.Find("Lock").gameObject.SetActive(j >= 2 && !ProductManagerTouch.Instance.IsSubscribed);
                        scrollMenu.Items.Add(item.transform);
                    }
                    scrollMenu.Init();
                    scrollMenu.ResetScroll();
                    CategoriesGOs.Add(scrollMenu);
                }
            }
            else
    		{
                GameObject catGO = Instantiate(ScrollController.gameObject, ScrollController.transform.parent);
                catGO.name = "Slider" + GameType.ToString();
                int nrItems = LevelIcons.Length;
                int poolIdx = 0;
                ScrollMenuController scrollMenu = catGO.GetComponent<ScrollMenuController>();
                for (int j = 0; j < nrItems; j++)
                {
                    GameObject item = Instantiate(ItemRefs[poolIdx++ % ItemRefs.Length], catGO.transform);
                    item.transform.position = Offset + Vector3.down * (YSpread * (j % 2 * 2 - 1)) + Vector3.right * (j * XSpread);
                    int tmpJ = j;
                    item.GetComponent<SpriteButton>().onClick.AddListener(() => OnItemClick(0, tmpJ, tmpJ));
                    if (LevelIcons != null && LevelIcons.Length > 0)
                    {
                        //item.transform.Find("Icon").GetComponent<SpriteRenderer>().sprite =
                        //    CaptureManager.Instance.GetExistingLevelSprite(GameType, j, LevelIcons[j]);
                    }
                    //item.transform.Find("Lock").gameObject.SetActive(j >= 2 && !ProductManagerTouch.Instance.IsSubscribed);
                    scrollMenu.Items.Add(item.transform);
                }
                scrollMenu.Init();
                scrollMenu.ResetScroll();
                CategoriesGOs.Add(scrollMenu);
            }
        }

        void OnCategorySelection(int category, bool snap)
        {
            if (CategoryConfig == null)
    		{
                category = 0;
                snap = true;
            }
            

            if (bubbleHideShowTween != null && bubbleHideShowTween.IsActive())
                bubbleHideShowTween.Complete(true);
            bubbleHideShowTween = null;

            if (snap)
            {
                if (currentCategory >= 0)
                {
                    CategoriesGOs[currentCategory].gameObject.SetActive(false);
                    CategoriesGOs[currentCategory].IsEnabled = false;
                }
                CategoriesGOs[category].gameObject.SetActive(true);
                CategoriesGOs[category].ResetScroll();
                CategoriesGOs[category].ResetItemsPosition();
                CategoriesGOs[category].transform.position = Vector3.zero;
                CategoriesGOs[category].IsEnabled = true;

                voiceHintCooldown = 5.0f;
            }
            else
            {
                CategoriesGOs[category].ResetScroll();
                CategoriesGOs[category].ResetItemsPosition();
                CategoriesGOs[category].transform.position = Vector3.down * 20.0f;
                CategoriesGOs[category].gameObject.SetActive(true);

                Sequence s = DOTween.Sequence();
                if (currentCategory >= 0)
                {
                    int tmpCategory = currentCategory;
                    CategoriesGOs[tmpCategory].IsEnabled = false;
                    s.Append(CategoriesGOs[tmpCategory].transform.DOMove(CategoriesGOs[tmpCategory].transform.position + Vector3.up * 20.0f, 1.0f).SetEase(Ease.InOutQuad));
                    s.Join(DOVirtual.Float(0f, 1.0f, 1.0f, (value) => UpdateBubbles(CategoriesGOs[tmpCategory], value)).SetEase(Ease.Linear));
               
                }
                s.Join(CategoriesGOs[category].transform.DOMove(Vector3.zero, 1.0f).SetEase(Ease.InOutQuad));
                s.Join(DOVirtual.Float(1.0f, 0.0f, 1.0f, (value) => UpdateBubbles(CategoriesGOs[category], value)).SetEase(Ease.Linear));
                if (currentCategory >= 0)
                {
                    int tmpCategory = currentCategory;
                    s.AppendCallback(() => CategoriesGOs[tmpCategory].gameObject.SetActive(false));
                }
                s.AppendCallback(() => CategoriesGOs[category].IsEnabled = true);
                bubbleHideShowTween = s;
            }

            currentCategory = category;
        }

        float bubbleCooldown = -1f; // Minimum time between next bubble emission
        int updateBubblesCallCounter = 0; // Just a hack to count odd and even number of calls to the bubles

    	private void Update()
    	{
            bubbleCooldown -= Time.deltaTime;

            // Animate Bubbles
            ScrollMenuController scrollMenu = CategoriesGOs[currentCategory];
            if (scrollMenu.Initialized)
            { 
                for (int i = 0; i < scrollMenu.Items.Count; i++)
    			{
                    Vector3 position = scrollMenu.Items[i].localPosition;
                    position.y = scrollMenu.ItemsOriginalPositions[i].y + Mathf.Sin(Time.time * 1.5f + i * 2.0f) * 0.25f;
                    scrollMenu.Items[i].localPosition = position;
                }
            }

            if (voiceHintCooldown > 0)
            {
                voiceHintCooldown -= Time.deltaTime;
                if (voiceHintCooldown <= 0)
                {
                    /*
                        Tap and select the painting you want to color. [Color in Environment, Touch and Fill] select_painting_to_color
                        Tap and select the drawing you want to trace. [la Tracing si Outline la leveluri]  select_drawing_trace
                        Tap and select the painting you want to color with crayons. [Color in Environment] select_painting_crayons
                        Tap and select the path you want to trace. [Tracing] tap_select_path_trace
                    */
                    List<string> hintVoices = new List<string>();
                    if (GameDataTouch.Instance.GameType == GameDataTouch.eGameType.TouchAndFill)
                    {
                        hintVoices.Add("select_painting_to_color");
                    }
                    else if (GameDataTouch.Instance.GameType == GameDataTouch.eGameType.Tracing)
                    {
                        hintVoices.Add("tap_select_path_trace");
                        hintVoices.Add("select_drawing_trace");
                    }
                    else if (GameDataTouch.Instance.GameType == GameDataTouch.eGameType.ColorEnvironment)
                    {
                        hintVoices.Add("select_painting_to_color");
                        hintVoices.Add("select_painting_crayons");
                    }
                    else if (GameDataTouch.Instance.GameType == GameDataTouch.eGameType.Outline)
                    {
                        hintVoices.Add("select_drawing_trace");
                    }
                    else if (GameDataTouch.Instance.GameType == GameDataTouch.eGameType.PatternAndStamps)
                    {
                        hintVoices.Add("select_painting_to_color");
                    }

                    if (hintVoices.Count > 0)
                        SoundManagerTouch.Instance.AddSFXToQueue(hintVoices[Random.Range(0, hintVoices.Count)], 1.0f, "voiceover", 2);
                }
            }
        }

    	void UpdateBubbles(ScrollMenuController scrollMenu, float interpolation)
    	{
            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
            bool emitBubble = bubbleCooldown < 0;
            if (emitBubble && ++updateBubblesCallCounter % 2 == 0)
                bubbleCooldown = 0.01f;

            float frequency = Mathf.PI * 2.0f;
            //float interpolationEdges = 1.0f - Mathf.Abs(interpolation - 0.5f) * 2.0f;
            //float amplitude = 2.0f * interpolationEdges;
            float amplitude = 2.0f * Mathf.Clamp01(interpolation * 5.0f);
            for (int i = 0; i < scrollMenu.Items.Count; i++)
    		{
                float offset = Mathf.Sin(interpolation * frequency + i * 2.5f) * amplitude;
                Vector3 position = scrollMenu.Items[i].localPosition;
                position.x = scrollMenu.ItemsOriginalPositions[i].x + offset;
                scrollMenu.Items[i].localPosition = position;

                if (emitBubble && MainActions.BubblesPS != null && Mathf.Abs(scrollMenu.Items[i].position.y) < Camera.main.orthographicSize + 1.0f)
    			{
                    emitParams.position = scrollMenu.Items[i].position + (Random.insideUnitCircle * 2.0f).ToVector3();
                    emitParams.velocity = Random.insideUnitCircle * 5.0f;
                    MainActions.BubblesPS.Emit(emitParams, 1);
    			}
    		}
    	}

        public void UpdateLockStatus()
    	{
            foreach (var scrollMenu in CategoriesGOs)
            {
                for (int i = 0; i < scrollMenu.Items.Count; i++)
                {
                    //scrollMenu.Items[i].transform.Find("Lock").gameObject.SetActive(i >= 2 && !ProductManagerTouch.Instance.IsSubscribed);
                }
            }
        }

        void OnItemClick(int category, int item, int globalIdx)
        {
            Debug.Log(category + " " + item + " " + globalIdx);

    
            if (TransitionManagerTouch.Instance.InTransition)
                return;

            if (OnBoardingControllerTouch.Instance.IsOnBoardingActive)
                return; // Don't do anything bello from here, like hints etc...

      //      if (!ProductManagerTouch.Instance.IsSubscribed && item >= 2)
    		//{
      //          OnBoardingControllerTouch.Instance.ShowInGame(0);
      //          return;
    		//}

            MainActions.CreateJungleMask(category, CategoriesGOs[category].xOffset);

            GameDataTouch.Instance.SelectedLevel = globalIdx;
            GameDataTouch.Instance.GameType = GameType;
            TransitionManagerTouch.Instance.ShowFade(new Color(0.53f, 0.84f, 1.0f), 0.5f, () => SceneLoader.Instance.LoadScene(GameDataTouch.SceneNames[(int)GameType]));
        }

        public void ScrollToOffset(float offset)
    	{
            CategoriesGOs[currentCategory].SetOffset(offset);
    	}
    }


}