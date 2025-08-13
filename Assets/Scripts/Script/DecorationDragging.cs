using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DecorationDragging : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public DecorationItem item;



    public int maxQty = 1;            // Max allowed in scene
    public int qtyCount;              // Remaining count
    public TMP_Text countText;
   

    [Header("Valid Placement Bounds")]
    public Vector2 minBounds = new Vector2(-5f, -3f);
    public Vector2 maxBounds = new Vector2(5f, 3f);

    private GameObject spawnedObj;
    private Camera mainCam;
    private ScrollRect parentScroll;
    private Image[] images;

    private int activePointerId = -1;
    private bool isDragging = false;
    private bool hasSpawned = false;
    private Vector2 dragStartPos;
    private const float dragThreshold = 20f;

    private void Start()
    {
        mainCam = Camera.main;
        parentScroll = GetComponentInParent<ScrollRect>();
        images = GetComponentsInChildren<Image>(true);

        if (countText == null)
            countText = GetComponentInChildren<TMP_Text>();


        qtyCount = maxQty; // start with full qty
        UpdateQtyFromScene();
    }

    private void Update()
    {
        // Runtime check: Update remaining qty & UI live
        UpdateQtyFromScene();
    }

    private void UpdateQtyFromScene()
    {
        if (item.isLocked) return;

        int placedCount = GameManager.instance.decoration.GetActiveItemCount(item.itemName);
        qtyCount = Mathf.Max(0, maxQty - placedCount);
        RefreshUI();
    }

    public bool options;
    private void RefreshUI()
    {
        if (countText != null)
            countText.text = qtyCount.ToString();

        foreach (var img in images)
            img.color = qtyCount < 1 ? Color.gray : Color.white;

        options = !(qtyCount < 1);

        float alpha = qtyCount > 0 ? 1f : 0.5f;
        foreach (var img in images)
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        parentScroll?.OnInitializePotentialDrag(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //if (qtyCount <= 0 || item.isLocked)
        //{
        //    parentScroll?.OnBeginDrag(eventData);
        //    hasSpawned = false;

        //    return; }

        isDragging = true;
        hasSpawned = false;
        dragStartPos = eventData.position;
        activePointerId = eventData.pointerId;

        parentScroll?.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || eventData.pointerId != activePointerId)
            return;

        Vector2 delta = eventData.position - dragStartPos;

        if (!hasSpawned)
        {
            // Horizontal drag to spawn
            if ((Mathf.Abs(delta.x) > dragThreshold && Mathf.Abs(delta.x) > Mathf.Abs(delta.y)) && (options) && !item.isLocked)
            {
                parentScroll?.StopMovement();
                hasSpawned = true;

                if (item.ItemUi != null)
                {
                    Vector3 spawnPos = GetWorldPosition(eventData.position);
                    spawnedObj = Instantiate(item.ItemUi, spawnPos, Quaternion.identity);
                    spawnedObj.GetComponent<Sorting>().isDragging = true;
                   
                }
            }
            else
            {
                // Still vertical scroll
                parentScroll?.OnDrag(eventData);
                return;
            }
        }

        if (spawnedObj != null)
            spawnedObj.transform.position = GetWorldPosition(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging || eventData.pointerId != activePointerId)
            return;

        isDragging = false;
        activePointerId = -1;

        if (!hasSpawned)
        {
            parentScroll?.OnEndDrag(eventData);
            return;
        }

        if (spawnedObj == null) return;

        spawnedObj.GetComponent<Sorting>().isDragging = false;

        Vector3 finalPos = spawnedObj.transform.position;

        if (IsWithinBounds(finalPos))
        {
            DecorationItem clonedItem = GameManager.instance.decoration.CloneItem(item);
            clonedItem.postionkey = $"{clonedItem.itemName}:{GameManager.instance.GenerateRandomKey()}";
            clonedItem.ItemPostion = finalPos;
            clonedItem.item = spawnedObj;


            spawnedObj.GetComponent<DecorationItemUI>().Item = clonedItem;
            GameManager.instance.decoration.AddActiveItem(clonedItem);
            GameManager.instance.decoration.SaveData(clonedItem);

            if (spawnedObj.TryGetComponent<Animator>(out Animator anim))
                anim.enabled = true;
            UpdateQtyFromScene(); // immediately refresh after placing
        }
        else
        {
            qtyCount++;
            foreach (var img in images)
                img.color = Color.white;
            Destroy(spawnedObj);
        }

        ExecuteEvents.Execute(parentScroll.gameObject, eventData, ExecuteEvents.endDragHandler);
    }

    private Vector3 GetWorldPosition(Vector3 screenPosition)
    {
        Vector3 worldPos = mainCam.ScreenToWorldPoint(screenPosition);
        worldPos.z = 0f;
        return worldPos;
    }

    private bool IsWithinBounds(Vector3 pos)
    {
        return pos.x >= minBounds.x && pos.x <= maxBounds.x &&
               pos.y >= minBounds.y && pos.y <= maxBounds.y;
    }
}

