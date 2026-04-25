using UnityEngine;

public class Meteor2022WJ1 : MonoBehaviour
{
    [Header("Movement & Velocity")]
    public float maxSpeed = 5f;
    public float acceleration = 2f;
    private float currentSpeed;
    private Vector3 currentDirection = Vector3.left;

    [Header("Hit Reward")]
    public double hitCoinReward = 50;
    // FJERNET: hitRewardCooldown og lastHitRewardTime, de ødelagde dit system!

    [Header("Stats")]
    public float maxHealth = 40f;
    private float currentHealth;
    public double coinReward = 10;
    public int diamondReward = 0;
    public float massFactor = 1f;

    [Header("Visuals")]
    public GameObject destructionParticles;
    private Vector3 initialScale;
    private float lastHitTime;
    private Vector3 lastPos;

    public float ActualVelocity { get; private set; }

    [Header("Smart Despawn")]
    [Tooltip("Hvor lang tid meteoren må eksistere uden for skærmen, før den forsvinder.")]
    public float offScreenLifetime = 5f;
    private float despawnTimer;

    private Camera mainCam;
    private bool hasExploded = false;

    void Start()
    {
        mainCam = Camera.main;
        currentHealth = maxHealth;
        initialScale = transform.localScale;
        currentSpeed = Random.Range(1f, maxSpeed);
        lastPos = transform.position;
        despawnTimer = offScreenLifetime;
    }

    void Update()
    {
        currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);
        transform.Translate(currentDirection * currentSpeed * Time.deltaTime, Space.World);

        if (Time.deltaTime > 0)
        {
            ActualVelocity = Vector3.Distance(transform.position, lastPos) / Time.deltaTime;
        }

        lastPos = transform.position;

        if (mainCam != null)
        {
            Vector3 viewportPos = mainCam.WorldToViewportPoint(transform.position);
            bool isOffScreen = viewportPos.x < -0.2f || viewportPos.x > 1.2f ||
                               viewportPos.y < -0.2f || viewportPos.y > 1.2f;

            if (isOffScreen)
            {
                despawnTimer -= Time.deltaTime;
                if (despawnTimer <= 0)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                despawnTimer = offScreenLifetime;
            }
        }
    }

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
        RewardDebug("TAKE DAMAGE START", "meteor=" + gameObject.name + " | damage=" + damage + " | currentHealthBefore=" + currentHealth + " | hasExploded=" + hasExploded, this);

        if (hasExploded) return;
        if (damage <= 0f) return;

        // Træk livet fra med det samme
        currentHealth -= damage;

        RewardDebug("HEALTH AFTER DAMAGE", "meteor=" + gameObject.name + " | currentHealthAfter=" + currentHealth + " | maxHealth=" + maxHealth, this);

        float healthPercent = currentHealth / maxHealth;
        transform.localScale = initialScale * Mathf.Clamp(healthPercent, 0.3f, 1f);

        // Tjek om den dør ELLER overlever
        if (currentHealth <= 0)
        {
            RewardDebug("METEOR WILL EXPLODE", "meteor=" + gameObject.name, this);
            Explode();
        }
        else
        {
            // Meteoren overlevede slaget! Giv ALTID hit reward (MeteorCollision's 0.05s cooldown beskytter os mod spam)
            if (GameManager.instance != null)
            {
                RewardDebug("HIT REWARD CALL", "meteor=" + gameObject.name + " | hitCoinReward=" + hitCoinReward, this);
                GameManager.instance.AddCoinsFromHit(hitCoinReward);
            }
        }
    }

    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        RewardDebug(
            "EXPLODE START",
            "meteor=" + gameObject.name +
            " | baseCoinReward=" + coinReward +
            " | diamondReward=" + diamondReward,
            this
        );

        double finalCoinReward = coinReward;
        int finalDiamondReward = diamondReward;

        MeteorController player = Object.FindFirstObjectByType<MeteorController>();

        // Auto-pilot penalty happens here.
        // Income bonus happens later inside GameManager.AddRewardCoins().
        if (player != null && player.isAutoPiloting)
        {
            double beforePilotTax = finalCoinReward;

            finalCoinReward *= 0.7;

            Debug.Log(
                "<color=orange>PILOT-SKAT:</color> Piloten tog 30% af mønterne. Før: " +
                beforePilotTax.ToString("F1") +
                " | Efter: " +
                finalCoinReward.ToString("F1") +
                " mønter. Income bonus bliver tilføjet i GameManager."
            );
        }
        else
        {
            Debug.Log(
                "<color=yellow>MANUELT KILL:</color> Base kill reward sendt til GameManager: " +
                finalCoinReward.ToString("F1") +
                " mønter. Income bonus bliver tilføjet i GameManager."
            );
        }

        if (GameManager.instance != null)
        {
            RewardDebug(
                "KILL REWARD CALL",
                "meteor=" + gameObject.name +
                " | rawCoinRewardSentToGameManager=" + finalCoinReward.ToString("F2"),
                this
            );

            GameManager.instance.AddCoinsFromEnemy(finalCoinReward);

            if (finalDiamondReward > 0)
            {
                GameManager.instance.AddDiamondsFromEnemy(finalDiamondReward);
                Debug.Log("<color=cyan>DIAMANT-BONUS:</color> Du fandt " + finalDiamondReward + " diamanter!");
            }
        }

        if (destructionParticles != null)
        {
            Instantiate(destructionParticles, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    private void RewardDebug(string source, string message, Object context = null)
    {
        if (GameManager.instance != null)
            GameManager.instance.RewardDebug(source, message, context);
        else
            Debug.Log("<color=#00E5FF>[REWARD DEBUG]</color> <b>" + source + "</b> | " + message, context);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (Time.time < lastHitTime + 0.5f) return;

            float damageToPlayer = ActualVelocity * massFactor * 0.5f;
            PlayerHealth playerHP = other.GetComponent<PlayerHealth>();

            if (playerHP != null && damageToPlayer > 0.1f)
            {
                playerHP.TakeDamage(damageToPlayer);
                lastHitTime = Time.time;
            }
        }
    }
}