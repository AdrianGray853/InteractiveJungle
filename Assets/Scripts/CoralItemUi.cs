using UnityEngine;

public class CoralItemUi : MonoBehaviour
{
    public coralsItem Item;
    [Header("Valid Placement Bounds")]
    public Vector2 minBounds = new Vector2(-5f, -3f);
    public Vector2 maxBounds = new Vector2(5f, 3f);
    private Vector3 offset;
    private bool isDragging = false;
    private Vector3 dragStartPos;
    private const float dragThreshold = 0.1f; // Small movement threshold to detect dragging

    public void OpenRemoveConsentPanel()
    {
        ConsentManager.Instance.ShowConsent(
            transform.position,
            onYes: () =>
            {
                Debug.Log("Consent accepted");
                GameManager.instance.corals.RemoveItemData(Item);
                Destroy(gameObject);
            },
            onNo: () =>
            {
                Debug.Log("Consent declined");
                // Optional: Play sound or animation here
            }
        );
    }

    public void CloseConsent()
    {
        //ConsentManager.Instance.CloseConsent();
    }
    void OnMouseDown()
    {
        if (UiManager.Instance.panelOpen) return;
        offset = transform.position - GetMouseWorldPos();
        isDragging = true;
        dragStartPos = transform.position;

        // Turn off masking during drag
        SpriteRenderer[] sprites = transform.GetComponentsInChildren<SpriteRenderer>();
        foreach (var item in sprites)
        {
            item.maskInteraction = SpriteMaskInteraction.None;
        }
    }

    void OnMouseDrag()
    {
        if (UiManager.Instance.panelOpen) return;
        if (isDragging)
        {
            transform.position = GetMouseWorldPos() + offset;
        }
    }

    void OnMouseUp()
    {
        if (UiManager.Instance.panelOpen) return;
        isDragging = false;

        // Restore mask visibility
        SpriteRenderer[] sprites = transform.GetComponentsInChildren<SpriteRenderer>();
        foreach (var item in sprites)
        {
            item.maskInteraction = SpriteMaskInteraction.None;
        }

        // Check if outside the aquarium

        // Save new position if inside
        float yPos = transform.position.y;


        Item.ItemPostion = transform.position;
        GameManager.instance.corals?.UpdateItemData(Item);

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
}
