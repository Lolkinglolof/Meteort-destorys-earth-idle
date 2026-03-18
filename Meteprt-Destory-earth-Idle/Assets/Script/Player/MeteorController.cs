using UnityEngine;

public class MeteorController : MonoBehaviour
{
    [HideInInspector] public bool isAutoPiloting = false;

    [Header("Movement Stats")]
    public float minSpeed = 2f;
    public float maxSpeed = 4f;
    public float acceleration = 0.1f;
    public float cameraScrollSpeed = 2f;

    [Header("Velocity Tracking")]
    public float CurrentActualSpeed { get; private set; }
    public Vector3 CurrentVelocity { get; private set; }

    private float currentTargetSpeed;
    private Vector3 lastPosition;
    private Camera mainCamera;
    private bool isGrabbing = false;

    [Header("Live Stats")]
    public float currentLiveMass;

    [Header("Impact Feedback")]
    public float knockbackDamping = 8f;
    public float controlLockTimeAfterHit = 0.15f;

    private Vector2 knockbackVelocity;
    private float controlLockTimer;

    void Start()
    {
        if (UpgradeManager.Instance != null)
        {
            maxSpeed = UpgradeManager.Instance.GetCurrentMaxSpeed();
            acceleration = UpgradeManager.Instance.GetCurrentAcceleration();
        }

        mainCamera = Camera.main;
        lastPosition = transform.position;
        currentTargetSpeed = maxSpeed;

        RefreshMeteorScale();
    }

    public void ApplyImpact(float speedLoss, float impactForce, bool breakGrabOnHit, float otherMass, Vector2 hitDirection)
    {
        currentTargetSpeed -= speedLoss;
        currentTargetSpeed = Mathf.Max(currentTargetSpeed, minSpeed);

        // Kun tungere objekter giver rigtigt knockback
        if (otherMass > currentLiveMass)
        {
            float massRatio = otherMass / Mathf.Max(currentLiveMass, 0.01f);
            massRatio = Mathf.Clamp(massRatio, 1f, 3f);

            knockbackVelocity = hitDirection.normalized * impactForce * massRatio;

            if (breakGrabOnHit)
            {
                isGrabbing = false;
                controlLockTimer = controlLockTimeAfterHit;
            }
        }
    }

    public void RefreshMeteorScale()
    {
        if (UpgradeManager.Instance == null) return;

        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health == null)
        {
            Debug.LogWarning("PlayerHealth mangler på " + gameObject.name);
            return;
        }

        float baseUpgradedMass = UpgradeManager.Instance.GetCurrentMass();
        float healthMultiplier = health.currentMassMultiplier;
        float enduranceMod = UpgradeManager.Instance.GetEnduranceMultiplier();

        currentLiveMass = baseUpgradedMass * healthMultiplier;

        float loss = 1f - healthMultiplier;
        float protectedLoss = loss * enduranceMod;
        float effectiveMultiplier = 1f - protectedLoss;

        float scaleFactor = UpgradeManager.Instance.scaleIncreasePerLevel;
        float massLevel = UpgradeManager.Instance.massLevel;

        float newScale = (1f + (massLevel - 1) * scaleFactor) * effectiveMultiplier;
        newScale = Mathf.Max(newScale, 0.3f);
        transform.localScale = new Vector3(newScale, newScale, newScale);

        PlayerSkade skadeScript = GetComponent<PlayerSkade>();
        if (skadeScript != null)
        {
            skadeScript.baseMass = baseUpgradedMass * effectiveMultiplier;
        }
    }

    void Update()
    {
        RefreshMeteorScale();

        // 80% ved autopilot
        float speedMultiplier = isAutoPiloting ? 0.2f : 1.0f;
        float dynamicMax = maxSpeed * speedMultiplier;
        float dynamicAccel = acceleration * speedMultiplier;

        controlLockTimer = Mathf.Max(0f, controlLockTimer - Time.deltaTime);

        if (!isAutoPiloting && controlLockTimer <= 0f)
        {
            if (Input.GetMouseButtonDown(0)) isGrabbing = true;
            if (Input.GetMouseButtonUp(0)) isGrabbing = false;

            if (isGrabbing)
            {
                currentTargetSpeed = Mathf.MoveTowards(currentTargetSpeed, dynamicMax, dynamicAccel * Time.deltaTime);
                MoveMeteorToMouse();
            }
            else
            {
                currentTargetSpeed = Mathf.MoveTowards(currentTargetSpeed, minSpeed, dynamicAccel * 2f * Time.deltaTime);
                transform.Translate(Vector3.right * cameraScrollSpeed * Time.deltaTime, Space.World);
            }
        }
        else if (!isAutoPiloting)
        {
            currentTargetSpeed = Mathf.MoveTowards(currentTargetSpeed, minSpeed, dynamicAccel * 2f * Time.deltaTime);
            transform.Translate(Vector3.right * cameraScrollSpeed * Time.deltaTime, Space.World);
        }

        // Knockback bevægelse
        if (knockbackVelocity.sqrMagnitude > 0.001f)
        {
            transform.position += (Vector3)(knockbackVelocity * Time.deltaTime);
            knockbackVelocity = Vector2.Lerp(knockbackVelocity, Vector2.zero, knockbackDamping * Time.deltaTime);
        }

        // Velocity tracking
        if (Time.deltaTime > 0f)
        {
            CurrentVelocity = (transform.position - lastPosition) / Time.deltaTime;
            CurrentActualSpeed = CurrentVelocity.magnitude;
        }

        lastPosition = transform.position;
    }

    public void AutoPilotMove(Vector3 targetPosition)
    {
        // 80% ved autopilot
        float speedMultiplier = 0.2f;
        float dynamicMax = maxSpeed * speedMultiplier;
        float dynamicAccel = acceleration * speedMultiplier;

        currentTargetSpeed = Mathf.MoveTowards(currentTargetSpeed, dynamicMax, dynamicAccel * Time.deltaTime);

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            currentTargetSpeed * Time.deltaTime
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CollisionImpact impact = other.GetComponent<CollisionImpact>();
        if (impact == null)
        {
            impact = other.GetComponentInParent<CollisionImpact>();
        }

        if (impact == null) return;

        MeteorController otherMeteor = other.GetComponentInParent<MeteorController>();

        // Ignorer hvis vi rammer os selv / vores egne child colliders
        if (otherMeteor == this) return;

        float otherMass = impact.objectMass;

        if (otherMeteor != null)
        {
            otherMass = otherMeteor.currentLiveMass;
        }

        Vector2 hitDirection = (transform.position - other.transform.position).normalized;

        ApplyImpact(
            impact.speedPenalty,
            impact.impactForce,
            impact.breakGrabOnHit,
            otherMass,
            hitDirection
        );
    }

    void MoveMeteorToMouse()
    {
        if (mainCamera == null) return;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f;

        Vector3 targetPosition = mainCamera.ScreenToWorldPoint(mousePos);
        targetPosition.z = 0f;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            currentTargetSpeed * Time.deltaTime
        );
    }
}