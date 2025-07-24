using UnityEngine;

public class AquariumItemUI : MonoBehaviour
{
    public SpriteRenderer dirtRender;

    [ContextMenu("FetchChildLocks")]

    public void SetDecoration()
    {
        dirtRender =transform.GetChild(3).GetComponent<SpriteRenderer>();
    }
}
