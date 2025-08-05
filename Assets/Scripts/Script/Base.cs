using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using static UnityEditor.Progress;



[System.Serializable]
public class BaseItem
{
    public string itemName;
    public GameObject item;
    public Button itemBtn;
    public HandleLock lockIcon;
    /*[HideInInspector]*/
    public string postionkey;
    public bool isLocked = false;
    public bool isDefault = false;
    public Vector2 ItemPostion;
    public GameObject ItemUi; // Prefab to instantiate

}

public enum SelectionMode
{
    Single,
    Multiple
}

public abstract class Base<T> : MonoBehaviour where T : BaseItem
{
    [Header("Selection Settings")]
    public SelectionMode selectionMode = SelectionMode.Single;

    [Header("Item List")]
    public List<T> items = new List<T>();

    public List<T> activeItems = new List<T>();

    protected string PlayerPrefsKey => $"Selected_{GetType().Name}";

    protected virtual void Start()
    {
        foreach (var item in items)
        {
            T capturedItem = item;

            if (item.lockIcon != null)
                item.lockIcon.Lock(item.isLocked);

            if (item.itemBtn != null)
            {
                item.itemBtn.onClick.AddListener(() =>
                {
                    Clicked(capturedItem);
                });
            }
        }

        LoadSavedItems();
    }

    public virtual void Clicked(T capturedItem)
    {
        if (!capturedItem.isLocked)
        {
            if (selectionMode == SelectionMode.Single)
            {
                SetActiveItem(capturedItem);
            }
            else
            {
                if (!activeItems.Contains(capturedItem))
                    AddActiveItem(capturedItem);
            }
            SaveData(capturedItem);
        }
        else
        {
            OnItemLocked(capturedItem);
        }
    }

    public virtual void SetActiveItem(T newItem)
    {
        foreach (var item in items)
            item.item?.SetActive(item == newItem);
        newItem.item?.SetActive(true);
        activeItems.Clear();
        activeItems.Add(newItem);
        Debug.Log($"Save Item");
        OnItemSelected(newItem);
        SaveSelectedItems();
        SaveItemData(newItem);
    }

    public virtual void AddActiveItem(T newItem)
    {
        newItem.item?.SetActive(true);

        //if (!activeItems.Contains(newItem))
        activeItems.Add(newItem);


    }
    public void SaveData(T newItem)
    {

        OnItemSelected(newItem);
        //SaveSelectedItems();
        AddItem(newItem.postionkey);
        SaveItemData(newItem);
    }

    public virtual void RemoveActiveItem(T item)
    {
        //if (!activeItems.Contains(item)) return;
        T match = activeItems.Find(x => x.postionkey == item.postionkey);




        int index = activeItems.IndexOf(match);
        //match.item?.SetActive(false);
        Destroy(match.item);

        activeItems.Remove(match);
        RemoveItem(match.postionkey);
        //SaveSelectedItems();
        //RemoveItemData(match);

        Debug.Log($"Removed: {match.postionkey}");
    }

    public void UnlockItem(string name)
    {
        foreach (var item in items)
        {
            if (item.itemName == name && item.isLocked)
            {
                item.isLocked = false;
                item.lockIcon?.Lock(false);
                Debug.Log($"{name} unlocked.");
                break;
            }
        }
    }

    protected virtual void OnItemSelected(T item) { }

    protected virtual void OnItemLocked(T item)
    {
        Debug.LogWarning($"{item.itemName} is locked.");
    }

    // ------------------- Core Save/Load for Selected Items -------------------

