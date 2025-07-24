using UnityEngine;
using UnityEngine.EventSystems;

public class BirdsDragging : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public BirdsItem item;
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

        Vector3 finalPos = spawnedObj.transform.position;

        if (IsWithinBounds(finalPos))
        {
            // ✅ Step 1: Clone the item
            BirdsItem clonedItem = GameManager.instance.birds.CloneItem(item);

            // ✅ Step 2: Assign unique key using time (safe)
            string uniqueKey = $"{clonedItem.itemName}:{GenerateRandomKey()}";
            clonedItem.postionkey = uniqueKey;

            // ✅ Step 3: Assign position and prefab reference
            clonedItem.ItemPostion = finalPos;
            clonedItem.item = spawnedObj;

            // ✅ Step 4: Link UI and set data
            spawnedObj.GetComponent<BirdsItemUi>().Item = clonedItem;

            // ✅ Step 5: Add to active list and save data
            GameManager.instance.birds.AddActiveItem(clonedItem);
            GameManager.instance.birds.SaveData(clonedItem);

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
    public  string GenerateRandomKey(int length = 8)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        System.Random random = new System.Random();
        char[] result = new char[length];

        for (int i = 0; i < length; i++)
        {
            result[i] = chars[random.Next(chars.Length)];
        }

        return new string(result);
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
