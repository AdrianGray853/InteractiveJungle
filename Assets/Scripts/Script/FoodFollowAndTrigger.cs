using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class FoodFollowAndTrigger : MonoBehaviour
{
    public float checkRadius = 1.5f;
    public LayerMask animalLayer;
    public Sprite[] sprites; // [0] idle, [1] dragging

    private Camera cam;
    private Image image;
    private bool isInteracted = false;
    private bool isReturning = false;
    private Vector3 originalPosition;
    public bool isPlaced;
    void Start()
    {
        cam = Camera.main;
        image = GetComponent<Image>();

        if (sprites.Length > 1 && image != null)
            image.sprite = sprites[1]; // dragging sprite

        originalPosition = transform.position;
        StartCoroutine(AutoReturnAfterTime(10f)); // 10 seconds
    }

    void Update()
    {
        //#if UNITY_EDITOR
        //        if (Input.GetMouseButton(0) && !isPlaced)
        //        {
        //            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        //            mousePos.z = 0;
        //            transform.position = mousePos;
        //        }
        //        else if (!isInteracted)
        //        {
        //            CheckProximity();
        //        }
        //#else
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {
                    if (!isPlaced)
                    {

                        Vector3 touchPos = cam.ScreenToWorldPoint(touch.position);
                    touchPos.z = 0;
                    transform.position = touchPos;
                    }
                }
                else if (touch.phase == TouchPhase.Ended && !isInteracted)
                {
                    CheckProximity();
                }
            }
        }
        CheckProximity();

        //#endif
    }

    void CheckProximity()
    {
        if (isInteracted || !isPlaced)
        {
            return;
        }
        Collider2D hit = Physics2D.OverlapCircle(transform.position, checkRadius, animalLayer);
        if (hit != null && hit.gameObject.GetComponent<AnimalController>())
        {
            isInteracted = true;

            Debug.Log($"Food reached {hit.name}");

            if (sprites.Length > 0 && image != null)
                image.sprite = sprites[0]; // back to idle sprite
            AnimalController animal = hit.gameObject.GetComponent<AnimalController>();
            animal.eat = true;
            StopAllCoroutines();
            //Destroy(gameObject);
            StartCoroutine(FeedAndDisable());
            gameObject.SetActive(false);
            transform.position = animal.currentFood.position;

        }
        else
        {
            // Optional: Destroy if outside bounds
        }
    }

    private IEnumerator FeedAndDisable()
    {
        yield return new WaitForSeconds(0.0f);
        //Destroy(gameObject); // disable drag behavior after feeding
    }
    public void CompleteEating()
    {
        gameObject.SetActive(true);
        StartCoroutine(AutoReturnAfterTime(0.1f)); // 10 seconds
        //isInteracted = false;
        isReturning = false;
    }
    private IEnumerator AutoReturnAfterTime(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (!isReturning)
        {
            isReturning = true;

            // Change sprite back to idle
            if (sprites.Length > 0 && image != null)
                image.sprite = sprites[0];

            // Smooth move back to original position
            float duration = 0.5f;
            float elapsed = 0f;
            Vector3 start = transform.position;

            while (elapsed < duration)
            {
                transform.position = Vector3.Lerp(start, originalPosition, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = originalPosition;
            Destroy(gameObject); // disable drag behavior after feeding

        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }
#endif
}
