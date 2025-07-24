using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[System.Serializable]
public class HouseItem : BaseItem
{
    public HousDragging drag;
    public GameObject ItemUi; // Prefab to instantiate
}

public class House : Base<HouseItem>
{
    public string key = "House";

    protected override void Start()
    {

        levelCounts = PlayerPrefs.GetInt(key, 1);
        UnlockLevels();
        base.Start();

    }
    public override HouseItem CloneItem(HouseItem original)
    {
        return new HouseItem
        {
            itemName = original.itemName,
            ItemUi = original.ItemUi,
            isLocked = original.isLocked,
            isDefault = false,
            postionkey = original.postionkey,
            // Don't assign postionkey or item here; assign later
        };
    }
    protected override void OnItemSelected(HouseItem item)
    {
        base.OnItemSelected(item);

        //if (item.houseDrag != null)
        //    item.houseDrag.houseItem = item;
    }

    protected override void OnItemLocked(HouseItem item)
    {
        base.OnItemLocked(item);
        // Optional: show popup, sound, etc.
    }

    // ---------------------- Custom Save / Load Overrides ----------------------

    public override void SaveItemData(HouseItem item)
    {
        if (item.item == null) return;
        OnItemSelected(item);
        string key = item.postionkey;
        Debug.Log(key);
        Vector3 pos = item.ItemPostion;
        PlayerPrefs.SetFloat($"{key}_x", pos.x);
        PlayerPrefs.SetFloat($"{key}_y", pos.y);
        PlayerPrefs.Save();

        Debug.Log($" Saved position for {key} at {pos}");
    }
    public override void UpdateItemData(HouseItem item)
    {
        if (item.item == null) return;
        //OnItemSelected(item);
        string key = item.postionkey;

        Vector3 pos = item.ItemPostion;
        PlayerPrefs.SetFloat($"{key}_x", pos.x);
        PlayerPrefs.SetFloat($"{key}_y", pos.y);
        PlayerPrefs.Save();

        Debug.Log($" Saved position for {key} at {pos}");
    }

    public override void LoadItemData(HouseItem item)
    {
        string key = item.postionkey;


        float x = PlayerPrefs.GetFloat($"{key}_x");
        float y = PlayerPrefs.GetFloat($"{key}_y");
        Vector3 savedPos = new Vector3(x, y, 0f);

        GameObject obj = Instantiate(item.ItemUi, savedPos, Quaternion.identity);
        item.item = obj;
        item.ItemPostion = savedPos;
        obj.GetComponent<HouseItemUI>().Item = item;
        SpriteRenderer[] sprites = obj.GetComponentsInChildren<SpriteRenderer>();
        foreach (var itemss in sprites)
        {
            itemss.maskInteraction = SpriteMaskInteraction.None;
        }
        //AddActiveItem(item);

        Debug.Log($" Loaded {key} at {savedPos}");



        if (PlayerPrefs.HasKey($"{key}_x") && PlayerPrefs.HasKey($"{key}_y"))
        {

        }
        else
        {
            //Debug.Log($"❌ No saved position for {item.itemName}");
        }
    }

    public override void RemoveItemData(HouseItem item)
    {
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
            if (i >= levelCounts)
            {
                items[i].isLocked = true;
                itemObjects[i].GetComponent<HousDragging>().item.isLocked = true;
            }
            else
            {
                items[i].isLocked = false;
                itemObjects[i].GetComponent<HousDragging>().item.isLocked = false;

            }
        }
        PlayerPrefs.SetInt(key, levelCounts);

    }
    [ContextMenu("FetchChildLocks")]
    public void SetRange()
    {
        items.Clear(); // Clear existing items

        for (int i = 0; i < itemObjects.Length; i++)
        {
            HouseItem newItem = new HouseItem(); // ✅ Correct class

            newItem.itemName = $"House{i + 1}";
            newItem.item = itemcomp; // ✔ Set correct visual object (individual)
            newItem.ItemUi = item[i]; // ✔ Set correct visual object (individual)
            newItem.drag = itemObjects[i].GetComponent<HousDragging>(); // ✔ Set correct visual object (individual)
                                                                        // ✔ Set correct visual object (individual)
            newItem.itemBtn = itemObjects[i].GetComponent<Button>();
            newItem.lockIcon = itemObjects[i].GetComponent<HandleLock>();
            newItem.isLocked = false; // or false based on logic
            //newItem.isDefault = (i == 0); // Make first item default
            itemObjects[i].GetComponent<HousDragging>().item = newItem;
            // Optional: Assign decoration drag
            //newItem.decorationDrag = itemObjects[i].GetComponent<DecorationDragging>();

            items.Add(newItem); // ✅ Add to list
        }

        Debug.Log("Items assigned to Decoration");
    }
}
