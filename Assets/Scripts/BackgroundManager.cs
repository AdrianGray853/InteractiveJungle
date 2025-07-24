//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class BackgroundManager : MonoBehaviour
//{
//    public List<BaseItem> backgrounds = new List<BaseItem>();
//    public BaseItem activeBackground;

//    private const string SelectedBackgroundKey = "SelectedBackground";

//    void Start()
//    {
//        // Assign click listeners and update lock visuals
//        foreach (var bg in backgrounds)
//        {
//            BaseItem selectedBg = bg;

//            if (bg.lockIcon != null)
//                bg.lockIcon.Lock(bg.isLocked);

//            bg.ItemBtn.onClick.AddListener(() =>
//            {
//                if (!selectedBg.isLocked)
//                {
//                    SetActiveBackground(selectedBg);
//                    SaveSelectedBackground(selectedBg.ItemName);
//                }
//                else
//                {
//                    Debug.LogWarning($"{selectedBg.ItemName} is locked.");
//                }
//            });
//        }

//        // Restore saved background
//        string savedBackgroundName = PlayerPrefs.GetString(SelectedBackgroundKey, "");
//        BaseItem savedBg = backgrounds.Find(b => b.ItemName == savedBackgroundName && !b.isLocked);

//        if (savedBg != null)
//        {
//            SetActiveBackground(savedBg);
//        }
//        else
//        {
//            // Fallback to default background
//            BaseItem defaultBg = backgrounds.Find(b => b.isDefault && !b.isLocked);
//            if (defaultBg != null)
//            {
//                SetActiveBackground(defaultBg);
//            }
//        }
//    }

//    public void SetActiveBackground(BaseItem newBackground)
//    {
//        foreach (var bg in backgrounds)
//        {
//            bool isActive = bg == newBackground;
//            bg.Item.SetActive(isActive);
//        }

//        activeBackground = newBackground;
//        Debug.Log("Switched to background: " + newBackground.Item);
//    }

//    public void UnlockBackground(string name)
//    {
//        foreach (var bg in backgrounds)
//        {
//            if (bg.ItemName == name && bg.isLocked)
//            {
//                bg.isLocked = false;
//                if (bg.lockIcon != null)
//                    bg.lockIcon.Lock(false);

//                // Optional: update PlayerPrefs if it was previously blocked
//                Debug.Log($"{name} unlocked.");
//                break;
//            }
//        }
//    }

//    private void SaveSelectedBackground(string name)
//    {
//        PlayerPrefs.SetString(SelectedBackgroundKey, name);
//        PlayerPrefs.Save();
//    }
//}
