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

    private float currentTargetSpeed;
    private Vector3 lastPosition;
    private Camera mainCamera;
    private bool isGrabbing = false;
    private UpgradeManager upgradeManager;
    public Vector3 CurrentVelocity { get; private set; }
    [Header("Live Stats")]
    public float currentLiveMass;
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

    public void ApplyImpact(float speedLoss)
    {
        currentTargetSpeed -= speedLoss;
        currentTargetSpeed = Mathf.Max(currentTargetSpeed, minSpeed);

        if (speedLoss > 5f) isGrabbing = false;

        Debug.Log("<color=blue>IMPACT:</color> Fart reduceret med " + speedLoss);
    }
    public void RefreshMeteorScale()
    {
        if (UpgradeManager.Instance == null) return;

        float baseUpgradedMass = UpgradeManager.Instance.GetCurrentMass();
        float healthMultiplier = GetComponent<PlayerHealth>().currentMassMultiplier;
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

        if (!isAutoPiloting)
        {
            if (Input.GetMouseButtonDown(0)) isGrabbing = true;
            if (Input.GetMouseButtonUp(0)) isGrabbing = false;

            if (isGrabbing)
            {
                currentTargetSpeed = Mathf.MoveTowards(currentTargetSpeed, maxSpeed, acceleration * Time.deltaTime);
                MoveMeteorToMouse();
            }
            else
            {
                currentTargetSpeed = Mathf.MoveTowards(currentTargetSpeed, minSpeed, acceleration * 2f * Time.deltaTime);
                transform.Translate(Vector3.right * cameraScrollSpeed * Time.deltaTime);
            }
        }

        if (Time.deltaTime > 0)
        {
            CurrentVelocity = (transform.position - lastPosition) / Time.deltaTime;
            CurrentActualSpeed = CurrentVelocity.magnitude;
        }
        lastPosition = transform.position;
    }
    public void AutoPilotMove(Vector3 targetPosition)
    {
        currentTargetSpeed = Mathf.MoveTowards(currentTargetSpeed, maxSpeed, acceleration * Time.deltaTime);

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            currentTargetSpeed * Time.deltaTime
        );
    }
    void MoveMeteorToMouse()
    {
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