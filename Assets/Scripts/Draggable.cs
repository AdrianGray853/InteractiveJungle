using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    protected Canvas canvas;
    protected RectTransform rectTransform;
    protected CanvasGroup canvasGroup;
    public Vector3 startPosition;
    protected Coroutine returnCoroutine;
    public float returnSpeed = 10f;

    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }
    protected virtual void Start()
    {

    }
    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if (GameManager.instance.currentDrag != null) return;

        if (returnCoroutine != null)
            StopCoroutine(returnCoroutine);


        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        //if (GameManager.instance.currentDrag != null) return;

        if (canvas != null)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
        else
        {
            transform.position = Camera.main.ScreenToWorldPoint(new Vector3(
                Input.mousePosition.x,
                Input.mousePosition.y,
                10f
            ));
        }
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        if (GameManager.instance.currentDrag != null) return;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        returnCoroutine = StartCoroutine(SmoothReturnToStart());
    }

    protected virtual IEnumerator SmoothReturnToStart()
    {
        float elapsed = 0f;
        Vector3 current = rectTransform.position;

        while (Vector3.Distance(rectTransform.position, startPosition) > 0.01f)
        {
            elapsed += Time.deltaTime * returnSpeed;
            rectTransform.position = Vector3.Lerp(current, startPosition, elapsed);
            yield return null;
        }

        rectTransform.position = startPosition;
        returnCoroutine = null;
    }
}
