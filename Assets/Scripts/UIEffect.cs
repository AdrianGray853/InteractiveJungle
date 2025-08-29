using UnityEngine;

public class UIEffect : MonoBehaviour
{
    public enum EffectType
    {
        None,
        Rotate,
        ScaleUp,
        ScaleDown,
        Pulse,
        Bounce,
        FadeIn,
        FadeOut
    }

    [Header("Effect Settings")]
    public EffectType effectType = EffectType.None;
    public float speed = 2f;
    public float rotateSpeed = 90f; // degrees per second
    public float scaleAmount = 1.2f;
    public float fadeDuration = 1f;

    private Vector3 originalScale;
    private CanvasGroup canvasGroup;

    void Start()
    {
        originalScale = transform.localScale;

        // For fade effects
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null && (effectType == EffectType.FadeIn || effectType == EffectType.FadeOut))
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    void Update()
    {
        switch (effectType)
        {
            case EffectType.Rotate:
                Rotate();
                break;

            case EffectType.ScaleUp:
                ScaleUp();
                break;

            case EffectType.ScaleDown:
                ScaleDown();
                break;

            case EffectType.Pulse:
                Pulse();
                break;

            case EffectType.Bounce:
                Bounce();
                break;

            case EffectType.FadeIn:
                FadeIn();
                break;

            case EffectType.FadeOut:
                FadeOut();
                break;
        }
    }

    private void Rotate()
    {
        transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);
    }

    private void ScaleUp()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, originalScale * scaleAmount, Time.deltaTime * speed);
    }

    private void ScaleDown()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, originalScale / scaleAmount, Time.deltaTime * speed);
    }

    private void Pulse()
    {
        float scale = Mathf.PingPong(Time.time * speed, scaleAmount - 1f) + 1f;
        transform.localScale = originalScale * scale;
    }

    private void Bounce()
    {
        float scale = 1f + Mathf.Abs(Mathf.Sin(Time.time * speed)) * (scaleAmount - 1f);
        transform.localScale = originalScale * scale;
    }

    private void FadeIn()
    {
        if (canvasGroup == null) return;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 1f, Time.deltaTime / fadeDuration);
    }

    private void FadeOut()
    {
        if (canvasGroup == null) return;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 0f, Time.deltaTime / fadeDuration);
    }
}
