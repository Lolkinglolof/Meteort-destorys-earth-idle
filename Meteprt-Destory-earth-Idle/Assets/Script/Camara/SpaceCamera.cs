using UnityEngine;

public class SpaceCamera : MonoBehaviour
{
    [Header("Targeting")]
    public Transform target;
    private MeteorController playerController;
    private Camera cam;

    [Header("Smoothness")]
    public float smoothTime = 0.25f;
    private Vector3 currentVelocity = Vector3.zero;

    // --- NYT: DYNAMISKE GRÆNSER ---
    [Header("Dynamic Vertical Limits")]
    [Tooltip("Hvor meget banen starter med at fylde (Når scale er 1)")]
    public float baseMinY = -8f;
    public float baseMaxY = 8f;

    [Tooltip("Hvor meget ekstra plads der lægges til toppen og bunden, for hver gang du vokser 1 i scale.")]
    public float boundaryExpansionFactor = 4f;

    [Header("Dynamic Zoom")]
    public float minSize = 5f;
    public float maxSize = 8f;
    public float zoomSpeed = 2f;

    [Header("Size Adaptation")]
    public float sizeSensitivity = 1.5f;

    [Header("Look Ahead")]
    public float leadAmount = 0.3f;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (target != null)
        {
            playerController = target.GetComponent<MeteorController>();
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. BEREGN ØNSKET POSITION
        Vector3 targetPos = target.position;

        if (playerController != null)
        {
            targetPos.x += (playerController.CurrentActualSpeed * leadAmount);
        }

        // =========================================================
        // --- NY LOGIK: UDVID VERDEN BASERET PÅ STØRRELSE ---
        // =========================================================

        // Vi tjekker hvor meget større end standard-størrelsen (1f) meteoren er
        float extraScale = Mathf.Max(0, target.localScale.x - 1f);

        // Vi regner ud hvor meget ekstra plads vi skal give kameraet
        float expansion = extraScale * boundaryExpansionFactor;

        // Vi udvider grænserne
        float currentMinY = baseMinY - expansion;
        float currentMaxY = baseMaxY + expansion;

        // Vi låser kameraets Y til de NYE, udvidede grænser
        targetPos.y = Mathf.Clamp(targetPos.y, currentMinY, currentMaxY);

        targetPos.z = -10f;

        // Bevæg kameraet blødt mod den begrænsede position
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, smoothTime);

        // =========================================================
        // 2. ZOOM LOGIK
        if (playerController != null)
        {
            float speedPercent = Mathf.InverseLerp(0, playerController.maxSpeed, playerController.CurrentActualSpeed);
            float speedZoom = Mathf.Lerp(minSize, maxSize, speedPercent);
            float playerScale = target.localScale.x;
            float sizeOffset = playerScale * sizeSensitivity;

            float finalTargetSize = speedZoom + sizeOffset;
            cam.orthographicSize = Mathf.MoveTowards(cam.orthographicSize, finalTargetSize, zoomSpeed * Time.deltaTime);
        }
    }
}