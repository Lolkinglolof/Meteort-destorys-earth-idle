using UnityEngine;

public class StaticPlanet : MonoBehaviour
{
    [Header("References")]
    public GameObject planetVisuals;
    public Collider2D planetCollider;

    [Header("Settings")]
    public float activationDistance = 30f;

    private Transform playerTransform;
    private bool isActivated = false;

    void Start()
    {
        // Vi finder spillerens Transform her, så vi kan tracke hans location i Update
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }

        // Start deaktiveret
        if (planetVisuals != null) planetVisuals.SetActive(false);
        if (planetCollider != null) planetCollider.enabled = false;
    }

    void Update()
    {
        // Hvis vi ikke har fundet spilleren endnu, kan vi ikke udregne afstand
        if (playerTransform == null) return;

        // Her tjekker vi spillerens lokation lige nu!
        // Vi trækker spillerens X fra planetens X. 
        // Hvis planeten er på 1000 og spilleren er på 970, er distance = 30.
        float distance = transform.position.x - playerTransform.position.x;

        // Tjek om spilleren er tæt på lokationen
        if (!isActivated && distance <= activationDistance && distance > -5f)
        {
            ActivatePlanet();
        }
    }

    void ActivatePlanet()
    {
        isActivated = true;
        if (planetVisuals != null) planetVisuals.SetActive(true);
        if (planetCollider != null) planetCollider.enabled = true;

        Debug.Log("Spilleren er tæt på planetens lokation! Aktiverer: " + gameObject.name);
    }
}