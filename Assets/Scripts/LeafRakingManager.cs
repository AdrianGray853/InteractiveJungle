using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LeafRakingManager : MonoBehaviour
{
    [Header("Leaf Settings")]
    public GameObject[] leafPrefab;        // Leaf sprite prefab
    public Transform leafParent;         // Container for leaves in hierarchy
    public float spawnAreaMinX = -5f;
    public float spawnAreaMaxX = 5f;
    public float spawnAreaMinY = -3f;
    public float spawnAreaMaxY = -1f;

    [Header("Timing")]
    public float accumulationTime = 30f; // Time to spawn leaves
    public GameObject spawningParticle;
    [Header("Rake Settings")]
    public Transform rake;               // Rake visual
    public float rakeRadius = 1f;        // Radius for detecting leaves

    private bool isRakingPhase = false;
    private Camera mainCam;
    private List<GameObject> spawnedLeaves = new List<GameObject>();

    void Start()
    {
        mainCam = Camera.main;
        //StartCoroutine(SpawnLeavesRoutine());
    }

    IEnumerator SpawnLeavesRoutine()
    {
        float elapsed = 0f;
        while (elapsed < accumulationTime)
        {
            SpawnLeaf();
            yield return new WaitForSeconds(Random.Range(0.5f, 5f)); // Random delay
            elapsed += Time.deltaTime; // Increment with time passed
        }

        // End accumulation phase
        isRakingPhase = true;
    }

    void SpawnLeaf()
    {
        Vector3 spawnPos = new Vector3(
            Random.Range(spawnAreaMinX, spawnAreaMaxX),
            Random.Range(spawnAreaMinY, spawnAreaMaxY),
            0f
        );
        GameObject leaf = Instantiate(leafPrefab[Random.Range(0,leafPrefab.Length)], spawnPos, Quaternion.identity, leafParent);
        //spawnedLeaves.Add(leaf);
    }
    int counter;
    public GameObject SpawnLeaf(Vector3 postion)
    {
        counter++;
        Vector3 spawnPos = postion; 
        if (counter >= 30)
        {
            spawningParticle.SetActive(false);
        }
        GameObject leaf = Instantiate(leafPrefab[Random.Range(0,leafPrefab.Length)], spawnPos, Quaternion.identity, leafParent);
        //spawnedLeaves.Add(leaf);
        return leaf;
    }
    public void ResetCounter()
    {
        counter--;// = 0;
        if (counter <= 10)
        {
            spawningParticle.SetActive(true);
            spawningParticle.GetComponent<ParticleSystem>().Play();

        }

    }
    void Update()
    {
        if (!isRakingPhase) return;

        // Move rake with mouse or touch
#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            rake.position = mousePos;
            RakeCheck(mousePos);
        }
#else
        if (Input.touchCount > 0)
        {
            Vector3 touchPos = mainCam.ScreenToWorldPoint(Input.GetTouch(0).position);
            touchPos.z = 0f;
            rake.position = touchPos;
            RakeCheck(touchPos);
        }
#endif
    }

    void RakeCheck(Vector3 rakePos)
    {
        // Check for leaves in rake area
        Collider2D[] hits = Physics2D.OverlapCircleAll(rakePos, rakeRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Leaf"))
            {
                StartCoroutine(FadeAndRemove(hit.gameObject));
            }
        }
    }

    IEnumerator FadeAndRemove(GameObject leaf)
    {
        spawnedLeaves.Remove(leaf);

        // Optional: move leaf slightly in rake direction
        Vector3 pushDir = (leaf.transform.position - rake.position).normalized;
        Vector3 targetPos = leaf.transform.position + pushDir * 0.5f;

        SpriteRenderer sr = leaf.GetComponent<SpriteRenderer>();
        Color startColor = sr.color;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            leaf.transform.position = Vector3.Lerp(leaf.transform.position, targetPos, t);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
            yield return null;
        }

        Destroy(leaf);
    }

    void OnDrawGizmosSelected()
    {
        // Show rake radius in editor
        Gizmos.color = Color.yellow;
        if (rake != null)
            Gizmos.DrawWireSphere(rake.position, rakeRadius);
    }
}
