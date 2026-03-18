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
    public double coinReward = 10;       // Udbetales ved eksplosion
    public int diamondReward = 0; // Sæt denne til 1, 2 eller 5 i Unity for "Rare" meteorer
    public float massFactor = 1f;

    [Header("Visuals")]
    public GameObject destructionParticles;
    private Vector3 initialScale;
    private float lastHitTime;
    private Vector3 lastPos;

    public float ActualVelocity { get; private set; }

    [Header("Smart Despawn")]
    [Tooltip("Hvor lang tid meteoren må eksistere uden for skærmen, før den forsvinder.")]
    public float offScreenLifetime = 3f;
    private float despawnTimer;
    private bool isVisible = false;

    void Start()
    {
        currentHealth = maxHealth;
        initialScale = transform.localScale;

        // Start med en tilfældig fart mellem 1 og maxSpeed
        currentSpeed = Random.Range(1f, maxSpeed);
        lastPos = transform.position;

        // Start despawn-timeren på max
        despawnTimer = offScreenLifetime;
    }

    void Update()
    {
        // 1. ACCELERATION: Meteoren vil altid prøve at nå sin maxSpeed
        currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);

        // 2. BEVÆGELSE: Flyt i den aktuelle retning
        transform.Translate(currentDirection * currentSpeed * Time.deltaTime, Space.World);

        // 3. BEREGN FAKTISK VELOCITY (Brugt til skadesberegning)
        if (Time.deltaTime > 0)
        {
            ActualVelocity = Vector3.Distance(transform.position, lastPos) / Time.deltaTime;
        }
        lastPos = transform.position;

        // 4. SMART DESPAWN LOGIKKEN
        if (!isVisible)
        {
            despawnTimer -= Time.deltaTime;
            if (despawnTimer <= 0)
            {
                // Vi udbetaler IKKE penge, hvis den bare forsvinder af sig selv
                Destroy(gameObject);
            }
        }
        else
        {
            // Reset timer når spilleren ser objektet
            despawnTimer = offScreenLifetime;
        }
    }

    // --- UNITYS INDBYGGEDE KAMERA-MAGI ---
    void OnBecameVisible() { isVisible = true; }
    void OnBecameInvisible() { isVisible = false; }

    // --- KNOCKBACK ---
    public void ApplyKnockback(Vector3 playerPosition, float impactForce)
    {
        Vector3 pushDirection = (transform.position - playerPosition).normalized;
        currentDirection = pushDirection;

        float dampenedSpeed = Mathf.Sqrt(impactForce) * 2f;
        float absoluteMax = 10f;
        currentSpeed = Mathf.Clamp(dampenedSpeed, 2f, absoluteMax);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        // Visuel skalering baseret på HP
        float healthPercent = currentHealth / maxHealth;
        transform.localScale = initialScale * Mathf.Clamp(healthPercent, 0.3f, 1f);

        if (currentHealth <= 0) Explode();
    }

    // --- HER UDBETALES BELØNNINGEN ---
    void Explode()
    {
        // 1. Forbered belønningerne (Vi starter med fuldt beløb)
        double finalCoinReward = coinReward;
        int finalDiamondReward = diamondReward;

        // Find spillerens controller for at tjekke pilot-status
        MeteorController player = Object.FindFirstObjectByType<MeteorController>();

        // 2. PILOT-SKAT (Kun på mønter!)
        if (player != null && player.isAutoPiloting)
        {
            // Piloten tager 30% i løn (Du beholder 70%)
            finalCoinReward = coinReward * 0.7;

            Debug.Log("<color=orange>PILOT-SKAT:</color> Piloten tog 30% af mønterne. Du fik: "
                + finalCoinReward.ToString("F1") + " mønter.");

            // Bemærk: Vi rører ikke finalDiamondReward her, så den forbliver 100%
        }
        else
        {
            Debug.Log("<color=yellow>MANUELT KILL:</color> Du fik fuld belønning: "
                + coinReward + " mønter!");
        }

        // 3. UDBETALING VIA GAMEMANAGER
        if (GameManager.instance != null)
        {
            // Tilføj mønter (måske beskattet)
            GameManager.instance.AddCoins(finalCoinReward);

            // Tilføj diamanter (Altid ubeskåret!)
            if (finalDiamondReward > 0)
            {
                GameManager.instance.AddDiamonds(finalDiamondReward);
                Debug.Log("<color=cyan>DIAMANT-BONUS:</color> Du fandt " + finalDiamondReward + " diamanter!");
            }
        }

        // 4. Visuelle effekter (Partikler)
        if (destructionParticles != null)
        {
            Instantiate(destructionParticles, transform.position, Quaternion.identity);
        }

        // 5. Fjern meteoren fra spillet
        Destroy(gameObject);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Undgå at skade spilleren hver eneste frame
            if (Time.time < lastHitTime + 0.5f) return;

            float damageToPlayer = ActualVelocity * massFactor * 0.5f;
            PlayerHealth playerHP = other.GetComponent<PlayerHealth>();

            if (playerHP != null && damageToPlayer > 0.1f)
            {
                playerHP.TakeDamage(damageToPlayer);
                lastHitTime = Time.time;
            }

            // Meteoren tager selv skade ved sammenstød
            TakeDamage(1f);
        }
    }
}