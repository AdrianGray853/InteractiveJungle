using UnityEngine;
using UnityEngine.UI;

public class HandleLock : MonoBehaviour
{
    public GameObject[] lockItems;

    [ContextMenu("FetchChildLocks")]
    public void GetValues()
    {
        // Get all child transforms, excluding self
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        var itemsList = new System.Collections.Generic.List<GameObject>();

        foreach (Transform child in allChildren)
        {
            if (child != transform) // Exclude the parent
                itemsList.Add(child.gameObject);
        }

        lockItems = itemsList.ToArray();
    }

    public void Lock(bool check)
    {
        foreach (var item in lockItems)
        {
            item.SetActive(check);
        }
        GetComponent<Button>().interactable = !check;
    }
}
