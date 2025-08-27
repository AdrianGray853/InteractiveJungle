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

    public class CategorySelector : MonoBehaviour
    {
        public Vector3 SelectedScale = Vector3.one * 1.15f;
        public Collider2D[] Categories;
        public System.Action<int, bool> OnCategorySelect; // CategoryIdx, Snap
        public Transform ShadowSprite;
        public Vector3 ShadowScale = new Vector3(1.05f, 1.03f, 1f);

        int selectedCategory = -1;
        Vector3[] originalScales;
        int lastTouchedCategoryIdx = -1;

        Tween selectTween;

        const float SCALE_TIME = 0.2f;


        private void Awake()
        {
            originalScales = new Vector3[Categories.Length];
            for (int i = 0; i < Categories.Length; i++)
                originalScales[i] = Categories[i].transform.localScale;
        }

        // Start is called before the first frame update
        void Start()
        {
            if (selectedCategory == -1)
                SelectCategory(3, true);    
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Vector3 worldPos = DragManagerTouch.GetWorldSpacePos(touch.position);
                if (touch.phase == TouchPhase.Began)
                {
                    lastTouchedCategoryIdx = GetTouchedCategoryIdx(worldPos);
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    int touchedCategory = GetTouchedCategoryIdx(worldPos);
                    if (touchedCategory == lastTouchedCategoryIdx && touchedCategory >= 0 && touchedCategory != selectedCategory)
                        SelectCategory(touchedCategory);
                }
            }
        }

        int GetTouchedCategoryIdx(Vector3 position)
        {
            int touchedIdx = -1;
            int touchedOrder = -100000;

            for (int i = 0; i < Categories.Length; i++)
            {
                int sortOrder = Categories[i].GetComponent<SpriteRenderer>().sortingOrder;
                if (Categories[i].OverlapPoint(position) && sortOrder > touchedOrder)
                {
                    touchedIdx = i;
                    touchedOrder = sortOrder;
                }
            }
            return touchedIdx;
        }

        void PlayCanvasVoice()
    	{
            /*
                Great choice! [In menu]
                Go ahead and pick your next canvas. [In menu]
                Ready to color? Just choose a painting! [In menu]
                Time to draw! Make your pick [In menu]
            */

            List<string> voiceSounds = new List<string>() { "great_choice", "pick_next_canvas", "time_draw" };
            if (GameDataTouch.Instance.GameType != GameDataTouch.eGameType.Tracing)
                voiceSounds.Add("ready_to_color");
            SoundManagerTouch.Instance.AddSFXToQueue(voiceSounds[Random.Range(0, voiceSounds.Count)], 1.0f, "voiceover", 2);
        }

        public void SelectCategory(int category, bool snap = false)
        {
            if (selectTween != null)
                selectTween.Kill(true);
            selectTween = null;

            if (snap)
            {
                if (selectedCategory >= 0)
                {
                    Categories[selectedCategory].GetComponent<SpriteRenderer>().sortingOrder = 2;
                    Categories[selectedCategory].transform.localScale = originalScales[selectedCategory];
                }
                Categories[category].GetComponent<SpriteRenderer>().sortingOrder = 4;
                Categories[category].transform.localScale = Vector3.Scale(originalScales[category], SelectedScale);
                ShadowSprite.transform.position = Categories[category].transform.position;
                ShadowSprite.transform.localScale = Vector3.Scale(Categories[category].transform.localScale, ShadowScale);

                if (gameObject.activeInHierarchy)
                {
                    PlayCanvasVoice();
                    SoundManagerTouch.Instance.AddSFXToQueue("pick_category", 1.0f, "voiceover", 2, 0.5f);
                }
                else
    			{
                    PlayCanvasVoice();
                }
            }
            else
            {
                Sequence s = DOTween.Sequence();
                if (selectedCategory >= 0)
                {
                    Categories[selectedCategory].GetComponent<SpriteRenderer>().sortingOrder = 2;
                    s.Append(Categories[selectedCategory].transform.DOScale(originalScales[selectedCategory], SCALE_TIME));
                }
                ShadowSprite.transform.position = Categories[category].transform.position;
                ShadowSprite.transform.localScale = Categories[category].transform.localScale;
                Categories[category].GetComponent<SpriteRenderer>().sortingOrder = 4;
                s.Join(Categories[category].transform.DOScale(Vector3.Scale(originalScales[category], SelectedScale), SCALE_TIME));
                s.Join(ShadowSprite.transform.DOScale(Vector3.Scale(originalScales[category], Vector3.Scale(SelectedScale, ShadowScale)), SCALE_TIME));
                selectTween = s;

                string[] categorySounds =
                {
                    "farm", "ocean", "birds", "zoo", "dinosaurs", "others", "vehicles"
                };

                string[] additionalVoice =
                {
                    "time_draw_cute_farm_animals", // farm
                    "dive_sea_imagination", // ocen
                    "choose_adorable_draw", // birds
                    "how_many_cute_animals", // zoo
                    "check_adorable_dinosaurs", // dino
                    "get_creative", // others
                    "ready_draw_awesome_vehicles" // vehicles
                };

                if (gameObject.activeInHierarchy)
                {
                    SoundManagerTouch.Instance.PlaySFX("WinSticker");
                    SoundManagerTouch.Instance.AddSFXToQueue(categorySounds[category], 1.0f, "voiceover", 2);
                    if (Random.value < 0.25f)
                        SoundManagerTouch.Instance.AddSFXToQueue(additionalVoice[category], 1.0f, "voiceover", 1, 0.5f);
                }
            }
            selectedCategory = category;

            OnCategorySelect?.Invoke(category, snap);
        }

        public int GetCategoryIdx() => selectedCategory;
    }


}