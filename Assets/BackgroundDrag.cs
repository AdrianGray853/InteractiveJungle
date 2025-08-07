using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BackgroundDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IInitializePotentialDragHandler
{
    public BackgroundItem item;
    private GameObject spawnedObj;
    private Camera mainCam;

    [Header("Valid Placement Bounds")]
    public Vector2 minBounds = new Vector2(-5f, -3f);
    public Vector2 maxBounds = new Vector2(5f, 3f);

    private int activePointerId = -1;
    private bool isDragging = false;
    private bool hasSpawned = false;
    private Vector2 dragStartPos;
    private const float dragThreshold = 20f; // pixels

    private ScrollRect parentScroll;

    private void Start()
    {
        mainCam = Camera.main;
        parentScroll = GetComponentInParent<ScrollRect>();
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        // Allow ScrollRect to work unless horizontal drag confirmed
        if (parentScroll != null)
        {
            parentScroll.OnInitializePotentialDrag(eventData);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item.isLocked)
            return;

        isDragging = true;
        hasSpawned = false;
        dragStartPos = eventData.position;
        activePointerId = eventData.pointerId;

        // Let ScrollRect start first
        if (parentScroll != null)
        {
            parentScroll.OnBeginDrag(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || eventData.pointerId != activePointerId)
            return;

        Vector2 delta = eventData.position - dragStartPos;

        // Not yet started dragging item
        if (!hasSpawned)
        {
            if (Mathf.Abs(delta.x) > dragThreshold && Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                // Now we cancel ScrollRect drag and begin our item drag
                if (parentScroll != null)
                {
                    parentScroll.OnEndDrag(eventData);
                }

                hasSpawned = true;

                if (item.ItemUi != null)
                {
                    transform.localScale *= 1.1f;
                    Vector3 spawnPos = GetWorldPosition(eventData.position);
                    spawnedObj = Instantiate(item.ItemUi, spawnPos, Quaternion.identity);

                    SpriteRenderer sr = spawnedObj.GetComponent<SpriteRenderer>() ?? spawnedObj.GetComponentInChildren<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.sprite = GetComponent<Image>().sprite;
                        sr.sortingOrder = 100;
                    }

                    Sorting sorting = spawnedObj.GetComponent<Sorting>();
                    if (sorting != null)
                        sorting.isDragging = true;
                }
            }
            else
            {
                // Let scroll happen
                if (parentScroll != null)
                {
                    parentScroll.OnDrag(eventData);
                }
                return;
            }
        }

        if (spawnedObj != null)
        {
            spawnedObj.transform.position = GetWorldPosition(eventData.position);
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
            if (parentScroll != null)
            {
                parentScroll.OnEndDrag(eventData);
            }
            return;
        }

        transform.localScale /= 1.1f;

        if (spawnedObj != null)
        {
            Sorting sorting = spawnedObj.GetComponent<Sorting>();
            if (sorting != null)
                sorting.isDragging = false;

            Vector3 finalPos = spawnedObj.transform.position;

            Destroy(spawnedObj);

            if (IsWithinBounds(finalPos))
            {
                GameManager.instance.background.Clicked(item);
            }
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
