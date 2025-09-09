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

    [Header("Cleaning Settings")]
    public float overlapRadius = 1f; // World units for rake detection
    public LayerMask leafLayer;      // Layer for leaves in world space

    private Camera mainCam;

    protected override void Start()
    {
        image = GetComponent<Image>();
        originalRotation = transform.rotation;
        originalPosition = transform.position;
        mainCam = Camera.main;

        if (image != null && sprites.Length > 0)
            image.sprite = sprites[0]; // idle
    }

    protected void Update()
    {
        if (isDragging)
        {
            TryCleanIfTouchingLeaves();
        }
    }

    private void TryCleanIfTouchingLeaves()
    {
        // Convert rake UI position to world position
        //Vector3 worldPos = mainCam.ScreenToWorldPoint(rectTransform.position);
        //worldPos.z = 0f;

        // Check for leaves within world radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(rectTransform.position, overlapRadius, leafLayer);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Leaf"))
            {
                StartCoroutine(FadeAndRemoveLeaf(hit.gameObject, rectTransform.position));
            }
        }
    }

    private IEnumerator FadeAndRemoveLeaf(GameObject leaf, Vector3 rakePos)
    {
        SpriteRenderer sr = leaf.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Vector3 startPos = leaf.transform.position;
        Vector3 pushDir = (leaf.transform.position - rakePos).normalized;
        Vector3 targetPos = startPos + pushDir * 0.3f; // push 0.3 world units

        Color startColor = sr.color;

        float t = 0f;
        while (t < 1f)
        {
            if (sr == null) yield break; // stops if destroyed
            t += Time.deltaTime * 2f; // speed multiplier
            if(leaf!=null)
            leaf.transform.position = Vector3.Lerp(startPos, targetPos, t);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
            yield return null;
        }

        Destroy(leaf);
        GameManager.instance.rakingManager.ResetCounter();
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if(GameManager.instance.currentDrag) return;
        base.OnBeginDrag(eventData);
        isDragging = true;
        startPosition = rectTransform.position;

        if (image != null && sprites.Length > 1)
            image.sprite = sprites[1];

        SoundManager.Instance.PlaySFXMusic(SFXType.RakingTheLeaves);
        SoundManager.Instance.PlayVoiceOver(VoiceOverType.RakeTheLeavesOnTheGround);

    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        isDragging = false;

        if (image != null && sprites.Length > 0)
            image.sprite = sprites[0];
        SoundManager.Instance.StopSfxMusic();
        SoundManager.Instance.StopVoiceOverMusic();

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

    private void OnDrawGizmosSelected()
    {
        if (mainCam == null) mainCam = Camera.main;
        Vector3 worldPos = Application.isPlaying ? mainCam.ScreenToWorldPoint(rectTransform.position) : Vector3.zero;
        worldPos.z = 0f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(worldPos, overlapRadius);
    }
}
