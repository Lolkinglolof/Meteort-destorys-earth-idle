using UnityEngine;

public class PlanetCore : MonoBehaviour
{
    [Header("Planet Indstillinger")]
    // Sæt denne højt! Spilleren SKAL opgradere for at kunne ødelægge den.
    public float maxPlanetHealth = 2000f;
    private float currentPlanetHealth;

    [Header("Visuals & Belønning")]
    public GameObject giantExplosionPrefab; // Her kan du trække en KÆMPE pixel-eksplosion ind!

    void Start()
    {
        currentPlanetHealth = maxPlanetHealth;
    }

    // Dette sker, når meteoren (spilleren) smadrer ind i Jordens overflade
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

            if (playerHealth != null && playerHealth.currentHealth > 0)
            {
                // Spillerens tilbageværende liv fungerer som deres "Impact Force" (kollisionskraft)
                float impactForce = playerHealth.currentHealth;

                // Giv skade til planeten
                currentPlanetHealth -= impactForce;
                Debug.Log("<color=orange>Meteoren ramte Jorden med " + impactForce + " kraft! Jordens HP er nu: " + currentPlanetHealth + "</color>");

                if (currentPlanetHealth <= 0)
                {
                    // ==========================================
                    // SUCCESS! SPILLEREN VAR STOR NOK!
                    // ==========================================
                    DestroyPlanet();

                    // Sørg for at spilleren overlever visuelt, eller gør noget fedt her
                }
                else
                {
                    // ==========================================
                    // FEJLSLÅET! SPILLEREN VAR FOR LILLE!
                    // ==========================================
                    Debug.Log("<color=red>Meteoren var for lille og blev knust mod Jordens overflade!</color>");

                    // Dræb spilleren øjeblikkeligt (ved at give dem mere skade end de har liv)
                    // Dette udløser automatisk din fede Slow-Motion Game Over, vi lavede tidligere!
                    playerHealth.TakeDamage(999999f);
                }
            }
        }
    }

    void DestroyPlanet()
    {
        Debug.Log("<color=green>KABOOM! JORDEN ER UDSLETTET!</color>");

        // 1. Spil en gigantisk eksplosion lige på Jordens position
        if (giantExplosionPrefab != null)
        {
            Instantiate(giantExplosionPrefab, transform.position, Quaternion.identity);
        }

        // 2. Skjul Jorden, så det ligner den er sprunget i luften
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null) sprite.enabled = false;

        MeshRenderer mesh = GetComponent<MeshRenderer>();
        if (mesh != null) mesh.enabled = false;

        Collider2D coll = GetComponent<Collider2D>();
        if (coll != null) coll.enabled = false;

        // 3. Her kan vi senere aktivere en "DU VANDT!" skærm i stedet for Game Over.
        // For nu lader vi bare spilleren svæve over resterne af planeten og nyde sejren!
    }
}