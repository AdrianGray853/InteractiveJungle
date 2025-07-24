using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteBtn : MonoBehaviour
{
    [Header("Color Settings")]
    private Color normalColor = Color.white;
    private Color hoverColor = Color.white;
    private Color pressedColor = Color.white;

    //[Header("Scale Settings")]
    //private Vector3 normalScale = new Vector3(0.5f, 0.5f, 0.5f);
    //private Vector3 hoverScale = new Vector3(0.5f, 0.5f, 0.5f);
    //private Vector3 pressedScale = new Vector3(0.5f, 0.5f, 0.5f);

    [Header("Events")]
    public UnityEvent onClick;          // Main action
    public UnityEvent onClickComplete;  // Extra event e.g., close panel

    private SpriteRenderer spriteRenderer;
    private bool isMouseOver = false;
    private bool isPressed = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ResetToNormal();
    }

    private void OnMouseEnter()
    {
        isMouseOver = true;
        if (!isPressed)
        {
            spriteRenderer.color = hoverColor;
            //transform.localScale = hoverScale;
        }
    }

    private void OnMouseExit()
    {
        isMouseOver = false;
        if (!isPressed)
        {
            ResetToNormal();
        }
    }

    private void OnMouseDown()
    {
        if (isMouseOver)
        {
            isPressed = true;
            spriteRenderer.color = pressedColor;
            //transform.localScale = pressedScale;
        }
    }

    private void OnMouseUp()
    {
        if (isMouseOver && isPressed)
        {
            //onClick?.Invoke();         // Run main logic
            //onClickComplete?.Invoke(); // Then close panel or run extra event
        }

        isPressed = false;

        if (isMouseOver)
        {
            spriteRenderer.color = hoverColor;
            //transform.localScale = hoverScale;
        }
        else
        {
            ResetToNormal();
        }
    }

    private void ResetToNormal()
    {
        spriteRenderer.color = normalColor;
        //transform.localScale = normalScale;
    }
}
