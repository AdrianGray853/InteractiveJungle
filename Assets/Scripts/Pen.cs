using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Pen : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public RectTransform drawingArea;
    public GameObject brushPrefab;
    public float brushSize = 10f;
    public float spacing = 5f; // Distance between points to draw a continuous line
    private bool check;
    private Vector2 previousPosition;

    public void EditChart(bool _check)
    {
        check = _check;
        if (check)
        {

            Time.timeScale = 0;

        }
        else if (!check)
        {

            Time.timeScale = 1;
        }

    }
    private void Draw(Vector2 position)
    {
        if (check)
        {
            GameObject brush = Instantiate(brushPrefab);
            brush.transform.SetParent(drawingArea, false);
            brush.transform.localPosition = position;
            brush.GetComponent<RectTransform>().sizeDelta = new Vector2(brushSize, brushSize);
        }
    }

    private void DrawBetweenPoints(Vector2 start, Vector2 end)
    {
        float distance = Vector2.Distance(start, end);
        Vector2 direction = (end - start).normalized;

        for (float i = 0; i < distance; i += spacing)
        {
            Vector2 position = start + direction * i;
            Draw(position);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(drawingArea, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);
        Draw(localPoint);
        previousPosition = localPoint;
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(drawingArea, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);
        DrawBetweenPoints(previousPosition, localPoint);
        previousPosition = localPoint;
    }
}
