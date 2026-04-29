using UnityEngine;
using System.Collections.Generic;

public class AtmosphereShieldWeapon : MonoBehaviour
{
    [Header("Shield Status")]
    public bool shieldUnlocked = false;
    public bool shieldActive = true;

    [Header("Shield Damage")]
    public float shieldRadius = 3f;

    [Tooltip("Damage per second given to enemies inside the shield.")]
    public float burnDamagePerSecond = 5f;

    [Tooltip("How often the shield damages enemies. 0.25 = 4 times per second.")]
    public float damageTickInterval = 0.25f;

    [Header("Target Settings")]
    public LayerMask enemyLayers = ~0;

    [Tooltip("If true, enemies must have one of the allowed tags.")]
    public bool requireMeteorTag = true;

    public string[] allowedMeteorTags = new string[] { "SmallDebris", "RareMeteor", "Enemy" };

    [Header("Visuals")]
    public GameObject shieldVisual;
    public bool scaleVisualWithRadius = true;

    private GameObject spawnedShieldVisual;

    private float tickTimer;

    private readonly HashSet<int> damagedThisTick = new HashSet<int>();

    void Start()
    {
        ApplyUpgradeManagerStats();
        UpdateShieldVisual();
    }

    void Update()
    {
        ApplyUpgradeManagerStats();

        if (!shieldUnlocked || !shieldActive)
        {
            if (shieldVisual != null && shieldVisual.activeSelf)
                shieldVisual.SetActive(false);

            return;
        }

        if (shieldVisual != null && !shieldVisual.activeSelf)
            shieldVisual.SetActive(true);

        UpdateShieldVisual();

        tickTimer += Time.deltaTime;

        if (tickTimer >= damageTickInterval)
        {
            tickTimer = 0f;
            DamageEnemiesInsideShield();
        }
    }

    void ApplyUpgradeManagerStats()
    {
        if (UpgradeManager.Instance == null)
        {
            shieldUnlocked = false;
            return;
        }

        shieldUnlocked = UpgradeManager.Instance.GetAtmosphereShieldUnlocked();

        if (!shieldUnlocked)
        {
            UpdateShieldVisual();
            return;
        }

        shieldRadius = UpgradeManager.Instance.GetCurrentAtmosphereShieldRadius();
        burnDamagePerSecond = UpgradeManager.Instance.GetCurrentAtmosphereShieldDamage();
        damageTickInterval = UpgradeManager.Instance.GetCurrentAtmosphereShieldTickInterval();
    }

    void DamageEnemiesInsideShield()
    {
        damagedThisTick.Clear();

        if (shieldRadius <= 0f)
            return;

        if (burnDamagePerSecond <= 0f)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            shieldRadius,
            enemyLayers
        );

        float damageThisTick = burnDamagePerSecond * damageTickInterval;

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            Meteor2022WJ1 enemy = hit.GetComponentInParent<Meteor2022WJ1>();

            if (enemy == null)
                continue;

            int enemyID = enemy.gameObject.GetInstanceID();

            if (damagedThisTick.Contains(enemyID))
                continue;

            if (requireMeteorTag && !HasAllowedMeteorTag(hit, enemy))
                continue;

            damagedThisTick.Add(enemyID);

            enemy.TakeDamage(damageThisTick);
        }
    }

    bool HasAllowedMeteorTag(Collider2D hit, Meteor2022WJ1 enemy)
    {
        if (allowedMeteorTags == null || allowedMeteorTags.Length == 0)
            return true;

        Transform current = hit.transform;

        while (current != null)
        {
            for (int i = 0; i < allowedMeteorTags.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(allowedMeteorTags[i]) && current.CompareTag(allowedMeteorTags[i]))
                    return true;
            }

            current = current.parent;
        }

        for (int i = 0; i < allowedMeteorTags.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(allowedMeteorTags[i]) && enemy.CompareTag(allowedMeteorTags[i]))
                return true;
        }

        return false;
    }

    void UpdateShieldVisual()
    {
        GameObject visualToUse = GetOrCreateShieldVisual();

        if (visualToUse == null)
            return;

        bool shouldShow = shieldUnlocked && shieldActive;
        visualToUse.SetActive(shouldShow);

        if (!shouldShow)
            return;

        visualToUse.transform.localPosition = Vector3.zero;
        visualToUse.transform.localRotation = Quaternion.identity;

        if (!scaleVisualWithRadius)
            return;

        float targetWorldDiameter = shieldRadius * 2f;

        SpriteRenderer spriteRenderer = visualToUse.GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            float spriteBaseDiameter = Mathf.Max(
                spriteRenderer.sprite.bounds.size.x,
                spriteRenderer.sprite.bounds.size.y
            );

            if (spriteBaseDiameter <= 0f)
                spriteBaseDiameter = 1f;

            float parentScaleX = Mathf.Abs(transform.lossyScale.x);
            float parentScaleY = Mathf.Abs(transform.lossyScale.y);

            float parentScale = Mathf.Max(parentScaleX, parentScaleY);

            if (parentScale <= 0f)
                parentScale = 1f;

            float correctedLocalScale = targetWorldDiameter / (spriteBaseDiameter * parentScale);

            visualToUse.transform.localScale = new Vector3(
                correctedLocalScale,
                correctedLocalScale,
                1f
            );
        }
        else
        {
            float parentScaleX = Mathf.Abs(transform.lossyScale.x);
            float parentScaleY = Mathf.Abs(transform.lossyScale.y);

            float parentScale = Mathf.Max(parentScaleX, parentScaleY);

            if (parentScale <= 0f)
                parentScale = 1f;

            float correctedLocalScale = targetWorldDiameter / parentScale;

            visualToUse.transform.localScale = new Vector3(
                correctedLocalScale,
                correctedLocalScale,
                1f
            );
        }
    }

    public void SetShieldStats(float newRadius, float newBurnDamagePerSecond, float newTickInterval)
    {
        shieldRadius = Mathf.Max(0f, newRadius);
        burnDamagePerSecond = Mathf.Max(0f, newBurnDamagePerSecond);
        damageTickInterval = Mathf.Max(0.05f, newTickInterval);

        UpdateShieldVisual();
    }

    public void SetShieldUnlocked(bool unlocked)
    {
        shieldUnlocked = unlocked;
        UpdateShieldVisual();
    }

    public void SetShieldActive(bool active)
    {
        shieldActive = active;
        UpdateShieldVisual();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f);
        Gizmos.DrawSphere(transform.position, shieldRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, shieldRadius);
    }
    GameObject GetOrCreateShieldVisual()
    {
        if (shieldVisual == null)
            return null;

        // If shieldVisual is already a real object in the scene, use it directly.
        if (shieldVisual.scene.IsValid())
        {
            spawnedShieldVisual = shieldVisual;
            return spawnedShieldVisual;
        }

        // If shieldVisual is a prefab from the Project folder, spawn it once.
        if (spawnedShieldVisual == null)
        {
            spawnedShieldVisual = Instantiate(shieldVisual, transform);
            spawnedShieldVisual.transform.localPosition = Vector3.zero;
            spawnedShieldVisual.transform.localRotation = Quaternion.identity;
            spawnedShieldVisual.transform.localScale = Vector3.one;
        }

        return spawnedShieldVisual;
    }
}