    public virtual void SaveSelectedItems()
    {
        // Format: itemName:index
        List<string> entries = new List<string>();
        for (int i = 0; i < activeItems.Count; i++)
        {
            string key = $"{activeItems[i].itemName}:{i}";

            entries.Add(key);
        }

        string joined = string.Join(",", entries);
        PlayerPrefs.SetString(PlayerPrefsKey, joined);
        PlayerPrefs.Save();

        Debug.Log($"[SAVE SelectedItems With Index] {joined}");
    }
    public virtual void RemoveItem(string postion)
    {
        // Get saved string
        string saved = PlayerPrefs.GetString(PlayerPrefsKey, "");

        if (string.IsNullOrEmpty(saved))
            return;

        List<string> entries = saved.Split(',').ToList();

        // Remove the exact match
        if (entries.Remove(postion))
        {
            string updated = string.Join(",", entries);
            PlayerPrefs.SetString(PlayerPrefsKey, updated);
            PlayerPrefs.Save();

            Debug.Log($"[Removed {postion}] → Updated: {updated}");
        }
        else
        {
            Debug.LogWarning($"[RemoveItem] Postion not found: {postion}");
        }
    }
    public virtual void AddItem(string position)
    {
        // Get the existing saved string
        string saved = PlayerPrefs.GetString(PlayerPrefsKey, "");

        List<string> entries = string.IsNullOrEmpty(saved)
            ? new List<string>()
            : saved.Split(',').ToList();

        // Avoid duplicates
        if (!entries.Contains(position))
        {
            entries.Add(position);
            string updated = string.Join(",", entries);
            PlayerPrefs.SetString(PlayerPrefsKey, updated);
            PlayerPrefs.Save();

            Debug.Log($"[Added {position}] → Updated: {updated}");
        }
        else
        {
            Debug.LogWarning($"[AddItem] Position already exists: {position}");
        }
    }


    public virtual bool isAvalible(string itemName)
    {
        return activeItems.Any(item => item.itemName == itemName);

        //string saved = PlayerPrefs.GetString(PlayerPrefsKey, "");
        //if (!string.IsNullOrEmpty(saved))
        //{
        //    string[] savedNames = saved.Split(',');

        //    foreach (var name in savedNames)
        //    {

        //        if (name == itemName)
        //            return true;


        //    }
        //}
        //return false;

    }
    public virtual void LoadSavedItems()
    {
        string saved = PlayerPrefs.GetString(PlayerPrefsKey, "");

        if (!string.IsNullOrEmpty(saved))
        {
            string[] savedEntries = saved.Split(',');
            Debug.Log($"saved : {saved}");
            int i = 0;
            foreach (var entry in savedEntries)
            {
                Debug.Log(i);
                i++;
                string[] parts = entry.Split(':');
                if (parts.Length != 2) continue;

                string itemName = parts[0];
                // int index = int.Parse(parts[1]); // ✅ if you want to use the index for any logic later

                T match = items.Find(x => x.itemName == itemName && !x.isLocked);

                if (match != null)
                {
                    if (selectionMode == SelectionMode.Single)
                    {
                        SetActiveItem(match);
                        break;
                    }
                    else
                    {


                        // ✅ Clone the item instead of using original
                        T cloned = CloneItem(match);
                        cloned.postionkey = entry;
                        // activate the clone in scene

                        AddActiveItem(cloned);
                        LoadItemData(cloned);
                    }
                    //LoadItemData(match);
                }
            }
        }
        else
        {
            // Load default item(s)
            T defaultItem = items.Find(x => x.isDefault && !x.isLocked);
            if (defaultItem != null)
            {
                if (selectionMode == SelectionMode.Single)
                    SetActiveItem(defaultItem);
                else
                {

                }

                Debug.Log($"LoadDeafult: {defaultItem.itemName}");

                LoadItemData(defaultItem);
            }
        }
    }

    public virtual string GenerateRandomKey(int length = 8)
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


    public abstract T CloneItem(T original);

    // ------------------- Virtual Save Hooks (Override in Derived) -------------------

    public virtual void SaveItemData(T item)
    {
        // Override in child (e.g. for saving positions)
    }

    public virtual void LoadItemData(T item)
    {
        // Override in child (e.g. for loading positions)
    }
    public virtual void RemoveItemData(T item)
    {
        // Override in child (e.g. to delete saved data)
    }
    public virtual void UpdateItemData(T item)
    {
        // Override in child (e.g. to delete saved data)
    }
}
