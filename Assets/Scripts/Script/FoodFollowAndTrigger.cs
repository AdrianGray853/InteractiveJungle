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
#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            transform.position = mousePos;
        }
        else if (!isInteracted)
        {
            CheckProximity();
        }
#else
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {
                    Vector3 touchPos = cam.ScreenToWorldPoint(touch.position);
                    touchPos.z = 0;
                    transform.position = touchPos;
                }
                else if (touch.phase == TouchPhase.Ended && !isInteracted)
                {
                    CheckProximity();
                }
            }
        }
#endif
    }

    void CheckProximity()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, checkRadius, animalLayer);
        if (hit != null)
        {
            isInteracted = true;

            Debug.Log($"Food reached {hit.name}");

            if (sprites.Length > 0 && image != null)
                image.sprite = sprites[0]; // back to idle sprite
            hit.gameObject.GetComponent<AnimalController>().eat = true;// ();// = false;
            StopAllCoroutines();
            Destroy(gameObject);
            StartCoroutine(FeedAndDisable());
        }
        else
        {
            // Optional: Destroy if outside bounds
        }
    }

    private IEnumerator FeedAndDisable()
    {
        yield return new WaitForSeconds(0.0f);
        Destroy(this); // disable drag behavior after feeding
    }

    private IEnumerator AutoReturnAfterTime(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (!isInteracted && !isReturning)
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
            Destroy(this); // Disable script after return
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
