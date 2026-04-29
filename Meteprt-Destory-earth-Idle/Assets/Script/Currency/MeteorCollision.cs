using UnityEngine;
using System.Collections.Generic;

public class MeteorCollision : MonoBehaviour
{
    private PlayerSkade skadeLogik;

    private Dictionary<int, float> hitCooldowns = new Dictionary<int, float>();

    [Header("Meteor Tags")]
    [Tooltip("Skriv de tags som skal tælle som meteorer.")]
    public string[] meteorTags = new string[] { "SmallDebris", "RareMeteor" };

    [Tooltip("Hvis true, skal meteoren have et af tagsene ovenfor. Hvis false, er Meteor2022WJ1 script nok.")]
    public bool requireMeteorTag = true;

    [Header("Hit Settings")]
    public float hitCooldown = 0.05f;

    void Start()
    {
        skadeLogik = GetComponent<PlayerSkade>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHitMeteor(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryHitMeteor(other);
    }

    private void TryHitMeteor(Collider2D other)
    {
        RewardDebug("COLLISION ENTER/STAY",
            "hitObject=" + other.name +
            " | tag=" + other.tag +
            " | layer=" + LayerMask.LayerToName(other.gameObject.layer),
            other
        );

        if (other.CompareTag("Planet"))
        {
            RewardDebug("PLANET HIT", "Planet detected");
            HandlePlanetImpact();
            return;
        }

        Meteor2022WJ1 enemy = other.GetComponent<Meteor2022WJ1>();

        if (enemy == null)
            enemy = other.GetComponentInParent<Meteor2022WJ1>();

        if (enemy == null)
        {
            RewardDebug("METEOR NOT FOUND",
                "No Meteor2022WJ1 on object or parent: " + other.name,
                other
            );
            return;
        }

        RewardDebug("METEOR FOUND",
            "enemy=" + enemy.name +
            " | enemyTag=" + enemy.tag,
            enemy
        );

        if (requireMeteorTag && !HasAllowedMeteorTag(other, enemy))
        {
            RewardDebug("METEOR BLOCKED BY TAG",
                "hitObject=" + other.name +
                " | hitTag=" + other.tag +
                " | enemy=" + enemy.name +
                " | enemyTag=" + enemy.tag,
                enemy
            );
            return;
        }

        if (skadeLogik == null)
        {
            RewardDebug("DAMAGE BLOCKED", "PlayerSkade was not found on player");
            return;
        }

        int instanceID = enemy.gameObject.GetInstanceID();

        if (hitCooldowns.ContainsKey(instanceID) && Time.time < hitCooldowns[instanceID] + hitCooldown)
        {
            RewardDebug("HIT BLOCKED BY COOLDOWN",
                "enemy=" + enemy.name +
                " | time=" + Time.time +
                " | lastHit=" + hitCooldowns[instanceID] +
                " | cooldown=" + hitCooldown,
                enemy
            );
            return;
        }

        hitCooldowns[instanceID] = Time.time;

        float damageValue = skadeLogik.baseMass;

        if (damageValue <= 0f)
        {
            RewardDebug("DAMAGE BLOCKED", "damageValue was 0 or below: " + damageValue);
            return;
        }

        RewardDebug("CALLING TAKE DAMAGE",
            "enemy=" + enemy.name +
            " | damage=" + damageValue,
            enemy
        );

        enemy.TakeDamage(damageValue);
    }

    private bool HasAllowedMeteorTag(Collider2D other, Meteor2022WJ1 enemy)
    {
        if (meteorTags == null || meteorTags.Length == 0)
            return true;

        Transform current = other.transform;

        while (current != null)
        {
            for (int i = 0; i < meteorTags.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(meteorTags[i]) && current.CompareTag(meteorTags[i]))
                    return true;
            }

            current = current.parent;
        }

        for (int i = 0; i < meteorTags.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(meteorTags[i]) && enemy.CompareTag(meteorTags[i]))
                return true;
        }

        return false;
    }
    private void RewardDebug(string source, string message, Object context = null)
    {
        if (GameManager.instance != null)
            GameManager.instance.RewardDebug(source, message, context);
        else
            Debug.Log("<color=#00E5FF>[REWARD DEBUG]</color> <b>" + source + "</b> | " + message, context);
    }
    void HandlePlanetImpact()
    {
        Debug.Log("PLANET DESTROYED! +10 Diamonds");
    }
}