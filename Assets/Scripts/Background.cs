using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class BackgroundItem : BaseItem
{
    public BackgroundDrag drag;
    
}

public class Background : Base<BackgroundItem>
{
    public string key = "Decoration";

    protected override void Start()
    {

        levelCounts = PlayerPrefs.GetInt(key, 0);
        UnlockLevels();
        base.Start();
        Debug.Log("Start");
    }
   
    protected override void OnItemSelected(BackgroundItem item)
    {
        base.OnItemSelected(item);

        //if (item.houseDrag != null)
        //    item.houseDrag.houseItem = item;
    }

    protected override void OnItemLocked(BackgroundItem item)
    {
        base.OnItemLocked(item);
        // Optional: show popup, sound, etc.
    }

    // ---------------------- Custom Save / Load Overrides ----------------------

    public override void SaveItemData(BackgroundItem item)
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
    public override void UpdateItemData(BackgroundItem item)
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

    public override void LoadItemData(BackgroundItem item)
    {

    }
    public override BackgroundItem CloneItem(BackgroundItem original)
    {
        return new BackgroundItem
        {
            itemName = original.itemName,
            ItemUi = original.ItemUi,
            isLocked = original.isLocked,
            isDefault = false,
            postionkey = original.postionkey,
            // Don't assign postionkey or item here; assign later
        };
    }
    public override void RemoveItemData(BackgroundItem item)
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
    public GameObject itemUi; // Shared item prefab ya visual
    public int levelCounts;

    // ✅ Unlock next background
    public int GetNextUnlock()
    {
        int index = levelCounts;
        int nextIndex = index + 1;
        return nextIndex;
    }
    public void UnlockNext()
    {
        int index = levelCounts;
        if (index < 0) return;

        // Next index
        int nextIndex = index + 1;
        if (nextIndex < items.Count && items[nextIndex].isLocked)
        {
            items[nextIndex].isLocked = false;
            items[nextIndex].drag.item.isLocked = false;
            levelCounts = Mathf.Max(levelCounts, nextIndex + 1);

            PlayerPrefs.SetInt(key, levelCounts);
            PlayerPrefs.Save();

            Debug.Log($"🎉 Unlocked {items[nextIndex].itemName}");
        }
    }
    [ContextMenu("UnlockLevls")]
    public void UnlockLevels()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (i > levelCounts)
            {
                items[i].isLocked = true;
                items[i].drag.item.isLocked = true;
            }
            else
            {
                items[i].isLocked = false;
                items[i].drag.item.isLocked = false;

            }
        }
        PlayerPrefs.SetInt(key, levelCounts);

    }

    [ContextMenu("Set Background")]
    public void SetRange()
    {
        items.Clear(); // Clear existing items

        for (int i = 0; i < itemObjects.Length; i++)
        {
            BackgroundItem newItem = new BackgroundItem(); // ✅ Correct class

            newItem.itemName = $"Background{i + 1}";
            newItem.item = item[i]; // ✔ Set correct visual object (individual)
            newItem.ItemUi = itemUi; // ✔ Set correct visual object (individual)

            // ✔ Set correct visual object (individual)
            newItem.itemBtn = itemObjects[i].GetComponent<Button>();
            newItem.lockIcon = itemObjects[i].GetComponent<HandleLock>();
            newItem.isLocked = false; // or false based on logic
            //newItem.isDefault = (i == 0); // Make first item default
            itemObjects[i].GetComponent<BackgroundDrag>().item = newItem;
            // Optional: Assign decoration drag
            newItem.drag = itemObjects[i].GetComponent<BackgroundDrag>();

            items.Add(newItem); // ✅ Add to list
        }

        Debug.Log("Items assigned to Decoration");
    }

}
