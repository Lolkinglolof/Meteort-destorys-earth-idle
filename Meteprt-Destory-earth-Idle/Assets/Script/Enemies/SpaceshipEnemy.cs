using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpaceshipEnemy : MonoBehaviour
{
    private static readonly List<SpaceshipEnemy> allShips = new List<SpaceshipEnemy>();

    [Header("Target")]
    public string playerTag = "Player";
    public Transform player;

    [Header("Health")]
    public float maxHealth = 120f;
    private float currentHealth;
    private bool isDead = false;
    private Rigidbody2D rb;
    [Header("Rewards")]
    public double coinReward = 250;
    public int diamondReward = 0;

    [Header("Movement")]
    public float patrolSpeed = 2.5f;
    public float chaseSpeed = 4.5f;
    public Vector2 patrolDirection = Vector2.left;

    [Tooltip("Hvis sprite peger forkert, ændr denne. Fx 0, 90, -90 eller 180.")]
    public float spriteRotationOffset = 0f;

    public bool faceMoveDirection = true;

    [Header("Detection")]
    public float detectionRadius = 8f;
    public float loseRadius = 13f;
    public float attackRadius = 7f;
    public float preferredDistanceToPlayer = 4f;

    private bool isEngaged = false;

    [Header("Laser")]
    public Transform laserStartPoint;
    public LineRenderer laserLine;

    public float laserDamage = 18f;
    public float laserCooldown = 2f;
    public float laserWarningTime = 0.45f;
    public float laserVisibleTime = 0.15f;
    public float laserHitRadius = 0.35f;

    public float laserWidth = 0.08f;
    public Color laserWarningColor = Color.red;
    public Color laserFireColor = Color.white;

    private float laserCooldownTimer;
    private bool isShooting = false;

    [Header("Backup Call")]
    [Range(0f, 1f)]
    public float backupCallHealthPercent = 0.5f;

    public float backupCallRadius = 18f;
    public float backupAlertDuration = 20f;

    private bool hasCalledBackup = false;
    private float alertTimer = 0f;
    private Vector3 lastKnownPlayerPosition;

    [Header("Collision With Player")]
    public float contactDamageToPlayer = 25f;
    public float playerMassDamageMultiplier = 1.2f;
    public float contactCooldown = 0.5f;

    private float lastContactTime = -999f;

    [Header("Visuals")]
    public GameObject explosionPrefab;
    public float destroyDelay = 0.1f;

    [Header("Hit Feedback")]
    public bool useHitFeedback = true;

    public float hitKnockbackForce = 1.2f;
    public float hitKnockbackDamageMultiplier = 0.03f;
    public float hitKnockbackDamping = 8f;
    public float maxHitKnockbackSpeed = 5f;

    public Color hitFlashColor = Color.red;
    public float hitFlashDuration = 0.08f;

    public GameObject hitImpactParticlesPrefab;

    private Vector2 hitKnockbackVelocity;
    private SpriteRenderer[] spriteRenderers;
    private Color[] originalSpriteColors;
    private Coroutine hitFlashCoroutine;
    [Tooltip("Hvor længe AI movement bliver stoppet når skibet bliver ramt.")]
    public float hitStunDuration = 0.12f;
    [Tooltip("Hvor langt rumskibet bliver skubbet tilbage når det bliver ramt.")]
    public float hitKnockbackDistance = 0.8f;

    [Tooltip("Ekstra knockback distance baseret på damage.")]
    public float hitKnockbackDistanceDamageMultiplier = 0.01f;

    public float maxHitKnockbackDistance = 1.8f;

    [Tooltip("Hvor hurtigt knockback-animationen sker.")]
    public float hardKnockbackDuration = 0.14f;

    [Tooltip("Hvor længe AI venter efter knockback før den må jage igen.")]
    public float postHitAiLockDuration = 0.18f;
    [Tooltip("Hvor meget spillerens fart påvirker knockback distance.")]
    public float hitKnockbackSpeedMultiplier = 0.08f;
    private bool isHardKnockbackActive;
    private float postHitAiLockTimer;
    private Coroutine hardKnockbackCoroutine;
    private Transform moveRoot;

    private float hitStunTimer;
    void OnEnable()
    {
        if (!allShips.Contains(this))
            allShips.Add(this);
    }

    void OnDisable()
    {
        allShips.Remove(this);
    }

    void Start()
    {
        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
            rb = GetComponentInParent<Rigidbody2D>();

        moveRoot = rb != null ? rb.transform : transform;

        FindPlayerIfMissing();
        SetupLaser();
        CacheSpriteRenderers();
    }

    void Update()
    {
        if (isDead)
            return;

        if (isHardKnockbackActive)
            return;

        if (postHitAiLockTimer > 0f)
        {
            postHitAiLockTimer -= Time.deltaTime;
            return;
        }

        FindPlayerIfMissing();

        if (hitStunTimer > 0f)
        {
            hitStunTimer -= Time.deltaTime;
            //ApplyHitKnockbackMovement();
            return;
        }

        FindPlayerIfMissing();

        if (laserCooldownTimer > 0f)
            laserCooldownTimer -= Time.deltaTime;

        if (alertTimer > 0f)
            alertTimer -= Time.deltaTime;

        if (player == null)
        {
            Patrol();
            ApplyHitKnockbackMovement();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRadius)
        {
            isEngaged = true;
            lastKnownPlayerPosition = player.position;
        }

        if (isEngaged)
        {
            if (distanceToPlayer > loseRadius)
            {
                isEngaged = false;
            }
            else
            {
                ChaseAndAttack(distanceToPlayer);
                return;
            }
        }

        if (alertTimer > 0f)
        {
            lastKnownPlayerPosition = player.position;

            if (distanceToPlayer <= detectionRadius)
            {
                isEngaged = true;
                ChaseAndAttack(distanceToPlayer);
                return;
            }

            MoveToward(lastKnownPlayerPosition, chaseSpeed * 0.85f);
            return;
        }

        Patrol();
    }
    void MoveShip(Vector2 movement)
    {
        if (movement.sqrMagnitude <= 0.0001f)
            return;

        if (moveRoot == null)
            moveRoot = rb != null ? rb.transform : transform;

        Vector3 newPosition = moveRoot.position + (Vector3)movement;

        moveRoot.position = newPosition;

        if (rb != null && rb.bodyType != RigidbodyType2D.Static)
        {
            rb.position = newPosition;
        }
    }
    void FindPlayerIfMissing()
    {
        if (player != null)
            return;

        GameObject foundPlayer = GameObject.FindGameObjectWithTag(playerTag);

        if (foundPlayer != null)
            player = foundPlayer.transform;
    }

    void Patrol()
    {
        MoveInDirection(patrolDirection, patrolSpeed);
    }

    void ChaseAndAttack(float distanceToPlayer)
    {
        if (player == null)
            return;

        lastKnownPlayerPosition = player.position;

        if (distanceToPlayer > preferredDistanceToPlayer)
        {
            MoveToward(player.position, chaseSpeed);
        }

        if (distanceToPlayer <= attackRadius)
        {
            TryShootLaser();
        }
    }

    void MoveToward(Vector3 targetPosition, float speed)
    {
        Vector2 direction = targetPosition - transform.position;
        MoveInDirection(direction, speed);
    }

    void MoveInDirection(Vector2 direction, float speed)
    {
        if (direction.sqrMagnitude <= 0.001f)
            return;

        Vector2 normalizedDirection = direction.normalized;

        MoveShip(normalizedDirection * speed * Time.deltaTime);

        if (faceMoveDirection)
        {
            float angle = Mathf.Atan2(normalizedDirection.y, normalizedDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle + spriteRotationOffset);
        }
    }

    void TryShootLaser()
    {
        if (isShooting)
            return;

        if (laserCooldownTimer > 0f)
            return;

        if (player == null)
            return;

        StartCoroutine(ShootLaserRoutine());
    }

    IEnumerator ShootLaserRoutine()
    {
        isShooting = true;
        laserCooldownTimer = laserCooldown;

        if (laserLine != null)
        {
            laserLine.enabled = true;
            laserLine.startColor = laserWarningColor;
            laserLine.endColor = laserWarningColor;
        }

        float warningTimer = 0f;

        // Warning laser følger spilleren, så spilleren kan se at den bliver targeted.
        while (warningTimer < laserWarningTime)
        {
            warningTimer += Time.deltaTime;

            if (player != null)
                DrawLaser(player.position);

            yield return null;
        }

        if (player != null)
        {
            Vector3 fireTargetPosition = player.position;

            if (laserLine != null)
            {
                laserLine.startColor = laserFireColor;
                laserLine.endColor = laserFireColor;
            }

            DrawLaser(fireTargetPosition);

            PlayerHealth playerHealth = GetPlayerHealth();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(laserDamage);
            }
        }

        yield return new WaitForSeconds(laserVisibleTime);

        if (laserLine != null)
            laserLine.enabled = false;

        isShooting = false;
    }

    void DrawLaser(Vector3 targetPosition)
    {
        if (laserLine == null)
            return;

        laserLine.positionCount = 2;
        laserLine.SetPosition(0, GetLaserStartPosition());
        laserLine.SetPosition(1, targetPosition);
    }

    Vector3 GetLaserStartPosition()
    {
        if (laserStartPoint != null)
            return laserStartPoint.position;

        return transform.position;
    }

    void SetupLaser()
    {
        if (laserLine == null)
        {
            GameObject laserObject = new GameObject("LaserLine");
            laserObject.transform.SetParent(transform);
            laserObject.transform.localPosition = Vector3.zero;

            laserLine = laserObject.AddComponent<LineRenderer>();
        }

        laserLine.enabled = false;
        laserLine.useWorldSpace = true;
        laserLine.positionCount = 2;
        laserLine.startWidth = laserWidth;
        laserLine.endWidth = laserWidth;
        laserLine.sortingOrder = 100;

        if (laserLine.material == null)
        {
            Shader shader = Shader.Find("Sprites/Default");

            if (shader != null)
                laserLine.material = new Material(shader);
        }
    }

    public void TakeDamage(float damage)
    {
        Vector3 sourcePosition = transform.position;

        if (player != null)
            sourcePosition = player.position;

        TakeDamage(damage, sourcePosition);
    }

    public void TakeDamage(float damage, Vector3 sourcePosition)
    {
        if (isDead)
            return;

        if (damage <= 0f)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        PlayHitFeedback(damage, sourcePosition);

        TryCallBackup();

        if (currentHealth <= 0f)
            Die();
    }

    void TryCallBackup()
    {
        if (hasCalledBackup)
            return;

        if (currentHealth > maxHealth * backupCallHealthPercent)
            return;

        hasCalledBackup = true;

        Vector3 alertPosition = transform.position;

        if (player != null)
            alertPosition = player.position;

        for (int i = 0; i < allShips.Count; i++)
        {
            SpaceshipEnemy ship = allShips[i];

            if (ship == null)
                continue;

            if (ship == this)
                continue;

            if (ship.isDead)
                continue;

            float distance = Vector2.Distance(transform.position, ship.transform.position);

            if (distance <= backupCallRadius)
            {
                ship.ReceiveBackupCall(alertPosition, backupAlertDuration);
            }
        }
    }

    public void ReceiveBackupCall(Vector3 playerPosition, float duration)
    {
        if (isDead)
            return;

        lastKnownPlayerPosition = playerPosition;
        alertTimer = Mathf.Max(alertTimer, duration);
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (GameManager.instance != null)
        {
            GameManager.instance.AddCoinsFromEnemy(coinReward);

            if (diamondReward > 0)
                GameManager.instance.AddDiamondsFromEnemy(diamondReward);
        }

        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < sprites.Length; i++)
            sprites[i].enabled = false;

        MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < meshes.Length; i++)
            meshes[i].enabled = false;

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = false;

        if (laserLine != null)
            laserLine.enabled = false;

        Destroy(gameObject, destroyDelay);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        HandlePlayerContact(other.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandlePlayerContact(collision.gameObject);
    }

    void HandlePlayerContact(GameObject otherObject)
    {
        if (otherObject == null)
            return;

        if (!otherObject.CompareTag(playerTag))
            return;

        if (Time.time < lastContactTime + contactCooldown)
            return;

        lastContactTime = Time.time;

        float damageToShip = 10f;

        MeteorController playerController = otherObject.GetComponent<MeteorController>();

        if (playerController != null)
        {
            damageToShip += playerController.currentLiveMass * playerMassDamageMultiplier;
        }

        TakeDamage(damageToShip, otherObject.transform.position);

        PlayerHealth playerHealth = otherObject.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            playerHealth = otherObject.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
            playerHealth = otherObject.GetComponentInChildren<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(contactDamageToPlayer);
        }
    }
    void CacheSpriteRenderers()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        if (spriteRenderers == null || spriteRenderers.Length == 0)
            return;

        originalSpriteColors = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
                originalSpriteColors[i] = spriteRenderers[i].color;
        }
    }

    void PlayHitFeedback(float damage, Vector3 sourcePosition)
    {
        if (!useHitFeedback)
            return;

        StartHardKnockback(damage, sourcePosition);
        PlayHitFlash();
        SpawnHitParticles();
    }
    void StartHardKnockback(float damage, Vector3 sourcePosition)
    {
        MeteorController playerController = null;

        if (player != null)
        {
            playerController = player.GetComponent<MeteorController>();

            if (playerController == null)
                playerController = player.GetComponentInParent<MeteorController>();

            if (playerController == null)
                playerController = player.GetComponentInChildren<MeteorController>();
        }

        Vector2 direction;

        // Top-down impact feeling:
        // Hvis spilleren har fart, skubber vi rumskibet i spillerens bevægelsesretning.
        if (playerController != null && playerController.CurrentVelocity.sqrMagnitude > 0.01f)
        {
            direction = playerController.CurrentVelocity.normalized;
        }
        else
        {
            direction = transform.position - sourcePosition;

            if (direction.sqrMagnitude <= 0.001f)
                direction = -patrolDirection;

            direction.Normalize();
        }

        float playerSpeedBonus = 0f;

        if (playerController != null)
        {
            playerSpeedBonus = playerController.CurrentActualSpeed * hitKnockbackSpeedMultiplier;
        }

        float distance =
            hitKnockbackDistance +
            damage * hitKnockbackDistanceDamageMultiplier +
            playerSpeedBonus;

        distance = Mathf.Clamp(distance, 0.6f, maxHitKnockbackDistance);

        if (hardKnockbackCoroutine != null)
            StopCoroutine(hardKnockbackCoroutine);

        hardKnockbackCoroutine = StartCoroutine(HardKnockbackRoutine(direction, distance));
    }

    IEnumerator HardKnockbackRoutine(Vector2 direction, float distance)
    {
        isHardKnockbackActive = true;

        float timer = 0f;
        float lastProgress = 0f;

        while (timer < hardKnockbackDuration)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(timer / hardKnockbackDuration);

            // Ease out, så skubbet starter hårdt og stopper blødt
            float easedProgress = 1f - Mathf.Pow(1f - progress, 2f);

            float deltaProgress = easedProgress - lastProgress;
            lastProgress = easedProgress;

            Vector2 movement = direction * distance * deltaProgress;

            MoveShipHard(movement);

            yield return null;
        }

        isHardKnockbackActive = false;
        postHitAiLockTimer = postHitAiLockDuration;
        hardKnockbackCoroutine = null;
    }

    void MoveShipHard(Vector2 movement)
    {
        if (movement.sqrMagnitude <= 0.0001f)
            return;

        if (moveRoot == null)
            moveRoot = rb != null ? rb.transform : transform;

        Vector3 newPosition = moveRoot.position + (Vector3)movement;

        moveRoot.position = newPosition;

        if (rb != null && rb.bodyType != RigidbodyType2D.Static)
        {
            rb.position = newPosition;
        }
    }
    void ApplyHitKnockbackMovement()
    {
        if (hitKnockbackVelocity.sqrMagnitude <= 0.001f)
            return;

        Vector2 movement = hitKnockbackVelocity * Time.deltaTime;

        if (rb != null && rb.bodyType != RigidbodyType2D.Static)
        {
            rb.position += movement;
        }
        else
        {
            transform.position += (Vector3)movement;
        }

        hitKnockbackVelocity = Vector2.Lerp(
            hitKnockbackVelocity,
            Vector2.zero,
            hitKnockbackDamping * Time.deltaTime
        );
    }

    void PlayHitFlash()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
            CacheSpriteRenderers();

        if (spriteRenderers == null || spriteRenderers.Length == 0)
            return;

        if (hitFlashCoroutine != null)
            StopCoroutine(hitFlashCoroutine);

        hitFlashCoroutine = StartCoroutine(HitFlashRoutine());
    }

    IEnumerator HitFlashRoutine()
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
                spriteRenderers[i].color = hitFlashColor;
        }

        yield return new WaitForSeconds(hitFlashDuration);

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null && originalSpriteColors != null && i < originalSpriteColors.Length)
                spriteRenderers[i].color = originalSpriteColors[i];
        }

        hitFlashCoroutine = null;
    }

    void SpawnHitParticles()
    {
        if (hitImpactParticlesPrefab == null)
            return;

        GameObject particles = Instantiate(
            hitImpactParticlesPrefab,
            transform.position,
            Quaternion.identity
        );

        Destroy(particles, 2f);
    }
    float DistancePointToLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        Vector2 line = lineEnd - lineStart;

        float lineLengthSquared = line.sqrMagnitude;

        if (lineLengthSquared <= 0.001f)
            return Vector2.Distance(point, lineStart);

        float t = Vector2.Dot(point - lineStart, line) / lineLengthSquared;
        t = Mathf.Clamp01(t);

        Vector2 projection = lineStart + t * line;

        return Vector2.Distance(point, projection);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, loseRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, backupCallRadius);
    }
    PlayerHealth GetPlayerHealth()
    {
        if (player == null)
            return null;

        PlayerHealth hp = player.GetComponent<PlayerHealth>();

        if (hp == null)
            hp = player.GetComponentInParent<PlayerHealth>();

        if (hp == null)
            hp = player.GetComponentInChildren<PlayerHealth>();

        return hp;
    }
}