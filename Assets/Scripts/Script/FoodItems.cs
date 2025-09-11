using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;

public class FoodItems : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Sprite[] sprites; // [0] idle, [1] dragging
    public float checkRadius = 1.5f;
    public LayerMask animalLayer;
    public GameObject obj;
    private Image image;
    private GameObject spawnedDragObj;
    private Camera cam;
    private Vector3 dragOffset;

    void Start()
    {
        cam = Camera.main;
        image = GetComponent<Image>();
        if (sprites.Length > 0 && image != null)
            image.sprite = sprites[0];
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (spawnedDragObj != null || GameManager.instance.currentDrag != null) return;

        // Instantiate a copy of this object for dragging
        spawnedDragObj = Instantiate(obj, transform.position, Quaternion.identity);
        //Destroy(spawnedDragObj.GetComponent<FoodItems>()); // Remove this script from clone to avoid recursive behavior

        //var dragScript = spawnedDragObj.AddComponent<FoodItemDrag>();
        //dragScript.Init(sprites, checkRadius, animalLayer);

        GameManager.instance.currentSpawnedFood = spawnedDragObj;
        GameManager.instance.currentDrag = spawnedDragObj;
    }
    
    public void OnDrag(PointerEventData eventData) { /* no need to handle here */ }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (spawnedDragObj != null)
        {

            //spawnedDragObj.GetComponent<FoodFollowAndTrigger>().enabled = true;
            spawnedDragObj.GetComponent<FoodFollowAndTrigger>().isPlaced = true;


        }
        GameManager.instance.currentDrag = null;



    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }
#endif
}
