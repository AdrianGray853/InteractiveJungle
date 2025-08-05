using UnityEngine;

public class BirdsItemUi : MonoBehaviour
{
    public BirdsItem Item;
    [Header("Valid Placement Bounds")]
    public Vector2 minBounds = new Vector2(-5f, -3f);
    public Vector2 maxBounds = new Vector2(5f, 3f);
    private Vector3 offset;
    private bool isDragging = false;
    private Vector3 dragStartPos;
    private const float dragThreshold = 0.1f; // Small movement threshold to detect dragging

    public void OpenRemoveConsentPanel()
    {
        if (UiManager.Instance.panelOpen) return;

        ConsentManager.Instance.ShowConsent(
            transform.position,
            onYes: () =>
            {
                Debug.Log("Consent accepted");
                GameManager.instance.birds.RemoveItemData(Item);
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
    private void OnApplicationQuit()
    {
        SaveAllPositions();
    }
    public void SaveAllPositions()
    {

        Item.ItemPostion = transform.position;
        GameManager.instance.birds?.UpdateItemData(Item);
    }
    private bool IsWithinBounds(Vector3 pos)
    {
        return pos.x >= minBounds.x && pos.x <= maxBounds.x &&
               pos.y >= minBounds.y && pos.y <= maxBounds.y;
    }
}
