using UnityEngine;
using System.Collections;
using UnityEngine.UIElements.Experimental;

public class AnimalController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public Vector2 waitTimeRange = new Vector2(1f, 3f);
    public Vector2 moveDistanceRange = new Vector2(1f, 3f);
    public bool isFacingRightAtStart = true;

    [Header("Satiety Settings")]
    public float satiety = 0f;
    public float maxSatiety = 100f;
    public float eatingRate = 20f; // per second
    public float foodDetectRadius = 2f;
    public LayerMask foodLayer;
    public float distanceToStopFromFood = 0.4f;
    [Header("Obstacle Detection")]
    public float rayDistance = 0.5f;
    public LayerMask obstacleLayer;

    [Header("Camera Bounds")]
    public float padding = 1f;
    private float leftBound;
    private float rightBound;

    private Animator animator;
    public bool isMoving = false;
    public bool isEating = false;
    private Vector2 direction;
    public Transform currentFood;
    public bool eat;

    private void Start()
    {
        animator = transform.GetChild(0).GetComponent<Animator>();
        UpdateCameraBounds();
        direction = isFacingRightAtStart ? Vector2.right : Vector2.left;
        FaceDirection(isFacingRightAtStart);
        StartCoroutine(BehaviorRoutine());
        satiety = 0;
        if (currentFood != null)
            currentFood.gameObject.SetActive(false);
    }
    private void FaceDirection(bool faceRight)
    {
        Vector3 scale = transform.localScale;
        if (isFacingRightAtStart)
        {
            scale.x = faceRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        }
        else
        {
            scale.x = faceRight ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        }
        transform.localScale = scale;
    }

    private void Update()
    {
        SetAnimation(isMoving, isEating);

        if (isEating || !isMoving) return;
        if (eat && currentFood != null)
        {
            // Move towards food
            Vector2 dirToFood = (currentFood.position - transform.position).normalized;
            direction = dirToFood.x < 0 ? Vector2.left : Vector2.right;
            transform.Translate(dirToFood * moveSpeed * Time.deltaTime);
            SetAnimation(false, true);
            animator.SetBool("Eating", true);
            Debug.Log("Eat food");
            isEating = true;
            isMoving = false;
            // Flip
            FaceDirection(direction.x > 0);

            StartCoroutine(EatFood(currentFood.gameObject));

            return;
        }

        // Normal patrol movement
        Vector2 moveDir = new Vector2(direction.x, 0f);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDir, rayDistance, obstacleLayer);
        if (hit.collider != null)
        {
            StopMovement();
            return;
        }

        transform.Translate(moveDir * moveSpeed * Time.deltaTime);

        FaceDirection(direction.x > 0);


        float xPos = transform.position.x;
        if (xPos <= leftBound + padding || xPos >= rightBound - padding)
        {
            StopMovement();
        }
    }

    private IEnumerator BehaviorRoutine()
    {
        while (true)
        {
            // Check for food
            Collider2D food = Physics2D.OverlapCircle(transform.position, foodDetectRadius, foodLayer);
            if (food != null && satiety < maxSatiety)
            {
                currentFood = food.transform;
                isMoving = true;

                if (!isEating)
                    SetAnimation(true, false);
                yield return new WaitUntil(() => !isMoving); // wait till done eating
                continue;
            }
            satiety -= eatingRate * Time.deltaTime;
            // Idle wait
            float wait = Random.Range(waitTimeRange.x, waitTimeRange.y);
            if (!isEating)
                SetAnimation(false, false);
            yield return new WaitForSeconds(wait);

            // Start patrol move
            direction = Random.value < 0.5f ? Vector2.left : Vector2.right;
            float distance = Random.Range(moveDistanceRange.x, moveDistanceRange.y);
            float moveDuration = distance / moveSpeed;

            isMoving = true;
            if (!isEating)
                SetAnimation(true, false);

            float endTime = Time.time + moveDuration;
            while (Time.time < endTime && isMoving)
                yield return null;

            StopMovement();
        }
    }

    private IEnumerator EatFood(GameObject food)
    {
        SoundManager.Instance.PlaySFXMusic(SFXType.AnimalsEating);

        isEating = true;
        isMoving = false;
        SetAnimation(false, true);
        food.gameObject.SetActive(true);
        while (satiety < maxSatiety)
        {
            satiety += eatingRate * Time.deltaTime;
            yield return null;
        }

        food.gameObject.SetActive(false);
        GameManager.instance.currentSpawnedFood.GetComponent<FoodFollowAndTrigger>().CompleteEating();
        eat = false;
        isEating = false;
        SetAnimation(true, false);
        SoundManager.Instance.StopSfxMusic();

        this.Invoke(() =>
        {
            satiety = 0;

        }, 15);

    }

    private void StopMovement()
    {
        isMoving = false;
        SetAnimation(false, false);
    }

    private void SetAnimation(bool walking, bool eating)
    {
        //if (animator == null) return;
        animator.SetBool("Walking", walking);
        animator.SetBool("Eating", eating);
    }

    private void UpdateCameraBounds()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        Vector3 camCenter = cam.transform.position;

        leftBound = camCenter.x - camWidth / 2f;
        rightBound = camCenter.x + camWidth / 2f;
    }

    private void OnDrawGizmosSelected()
    {
        UpdateCameraBounds();
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(leftBound + padding, transform.position.y - 0.5f), new Vector3(leftBound + padding, transform.position.y + 0.5f));
        Gizmos.DrawLine(new Vector3(rightBound - padding, transform.position.y - 0.5f), new Vector3(rightBound - padding, transform.position.y + 0.5f));

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, foodDetectRadius);
    }
}
