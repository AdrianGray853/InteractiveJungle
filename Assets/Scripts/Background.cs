using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class BackgroundItem : BaseItem
{
    // You can add more background-specific fields here if needed (e.g. theme ID, day/night toggle)
}

public class Background : Base<BackgroundItem>
{
    public string key = "Background";
    protected override void Start()
    {
        levelCounts = PlayerPrefs.GetInt(key, 1);
        UnlockLevels();
        base.Start();

    }
    protected override void OnItemSelected(BackgroundItem item)
    {
        base.OnItemSelected(item);
        // Optional: Add fade-in animation or background-specific effects
    }

    protected override void OnItemLocked(BackgroundItem item)
    {
        base.OnItemLocked(item);
        // Optional: show a UI lock warning, animation, etc.
    }

    // Background doesn't need to save position, so these are left empty

    public override void SaveItemData(BackgroundItem item)
    {
        // No additional save logic needed for background
    }

    public override void LoadItemData(BackgroundItem item)
    {
        // No additional load logic needed for background
    }

    public override void RemoveItemData(BackgroundItem item)
    {
        // Nothing to remove
    }
    [Header("Load List Settings")]

    public GameObject[] itemObjects;
    public GameObject[] item; // Shared item prefab ya visual
    //public GameObject itemcomp; // Shared item prefab ya visual
    public int levelCounts;

    [ContextMenu("UnlockLevls")]
    public void UnlockLevels()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (i >= levelCounts)
            {
                items[i].isLocked = true;
            }
            else
            {
                items[i].isLocked = false;

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
            BackgroundItem newItem = new BackgroundItem(); // ✅ Correct class

            newItem.itemName = $"Background{i + 1}";
            newItem.item = item[i]; // ✔ Set correct visual object (individual)
           
                                                                          // ✔ Set correct visual object (individual)
            newItem.itemBtn = itemObjects[i].GetComponent<Button>();
            newItem.lockIcon = itemObjects[i].GetComponent<HandleLock>();
            newItem.isLocked = false; // or false based on logic
            //newItem.isDefault = (i == 0); // Make first item default
            //itemObjects[i].GetComponent<AnimalDragging>().item = newItem;
            // Optional: Assign decoration drag
            //newItem.decorationDrag = itemObjects[i].GetComponent<DecorationDragging>();

            items.Add(newItem); // ✅ Add to list
        }

        Debug.Log("Items assigned to Decoration");
    }
    public override BackgroundItem CloneItem(BackgroundItem original)
    {
        return new BackgroundItem
        {
            itemName = original.itemName,
            item = original.item,
            isLocked = original.isLocked,
            isDefault = false,
            postionkey = original.postionkey,
            // Don't assign postionkey or item here; assign later
        };
    }
}
