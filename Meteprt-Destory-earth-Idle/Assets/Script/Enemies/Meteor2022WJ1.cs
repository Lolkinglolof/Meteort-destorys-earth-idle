using UnityEngine;

public class Meteor2022WJ1 : MonoBehaviour
{
    [Header("Movement & Velocity")]
    public float maxSpeed = 5f;
    public float acceleration = 2f;      // Hvor hurtigt den når maxSpeed igen
    private float currentSpeed;
    private Vector3 currentDirection = Vector3.left; // Startretning

    [Header("Stats")]
    public float maxHealth = 40f;
    private float currentHealth;
    public double coinReward = 10;
    public float massFactor = 1f;

    [Header("Visuals")]
    public GameObject destructionParticles;
    private Vector3 initialScale;
    private float lastHitTime;
    private Vector3 lastPos;

    public float ActualVelocity { get; private set; }

    void Start()
    {
        currentHealth = maxHealth;
        initialScale = transform.localScale;

        // Start med en tilfældig fart mellem 1 og maxSpeed
        currentSpeed = Random.Range(1f, maxSpeed);
        lastPos = transform.position;
    }

    void Update()
    {
        // 1. ACCELERATION: Meteoren vil altid prøve at nå sin maxSpeed i den retning den peger
        currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);

        // 2. BEVÆGELSE: Flyt i den aktuelle retning
        transform.Translate(currentDirection * currentSpeed * Time.deltaTime, Space.World);

        // 3. BEREGN FAKTISK VELOCITY
        if (Time.deltaTime > 0)
        {
            ActualVelocity = Vector3.Distance(transform.position, lastPos) / Time.deltaTime;
        }
        lastPos = transform.position;

        // Cleanup
        if (transform.position.x < -20f || transform.position.y > 15f || transform.position.y < -15f)
        {
            Destroy(gameObject);
        }
    }

    // --- NY FUNKTION: KNOCKBACK ---
    public void ApplyKnockback(Vector3 playerPosition, float impactForce)
    {
        // 1. Find retningen væk fra spilleren
        // (Mål-position minus Start-position giver retningen)
        Vector3 pushDirection = (transform.position - playerPosition).normalized;

        // Vi sætter den nye retning. Da der ikke er luftmodstand i rummet, 
        // vil den fortsætte i denne retning indtil den rammer noget andet.
        currentDirection = pushDirection;

        // 2. DÆMPET FYSIK: Vi bruger kvadratroden af kraften for at undgå uendelig fart.
        // Formel: $v = \sqrt{ImpactForce} \times 2$
        // Dette gør, at progressionen føles naturlig, selvom din Masse bliver meget høj.
        float dampenedSpeed = Mathf.Sqrt(impactForce) * 2f;

        // 3. LOFT (Cap): Sæt en absolut grænse for, hvor hurtigt den må flyve væk.
        // Vi sætter den til 15f, så spilleren har en chance for at indhente den.
        float absoluteMax = 10f;

        // Clamp sikrer at farten altid er mellem 2 og 15.
        currentSpeed = Mathf.Clamp(dampenedSpeed, 2f, absoluteMax);

        Debug.Log("<color=green>KNOCKBACK:</color> Kraft: " + impactForce + " -> Resultatfart: " + currentSpeed);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        float healthPercent = currentHealth / maxHealth;
        transform.localScale = initialScale * Mathf.Clamp(healthPercent, 0.3f, 1f);

        if (currentHealth <= 0) Explode();
    }

    void Explode() { /* Som før */ if (destructionParticles) Instantiate(destructionParticles, transform.position, Quaternion.identity); Destroy(gameObject); }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (Time.time < lastHitTime + 0.5f) return;

            // Vi kalder Knockback herfra eller fra PlayerSkade
            float damageToPlayer = ActualVelocity * massFactor * 0.5f;
            PlayerHealth playerHP = other.GetComponent<PlayerHealth>();

            if (playerHP != null && damageToPlayer > 0.1f)
            {
                playerHP.TakeDamage(damageToPlayer);
                lastHitTime = Time.time;
            }

            TakeDamage(1f);
        }
    }
}