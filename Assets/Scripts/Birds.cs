using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;
using System.Collections;

[System.Serializable]
public class BirdsItem : BaseItem
{
    public BirdsDragging drag;
}
public class Birds : Base<BirdsItem>
{
    public string key;
    public int deafultOpening;
    public ScrollRect scrollRect;
    public float scrollSpeed = 10f; // Higher = faster
    protected override void Start()
    {
        levelCounts = PlayerPrefs.GetInt(key,deafultOpening);
        UnlockLevels();
        base.Start();
        int value = Mathf.Clamp((levelCounts), 0, scrollRect.content.transform.childCount - 1);

        RectTransform rect = scrollRect.content.transform.GetChild(value).GetComponent<RectTransform>();


        ScrollTo(rect);

    }
    public void ScrollTo(RectTransform target)
    {
        StartCoroutine(ScrollToCoroutine(target));
    }

    private IEnumerator ScrollToCoroutine(RectTransform target)
    {
        RectTransform content = scrollRect.content;
        RectTransform viewport = scrollRect.viewport;

        // Viewport ka center world point
        Vector3 viewportCenterWorld = viewport.position;

        // Target ka world point
        Vector3 targetWorld = target.position - new Vector3(0, 4, 0);

        // Offset (target ko viewport ke center pe lana hai, vertical me Y axis ka use hoga)
        float diff = viewportCenterWorld.y - targetWorld.y;

        // New Y position (x,z preserve)
        float newY = content.position.y + diff;

        // Clamp karna hoga taake content bahar na nikle
        float contentHeight = content.rect.height;
        float viewportHeight = viewport.rect.height;
        float minY = viewport.position.y - (contentHeight - viewportHeight) * 0.5f;
        float maxY = viewport.position.y + (contentHeight - viewportHeight) * 0.5f;

        newY = Mathf.Clamp(newY, minY, maxY);

        // Smoothly move only Y
        while (Mathf.Abs(content.position.y - newY) > 0.01f)
        {
            float y = Mathf.Lerp(content.position.y, newY, Time.deltaTime * scrollSpeed);
            content.position = new Vector3(content.position.x, y, content.position.z);
            yield return null;
        }

        // Snap to final pos
        content.position = new Vector3(content.position.x, newY, content.position.z);
    }

    protected override void OnItemSelected(BirdsItem item)
    {
        base.OnItemSelected(item);

        //if (item.houseDrag != null)
        //    item.houseDrag.houseItem = item;
    }

    protected override void OnItemLocked(BirdsItem item)
    {
        base.OnItemLocked(item);
        // Optional: show popup, sound, etc.
    }

    // ---------------------- Custom Save / Load Overrides ----------------------

    public override void SaveItemData(BirdsItem item)
    {
        Debug.Log($"Save");
        if (item.item == null)
        {
            Debug.Log($"Update");

            return;
        }
        OnItemSelected(item);
        string key = item.postionkey;
        Debug.Log(key);
        Vector3 pos = item.ItemPostion;
        PlayerPrefs.SetFloat($"{key}_x", pos.x);
        PlayerPrefs.SetFloat($"{key}_y", pos.y);
        PlayerPrefs.Save();

        Debug.Log($" Saved position for {key} at {pos}");
    }
   
    public override void UpdateItemData(BirdsItem item)
    {
        if (item.item == null) return;
        //OnItemSelected(item);
        string key = item.postionkey;

        Vector3 pos = item.ItemPostion;
        PlayerPrefs.SetFloat($"{key}_x", pos.x);
        PlayerPrefs.SetFloat($"{key}_y", pos.y);
        PlayerPrefs.Save();
        SaveItemData(item);
        Debug.Log($" Saved position for {key} at {pos}");
    }

