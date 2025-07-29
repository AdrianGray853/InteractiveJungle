using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BackgroundDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public BackgroundItem item;
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
            spawnedObj.GetComponent<SpriteRenderer>().sprite = GetComponent<Image>().sprite;
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

        Vector3 finalPos = spawnedObj.transform.position;


        // ✅ Step 1: Clone the item
        //BackgroundItem clonedItem = GameManager.instance.background.CloneItem(item);

        //// ✅ Step 2: Assign unique key using time (safe)
        //string uniqueKey = $"{clonedItem.itemName}:{GameManager.instance.background.activeItems.Count}";
        //clonedItem.postionkey = uniqueKey;

        //// ✅ Step 3: Assign position and prefab reference
        //clonedItem.ItemPostion = finalPos;
        //clonedItem.item = spawnedObj;

        // ✅ Step 4: Link UI and set data
        //spawnedObj.GetComponent<bACKGR>().Item = clonedItem;

        // ✅ Step 5: Add to active list and save data
        Destroy(spawnedObj);
        if (IsWithinBounds(finalPos))
            GameManager.instance.background.Clicked(item);



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
