using UnityEngine;
using TMPro; // Husk denne for at bruge TextMesh Pro

public partial class SpeedometerUI : MonoBehaviour
{
    [Header("References")]
    public MeteorController playerController;
    public TextMeshProUGUI speedText;

    [Header("Settings")]
    [Tooltip("Hvad skal vi gange Unity-farten med for at få et realistisk tal?")]
    public float speedMultiplier = 120f;

    [Tooltip("Hvor hurtigt skal tallene tælle op/ned på skærmen (smoothness)")]
    public float displaySmoothness = 10f;

    private float displayedSpeed;

    void Update()
    {
        if (playerController == null || speedText == null) return;

        // 1. Hent den aktuelle fart fra din controller
        float actualSpeed = playerController.CurrentActualSpeed;

        // 2. Omregn til "falske" kilometer i timen
        float targetSpeed = actualSpeed * speedMultiplier;

        // 3. Gør tallet "smooth", så det ikke hopper for voldsomt
        // Vi bruger Lerp til at bevæge 'displayedSpeed' mod 'targetSpeed'
        displayedSpeed = Mathf.Lerp(displayedSpeed, targetSpeed, Time.deltaTime * displaySmoothness);

        // 4. Opdater teksten (0 betyder ingen decimaler)
        // Vi bruger "F0" for at vise hele tal, f.eks. "1250 km/t"
        speedText.text = displayedSpeed.ToString("F0") + " km/t";

        // Bonus: Du kan også gøre teksten lidt større, jo hurtigere man flyver!
        // speedText.transform.localScale = Vector3.one * (1f + (actualSpeed * 0.02f));
    }
}