    public override void LoadItemData(BirdsItem item)
    {
        string key = item.postionkey;


        float x = PlayerPrefs.GetFloat($"{key}_x");
        float y = PlayerPrefs.GetFloat($"{key}_y");
        Vector3 savedPos = new Vector3(x, y, 0f);

        GameObject obj = Instantiate(item.ItemUi, savedPos, Quaternion.identity);
        item.item = obj;
        item.ItemPostion = savedPos;
        obj.transform.GetComponent<BirdsItemUi>().Item = item;
        SpriteRenderer[] sprites = obj.GetComponentsInChildren<SpriteRenderer>();
        foreach (var itemss in sprites)
        {
            itemss.maskInteraction = SpriteMaskInteraction.None;
        }
        //AddActiveItem(item);
        obj.transform.GetComponent<BirdController>().enabled = true;
        if (obj.transform.GetChild(0).TryGetComponent<Animator>(out Animator anim))
        {
            anim.enabled = true;
        }
        Debug.Log($" Loaded {key} at {savedPos}");



        if (PlayerPrefs.HasKey($"{key}_x") && PlayerPrefs.HasKey($"{key}_y"))
        {

        }
        else
        {
            //Debug.Log($"❌ No saved position for {item.itemName}");
        }
    }

    public override void RemoveItemData(BirdsItem item)
    {

       
        Debug.Log($"{item.postionkey}");
        string key = item.postionkey;

        PlayerPrefs.DeleteKey($"{key}_x");
        PlayerPrefs.DeleteKey($"{key}_y");
        RemoveActiveItem(item);

        PlayerPrefs.Save();

        Debug.Log($"🗑️ Removed saved position for {item.postionkey}");
    }
    public  void RemoveItemData(int index)
    {
        BirdsItem item = activeItems[index];
        string key = item.postionkey;

        PlayerPrefs.DeleteKey($"{key}_x");
        PlayerPrefs.DeleteKey($"{key}_y");
        RemoveActiveItem(item);

        PlayerPrefs.Save();

        Debug.Log($"🗑️ Removed saved position for {item.itemName}");
    }

    [Header("Load List Settings")]

    public GameObject[] itemObjects;
    public GameObject[] item; // Shared item prefab ya visual
    public GameObject itemcomp; // Shared item prefab ya visual
    public int levelCounts;

    [ContextMenu("UnlockLevls")]
    public void UnlockLevels()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (i > levelCounts)
            {
                items[i].isLocked = true;
                itemObjects[i].GetComponent<BirdsDragging>().item.isLocked = true;
            }
            else
            {
                items[i].isLocked = false;
                itemObjects[i].GetComponent<BirdsDragging>().item.isLocked = false;

            }
        }
        PlayerPrefs.SetInt(key, levelCounts);

    }


    [ContextMenu("SetAnimals")]
    public void SetRange()
    {
        items.Clear(); // Clear existing items

        for (int i = 0; i < itemObjects.Length; i++)
        {
            BirdsItem newItem = new BirdsItem(); // ✅ Correct class

            newItem.itemName = $"Birds{i + 1}";
            newItem.item = itemcomp; // ✔ Set correct visual object (individual)
            newItem.ItemUi = item[i]; // ✔ Set correct visual object (individual)
            newItem.drag = itemObjects[i].GetComponent<BirdsDragging>(); // ✔ Set correct visual object (individual)
                                                                          // ✔ Set correct visual object (individual)
            newItem.itemBtn = itemObjects[i].GetComponent<Button>();
            newItem.lockIcon = itemObjects[i].GetComponent<HandleLock>();
            newItem.isLocked = false; // or false based on logic
            //newItem.isDefault = (i == 0); // Make first item default
            itemObjects[i].GetComponent<BirdsDragging>().item = newItem;
            // Optional: Assign decoration drag
            //newItem.decorationDrag = itemObjects[i].GetComponent<DecorationDragging>();

            items.Add(newItem); // ✅ Add to list
        }

        Debug.Log("Items assigned to Decoration");
    }
    public override BirdsItem CloneItem(BirdsItem original)
    {
        return new BirdsItem
        {
            itemName = original.itemName,
            ItemUi = original.ItemUi,
            isLocked = original.isLocked,
            isDefault = false,
            postionkey = original.postionkey,
            // Don't assign postionkey or item here; assign later
        };
    }


}
