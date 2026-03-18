using UnityEngine;

public class MoonCore : MonoBehaviour
{
    [Header("Måne Indstillinger")]
    public float maxMoonHealth = 500f;
    private float currentMoonHealth;

    [Header("Belønning for at smadre den")]
    public double coinReward = 1500; // Rettet til double
    public int diamondReward = 5;

    [Header("Visuals")]
    public GameObject explosionPrefab;

    void Start()
    {
        currentMoonHealth = maxMoonHealth;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

            if (playerHealth != null && playerHealth.currentHealth > 0)
            {
                float impactForce = playerHealth.currentHealth;

                currentMoonHealth -= impactForce;

                if (currentMoonHealth <= 0)
                {
                    DestroyMoon();
                }
                else
                {
                    playerHealth.TakeDamage(9999f);
                }
            }
        }
    }

    void DestroyMoon()
    {

        if (GameManager.instance != null)
        {
            // RETTET: Bruger nu dine indbyggede GameManager metoder
            GameManager.instance.AddCoins(coinReward);
            GameManager.instance.AddDiamonds(diamondReward);
        }

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null) sprite.enabled = false;

        MeshRenderer mesh = GetComponent<MeshRenderer>();
        if (mesh != null) mesh.enabled = false;

        Collider2D coll = GetComponent<Collider2D>();
        if (coll != null) coll.enabled = false;

        Destroy(gameObject, 2f);
    }
}