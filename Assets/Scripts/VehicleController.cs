using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class VehicleController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public Vector2 waitTimeRange = new Vector2(1f, 2f);
    public Vector2 moveDistanceRange = new Vector2(2f, 5f);
    public bool isFacingRightAtStart = true;

    [Header("Obstacle Detection")]
    public float rayDistance = 0.5f;
    public LayerMask obstacleLayer;

    [Header("Camera Bounds")]
    public float padding = 1f;
    private float leftBound;
    private float rightBound;

    private Animator animator;
    private bool isMoving = false;
    private Vector2 direction;
    private bool isFacingRight;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        UpdateCameraBounds();

        // Set initial facing direction
        isFacingRight = isFacingRightAtStart;
        direction = isFacingRight ? Vector2.right : Vector2.left;
        FaceDirection(isFacingRight);
        StartCoroutine(MoveRoutine());
    }

    private void Update()
    {
        if (!isMoving) return;

        Vector2 moveDir = new Vector2(direction.x, 0f);

        // Detect obstacle
        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDir, rayDistance, obstacleLayer);
        if (hit.collider != null)
        {
            StopMovement();
            return;
        }

        // Move vehicle
        transform.Translate(moveDir * moveSpeed * Time.deltaTime);

        // Update flip
        FaceDirection(direction.x > 0);

        // Stop at screen edge
        float xPos = transform.position.x;
        if (xPos <= leftBound + padding || xPos >= rightBound - padding)
        {
            StopMovement();
        }
    }

    private IEnumerator MoveRoutine()
    {
        while (true)
        {
            // Wait idle
            float wait = Random.Range(waitTimeRange.x, waitTimeRange.y);
            SetAnimation(false);
            yield return new WaitForSeconds(wait);

            // Choose random direction
            isFacingRight = Random.value < 0.5f;
            direction = isFacingRight ? Vector2.right : Vector2.left;

            float distance = Random.Range(moveDistanceRange.x, moveDistanceRange.y);
            float moveTime = distance / moveSpeed;

            isMoving = true;
            SetAnimation(true);

            float endTime = Time.time + moveTime;
            while (Time.time < endTime && isMoving)
                yield return null;

            StopMovement();
        }
    }

    private void StopMovement()
    {
        isMoving = false;
        SetAnimation(false);
    }

    private void SetAnimation(bool moving)
    {
        if (animator != null)
        {
            animator.enabled = moving;
        }
    }

    private void FaceDirection(bool faceRight)
    {
        if (isFacingRightAtStart)
        {
            Vector3 scale = transform.localScale;
            scale.x = faceRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        else
        {
            Vector3 scale = transform.localScale;
            scale.x = faceRight ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        
    }

    private void UpdateCameraBounds()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float height = 2f * cam.orthographicSize;
        float width = height * cam.aspect;
        Vector3 camPos = cam.transform.position;

        leftBound = camPos.x - width / 2f;
        rightBound = camPos.x + width / 2f;
    }

    private void OnDrawGizmosSelected()
    {
        UpdateCameraBounds();
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(leftBound + padding, transform.position.y - 0.5f), new Vector3(leftBound + padding, transform.position.y + 0.5f));
        Gizmos.DrawLine(new Vector3(rightBound - padding, transform.position.y - 0.5f), new Vector3(rightBound - padding, transform.position.y + 0.5f));

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(direction.normalized * rayDistance));
    }
}
