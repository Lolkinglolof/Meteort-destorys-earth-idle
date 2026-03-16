using UnityEngine;
using System.Collections.Generic;

public class MeteorCollision : MonoBehaviour
{
    private PlayerSkade skadeLogik;

    private Dictionary<int, float> hitCooldowns = new Dictionary<int, float>();

    void Start()
    {
        skadeLogik = GetComponent<PlayerSkade>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("SmallDebris") || other.CompareTag("RareMeteor"))
        {
            int instanceID = other.gameObject.GetInstanceID();

            if (hitCooldowns.ContainsKey(instanceID) && Time.time < hitCooldowns[instanceID] + 0.5f)
            {
                return;
            }

            GameManager.instance.AddCoins(50);
            Debug.Log("Impact! +50 coins. Nu beregnes skade...");

            Meteor2022WJ1 enemy = other.GetComponent<Meteor2022WJ1>();
            if (enemy == null) enemy = other.GetComponentInParent<Meteor2022WJ1>();

            if (enemy != null && skadeLogik != null)
            {
                float damageValue = skadeLogik.baseMass;
                enemy.TakeDamage(damageValue);

                hitCooldowns[instanceID] = Time.time;
            }
        }
        if (other.CompareTag("Planet"))
        {
            HandlePlanetImpact();
        }
    }

    void HandlePlanetImpact()
    {
        GameManager.instance.AddDiamonds(10);
        Debug.Log("PLANET DESTROYED! +10 Diamonds");
    }
}