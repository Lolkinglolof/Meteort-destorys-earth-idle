using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("Levels (Saved)")]
    public int speedLevel = 1;
    public int accelLevel = 1;
    public int massLevel = 1;
    public int enduranceLevel = 1;
    public int healthLevel = 1;    
    public int autoPilotLevel = 0; 

    [Header("Settings: Speed")]
    public float baseMaxSpeed = 5f;
    public float speedIncreasePerLevel = 1.5f;
    public float baseSpeedCost = 100f;

    [Header("Settings: Acceleration")]
    public float baseAcceleration = 5f;
    public float accelIncreasePerLevel = 2f;
    public float baseAccelCost = 250f;

    [Header("Settings: Mass")]
    public float baseMass = 10f;
    public float massIncreasePerLevel = 5f;
    public float baseMassCost = 500f;
    public float scaleIncreasePerLevel = 0.2f;

    [Header("Settings: Endurance")]
    public float baseEnduranceCost = 300f;
    public float enduranceReductionPerLevel = 0.05f;

    [Header("Settings: Health (Kun Mønter)")]
    public float baseHealth = 100f;
    public float healthIncreasePerLevel = 20f;
    public float baseHealthCost = 250f;

    [Header("Settings: Auto-Pilot")]
    public float baseAutoPilotTime = 5f;
    public float autoPilotTimeIncrease = 2.5f; 
    public float baseAutoPilotCoinCost = 650f;
    public float autoPilotCoinCostIncrease = 935f;

    [Header("UI References: Speed")]
    public TextMeshProUGUI speedStatsText;
    public TextMeshProUGUI speedCostText;
    public TextMeshProUGUI speedLevelText;
    public Button speedUpgradeButton;

    [Header("UI References: Acceleration")]
    public TextMeshProUGUI accelStatsText;
    public TextMeshProUGUI accelCostText;
    public TextMeshProUGUI accelLevelText;
    public Button accelUpgradeButton;

    [Header("UI References: Mass")]
    public TextMeshProUGUI massStatsText;
    public TextMeshProUGUI massCostText;
    public Button massUpgradeButton;

    [Header("UI References: Endurance")]
    public TextMeshProUGUI enduranceStatsText;
    public TextMeshProUGUI enduranceCostText;
    public TextMeshProUGUI enduranceLevelText;
    public Button enduranceUpgradeButton;

    [Header("UI References: Health")]
    public TextMeshProUGUI healthStatsText;
    public TextMeshProUGUI healthCostText;
    public TextMeshProUGUI healthLevelText;
    public Button healthUpgradeButton;

    [Header("UI References: Auto-Pilot")]
    public TextMeshProUGUI autoPilotStatsText;
    public TextMeshProUGUI autoPilotCostText;
    public TextMeshProUGUI autoPilotLevelText;
    public Button autoPilotUpgradeButton;

    [Header("Live Gameplay UI")]
    public TextMeshProUGUI liveMassDisplay;

    void Awake()
    {
        if (Instance == null) Instance = this;
        LoadUpgrades();
    }

    void Start()
    {
        UpdateUI();
        UpdatePlayerStats();
    }

    // --- GEM LOGIK ---
    void SaveUpgrades()
    {
        PlayerPrefs.SetInt("SpeedLevel", speedLevel);
        PlayerPrefs.SetInt("AccelLevel", accelLevel);
        PlayerPrefs.SetInt("MassLevel", massLevel);
        PlayerPrefs.SetInt("EnduranceLevel", enduranceLevel);
        PlayerPrefs.SetInt("HealthLevel", healthLevel);       
        PlayerPrefs.SetInt("AutoPilotLevel", autoPilotLevel); 
        PlayerPrefs.Save();
    }

    void LoadUpgrades()
    {
        speedLevel = PlayerPrefs.GetInt("SpeedLevel", 1);
        accelLevel = PlayerPrefs.GetInt("AccelLevel", 1);
        massLevel = PlayerPrefs.GetInt("MassLevel", 1);
        enduranceLevel = PlayerPrefs.GetInt("EnduranceLevel", 1);
        healthLevel = PlayerPrefs.GetInt("HealthLevel", 1);
        autoPilotLevel = PlayerPrefs.GetInt("AutoPilotLevel", 0);
    }

    // --- KØBS-FUNKTIONER ---
    public void UpgradeSpeed()
    {
        if (GameManager.instance.SpendCoins(GetSpeedUpgradeCost()))
        {
            speedLevel++;
            SaveUpgrades();
            UpdatePlayerStats();
            UpdateUI();
        }
    }

    public void UpgradeAcceleration()
    {
        if (GameManager.instance.SpendCoins(GetAccelUpgradeCost()))
        {
            accelLevel++;
            SaveUpgrades();
            UpdatePlayerStats();
            UpdateUI();
        }
    }

    public void UpgradeMass()
    {
        float coinCost = GetMassCoinCost();
        int diamondCost = GetMassDiamondCost();

        if (GameManager.instance.coins >= coinCost && GameManager.instance.diamonds >= diamondCost)
        {
            if (GameManager.instance.SpendCoins(coinCost))
            {
                GameManager.instance.AddDiamonds(-diamondCost);
                massLevel++;
                SaveUpgrades();
                UpdatePlayerStats();
                UpdateUI();
            }
        }
    }

    public void UpgradeEndurance()
    {
        float coinCost = GetEnduranceUpgradeCost();
        int diamondCost = GetEnduranceDiamondCost();

        if (GameManager.instance.coins >= coinCost && GameManager.instance.diamonds >= diamondCost)
        {
            if (GameManager.instance.SpendCoins(coinCost))
            {
                GameManager.instance.AddDiamonds(-diamondCost);
                enduranceLevel++;
                SaveUpgrades();
                UpdatePlayerStats();
                UpdateUI();
            }
        }
    }
    public void UpgradeHealth()
    {
        if (GameManager.instance.SpendCoins(GetHealthUpgradeCost()))
        {
            healthLevel++;
            SaveUpgrades();
            UpdatePlayerStats();
            UpdateUI();
        }
    }
    public void UpgradeAutoPilot()
    {
        float coinCost = GetAutoPilotCoinCost();
        int diamondCost = GetAutoPilotDiamondCost();

        if (GameManager.instance.coins >= coinCost && GameManager.instance.diamonds >= diamondCost)
        {
            if (GameManager.instance.SpendCoins(coinCost))
            {
                GameManager.instance.AddDiamonds(-diamondCost);
                autoPilotLevel++;
                SaveUpgrades();
                UpdatePlayerStats();
                UpdateUI();
            }
        }
    }
    public float GetCurrentMaxSpeed() => baseMaxSpeed + (speedLevel - 1) * speedIncreasePerLevel;
    public float GetNextMaxSpeed() => baseMaxSpeed + (speedLevel) * speedIncreasePerLevel;
    public float GetSpeedUpgradeCost() => baseSpeedCost * speedLevel;

    public float GetCurrentAcceleration() => baseAcceleration + (accelLevel - 1) * accelIncreasePerLevel;
    public float GetNextAcceleration() => baseAcceleration + (accelLevel) * accelIncreasePerLevel;
    public float GetAccelUpgradeCost() => baseAccelCost * accelLevel;

    public float GetCurrentMass() => baseMass + (massLevel - 1) * massIncreasePerLevel;
    public float GetNextMass() => baseMass + (massLevel) * massIncreasePerLevel;
    public float GetMassCoinCost() => baseMassCost + (massLevel - 1) * 500f;
    public int GetMassDiamondCost() => (massLevel < 3) ? 0 : Mathf.FloorToInt(massLevel / 10);

    public float GetEnduranceMultiplier() => Mathf.Clamp(1f - (enduranceLevel - 1) * enduranceReductionPerLevel, 0.5f, 1f);
    public float GetEnduranceUpgradeCost() => baseEnduranceCost * enduranceLevel;
    public int GetEnduranceDiamondCost() => (enduranceLevel < 3) ? 0 : Mathf.FloorToInt(enduranceLevel / 6); 

    public float GetCurrentMaxHealth() => baseHealth + (healthLevel - 1) * healthIncreasePerLevel;
    public float GetNextMaxHealth() => baseHealth + (healthLevel) * healthIncreasePerLevel;
    public float GetHealthUpgradeCost() => baseHealthCost * healthLevel;

    // BEREGNINGER FOR AUTO-PILOT (Koster 650 + 935 per level, og 2 diamanter for hver 3 level)
    public float GetCurrentAutoPilotTime() => autoPilotLevel == 0 ? 0f : baseAutoPilotTime + (autoPilotLevel - 1) * autoPilotTimeIncrease;
    public float GetNextAutoPilotTime() => baseAutoPilotTime + (autoPilotLevel) * autoPilotTimeIncrease;

    // Prisen stiger nu baseret på det rene level (ingen -1 for at regne pris ud)
    public float GetAutoPilotCoinCost() => baseAutoPilotCoinCost + (autoPilotLevel) * autoPilotCoinCostIncrease;
    public int GetAutoPilotDiamondCost() => (autoPilotLevel < 3) ? 0 : Mathf.FloorToInt(autoPilotLevel / 3) * 2;

    void Update()
    {
        if (GameManager.instance != null)
        {
            if (speedUpgradeButton != null) speedUpgradeButton.interactable = (GameManager.instance.coins >= GetSpeedUpgradeCost());
            if (accelUpgradeButton != null) accelUpgradeButton.interactable = (GameManager.instance.coins >= GetAccelUpgradeCost());

            if (massUpgradeButton != null)
                massUpgradeButton.interactable = (GameManager.instance.coins >= GetMassCoinCost() && GameManager.instance.diamonds >= GetMassDiamondCost());

            if (enduranceUpgradeButton != null)
                enduranceUpgradeButton.interactable = (GameManager.instance.coins >= GetEnduranceUpgradeCost() && GameManager.instance.diamonds >= GetEnduranceDiamondCost());

            // Health knap (kun mønter)
            if (healthUpgradeButton != null)
                healthUpgradeButton.interactable = (GameManager.instance.coins >= GetHealthUpgradeCost());

            // Auto-Pilot knap (mønter og diamanter)
            if (autoPilotUpgradeButton != null)
                autoPilotUpgradeButton.interactable = (GameManager.instance.coins >= GetAutoPilotCoinCost() && GameManager.instance.diamonds >= GetAutoPilotDiamondCost());

            if (liveMassDisplay != null)
            {
                MeteorController player = Object.FindFirstObjectByType<MeteorController>();
                if (player != null)
                {
                    liveMassDisplay.text = "Aktuel Masse: " + player.currentLiveMass.ToString("F1");
                    liveMassDisplay.color = player.currentLiveMass < 5f ? Color.red : Color.white;
                }
            }
        }
    }

    public void UpdateUI()
    {
        // Speed
        if (speedStatsText != null) speedStatsText.text = "Fart: " + GetCurrentMaxSpeed().ToString("F1") + " -> " + GetNextMaxSpeed().ToString("F1");
        if (speedCostText != null) speedCostText.text = "Pris: " + GetSpeedUpgradeCost().ToString("F0");
        if (speedLevelText != null) speedLevelText.text = "Lvl: " + speedLevel;

        // Accel
        if (accelStatsText != null) accelStatsText.text = "Acc: " + GetCurrentAcceleration().ToString("F1") + " -> " + GetNextAcceleration().ToString("F1");
        if (accelCostText != null) accelCostText.text = "Pris: " + GetAccelUpgradeCost().ToString("F0");
        if (accelLevelText != null) accelLevelText.text = "Lvl: " + accelLevel;

        // Mass
        if (massStatsText != null) massStatsText.text = "Masse: " + GetCurrentMass().ToString("F0") + " -> " + GetNextMass().ToString("F0");
        if (massCostText != null)
        {
            int dCost = GetMassDiamondCost();
            massCostText.text = "Pris: " + GetMassCoinCost().ToString("F0") + (dCost > 0 ? " & " + dCost + " Dia" : "");
        }

        // Endurance
        if (enduranceStatsText != null)
        {
            float currentResist = (1f - GetEnduranceMultiplier()) * 100f;
            float nextResist = (1f - (Mathf.Clamp(1f - (enduranceLevel) * enduranceReductionPerLevel, 0.5f, 1f))) * 100f;
            enduranceStatsText.text = "Endurance: " + currentResist.ToString("F0") + "% -> " + nextResist.ToString("F0") + "%";
        }
        if (enduranceCostText != null)
        {
            int dCost = GetEnduranceDiamondCost();
            enduranceCostText.text = "Pris: " + GetEnduranceUpgradeCost().ToString("F0") + (dCost > 0 ? " & " + dCost + " Dia" : "");
        }
        if (enduranceLevelText != null) enduranceLevelText.text = "Lvl: " + enduranceLevel;

        // --- HEALTH UI ---
        if (healthStatsText != null)
            healthStatsText.text = "Max HP: " + GetCurrentMaxHealth().ToString("F0") + " -> " + GetNextMaxHealth().ToString("F0");
        if (healthCostText != null)
            healthCostText.text = "Pris: " + GetHealthUpgradeCost().ToString("F0"); // Kun mønter!
        if (healthLevelText != null)
            healthLevelText.text = "Lvl: " + healthLevel;

        if (autoPilotStatsText != null)
        {
            // Viser "LÅST" hvis man ikke har købt den endnu
            if (autoPilotLevel == 0)
                autoPilotStatsText.text = "Auto-Pilot: LÅST -> " + GetNextAutoPilotTime().ToString("F1") + "s";
            else
                autoPilotStatsText.text = "Auto-Pilot: " + GetCurrentAutoPilotTime().ToString("F1") + "s -> " + GetNextAutoPilotTime().ToString("F1") + "s";
        }
        if (autoPilotCostText != null)
        {
            int dCost = GetAutoPilotDiamondCost();
            autoPilotCostText.text = "Pris: " + GetAutoPilotCoinCost().ToString("F0") + (dCost > 0 ? " & " + dCost + " Dia" : "");
        }
        if (autoPilotLevelText != null)
            autoPilotLevelText.text = "Lvl: " + autoPilotLevel;
    }

    void UpdatePlayerStats()
    {
        MeteorController player = Object.FindFirstObjectByType<MeteorController>();

        if (player != null)
        {
            player.maxSpeed = GetCurrentMaxSpeed();
            player.acceleration = GetCurrentAcceleration();

            PlayerSkade skadeScript = player.GetComponent<PlayerSkade>();
            if (skadeScript != null)
            {
                skadeScript.baseMass = GetCurrentMass();
            }

            PlayerHealth healthScript = player.GetComponent<PlayerHealth>();
            if (healthScript != null)
            {
                healthScript.UpgradeMaxHealth(GetCurrentMaxHealth());
            }

            player.RefreshMeteorScale();
        }
    }
}