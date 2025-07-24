using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;


public class Cleaning : Draggable
{
    public Sprite[] sprites; // 0 = idle, 1 = dragging
    private Image image;

    private bool isDragging = false;
    private Quaternion originalRotation;
    private Vector3 originalPosition;

    [Header("Rotation Target")]
    public Transform fixedTarget;

    [Header("Cleaning Settings")]
    public float cleanAmount = 0.1f; // how much to clean per second
    public float overlapRadius = 30f; // UI pixels distance from overlay center
    public float angle = 30f; // UI pixels distance from overlay center
    //public GameObject waterDroping;
    protected override void Start()
    {
        image = GetComponent<Image>();
        originalRotation = transform.rotation;
        originalPosition = transform.position;

        if (image != null && sprites.Length > 0)
        {
            image.sprite = sprites[0]; // idle
        }
    }

    protected void Update()
    {
        if (isDragging)
        {
            RotateTowardsTarget();

            // Cleaning logic: only clean if overlapping the dirt area
            TryCleanIfTouchingOverlay();

        }
    }

    private void RotateTowardsTarget()
    {
        Vector3 targetPos = fixedTarget != null ? fixedTarget.position : originalPosition;
        Vector3 direction = targetPos - transform.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private void TryCleanIfTouchingOverlay()
    {
      
     
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        isDragging = true;
        startPosition = rectTransform.position;

        if (image != null && sprites.Length > 1)
            image.sprite = sprites[1];
       
        //waterDroping.SetActive(true);


    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        isDragging = false;
        //waterDroping.SetActive(false);

        if (image != null && sprites.Length > 0)
            image.sprite = sprites[0];
      

        StartCoroutine(SmoothReturnToStartAndRotation());
    }

    private IEnumerator SmoothReturnToStartAndRotation()
    {
        float duration = 0.3f;
        float elapsed = 0f;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            transform.position = Vector3.Lerp(startPos, originalPosition, t);
            transform.rotation = Quaternion.Lerp(startRot, originalRotation, t);

            yield return null;
        }

        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {

    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Dirty"))
        {

        }
    }
}
