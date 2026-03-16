using UnityEngine;

public class PlayerBoundary : MonoBehaviour
{
    [Header("Vertical Limits")]
    public float minY = -8f;
    public float maxY = 8f;

    void LateUpdate()
    {
        // Vi bruger LateUpdate for at sikre, at vi retter positionen 
        // EFTER at MeteorController har flyttet på os.
        Vector3 pos = transform.position;

        // Clamp sikrer, at værdien holdes inden for min og max
        // Formel: $y = \max(minY, \min(maxY, currentY))$
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        transform.position = pos;
    }
}