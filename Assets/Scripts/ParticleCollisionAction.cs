using UnityEngine;
using System.Collections.Generic;

public class ParticleCollisionAction : MonoBehaviour
{
    private ParticleSystem ps;
    private List<ParticleCollisionEvent> collisionEvents;

    [Header("Spawn Settings")]
    public float yOffsetRange = 0.2f; // small vertical variation

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    void OnParticleCollision(GameObject other)
    {
        int numEvents = ps.GetCollisionEvents(other, collisionEvents);

        for (int i = 0; i < numEvents; i++)
        {
            Vector3 hitPos = collisionEvents[i].intersection;

            // Add random Y offset
            hitPos.y += Random.Range(-yOffsetRange, yOffsetRange);

            Debug.Log("Particle hit at: " + hitPos);

            // Spawn leaf at adjusted position
            GameManager.instance.rakingManager.SpawnLeaf(hitPos);
        }
    }
}
