using UnityEngine;

public class SpaceSpawner : MonoBehaviour
{
    [Header("Targeting Player")]
    public PlayerBoundary playerBoundary;

    [Header("Prefabs to Spawn")]
    public GameObject[] smallObjects;
    public GameObject[] rareObjects;

    [Header("Spawn Settings")]
    public float spawnRate = 2f;

    [Range(0, 100)]
    [Tooltip("Hvor stor chance er der for en Rare? (0-100). 10-15% anbefales for 'Rare' følelse.")]
    public float rareSpawnChance = 10f;

    private float nextSpawnTime;

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnLogic();
            nextSpawnTime = Time.time + spawnRate;
        }
    }

    void SpawnLogic()
    {
        float minY = -8f;
        float maxY = 8f;

        if (playerBoundary != null)
        {
            minY = playerBoundary.currentMinY;
            maxY = playerBoundary.currentMaxY;
        }

        float randomY = Random.Range(minY, maxY);
        // Spawner 15 enheder til højre for spawnerens position
        Vector3 spawnPos = new Vector3(transform.position.x + 15f, randomY, 0);

        // --- CHANCE-BASERET SPAWN ---
        float roll = Random.Range(0f, 100f);

        if (roll <= rareSpawnChance)
        {
            SpawnFromPool(rareObjects, spawnPos);
        }
        else
        {
            SpawnFromPool(smallObjects, spawnPos);
        }
    }

    void SpawnFromPool(GameObject[] pool, Vector3 pos)
    {
        if (pool != null && pool.Length > 0)
        {
            int randomIndex = Random.Range(0, pool.Length);
            Instantiate(pool[randomIndex], pos, Quaternion.identity);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Sikrer at vi har værdier selvom spillet ikke kører
        float minY = (playerBoundary != null) ? playerBoundary.currentMinY : -8f;
        float maxY = (playerBoundary != null) ? playerBoundary.currentMaxY : 8f;

        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        float centerY = (minY + maxY) / 2f;
        Vector3 center = new Vector3(transform.position.x + 15f, centerY, 0f);

        float height = maxY - minY;
        // Vi sikrer os at højden altid er mindst 0.1 så vi kan se den
        Vector3 size = new Vector3(1f, Mathf.Max(height, 0.1f), 1f);

        Gizmos.DrawCube(center, size);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, size);
    }
}