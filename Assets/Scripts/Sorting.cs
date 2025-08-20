using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(SortingGroup))]
public class Sorting : MonoBehaviour
{
    private SortingGroup sortingGroup;
    private Renderer rend; // to get bounds

    public bool isDragging;
    public bool rag;
    public float offsetY;

    void Start()
    {
        sortingGroup = GetComponent<SortingGroup>();
        //rend = GetComponentInChildren<Renderer>(); // catch child sprite if needed
    }

    void LateUpdate()
    {
        if (isDragging)
        {
            sortingGroup.sortingLayerName = "Spawned";
            return;
        }

        // get the lowest Y point (legs)
        float y = (rend != null ? rend.bounds.min.y : transform.position.y) - offsetY;

        if (rag)
        {
            sortingGroup.sortingLayerName = "Default"; // typo fixed (was "Deafult")
            return;
        }

        // Dynamically assign Sorting Layer based on Y position
        if (y > 2)
            sortingGroup.sortingLayerName = "Layer01";
        else if (y > 0)
            sortingGroup.sortingLayerName = "Layer02";
        else
            sortingGroup.sortingLayerName = "Layer03";

        // Adjust sortingOrder for fine-tuned depth
        sortingGroup.sortingOrder = Mathf.RoundToInt(-y * 100);
    }
}
