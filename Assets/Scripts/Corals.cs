﻿using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[System.Serializable]
public class coralsItem : BaseItem
{
    public CoralDragging drag;
}
public class Corals : Base<coralsItem>
{
    public string key = "Corals";

    protected override void Start()
    {

        levelCounts = PlayerPrefs.GetInt(key, 1);
        UnlockLevels();
        base.Start();


    }
    public override coralsItem CloneItem(coralsItem original)
    {
        return new coralsItem
        {
            itemName = original.itemName,
            ItemUi = original.ItemUi,
            isLocked = original.isLocked,
            isDefault = false,
            postionkey = original.postionkey,
            // Don't assign postionkey or item here; assign later
        };
    }
    protected override void OnItemSelected(coralsItem item)
    {
        base.OnItemSelected(item);

        //if (item.houseDrag != null)
        //    item.houseDrag.houseItem = item;
    }

    protected override void OnItemLocked(coralsItem item)
    {
        base.OnItemLocked(item);
        // Optional: show popup, sound, etc.
    }

    // ---------------------- Custom Save / Load Overrides ----------------------

    public override void SaveItemData(coralsItem item)
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
    public override void UpdateItemData(coralsItem item)
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

    public override void LoadItemData(coralsItem item)
    {
        string key = item.postionkey;


        float x = PlayerPrefs.GetFloat($"{key}_x");
        float y = PlayerPrefs.GetFloat($"{key}_y");
        Vector3 savedPos = new Vector3(x, y, 0f);

        GameObject obj = Instantiate(item.ItemUi, savedPos, Quaternion.identity);
        item.item = obj;
        item.ItemPostion = savedPos;
        obj.GetComponent<CoralItemUi>().Item = item;
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

    public override void RemoveItemData(coralsItem item)
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
                itemObjects[i].GetComponent<CoralDragging>().item.isLocked = true;
            }
            else
            {
                items[i].isLocked = false;
                itemObjects[i].GetComponent<CoralDragging>().item.isLocked = false;

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
            coralsItem newItem = new coralsItem(); // ✅ Correct class

            newItem.itemName = $"Corals{i + 1}";
            newItem.item = itemcomp; // ✔ Set correct visual object (individual)
            newItem.ItemUi = item[i]; // ✔ Set correct visual object (individual)
            newItem.drag = itemObjects[i].GetComponent<CoralDragging>(); // ✔ Set correct visual object (individual)
                                                                          // ✔ Set correct visual object (individual)
            newItem.itemBtn = itemObjects[i].GetComponent<Button>();
            newItem.lockIcon = itemObjects[i].GetComponent<HandleLock>();
            newItem.isLocked = false; // or false based on logic
            //newItem.isDefault = (i == 0); // Make first item default
            itemObjects[i].GetComponent<CoralDragging>().item = newItem;
            // Optional: Assign decoration drag
            //newItem.decorationDrag = itemObjects[i].GetComponent<DecorationDragging>();

            items.Add(newItem); // ✅ Add to list
        }

        Debug.Log("Items assigned to Decoration");
    }
}
