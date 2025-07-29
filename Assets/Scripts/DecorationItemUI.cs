using UnityEngine;

public class DecorationItemUI : MonoBehaviour
{
    public DecorationItem Item;

    [Header("Valid Placement Bounds")]
    public Vector2 minBounds = new Vector2(-5f, -3f);
    public Vector2 maxBounds = new Vector2(5f, 3f);

    private Vector3 offset;
    private bool isDragging = false;
    private Vector3 dragStartPos;
    private const float dragThreshold = 0.1f; // Small movement threshold to detect dragging

    public void OpenRemoveConsentPanel()
    {
        if (UiManager.Instance.isPanelOpen) return;

        ConsentManager.Instance.ShowConsent(
            transform.position,
            onYes: () =>
            {
                Debug.Log("Consent accepted");
                GameManager.instance.decoration.RemoveItemData(Item);
                Destroy(gameObject);
            },
            onNo: () =>
            {
                Debug.Log("Consent declined");
            }
        );
    }

    void OnMouseDown()
    {
        offset = transform.position - GetMouseWorldPos();
        isDragging = true;
        dragStartPos = transform.position;

        // Turn off masking during drag
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
        {
            sr.maskInteraction = SpriteMaskInteraction.None;
        }
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            transform.position = GetMouseWorldPos() + offset;
        }
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;
        SaveAllPositions();
        // ✅ Only open consent if position didn't change significantly
        if (Vector3.Distance(transform.position, dragStartPos) < dragThreshold)
        {
            OpenRemoveConsentPanel();
        }
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 screenMousePos = Input.mousePosition;
        screenMousePos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(screenMousePos);
    }

    private bool IsWithinBounds(Vector3 pos)
    {
        return pos.x >= minBounds.x && pos.x <= maxBounds.x &&
               pos.y >= minBounds.y && pos.y <= maxBounds.y;
    }

    private void OnApplicationQuit()
    {
        SaveAllPositions();
    }
    public void SaveAllPositions()
    {

        Item.ItemPostion = transform.position;
        GameManager.instance.decoration?.UpdateItemData(Item);
    }

}
