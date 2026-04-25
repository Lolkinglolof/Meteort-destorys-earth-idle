using UnityEngine;

public class SpaceSpawner : MonoBehaviour
{
    [Header("Targeting Player")]
    public PlayerBoundary playerBoundary;

    [Header("Tier 0 Prefabs - Default")]
    public GameObject[] smallObjects;
    public GameObject[] rareObjects;

    [Header("Tier 1 Prefabs - Unlocks After 18 Total Upgrades")]
    public GameObject[] tier1SmallObjects;
    public GameObject[] tier1RareObjects;

    [Header("Tier 2 Prefabs - Unlocks After 36 Total Upgrades")]
    public GameObject[] tier2SmallObjects;
    public GameObject[] tier2RareObjects;

    [Header("Tier 3 Prefabs - Unlocks After 54 Total Upgrades")]
    public GameObject[] tier3SmallObjects;
    public GameObject[] tier3RareObjects;

    [Header("Tier 4 Prefabs - Unlocks After 72 Total Upgrades")]
    public GameObject[] tier4SmallObjects;
    public GameObject[] tier4RareObjects;

    [Header("Spawn Settings")]
    public float spawnRate = 2f;

    [Range(0, 100)]
    public float rareSpawnChance = 10f;

    [Header("Difficulty Scaling")]
    public bool scaleSpawnRateWithTier = true;
    public float spawnRateReductionPerTier = 0.15f;
    public float minimumSpawnRate = 0.5f;

    private float nextSpawnTime;

    void Update()
    {
        float currentSpawnRate = GetCurrentSpawnRate();

        if (Time.time >= nextSpawnTime)
        {
            SpawnLogic();
            nextSpawnTime = Time.time + currentSpawnRate;
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
        Vector3 spawnPos = new Vector3(transform.position.x + 15f, randomY, 0f);

        float roll = Random.Range(0f, 100f);

        if (roll <= rareSpawnChance)
        {
            SpawnWeightedRare(spawnPos);
        }
        else
        {
            SpawnWeightedSmall(spawnPos);
        }
    }

    void SpawnWeightedSmall(Vector3 spawnPos)
    {
        int tier = GetCurrentEnemyTier();

        GameObject[] selectedPool = GetWeightedPool(
            tier,
            smallObjects,
            tier1SmallObjects,
            tier2SmallObjects,
            tier3SmallObjects,
            tier4SmallObjects
        );

        SpawnFromPool(selectedPool, spawnPos);
    }

    void SpawnWeightedRare(Vector3 spawnPos)
    {
        int tier = GetCurrentEnemyTier();

        GameObject[] selectedPool = GetWeightedPool(
            tier,
            rareObjects,
            tier1RareObjects,
            tier2RareObjects,
            tier3RareObjects,
            tier4RareObjects
        );

        SpawnFromPool(selectedPool, spawnPos);
    }

    GameObject[] GetWeightedPool(
        int tier,
        GameObject[] tier0Pool,
        GameObject[] tier1Pool,
        GameObject[] tier2Pool,
        GameObject[] tier3Pool,
        GameObject[] tier4Pool
    )
    {
        float roll = Random.Range(0f, 100f);

        // Tier 0: kun normale fjender
        if (tier <= 0)
        {
            return GetFallbackPool(tier0Pool, tier1Pool, tier2Pool, tier3Pool, tier4Pool);
        }

        // Tier 1:
        // 65% tier 1
        // 35% tier 0
        if (tier == 1)
        {
            if (roll < 65f && HasObjects(tier1Pool)) return tier1Pool;
            return GetFallbackPool(tier0Pool, tier1Pool, tier2Pool, tier3Pool, tier4Pool);
        }

        // Tier 2:
        // 65% tier 2
        // 27% tier 1
        // 8% tier 0
        if (tier == 2)
        {
            if (roll < 65f && HasObjects(tier2Pool)) return tier2Pool;
            if (roll < 92f && HasObjects(tier1Pool)) return tier1Pool;
            if (HasObjects(tier0Pool)) return tier0Pool;

            return GetFallbackPool(tier2Pool, tier1Pool, tier0Pool, tier3Pool, tier4Pool);
        }

        // Tier 3:
        // 65% tier 3
        // 25% tier 2
        // 10% tier 1
        // 0% tier 0
        if (tier == 3)
        {
            if (roll < 65f && HasObjects(tier3Pool)) return tier3Pool;
            if (roll < 90f && HasObjects(tier2Pool)) return tier2Pool;
            if (HasObjects(tier1Pool)) return tier1Pool;

            return GetFallbackPool(tier3Pool, tier2Pool, tier1Pool, tier4Pool, tier0Pool);
        }

        // Tier 4+:
        // 65% tier 4
        // 25% tier 3
        // 10% tier 2
        // 0% tier 1
        // 0% tier 0
        if (roll < 65f && HasObjects(tier4Pool)) return tier4Pool;
        if (roll < 90f && HasObjects(tier3Pool)) return tier3Pool;
        if (HasObjects(tier2Pool)) return tier2Pool;

        return GetFallbackPool(tier4Pool, tier3Pool, tier2Pool, tier1Pool, tier0Pool);
    }

    GameObject[] GetFallbackPool(
        GameObject[] first,
        GameObject[] second,
        GameObject[] third,
        GameObject[] fourth,
        GameObject[] fifth
    )
    {
        if (HasObjects(first)) return first;
        if (HasObjects(second)) return second;
        if (HasObjects(third)) return third;
        if (HasObjects(fourth)) return fourth;
        if (HasObjects(fifth)) return fifth;

        return null;
    }

    int GetCurrentEnemyTier()
    {
        if (UpgradeManager.Instance == null)
            return 0;

        return UpgradeManager.Instance.GetEnemyDifficultyTier();
    }

    float GetCurrentSpawnRate()
    {
        if (!scaleSpawnRateWithTier)
            return spawnRate;

        int tier = GetCurrentEnemyTier();

        float multiplier = 1f - (tier * spawnRateReductionPerTier);
        multiplier = Mathf.Clamp(multiplier, 0.25f, 1f);

        float scaledRate = spawnRate * multiplier;

        return Mathf.Max(scaledRate, minimumSpawnRate);
    }

    bool HasObjects(GameObject[] pool)
    {
        return pool != null && pool.Length > 0;
    }

    void SpawnFromPool(GameObject[] pool, Vector3 pos)
    {
        if (pool == null || pool.Length == 0)
            return;

        int randomIndex = Random.Range(0, pool.Length);

        if (pool[randomIndex] != null)
        {
            Instantiate(pool[randomIndex], pos, Quaternion.identity);
        }
    }

    void OnDrawGizmosSelected()
    {
        float minY = (playerBoundary != null) ? playerBoundary.currentMinY : -8f;
        float maxY = (playerBoundary != null) ? playerBoundary.currentMaxY : 8f;

        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);

        float centerY = (minY + maxY) / 2f;
        Vector3 center = new Vector3(transform.position.x + 15f, centerY, 0f);

        float height = maxY - minY;
        Vector3 size = new Vector3(1f, Mathf.Max(height, 0.1f), 1f);

        Gizmos.DrawCube(center, size);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, size);
    }
}