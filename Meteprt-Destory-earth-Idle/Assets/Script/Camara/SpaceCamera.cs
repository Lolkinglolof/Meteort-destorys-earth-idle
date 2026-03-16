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

    [Header("Vertical Limits")]
    [Tooltip("Sæt disse til det samme som i din PlayerBoundary script")]
    public float minY = -8f;
    public float maxY = 8f;

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

        // Kig fremad baseret på fart
        if (playerController != null)
        {
            targetPos.x += (playerController.CurrentActualSpeed * leadAmount);
        }

        // --- NY LOGIK: CLAMP KAMERAETS Y-AKSE ---
        // Vi låser kameraets Y, så det ikke filmer uden for banen
        targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);

        targetPos.z = -10f;

        // Bevæg kameraet blødt mod den begrænsede position
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, smoothTime);

        // 2. ZOOM LOGIK (Uændret, men stadig vigtig)
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