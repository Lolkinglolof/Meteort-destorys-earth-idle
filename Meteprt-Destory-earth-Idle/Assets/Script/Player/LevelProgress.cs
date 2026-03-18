using UnityEngine;
using UnityEngine.UI;

public class LevelProgress : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform earthTarget;
    public Slider progressBar;

    [Header("Fine Tuning")]
    [Tooltip("Hvor mange meter før kontakt skal baren sige 100%? (Gør baren mere tilfredsstillende)")]
    public float snapThreshold = 0.5f;

    private float initialGap;
    private bool levelComplete = false;

    void Start()
    {
        if (player != null && earthTarget != null)
        {
            initialGap = CalculateSurfaceDistance();
        }
    }

    void Update()
    {
        if (player == null || earthTarget == null || progressBar == null || levelComplete) return;

        float currentGap = CalculateSurfaceDistance();

        // Vi tjekker om vi er tæt nok på til at "snappe" til 100%
        if (currentGap <= snapThreshold)
        {
            FinishProgress();
            return;
        }

        // Beregn progress (0 til 1)
        float progress = Mathf.Clamp01(1f - (currentGap / initialGap));

        // Vi bruger SmoothStep for at gøre bevægelsen mere lækker de sidste par procent
        progressBar.value = Mathf.SmoothStep(progressBar.value, progress, 0.15f);
    }

    float CalculateSurfaceDistance()
    {
        float centerDistance = Vector3.Distance(player.position, earthTarget.position);

        // Vi henter radius, men tilføjer en lille "offset" buffer (0.2f) 
        // så baren føles som om den rammer 100% præcis ved visuel kontakt
        float playerRadius = (player.localScale.x * 0.5f) - 0.2f;
        float earthRadius = (earthTarget.localScale.x * 0.5f) - 0.2f;

        return centerDistance - playerRadius - earthRadius;
    }

    void FinishProgress()
    {
        levelComplete = true;
        progressBar.value = 1f; // Tving den i mål!
        OnReachGoal();
    }

    void OnReachGoal()
    {
        // Eventuel Level Complete logik her
    }
}