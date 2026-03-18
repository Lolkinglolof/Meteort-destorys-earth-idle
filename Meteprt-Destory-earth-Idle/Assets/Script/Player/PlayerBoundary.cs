using UnityEngine;

public class PlayerBoundary : MonoBehaviour
{
    [Header("Dynamic Vertical Limits")]
    [Tooltip("Hvor meget banen fylder for en standard meteor (Scale = 1)")]
    public float baseMinY = -8f;
    public float baseMaxY = 8f;

    [Tooltip("Hvor meget ekstra plads der gives pr. level mass")]
    public float boundaryExpansionFactor = 4f;

    // --- NYT: OFFENTLIGE TAL SOM DE ANDRE SCRIPTS LÆSER ---
    [Header("Live Data (Læses af Kamera og Spawner)")]
    public float currentMinY;
    public float currentMaxY;

    void LateUpdate()
    {
        // 1. Regn ud hvor meget større meteoren er
        float extraScale = Mathf.Max(0, transform.localScale.x - 1f);
        float expansion = extraScale * boundaryExpansionFactor;

        // 2. Opdater de offentlige variabler
        currentMinY = baseMinY - expansion;
        currentMaxY = baseMaxY + expansion;

        // 3. Hold spilleren inde i banen
        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(pos.y, currentMinY, currentMaxY);
        transform.position = pos;
    }
}