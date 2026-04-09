using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
    public bool isSystemOn = false;

    private Transform currentTarget;

    [Header("AI Radar Settings")]
    public List<string> tagsToAvoid = new List<string> { "Planet", "Obstacle", "Wall", "BlackHole" };
    [Header("UI Feedback")]
    public UnityEngine.UI.Image autoPilotIcon;
    public Sprite iconOnline;
    public Sprite iconOffline;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI hintText;

    private Color colorFlying = Color.white;
    private Color colorStandby = Color.yellow;
    private Color colorOffline = Color.gray;
    void Start()
    {
        controller = GetComponent<MeteorController>();

        // Load saved state (0 = off, 1 = on)
        isSystemOn = PlayerPrefs.GetInt("AutoPilotSavedState", 0) == 1;

        // <-- NYT: Sørg for at piloten er klar fra start, hvis den allerede var tændt.
        if (isSystemOn)
        {
            cooldownTimer = 0f;
        }
    }

    void Update()
    {
        // 0. SECURITY CHECK: If Auto-Pilot is not bought yet
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.autoPilotLevel == 0)
        {
            // Sluk for billedet og teksten fuldstændigt og stop funktionen
            if (autoPilotIcon != null) autoPilotIcon.gameObject.SetActive(false);
            if (statusText != null) statusText.gameObject.SetActive(false);
            if (hintText != null) hintText.gameObject.SetActive(false); // <-- 2. NY LINJE: Skjul også hint tekst
            return;
        }

        // 1. TOGGLE SYSTEM (P-Key)
        // 1. TOGGLE SYSTEM (P-Key)
        if (Input.GetKeyDown(KeyCode.P))
        {
            isSystemOn = !isSystemOn;

            PlayerPrefs.SetInt("AutoPilotSavedState", isSystemOn ? 1 : 0);
            PlayerPrefs.Save();

            if (!isSystemOn && isFlying)
            {
                DeactivatePilot(true);
            }
            else if (isSystemOn && cooldownTimer <= 0) // <-- NYT: Tjekker om den må starte
            {
                // Hvis tændt og piloten er udhvilet: Start med det samme! 
                ActivatePilot();
            }
            Debug.Log(isSystemOn
                ? "<color=cyan>AI SYSTEM: ONLINE</color>"
                : "<color=red>AI SYSTEM: OFFLINE</color>");
        }

        // 2. PLAYER OVERRIDE / EMERGENCY STOP
        if (isSystemOn && isFlying && Input.GetMouseButtonDown(0))
        {
            bool clickedOnUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

            if (!clickedOnUI)
            {
                Debug.Log("<color=orange>Manual override! Emergency stop activated. AI SYSTEM: OFFLINE</color>");

                isSystemOn = false;     // Shut down the entire system

                // Gem at systemet nu er slukket
                PlayerPrefs.SetInt("AutoPilotSavedState", 0);
                PlayerPrefs.Save();

                DeactivatePilot(false); // Stop pilot without starting cooldown
                return;                 // Important: stop the rest of Update immediately
            }
        }

        // 3. LOGIC FOR ACTIVATION AND COOLDOWN
        if (isSystemOn)
        {
            if (isFlying) HandleFlying();
            else HandleCooldown();
        }
        else
        {
            if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;
        }

        // 4. UPDATE UI ICON EVERY FRAME
        UpdateUIFeedback();
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

        // <-- NYT: Tjek for at undgå at restarte timeren, hvis den allerede flyver
        if (isFlying) return;

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

        // NYT: Start KUN en ny fuld cooldown, hvis vi reelt var i gang med at flyve. 
        // Hvis vi allerede var i gang med at tælle ned (f.eks. på pause), lader vi timeren køre videre fra hvor den slap.
        if (startCooldown && cooldownTimer <= 0)
        {
            cooldownTimer = maxCooldown;
        }
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
    // --- NY SMART UI FUNKTION ---
    // --- NY SMART UI FUNKTION MED TEKST ---
    void UpdateUIFeedback()
    {
        // 1. Check if the player has bought Auto-Pilot
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.autoPilotLevel == 0)
        {
            // Turn off image and texts completely and stop function
            if (autoPilotIcon != null) autoPilotIcon.gameObject.SetActive(false);
            if (statusText != null) statusText.gameObject.SetActive(false);
            if (hintText != null) hintText.gameObject.SetActive(false); // <-- 2. NY LINJE: Skjul også hint tekst
            return;
        }

        // 2. Sørg for at billedet og teksten er tændt, nu hvor vi ved, den ER købt
        if (autoPilotIcon != null && !autoPilotIcon.gameObject.activeSelf) autoPilotIcon.gameObject.SetActive(true);
        if (statusText != null && !statusText.gameObject.activeSelf) statusText.gameObject.SetActive(true);

        // <-- 3. NY BLOK: Tænd hint teksten og skriv automatisk [P]
        if (hintText != null && !hintText.gameObject.activeSelf)
        {
            hintText.gameObject.SetActive(true);
            hintText.text = "[P]";
            hintText.color = Color.white;
        }

        // 3. Resten af logikken for farver, sprites og tekst

        // 3. Resten af logikken for farver, sprites og tekst
        if (!isSystemOn)
        {
            // Systemet er slukket: Vis Offline billedet og skriv OFFLINE
            if (autoPilotIcon != null)
            {
                autoPilotIcon.sprite = iconOffline;
                autoPilotIcon.color = colorOffline;
            }
            if (statusText != null)
            {
                statusText.text = "OFFLINE";
                statusText.color = colorOffline;
            }
        }
        else
        {
            // Systemet er tændt: Vis Online billedet
            if (autoPilotIcon != null)
            {
                autoPilotIcon.sprite = iconOnline;
                autoPilotIcon.color = isFlying ? colorFlying : colorStandby;
            }

            // Skift teksten alt efter om den flyver eller holder pause (cooldown)
            if (statusText != null)
            {
                statusText.text = isFlying ? "ONLINE" : "STANDBY";
                statusText.color = isFlying ? colorFlying : colorStandby;
            }
        }
    }
}