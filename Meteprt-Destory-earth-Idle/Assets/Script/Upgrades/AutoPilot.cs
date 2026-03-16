using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

[RequireComponent(typeof(MeteorController))]
public class AutoPilot : MonoBehaviour
{
    private MeteorController controller;

    [Header("Timers")]
    private float activeTimer = 0f;
    private float cooldownTimer = 15f;

    private bool isFlying = false;

    [Header("System Status")]
    public bool isSystemOn = true;

    private Transform currentTarget;

    [Header("AI Radar Settings")]
    [Tooltip("Tilføj de tags her (f.eks. 'Planet', 'BlackHole'), som AI'en skal flyve udenom.")]
    public List<string> tagsToAvoid = new List<string> { "Planet", "Obstacle", "Wall", "BlackHole" };

    void Start()
    {
        controller = GetComponent<MeteorController>();

        isSystemOn = true;
        ActivatePilot();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            isSystemOn = !isSystemOn;

            if (isSystemOn)
            {
                Debug.Log("<color=cyan>AUTO-PILOT TÆNDT via 'P'!</color>");
                ActivatePilot();
            }
            else
            {
                Debug.Log("<color=red>AUTO-PILOT SLUKKET via 'P'!</color>");
                DeactivatePilot();
            }
        }

        if (isSystemOn && Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && !EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("<color=orange>Spilleren tog styringen! Auto-pilot afbrudt.</color>");
                isSystemOn = false;
                DeactivatePilot();
            }
        }

        if (isSystemOn)
        {
            if (isFlying)
            {
                activeTimer -= Time.deltaTime;
                if (activeTimer <= 0)
                {
                    DeactivatePilot();
                }
                else
                {
                    FindAndMoveToTarget();
                }
            }
            else
            {
                cooldownTimer -= Time.deltaTime;
                if (cooldownTimer <= 0)
                {
                    ActivatePilot();
                }
            }
        }
    }

    void ActivatePilot()
    {
        if (UpgradeManager.Instance == null) return;

        isFlying = true;
        controller.isAutoPiloting = true;

        activeTimer = UpgradeManager.Instance.GetCurrentAutoPilotTime();
        cooldownTimer = 15f;

        Debug.Log("<color=green>ALIEN PILOT FLYVER NU i " + activeTimer + " sekunder!</color>");
    }

    void DeactivatePilot()
    {
        isFlying = false;
        controller.isAutoPiloting = false;
        currentTarget = null;

        if (isSystemOn)
        {
            Debug.Log("<color=grey>ALIEN PILOT IDLE. Venter 15 sek. før han flyver igen.</color>");
        }
    }

    void FindAndMoveToTarget()
    {
        if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("SmallDebris");
            GameObject[] rareMeteors = GameObject.FindGameObjectsWithTag("RareMeteor");

            float closestDistance = Mathf.Infinity;

            foreach (GameObject enemy in enemies)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    currentTarget = enemy.transform;
                }
            }

            foreach (GameObject rare in rareMeteors)
            {
                float distance = Vector3.Distance(transform.position, rare.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    currentTarget = rare.transform;
                }
            }
        }
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
            bool shouldAvoid = false;

            foreach (string tag in tagsToAvoid)
            {
                if (hit.collider.CompareTag(tag))
                {
                    shouldAvoid = true;
                    break;
                }
            }

            if (shouldAvoid)
            {
                Vector3 avoidDirection = Vector3.Cross(moveDirection, Vector3.forward).normalized;
                finalTargetPosition = transform.position + (avoidDirection * 5f);
                Debug.DrawRay(transform.position, avoidDirection * 5f, Color.red);
            }
        }
        controller.AutoPilotMove(finalTargetPosition);
    }
}