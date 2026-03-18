using UnityEngine;
using System.Collections.Generic;

public class PlayerSkade : MonoBehaviour
{
    public float baseMass = 2f;
    private MeteorController controller;
    private PlayerHealth health;

    private Dictionary<int, float> hitCooldowns = new Dictionary<int, float>();

    void Start()
    {
        controller = GetComponent<MeteorController>();
        health = GetComponent<PlayerHealth>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CollisionImpact impactData = other.GetComponentInParent<CollisionImpact>();
        if (impactData != null && controller != null)
        {
            float otherMass = impactData.objectMass;

            MeteorController otherMeteor = other.GetComponentInParent<MeteorController>();
            if (otherMeteor != null && otherMeteor != controller)
            {
                otherMass = otherMeteor.currentLiveMass;
            }

            Vector2 hitDirection = (transform.position - other.transform.position).normalized;

            controller.ApplyImpact(
                impactData.speedPenalty,
                impactData.impactForce,
                impactData.breakGrabOnHit,
                otherMass,
                hitDirection
            );
        }

        if (other.CompareTag("Enemy") || other.CompareTag("SmallDebris"))
        {
            int instanceID = other.gameObject.GetInstanceID();

            if (hitCooldowns.ContainsKey(instanceID) && Time.time < hitCooldowns[instanceID] + 0.5f)
                return;

            float currentMultiplier = (health != null) ? health.currentMassMultiplier : 1f;
            float rawMass = baseMass * currentMultiplier;

            Vector3 playerVelocity = (controller != null) ? controller.CurrentVelocity : Vector3.zero;
            Vector3 enemyVelocity = Vector3.zero;

            Rigidbody2D enemyRb = other.GetComponentInParent<Rigidbody2D>();
            if (enemyRb != null)
            {
                enemyVelocity = new Vector3(enemyRb.linearVelocity.x, enemyRb.linearVelocity.y, 0);
            }

            float impactSpeed = Vector3.Distance(playerVelocity, enemyVelocity);
            float speedBonus = Mathf.Clamp(impactSpeed * 0.05f, 0f, 1.0f);
            float damageToEnemy = rawMass * (1f + speedBonus);

            Meteor2022WJ1 enemy = other.GetComponentInParent<Meteor2022WJ1>();

            if (enemy != null && damageToEnemy > 0.5f)
            {
                Debug.Log("<color=red>BRAG!</color> Skade: " + damageToEnemy.ToString("F1") + " (Impact Speed: " + impactSpeed.ToString("F1") + ")");

                enemy.TakeDamage(damageToEnemy);
                enemy.ApplyKnockback(transform.position, damageToEnemy * 0.5f);

                hitCooldowns[instanceID] = Time.time;
            }
        }
    }
}