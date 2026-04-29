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

    [Header("Camera Edge Safety")]
    [Tooltip("Extra padding so the camera does not show too close to the edge.")]
    public float verticalEdgePadding = 0.2f;

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
        if (target == null || cam == null) return;

        // =========================================================
        // 1. DYNAMIC WORLD / PLAYER LIMITS
        // =========================================================

        float extraScale = Mathf.Max(0f, target.localScale.x - 1f);
        float expansion = extraScale * boundaryExpansionFactor;

        float currentMinY = baseMinY - expansion;
        float currentMaxY = baseMaxY + expansion;

        float worldHeight = currentMaxY - currentMinY;

        // =========================================================
        // 2. ZOOM LOGIC, BUT DO NOT ALLOW CAMERA TO SEE OUTSIDE LIMITS
        // =========================================================

        if (playerController != null)
        {
            float speedPercent = Mathf.InverseLerp(
                0f,
                playerController.maxSpeed,
                playerController.CurrentActualSpeed
            );

            float speedZoom = Mathf.Lerp(minSize, maxSize, speedPercent);

            float playerScale = target.localScale.x;
            float sizeOffset = playerScale * sizeSensitivity;

            float finalTargetSize = speedZoom + sizeOffset;

            // Important:
            // The camera's orthographic size is half of the visible height.
            // If the camera size is bigger than half the world height,
            // it becomes impossible to hide beyond the border.
            float maxAllowedSize = (worldHeight * 0.5f) - verticalEdgePadding;

            // Safety, so it never becomes negative or broken.
            maxAllowedSize = Mathf.Max(1f, maxAllowedSize);

            finalTargetSize = Mathf.Clamp(finalTargetSize, minSize, maxAllowedSize);

            cam.orthographicSize = Mathf.MoveTowards(
                cam.orthographicSize,
                finalTargetSize,
                zoomSpeed * Time.deltaTime
            );
        }

        // =========================================================
        // 3. CALCULATE CAMERA TARGET POSITION
        // =========================================================

        Vector3 targetPos = target.position;

        if (playerController != null)
        {
            targetPos.x += playerController.CurrentActualSpeed * leadAmount;
        }

        targetPos.z = -10f;

        // =========================================================
        // 4. CLAMP CAMERA CENTER SO EDGES NEVER SHOW OUTSIDE LIMITS
        // =========================================================

        float halfCameraHeight = cam.orthographicSize;

        float cameraMinY = currentMinY + halfCameraHeight + verticalEdgePadding;
        float cameraMaxY = currentMaxY - halfCameraHeight - verticalEdgePadding;

        // If the camera is too zoomed out for the available space,
        // lock it to the center instead of breaking the clamp.
        if (cameraMinY > cameraMaxY)
        {
            targetPos.y = (currentMinY + currentMaxY) * 0.5f;
        }
        else
        {
            targetPos.y = Mathf.Clamp(targetPos.y, cameraMinY, cameraMaxY);
        }

        // =========================================================
        // 5. SMOOTH FOLLOW
        // =========================================================

        Vector3 smoothedPosition = Vector3.SmoothDamp(
            transform.position,
            targetPos,
            ref currentVelocity,
            smoothTime
        );

        // Hard clamp after smoothing too, so SmoothDamp never visually leaks past the limit.
        if (cameraMinY > cameraMaxY)
        {
            smoothedPosition.y = (currentMinY + currentMaxY) * 0.5f;
        }
        else
        {
            smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, cameraMinY, cameraMaxY);
        }

        smoothedPosition.z = -10f;

        transform.position = smoothedPosition;
    }
}