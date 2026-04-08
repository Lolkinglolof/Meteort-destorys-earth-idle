using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

[RequireComponent(typeof(MeteorController))]
public class AutoPilot : MonoBehaviour
{
    private MeteorController controller;

    [Header("Timers")]
    public float activeTimer = 0f;
    public float cooldownTimer = 0f;
    private float maxCooldown = 15f;

    private bool isFlying = false;

    [Header("System Status")]
    public bool isSystemOn = true;

    private Transform currentTarget;

    [Header("AI Radar Settings")]
    public List<string> tagsToAvoid = new List<string> { "Planet", "Obstacle", "Wall", "BlackHole" };

    void Start()
    {
        controller = GetComponent<MeteorController>();
        //isSystemOn = true;
    }

    void Update()
    {
        // 1. TÆND/SLUK SYSTEM (P-knappen)
        if (Input.GetKeyDown(KeyCode.P))
        {
            isSystemOn = !isSystemOn;

            if (!isSystemOn && isFlying)
                DeactivatePilot(true);

            Debug.Log(isSystemOn
                ? "<color=cyan>AI SYSTEM: ONLINE</color>"
                : "<color=red>AI SYSTEM: OFFLINE</color>");
        }

        // 2. SPILLER OVERTAGELSE / NØDSTOP
        if (isSystemOn && isFlying && Input.GetMouseButtonDown(0))
        {
            bool clickedOnUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

            if (!clickedOnUI)
            {
                Debug.Log("<color=orange>Manuel indgriben! Nødstop aktiveret. AI SYSTEM: OFFLINE</color>");

                isSystemOn = false;     // hele systemet slukkes
                DeactivatePilot(false); // stop piloten uden at starte cooldown
                return;                 // vigtigt: stop resten af Update med det samme
            }
        }

        // 3. LOGIK FOR AKTIVERING OG COOLDOWN
        if (isSystemOn)
        {
            if (isFlying) HandleFlying();
            else HandleCooldown();
        }
        else
        {
            if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;
        }
    }

    void HandleFlying()
    {
        activeTimer -= Time.deltaTime;
        if (activeTimer <= 0)
        {
            Debug.Log("<color=grey>Piloten er træt og holder pause.</color>");
            DeactivatePilot(true);
        }
        else
        {
            FindAndMoveToTarget();
        }
    }

    void HandleCooldown()
    {
        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;
        else ActivatePilot();
    }

    void ActivatePilot()
    {
        if (UpgradeManager.Instance == null) return;
        isFlying = true;
        controller.isAutoPiloting = true;
        activeTimer = UpgradeManager.Instance.GetCurrentAutoPilotTime();
        Debug.Log("<color=green>PILOT AKTIVERET (Pilot bruger kun 20% Speed af din stats - 30% Skat)</color>");
    }

    void DeactivatePilot(bool startCooldown)
    {
        isFlying = false;
        controller.isAutoPiloting = false;
        currentTarget = null;
        activeTimer = 0;
        if (startCooldown) cooldownTimer = maxCooldown;
    }

    // --- NAVIGATIONSLOGIK (Nu meget smartere) ---
    void FindAndMoveToTarget()
    {
        float closestDistance = Mathf.Infinity;
        Transform bestTarget = null;

        GameObject[] debris = GameObject.FindGameObjectsWithTag("SmallDebris");
        GameObject[] rare = GameObject.FindGameObjectsWithTag("RareMeteor");

        foreach (GameObject obj in debris)
        {
            float dist = Vector3.Distance(transform.position, obj.transform.position);

            if (dist < closestDistance && obj.transform.position.x > transform.position.x - 2f)
            {
                closestDistance = dist;
                bestTarget = obj.transform;
            }
        }

        foreach (GameObject obj in rare)
        {
            float dist = Vector3.Distance(transform.position, obj.transform.position);
            if (dist < closestDistance && obj.transform.position.x > transform.position.x - 2f)
            {
                closestDistance = dist;
                bestTarget = obj.transform;
            }
        }

        currentTarget = bestTarget;

        Vector3 finalTargetPosition;

        if (currentTarget != null)
        {
            finalTargetPosition = currentTarget.position;
        }
        else
        {
            finalTargetPosition = transform.position + (Vector3.right * 10f);
        }

        Vector3 moveDirection = (finalTargetPosition - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, 5f);

        if (hit.collider != null)
        {
            foreach (string tag in tagsToAvoid)
            {
                if (hit.collider.CompareTag(tag))
                {
                    Vector3 avoidDirection = Vector3.Cross(moveDirection, Vector3.forward).normalized;
                    finalTargetPosition = transform.position + (avoidDirection * 5f);
                    break;
                }
            }
        }

        // Pilot bruger stadig 20% fart via MeteorController
        controller.AutoPilotMove(finalTargetPosition);
    }
}