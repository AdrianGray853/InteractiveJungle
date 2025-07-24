using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI; // Needed for Image component

public class FoodItems : Draggable

{
    public float rotationSpeed = 180f; // degrees per second
    private Coroutine rotateCoroutine;
    public Sprite[] sprites; // 0 = idle, 1 = dragging
    private Image image;
    public int returnOffset;
    public GameObject fooditem;
    bool dragging;
    protected override void Start()
    {
        startPosition =  transform.position;
        image = GetComponent<Image>();
        if (sprites.Length > 0 && image != null)
        {
            image.sprite = sprites[0]; // Set initial to idle
        }
    }
   
    public override void OnBeginDrag(PointerEventData eventData)
    {
        dragging = true;
       
        base.OnBeginDrag(eventData);
        if (sprites.Length > 0 && image != null)
        {
            image.sprite = sprites[1];
        }
       

    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);

        //if (rotateCoroutine != null)
            //StopCoroutine(rotateCoroutine);
        //if (sprites.Length > 0 && image != null)
        //{
        //    image.sprite = sprites[1];
        //}
      
        //rotateCoroutine = StartCoroutine(RotateToAngle(targetZ));
    }
    private void Update()
    {
        
    }
    public override void OnEndDrag(PointerEventData eventData)
    {
        
        dragging = false;
     
        if(Vector3.Distance(transform.position,startPosition) < returnOffset)
        {
            base.OnEndDrag(eventData);
            if (sprites.Length > 0 && image != null)
            {
                image.sprite = sprites[0];
            }
        }
           

    }

    private IEnumerator RotateToAngle(float targetZ)
    {
        float startZ = transform.eulerAngles.z;
        if (startZ > 180f) startZ -= 360f; // Convert to -180 ~ 180 range

        float elapsed = 0f;
        float duration = Mathf.Abs(targetZ - startZ) / rotationSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float newZ = Mathf.Lerp(startZ, targetZ, t);
            transform.rotation = Quaternion.Euler(0, 0, newZ);
            yield return null;
        }

        transform.rotation = Quaternion.Euler(0, 0, targetZ);
    }
}
