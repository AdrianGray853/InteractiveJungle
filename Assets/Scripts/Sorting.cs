using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(SortingGroup))]
public class Sorting : MonoBehaviour
{
    private SortingGroup sortingGroup;

    void Start()
    {
        sortingGroup = GetComponent<SortingGroup>();
    }

    void LateUpdate()
    {
        float y = transform.position.y;

        // Dynamically assign Sorting Layer based on Y position
        if (y > 2)
            sortingGroup.sortingLayerName = "Layer01";
        else if (y > 0)
            sortingGroup.sortingLayerName = "Layer02";
        else
            sortingGroup.sortingLayerName = "Layer03";

        // Adjust sortingOrder for fine-tuned depth within layer
        sortingGroup.sortingOrder = Mathf.RoundToInt(-y * 100);
    }
}
