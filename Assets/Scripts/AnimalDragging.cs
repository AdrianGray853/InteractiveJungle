using UnityEngine;
using UnityEngine.EventSystems;

public class AnimalDragging : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    public AnimalItem item;
    private GameObject spawnedObj;

    private Camera mainCam;

    [Header("Valid Placement Bounds")]
    public Vector2 minBounds = new Vector2(-5f, -3f);
    public Vector2 maxBounds = new Vector2(5f, 3f);

    private void Start()
    {
        mainCam = Camera.main;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item.isLocked)
            return;
        if (item.ItemUi != null)
        {
            Vector3 spawnPos = GetWorldPosition(eventData.position);
            spawnedObj = Instantiate(item.ItemUi, spawnPos, Quaternion.identity);
            spawnedObj.GetComponent<Sorting>().isDragging = true;

            SpriteRenderer[] sprites = spawnedObj.GetComponentsInChildren<SpriteRenderer>();
            foreach (var item in sprites)
            {
                item.maskInteraction = SpriteMaskInteraction.None;
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (spawnedObj != null)
        {
            Vector3 pos = GetWorldPosition(eventData.position);
            spawnedObj.transform.position = pos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (spawnedObj == null) return;
        spawnedObj.GetComponent<Sorting>().isDragging = false;

        Vector3 finalPos = spawnedObj.transform.position;

        if (IsWithinBounds(finalPos))
        {
            // ✅ Step 1: Clone the item
            AnimalItem clonedItem = GameManager.instance.animal.CloneItem(item);

            // ✅ Step 2: Assign unique key using time (safe)
            string uniqueKey = $"{clonedItem.itemName}:{GameManager.instance.GenerateRandomKey()}";
            clonedItem.postionkey = uniqueKey;

            // ✅ Step 3: Assign position and prefab reference
            clonedItem.ItemPostion = finalPos;
            clonedItem.item = spawnedObj;

            // ✅ Step 4: Link UI and set data
            spawnedObj.GetComponent<AnimalItemUi>().Item = clonedItem;

            // ✅ Step 5: Add to active list and save data
            GameManager.instance.animal.AddActiveItem(clonedItem);
            GameManager.instance.animal.SaveData(clonedItem);

            // ✅ Optional: turn off sprite masking
            SpriteRenderer[] sprites = spawnedObj.GetComponentsInChildren<SpriteRenderer>();
            foreach (var s in sprites)
                s.maskInteraction = SpriteMaskInteraction.None;
        }
        else
        {
            Destroy(spawnedObj);
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