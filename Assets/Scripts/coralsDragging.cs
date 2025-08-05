using UnityEngine;
using UnityEngine.EventSystems;

public class CoralDragging : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    public coralsItem item;
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
        if (item.isLocked || GameManager.instance.animal.isAvalible(item.itemName))
            return;
        if (item.isLocked) return;
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
        if (item.isLocked || GameManager.instance.animal.isAvalible(item.itemName))
            return;
        if (spawnedObj != null)
        {
            Vector3 pos = GetWorldPosition(eventData.position);
            spawnedObj.transform.position = pos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (item.isLocked || GameManager.instance.animal.isAvalible(item.itemName))
            return;
        if (spawnedObj == null) return;
        spawnedObj.GetComponent<Sorting>().isDragging = false;

        Vector3 finalPos = spawnedObj.transform.position;

        if (IsWithinBounds(finalPos))
        {



            item.ItemPostion = finalPos;
            spawnedObj.GetComponent<CoralItemUi>().Item = item;
            int index = GameManager.instance.house.activeItems.Count;
            string uniqueKey = $"{item.itemName}:{index}";
            item.postionkey = uniqueKey;
            GameManager.instance.corals.AddActiveItem(item);
            GameManager.instance.corals.SaveData(item);
            //GameManager.instance.house.SaveItemData(houseItem);
            SpriteRenderer[] sprites = spawnedObj.GetComponentsInChildren<SpriteRenderer>();
            foreach (var item in sprites)
            {
                item.maskInteraction = SpriteMaskInteraction.None;
            }
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