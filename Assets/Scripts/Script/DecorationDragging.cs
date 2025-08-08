using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DecorationDragging : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public DecorationItem item;


    private GameObject spawnedObj;
    private Camera mainCam;

    [Header("Valid Placement Bounds")]
    public Vector2 minBounds = new Vector2(-5f, -3f);
    public Vector2 maxBounds = new Vector2(5f, 3f);

    private Vector2 dragStartPos;
    private bool isDragging = false;
    private bool hasSpawned = false;
    private int activePointerId = -1;

    private const float dragThreshold = 20f;

    public ScrollRect parentScroll;

    private void Start()
    {
        mainCam = Camera.main;
        parentScroll = GetComponentInParent<ScrollRect>();
        Refresh();


    }
    private void Update()
    {
        Refresh();

    }
    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        parentScroll?.OnInitializePotentialDrag(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
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

        // Spawn only after horizontal drag crosses threshold
        if (!hasSpawned)
        {
            if (Mathf.Abs(delta.x) > dragThreshold && Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                    if (item.isLocked || GameManager.instance.decoration.isAvalible(item.itemName))
                    {
                        isDragging = false;
                        return;
                    }

                hasSpawned = true;
                parentScroll?.OnEndDrag(eventData);

                if (item.ItemUi != null)
                {
                    Vector3 spawnPos = GetWorldPosition(eventData.position);
                    spawnedObj = Instantiate(item.ItemUi, spawnPos, Quaternion.identity);
                    GameManager.instance.currentDrag = spawnedObj;
                    spawnedObj.GetComponent<Sorting>().isDragging = true;
                }
            }
            else
            {
                parentScroll?.OnDrag(eventData);
                return;
            }
        }

        if (spawnedObj != null)
        {
            Vector3 pos = GetWorldPosition(eventData.position);
            spawnedObj.transform.position = pos;
        }
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

        if (spawnedObj == null)
            return;

        Vector3 finalPos = spawnedObj.transform.position;

        if (IsWithinBounds(finalPos))
        {
            spawnedObj.GetComponent<Sorting>().isDragging = false;

            DecorationItem clonedItem = GameManager.instance.decoration.CloneItem(item);
            string uniqueKey = $"{clonedItem.itemName}:{GameManager.instance.GenerateRandomKey()}";
            clonedItem.postionkey = uniqueKey;
            clonedItem.ItemPostion = finalPos;
            clonedItem.item = spawnedObj;

            spawnedObj.GetComponent<DecorationItemUI>().Item = clonedItem;

            GameManager.instance.decoration.AddActiveItem(clonedItem);
            GameManager.instance.decoration.SaveData(clonedItem);

         
        }
        else
        {
            Destroy(spawnedObj);
        }
        Refresh();
        GameManager.instance.currentDrag = null;

    }

    public void Refresh()
    {
        // Dim the item if it's not available
        if (GameManager.instance.decoration.isAvalible(item.itemName))
        {
            SetAlpha(0.85f); // 95% transparent
        }
        else
        {
            SetAlpha(1f); // Fully visible
        }
    }

    private void SetAlpha(float alpha)
    {
        Image[] images = GetComponentsInChildren<Image>(true);
        foreach (var img in images)
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }
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

