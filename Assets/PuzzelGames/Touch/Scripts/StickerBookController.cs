using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.Touch
{
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class StickerBookController : MonoBehaviour
    {

        public float DropDistance = 1.0f;

        public GameObject TouchAndFillGO;
        public StickerPageDesc[] TouchAndFillPages;
        public CategoryRangeConfigSO TouchAndFillConfig;
        public GameObject ColoringGO;
        public StickerPageDesc[] ColoringEnvPages;
        public CategoryRangeConfigSO ColoringEnvConfig;
        public GameObject StampsAndPatternsGO;
        public StickerPageDesc[] StampsAndPatternsPages;
        public CategoryRangeConfigSO StampsAndPatternsConfig;
        public GameObject OutlineGO;
        public StickerPageDesc[] OutlinePages;
        public CategoryRangeConfigSO OutlineStickers;
        public GameObject TracingGO;
        public StickerPageDesc[] TracingPages;
        public CategoryRangeConfigSO TracingConfig;

        public ScrollPanelTouch ScrollController;

        // For Sticker Book
        public GameObject NextPageButton;
        public GameObject PrevPageButton;
        public GameObject PhotoButton;
        public SpriteRenderer PhotoFrame;
        public GameObject[] HideFromPhotoObjects;
        public SpriteRenderer Paper;

        public CatAnimationController catAnimator;

        GameDataTouch.eGameType gameType;
        int currentPageIdx = 0;
        StickerPageDesc[] currentPages;

        bool introAnimationFinised = false;

        // Start is called before the first frame update
        void Awake()
        {
            SetStickerIndices(TouchAndFillConfig, TouchAndFillPages);
            SetStickerIndices(ColoringEnvConfig, ColoringEnvPages);
            SetStickerIndices(StampsAndPatternsConfig, StampsAndPatternsPages);
            SetStickerIndices(OutlineStickers, OutlinePages);
            SetStickerIndices(TracingConfig, TracingPages);
        }

        void SetStickerIndices(CategoryRangeConfigSO config, StickerPageDesc[] pages)
    	{
            foreach (var page in pages)
    		{
                foreach (var sticker in page.StickerTargets)
    			{
                    bool found = false;
                    foreach (var range in config.Ranges)
                    {
                        for (int i = 0; i < range.Stickers.Length; i++)
                        {
                            if (sticker.Unlocked.sprite == range.Stickers[i])
    						{
                                sticker.StickerIdx = range.Start + i;
                                found = true;
                                break;
    						}
                        }
                        if (found)
                            break;
                    }
    			}
    		}
    	}

        public StickerPageDesc[] GetPageDescFromGameType(GameDataTouch.eGameType gameType)
    	{
            if (gameType == GameDataTouch.eGameType.TouchAndFill)
            {
                return TouchAndFillPages;
            }
            else if (gameType == GameDataTouch.eGameType.ColorEnvironment)
            {
                return ColoringEnvPages;
            }
            else if (gameType == GameDataTouch.eGameType.PatternAndStamps)
            {
                return StampsAndPatternsPages;
            }
            else if (gameType == GameDataTouch.eGameType.Outline)
            {
                return OutlinePages;
            }
            else if (gameType == GameDataTouch.eGameType.Tracing)
            {
                return TracingPages;
            }

            return null;
        }

        public void IntroAnimationDone()
    	{
            if (gameType == GameDataTouch.eGameType.TouchAndFill)
            {
                InitSection(TouchAndFillGO, TouchAndFillPages);
            }
            else if (gameType == GameDataTouch.eGameType.ColorEnvironment)
            {
                InitSection(ColoringGO, ColoringEnvPages);
            }
            else if (gameType == GameDataTouch.eGameType.PatternAndStamps)
            {
                InitSection(StampsAndPatternsGO, StampsAndPatternsPages);
            }
            else if (gameType == GameDataTouch.eGameType.Outline)
            {
                InitSection(OutlineGO, OutlinePages);
            }
            else if (gameType == GameDataTouch.eGameType.Tracing)
            {
                InitSection(TracingGO, TracingPages);
            }

            introAnimationFinised = true;
        }

        public void Init(GameDataTouch.eGameType gameType)
    	{
            this.gameType = gameType;
            currentPageIdx = 0;
            currentPages = GetPageDescFromGameType(gameType);

            TouchAndFillGO.SetActive(false);
            ColoringGO.SetActive(false);
            StampsAndPatternsGO.SetActive(false);
            OutlineGO.SetActive(false);
            TracingGO.SetActive(false);

            NextPageButton.SetActive(false);
            PrevPageButton.SetActive(false);

            InitStickers(currentPages);

            introAnimationFinised = false;

            if (ScrollController.GetItems().Count > 0)
                SoundManagerTouch.Instance.PlaySFX("explore_drag_sticker_album", 1.0f, "voiceover", 3);
        }

        public void RemoveStickers()
    	{
            ScrollController.ReturnAllItems();
        }

        void InitSection(GameObject parent, StickerPageDesc[] pages, int page = 0)
    	{
            parent.SetActive(true);
            Debug.Assert(pages != null, "Please add some pages to " + parent.name);

            currentPageIdx = page;
            currentPages = pages;

            UpdatePages();
        }

        void InitStickers(StickerPageDesc[] pages)
    	{
            for (int i = 0; i < pages.Length; i++)
            {
                foreach (var sticker in pages[i].StickerTargets)
                {
                    if (ProgressManagerTouch.Instance.IsOnBookSticker(gameType, sticker.StickerIdx))
                    {
                        sticker.Locked.gameObject.SetActive(false);
                        sticker.Unlocked.gameObject.SetActive(true);
                    }
                    else if (ProgressManagerTouch.Instance.IsStickerUnlocked(gameType, sticker.StickerIdx)) // Patch this to show all stickers!
    				{
                        sticker.Locked.gameObject.SetActive(true);
                        sticker.Unlocked.gameObject.SetActive(true);
                        ScrollController.AddItem(sticker.Unlocked, sticker.StickerIdx);
                    }
                    else
    				{
                        sticker.Locked.gameObject.SetActive(true);
                        sticker.Unlocked.gameObject.SetActive(false);
                    }
                }
            }
        }

        void UpdatePages()
    	{
            for (int i = 0; i < currentPages.Length; i++)
            {
                currentPages[i].gameObject.SetActive(i == currentPageIdx);
            }

            NextPageButton.SetActive(currentPageIdx < currentPages.Length - 1);
            PrevPageButton.SetActive(currentPageIdx > 0);
        }

        public void NextPage()
    	{
            if (currentPageIdx < currentPages.Length - 1)
                currentPageIdx++;

            UpdatePages();

            SoundManagerTouch.Instance.PlaySFX("ChangePage");
        }

        public void PreviousPage()
    	{
            if (currentPageIdx > 0)
                currentPageIdx--;

            UpdatePages();
            SoundManagerTouch.Instance.PlaySFX("ChangePage");
        }

    	public bool TryDrop(ScrollPanelTouch.ItemDesc item)
    	{
            var stickerTargets = currentPages[currentPageIdx].StickerTargets;
            foreach (var sticker in stickerTargets)
            {
                if (sticker.StickerIdx != item.stickerIdx)
                    continue;

                if (item.collider.transform.position.Distance(sticker.Locked.transform.position) < DropDistance)
    			{
                    item.collider.transform.position = sticker.Locked.transform.position;
                    item.collider.transform.parent = item.parent;

                    // This automatically removes the sticker from Unlocked!
                    ProgressManagerTouch.Instance.AddToBookSticker(gameType, sticker.StickerIdx);
                    sticker.Locked.gameObject.SetActive(false);
                    SoundManagerTouch.Instance.PlaySFX("Click");
                    catAnimator.TriggerHearts();
                    return true;
    			}
            }
            return false;
    	}

        public void CapturePage()
    	{
            if (!introAnimationFinised)
                return; 
            CaptureManager.Instance.Screenshot(HideFromPhotoObjects, Paper.bounds, PhotoFrame, PhotoButton);
    	}

    	private void OnEnable()
    	{
            if (PhotoButton != null) // Shitty Unity Editor check
                PhotoButton.SetActive(true);
    	}

    	private void OnDisable()
    	{
            if (PhotoButton != null) // Same as above!
                PhotoButton.SetActive(false);
            RemoveStickers();
    	}

        public void PlayCorkSound()
    	{
            SoundManagerTouch.Instance.PlaySFX("CorkPop");
    	}

        public void PlayBookPage()
        {
            SoundManagerTouch.Instance.PlaySFX("BookPage");
        }
    }


}