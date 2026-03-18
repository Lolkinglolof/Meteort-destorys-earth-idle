using UnityEngine;

public class AtmosphereBurn : MonoBehaviour
{
    [Header("Atmosfære Indstillinger")]
    public float damagePerTick = 15f; // Hvor meget liv/masse man mister ad gangen
    public float tickRate = 0.5f;     // Hvor ofte man brænder (0.5 betyder hvert halve sekund)

    private float timer = 0f;

    // OnTriggerStay2D kører konstant, så længe spilleren er inde i den store grønne cirkel
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Tæl op med tiden
            timer += Time.fixedDeltaTime;

            // Når timeren når vores tickRate, giver vi skade og nulstiller
            if (timer >= tickRate)
            {
                timer = 0f;

                PlayerHealth health = other.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    // Dette udløser din TakeDamage i PlayerHealth, som fjerner liv, 
                    // gør dig mindre og skyder stumper ud!
                    health.TakeDamage(damagePerTick);
                    Debug.Log("<color=orange>Atmosfæren brænder! Giver " + damagePerTick + " skade.</color>");
                }
            }
        }
    }
}