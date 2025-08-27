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

    public class ScrollPanelTouch : MonoBehaviour
    {
        public float Size;
        public float ItemWidth = 1f;
        public float ItemHeight = 1f;
        public float Spacing = 0.2f;

        public float DetachDistance = 1.0f; // This settings is used to detach the item when dragpos exceed this value on X but not NoDetachDistance on Y
        public float NoDetachDistance = 2.0f; // See above ^

        public float ScrollRecoverSpeed = 3.0f;

        public Transform Center;
        public Collider2D Collider;
        public Transform Panel;

        public StickerBookController StickerBook;

        public Vector2 TargetResolution = new Vector2(2778.0f, 1284.0f); // IPhone 13 Pro Max

        float ScrollValue = 0f;
        int ItemIndexInAnimation = -1; // This is used to create forced space for the item that gets back to it's location, as we loose track of IsDragging and all items return to their place!
        Vector3 FingerOffset = new Vector2(-1.0f, 1.0f);

        public class ItemDesc
    	{
            public SpriteRenderer sprite;
            public Vector3 originalScale;
            public Vector3 originalPosition;
            public Collider2D collider;
            public Tween animation;
            public float offset;

            public int stickerIdx;
            public Transform parent;
        }

        List<ItemDesc> Items = new List<ItemDesc>();

        class DragDesc
    	{
            public ItemDesc Item;

            public int FingerId = -1;

            public Vector3 StartPos; // Touch initial position
            public Vector3 Position; // Touch current position
            public Vector3 DeltaPos; // Touch position difference from last update

            public bool IsDragging = false; // It is dragging and object? Relates to Item
            public bool IsScrolling = false; // Is it scrolling?

            public bool Released = false; // Was released on this frame
            public bool Pressed = false; // Was pressed on this frame

            public bool CanDetach = true; // Can grab the item and detach it from the scroll box
            public int OriginalIndex; // Index from where in array it was detached
        
            public bool IsItemReturning = false; // The item is getting back to the list (animating) we should not react as it will lead to numerous problems!
        }

        DragDesc DragInfo = new DragDesc();
        int SpriteOrder = 101;

    	// Start is called before the first frame update
    	void Awake()
        {
            /*
            Vector3 position = Panel.transform.localPosition;
            float targetRatio = TargetResolution.x / TargetResolution.y;
            float currentRatio = (float)Screen.width / Screen.height;
            float ratio = currentRatio / targetRatio;
            position.x = position.x * ratio + (1.0f - ratio) * 10.0f;
            Panel.transform.localPosition = position;
            ItemHeight *= ratio;
            ItemWidth *= ratio;
            Spacing *= ratio;
            Vector3 scale = transform.localScale;
            scale.x *= ratio;
            transform.localScale = scale;
            */
        }

        // Update is called once per frame
        void Update()
        {
            UpdateItemOffsets();
            UpdateDragInfo();
            UpdateDrag();
            UpdateScroll();
            UpdateItemsPosition();
            SoftClampScroll();
        }

        void UpdateItemOffsets()
        {
            float offset = 0f;
            int startIndex = 0;

            if (DragInfo.IsDragging)
            {
                offset = ItemHeight + Spacing - Mathf.Min(ItemHeight + Spacing, Mathf.Abs((DragInfo.Position - Center.position).x));
                startIndex = DragInfo.OriginalIndex;
            } else if (ItemIndexInAnimation >= 0)
            {
                offset = ItemHeight + Spacing - Mathf.Min(ItemHeight + Spacing, Mathf.Abs((DragInfo.Item.collider.transform.position - Center.position).x));
                startIndex = ItemIndexInAnimation;
            }

            SnapOffset(offset, startIndex);
            /*
            for (int i = 0; i < Items.Count; i++)
            {
                if (DragInfo.Item == null)
                {
                    if (i >= startIndex)
                        Items[i].offset = Mathf.MoveTowards(Items[i].offset, offset, 5.0f * Time.deltaTime);
                    else
                        Items[i].offset = Mathf.MoveTowards(Items[i].offset, 0f, 5.0f * Time.deltaTime);
                } 
                else
                {
                    if (i >= startIndex)
                        Items[i].offset = offset;
                    else
                        Items[i].offset = 0f;
                }
            }
            */
        }

        void SnapOffset(float offset, int startIndex = 0)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (i >= startIndex)
                    Items[i].offset = offset;
                else
                    Items[i].offset = 0f;
            }
        }

    	void UpdateDragInfo()
    	{
            DragInfo.Released = false;
            DragInfo.Pressed = false;

            if (DragInfo.IsDragging || DragInfo.IsScrolling)
    		{
                for (int i = 0; i < Input.touchCount; i++)
    			{
                    Touch touch = Input.GetTouch(i);
                    if (touch.fingerId == DragInfo.FingerId)
    				{
                        Vector3 touchPos = DragManagerTouch.GetWorldSpacePos(touch.position).SetZ(0);
                        DragInfo.DeltaPos = touchPos - DragInfo.Position;
                        DragInfo.Position = touchPos;
                        if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                        {
                            if (DragInfo.IsScrolling)
                                DragInfo.Item = null; // Because we loose track of the state remove the item, otherwise we'll confuse ourselves that we were dragging an item...

                            DragInfo.IsDragging = false;
                            DragInfo.IsScrolling = false;
                            DragInfo.Released = true;
                            DragInfo.FingerId = -1;
                        }
                        break;
    				}
    			}
    		} 
            else if (!DragInfo.IsItemReturning)
    		{
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    Vector3 touchPos = DragManagerTouch.GetWorldSpacePos(touch.position).SetZ(0);
                    if (touch.phase == TouchPhase.Began && Collider.OverlapPoint(touchPos))
    				{
                        DragInfo.StartPos = touchPos;
                        DragInfo.Position = touchPos;
                        DragInfo.DeltaPos = Vector3.zero;
                        DragInfo.IsScrolling = true;
                        DragInfo.IsDragging = false;
                        DragInfo.Pressed = true;
                        DragInfo.CanDetach = true;
                        DragInfo.Item = null;
                        DragInfo.FingerId = touch.fingerId;
    				}
                }
            }
    	}
    	void UpdateDrag()
    	{
            if (DragInfo.Pressed)
    		{
                Vector3 boxSize = new Vector3(ItemWidth, ItemHeight);
                for (int i = 0; i < Items.Count; i++)
    			{
                    //if (Items[i].collider.OverlapPoint(DragInfo.StartPos))
                    if (UtilsTouch.IsPointInBox2D(DragInfo.StartPos, Items[i].collider.transform.position, boxSize)) 
    				{
                        DragInfo.Item = Items[i];
                        DragInfo.OriginalIndex = i;
                        DragInfo.Item.sprite.sortingOrder = SpriteOrder++;
                        SoundManagerTouch.Instance.PlaySFX("PuzzlePlace");
                        break;
    				}
    			}
    		} 
            else if (DragInfo.Released)
    		{
                if (DragInfo.Item != null)
    			{
                    if (!StickerBook.TryDrop(DragInfo.Item))
    				{
                        Vector3 targetPos = GetItemPosition(DragInfo.OriginalIndex);
                        /*
                        if (DragInfo.OriginalIndex > 0 && DragInfo.OriginalIndex < Items.Count)
    					{ // Get a middle spot
                            targetPos = (GetItemPosition(DragInfo.OriginalIndex - 1) + targetPos) * 0.5f;
    					}
                        */
                        ItemIndexInAnimation = DragInfo.OriginalIndex;
                        if (DragInfo.Item.animation != null)
                            DragInfo.Item.animation.Kill(true);
                        DragInfo.IsItemReturning = true;
                        DragInfo.Item.animation = DragInfo.Item.sprite.transform.DOMove(targetPos, 0.3f).OnComplete(() =>
                            {
                                ItemIndexInAnimation = -1;
                                AttachItem(DragInfo.Item, DragInfo.OriginalIndex);
                                SnapOffset(0f, DragInfo.OriginalIndex);
                                DragInfo.IsItemReturning = false;
                                SoundManagerTouch.Instance.PlaySFX("PuzzlePlace");
                            });
    				} 
                    //else
                    //{
                    //    SoftClampScroll();
                    //}
    			}
    		}

            if (DragInfo.IsScrolling)
    		{
                if (Mathf.Abs((DragInfo.Position - DragInfo.StartPos).y) > NoDetachDistance)
    			{ // Can't detach any longer, out of detach range
                    DragInfo.CanDetach = false;
    			}
                if (DragInfo.CanDetach && 
                    DragInfo.Item != null &&
                    Mathf.Abs((DragInfo.Position - DragInfo.StartPos).x) > DetachDistance)
    			{
                    DragInfo.IsDragging = true;
                    DragInfo.IsScrolling = false;
                    DetachItem(DragInfo.Item);
                    SnapOffset(ItemHeight + Spacing, DragInfo.OriginalIndex);
                }
    		}
            else if (DragInfo.IsDragging)
    		{
                DragInfo.Item.sprite.transform.position = DragInfo.Position + FingerOffset;
    		}
    	}

    	void UpdateScroll()
    	{
            if (DragInfo.IsScrolling)
    		{
                ScrollValue += DragInfo.DeltaPos.y;
                //SoftClampScroll();
    		}
    	}

        void SoftClampScroll()
        {
            if (DragInfo.IsDragging || ItemIndexInAnimation >= 0)
                return;
            float WantedValue = Mathf.Clamp(ScrollValue, 0f, GetScrollMax());
            ScrollValue += (WantedValue - ScrollValue) * Time.deltaTime * ScrollRecoverSpeed;
            //UpdateItemsPosition();
        }


    	public void AddItem(SpriteRenderer sprite, int stickerIdx)
    	{
            ItemDesc item = new ItemDesc
                {
                    sprite = sprite,
                    originalScale = sprite.transform.localScale,
                    originalPosition = sprite.transform.localPosition,
                    collider = sprite.GetComponent<Collider2D>(),
                    stickerIdx = stickerIdx,
                    parent = sprite.transform.parent
                };
            item.collider.transform.parent = transform;

            SetupItem(sprite);
            Items.Add(item);
        }

        public void RemoveItem(int stickerIdx)
        {
            Items.RemoveAll(x => x.stickerIdx == stickerIdx);

            UpdateItemsPosition();
        }

        public void ReturnAllItems()
    	{
            foreach (var item in Items)
    		{
                if (item.animation != null)
                    item.animation.Kill(true);

                item.collider.transform.parent = item.parent;
                item.sprite.transform.localScale = item.originalScale;
                item.sprite.maskInteraction = SpriteMaskInteraction.None;
                item.collider.transform.position = item.originalPosition;
    		}
            Clear();
            UpdateItemsPosition();
    	}

        public void UpdateItemsPosition()
    	{
            Vector3 startPos = Center.position;
            startPos.y += Size * 0.5f - ItemHeight * 0.5f + ScrollValue;
            Vector3 increment = Vector3.up * (ItemHeight + Spacing);

            for (int i = 0; i < Items.Count; i++)
    		{
                Items[i].sprite.transform.position = startPos - increment * i + Vector3.down * Items[i].offset;
    		}
    	}

        public Vector3 GetItemPosition(int index)
    	{
    		Vector3 startPos = Center.position;
    		startPos.y += Size * 0.5f - ItemHeight * 0.5f + ScrollValue;
    		Vector3 increment = Vector3.up * (ItemHeight + Spacing);

    		return startPos - increment * index;
    	}

        void SetupItem(SpriteRenderer sprite)
    	{
            float scale = GetFitScale(sprite);
            sprite.transform.localScale = Vector3.one * scale;

            sprite.sortingOrder = 100;
            sprite.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
    	}

        float GetFitScale(SpriteRenderer sprite)
    	{
    		float wantedRatio = ItemWidth / ItemHeight;
    		float spriteRatio = sprite.bounds.size.x / sprite.bounds.size.y;
    		float scale;
    		if (spriteRatio > wantedRatio)
    		{ // Width fit
    			scale = ItemWidth / sprite.bounds.size.x;
    		}
    		else
    		{ // Height fit
    			scale = ItemHeight / sprite.bounds.size.y;
    		}
            scale /= sprite.transform.parent.lossyScale.x;
            scale *= sprite.transform.localScale.x;
            return scale;
    	}

        float GetScrollMax()
        {
            float scrollMax = Items.Count * (ItemHeight + Spacing) - Spacing - Size;// Camera.main.orthographicSize * 2.0f;
             return Mathf.Max(0, scrollMax);
        }

        public List<ItemDesc> GetItems()
    	{
            return Items;
    	}

    	void DetachItem(ItemDesc item)
    	{
            Items.Remove(item);
            //UpdateItemsPosition();

            if (item.animation != null)
                item.animation.Kill(true);
       
            item.animation = item.sprite.transform.DOScale(item.originalScale, 0.3f).SetEase(Ease.OutBack);
            item.sprite.maskInteraction = SpriteMaskInteraction.None;
        }

    	void AttachItem(ItemDesc item, int index)
    	{
            if (index < 0 || index > Items.Count)
                Items.Add(item);
            else
    		    Items.Insert(index, item);

    		//UpdateItemsPosition();

    		if (item.animation != null)
    			item.animation.Kill(true);

    		float scale = GetFitScale(item.sprite);
            item.animation = item.sprite.transform.DOScale(Vector3.one * scale, 0.3f).SetEase(Ease.InBack);
            item.sprite.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }

    	public void Clear()
    	{
            Items.Clear();
            ScrollValue = 0f;
        }

        public void NotifyLevelChange()
        {
            //gameObject.SetActive(true);
        }

    	private void OnDrawGizmosSelected()
    	{
            if (Center == null)
                return;

    		Gizmos.color = Color.green;
            Gizmos.DrawWireCube(Center.position, new Vector3(ItemWidth, Size, 0f));

            Gizmos.color = Color.blue;
            Vector3 itemPos = Center.position;
            itemPos.y -= Spacing * 0.5f + ItemHeight * 0.5f;
            Gizmos.DrawWireCube(itemPos, new Vector3(ItemWidth, ItemHeight, 0f));
    		itemPos = Center.position;
    		itemPos.y += Spacing * 0.5f + ItemHeight * 0.5f;
            Gizmos.DrawWireCube(itemPos, new Vector3(ItemWidth, ItemHeight, 0f));
        }

    #if UNITY_EDITOR
    	private void OnValidate()
    	{
            UpdateItemsPosition();
        }
    #endif
    }


}