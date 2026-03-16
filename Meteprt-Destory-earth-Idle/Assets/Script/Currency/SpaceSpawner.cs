using UnityEngine;

public class SpaceSpawner : MonoBehaviour
{
    [Header("Prefabs to Spawn")]
    public GameObject[] smallObjects;  // Mønter/Debris
    public GameObject[] rareObjects;   // Diamanter

    [Header("Spawn Settings")]
    public float spawnRate = 2f;
    public float spawnRangeY = 4f;

    private float nextSpawnTime;
    private int spawnCount = 0;

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnLogic();
            nextSpawnTime = Time.time + spawnRate;
            spawnCount++;
        }
    }

    void SpawnLogic()
    {
        Vector3 spawnPos = new Vector3(transform.position.x + 15f, Random.Range(-spawnRangeY, spawnRangeY), 0);

        // Hver 10. objekt er stadig en sjælden meteor
        if (spawnCount % 10 == 0 && spawnCount != 0)
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
        if (pool.Length > 0)
        {
            int randomIndex = Random.Range(0, pool.Length);
            Instantiate(pool[randomIndex], pos, Quaternion.identity);
        }
    }
}