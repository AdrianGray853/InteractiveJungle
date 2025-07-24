using UnityEngine;

public class CloudMover : MonoBehaviour
{
    public float speed = 1f;
    public bool moveRight = true;

    //public Vector2 yRange = new Vector2(-1f, 3f); // Min/max Y range for offset
    public Vector2 xSpawnOffsetRange = new Vector2(0.5f, 2f); // Random offset offscreen

    private float resetEdgeX;
    private float spawnEdgeX;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        CalculateEdges();
    }

    void Update()
    {
        Vector3 moveDir = moveRight ? Vector3.right : Vector3.left;
        transform.Translate(moveDir * speed * Time.deltaTime);

        if (moveRight && transform.position.x > resetEdgeX)
        {
            ResetPosition();
        }
        else if (!moveRight && transform.position.x < resetEdgeX)
        {
            ResetPosition();
        }
    }

    void CalculateEdges()
    {
        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        float buffer = Random.Range(xSpawnOffsetRange.x, xSpawnOffsetRange.y);

        if (moveRight)
        {
            resetEdgeX = cam.transform.position.x + camWidth / 2 + buffer;
            spawnEdgeX = cam.transform.position.x - camWidth / 2 - buffer;
        }
        else
        {
            resetEdgeX = cam.transform.position.x - camWidth / 2 - buffer;
            spawnEdgeX = cam.transform.position.x + camWidth / 2 + buffer;
        }
    }

    void ResetPosition()
    {
        Vector3 newPos = transform.position;
        newPos.x = spawnEdgeX;
        //newPos.y = Random.Range(yRange.x, yRange.y);
        transform.position = newPos;

        // Optional: vary speed for natural motion
        speed = Random.Range(0.1f, 1f);

        // Recalculate offset for next time (if desired)
        CalculateEdges();
    }
}
