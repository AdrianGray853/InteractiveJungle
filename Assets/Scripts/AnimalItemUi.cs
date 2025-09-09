using UnityEngine;
using UnityEngine.EventSystems;

public class AnimalItemUi : MonoBehaviour
{
    public AnimalItem Item;

    [Header("Valid Placement Bounds")]
    public Vector2 minBounds = new Vector2(-5f, -3f);
    public Vector2 maxBounds = new Vector2(5f, 3f);

    private Vector3 offset;
    private bool isDragging = false;
    private Vector3 dragStartPos;
    private const float dragThreshold = 0.1f;

    public void OpenRemoveConsentPanel()
    {
        ConsentManager.Instance.ShowConsent(
            transform.position,
            onYes: () =>
            {
                Debug.Log("Consent accepted");
                GameManager.instance.animal.RemoveItemData(Item);
                Destroy(gameObject);
            },
            onNo: () =>
            {
                Debug.Log("Consent declined");
            }
        );
    }

    void Update()
    {
        // Don’t drag if a panel is open or something else is being dragged
        if (UiManager.Instance.panelOpen ||
            (GameManager.instance.currentDrag != null && GameManager.instance.currentDrag != gameObject))
            return;

#if UNITY_EDITOR
        HandleMouseInput();
#else
        HandleTouchInput();
#endif
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (IsUIBlocked()) return;

            Vector3 worldPos = GetMouseWorldPos();
            if (IsPointerOverGameObject(worldPos))
            {
                GameManager.instance.currentDrag = gameObject;
                offset = transform.position - worldPos;
                isDragging = true;
                dragStartPos = transform.position;
                SetMask(false);


            }
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            transform.position = GetMouseWorldPos() + offset;
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            EndDrag();
        }
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 worldPos = GetTouchWorldPos(touch.position);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (IsUIBlocked()) return;

                    if (IsPointerOverGameObject(worldPos))
                    {
                        GameManager.instance.currentDrag = gameObject;
                        offset = transform.position - worldPos;
                        isDragging = true;
                        dragStartPos = transform.position;
                        SetMask(false);
                    }
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (isDragging)
                        transform.position = worldPos + offset;
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (isDragging)
                        EndDrag();
                    break;
            }
        }
    }

    void EndDrag()
    {
        GameManager.instance.currentDrag = null;
        isDragging = false;

        if (!IsWithinBounds(transform.position))
        {
            GameManager.instance.animal.RemoveItemData(Item);
            Destroy(gameObject);
            return;
        }

        Item.ItemPostion = transform.position;
        GameManager.instance.animal?.UpdateItemData(Item);

        if (Vector3.Distance(transform.position, dragStartPos) < dragThreshold)
        {
            OpenRemoveConsentPanel();
        }
        else
        {
            SoundManager.Instance.PlayClickSFX(SFXType.DragAndDrop);

        }
        SetMask(true);
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 screenPos = Input.mousePosition;
        screenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(screenPos);
    }

    Vector3 GetTouchWorldPos(Vector2 screenPos)
    {
        Vector3 screenPos3D = new Vector3(screenPos.x, screenPos.y, Camera.main.WorldToScreenPoint(transform.position).z);
        return Camera.main.ScreenToWorldPoint(screenPos3D);
    }

    bool IsWithinBounds(Vector3 pos)
    {
        return pos.x >= minBounds.x && pos.x <= maxBounds.x &&
               pos.y >= minBounds.y && pos.y <= maxBounds.y;
    }

    void SetMask(bool enable)
    {
        // Optional: sprite masking logic
    }

    bool IsPointerOverGameObject(Vector3 worldPos)
    {
        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        return hit != null && hit.transform == transform;
    }

    /// <summary>
    /// Prevents drag when pointer is over UI elements (e.g., buttons)
    /// </summary>
    bool IsUIBlocked()
    {
#if UNITY_EDITOR
        return EventSystem.current.IsPointerOverGameObject();
#else
        if (Input.touchCount > 0)
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        return false;
#endif
    }
}
