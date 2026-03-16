using UnityEngine;
using TMPro;

public class MeteorScanner : MonoBehaviour
{
    // STATISK REFERENCE: Dette er "fælleshjernen". 
    // Den husker, hvilken meteor der sidst blev scannet.
    private static MeteorScanner activeScanner;

    [Header("Meteor Data")]
    public string asteroidName = "2022 WJ1";
    public string asteroidSize = "ca. 0.5 – 1.1 m";

    [Header("UI References")]
    public GameObject infoCanvas;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI sizeText;

    [Header("Settings")]
    public float displayDuration = 3f;
    private float timer;
    private bool isScannerActive = false;

    void Start()
    {
        if (infoCanvas != null)
        {
            infoCanvas.SetActive(false);
            nameText.text = asteroidName;
            sizeText.text = asteroidSize;
        }
    }

    void Update()
    {
        if (isScannerActive)
        {
            timer -= Time.deltaTime;

            // Hold teksten vandret
            infoCanvas.transform.rotation = Quaternion.identity;

            if (timer <= 0)
            {
                CloseScanner();
            }
        }
    }

    void OnMouseEnter()
    {
        OpenScanner();
    }

    void OpenScanner()
    {
        // 1. Tjek om der allerede er en anden meteor, der er aktiv
        if (activeScanner != null && activeScanner != this)
        {
            // Bed den anden meteor om at lukke sin UI med det samme
            activeScanner.CloseScanner();
        }

        // 2. Gør denne meteor til den aktive
        activeScanner = this;

        if (infoCanvas == null) return;

        infoCanvas.SetActive(true);
        timer = displayDuration;
        isScannerActive = true;
    }

    public void CloseScanner()
    {
        if (infoCanvas != null) infoCanvas.SetActive(false);
        isScannerActive = false;

        // Hvis vi lukker, og vi stadig er den "aktive", så nulstil fælleshjernen
        if (activeScanner == this)
        {
            activeScanner = null;
        }
    }

    // VIGTIGT: Hvis meteoren bliver ødelagt (Explode), mens scanneren er aktiv
    void OnDestroy()
    {
        if (activeScanner == this)
        {
            activeScanner = null;
        }
    }
}