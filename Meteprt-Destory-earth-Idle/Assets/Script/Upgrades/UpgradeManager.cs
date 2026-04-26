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
    public int incomeLevel = 1;
    public int atmosphereShieldLevel = 0;

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

    [Header("Settings: Income")]
    public float baseIncomeCost = 1000f;
    public float incomeCostIncreasePerLevel = 500f;
    [Tooltip("Ekstra procent per income level. 0.001 betyder 0.001%. 1 betyder 1%.")]
    public float incomePercentIncreasePerLevel = 0.95f;

    [Header("Settings: Atmosphere Shield")]
    public float baseAtmosphereShieldCost = 25000f;
    public float atmosphereShieldCostIncreasePerLevel = 15000f;

    public float baseAtmosphereShieldRadius = 3f;
    public float atmosphereShieldRadiusIncreasePerLevel = 0.35f;

    public float baseAtmosphereShieldDamagePerSecond = 5f;
    public float atmosphereShieldDamageIncreasePerLevel = 2f;

    public float atmosphereShieldTickInterval = 0.25f;

    [Header("UI References: Atmosphere Shield")]
    public TextMeshProUGUI atmosphereShieldStatsText;
    public TextMeshProUGUI atmosphereShieldCostText;
    public TextMeshProUGUI atmosphereShieldLevelText;
    public Button atmosphereShieldUpgradeButton;

 

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
    public TextMeshProUGUI massLevelText;
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

    [Header("UI References: Income")]
    public TextMeshProUGUI incomeStatsText;
    public TextMeshProUGUI incomeCostText;
    public TextMeshProUGUI incomeLevelText;
    public Button incomeUpgradeButton;

    [Header("Live Gameplay UI")]
    public TextMeshProUGUI liveMassDisplay;

    [Header("DEV ONLY - Difficulty Debug")]
    [SerializeField] private int debugTotalUpgradeLevel;
    [SerializeField] private int debugEnemyDifficultyTier;
    [SerializeField] private int debugUpgradesUntilNextTier;

    [Header("Difficulty Settings")]
    [SerializeField] private int upgradesPerEnemyTier = 20;
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadUpgrades();
        UpdateDifficultyDebugInspector();
    }

    void Start()
    {
        UpdateDifficultyDebugInspector();
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
        PlayerPrefs.SetInt("IncomeLevel", incomeLevel);
        PlayerPrefs.SetInt("AtmosphereShieldLevel", atmosphereShieldLevel);
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
        incomeLevel = PlayerPrefs.GetInt("IncomeLevel", 1);
        atmosphereShieldLevel = PlayerPrefs.GetInt("AtmosphereShieldLevel", 0);
    }
    public void UpgradeIncome()
    {
        if (GameManager.instance.SpendCoins(GetIncomeUpgradeCost()))
        {
            incomeLevel++;
            SaveUpgrades();
            UpdatePlayerStats();
            UpdateUI();

            TutorialManager.Instance?.ReportUpgradeBought();
        }
    }
    public float GetIncomeUpgradeCost()
    {
        return baseIncomeCost + (incomeLevel - 1) * incomeCostIncreasePerLevel;
    }

    public float GetCurrentIncomeBonusPercent()
    {
        return (incomeLevel - 1) * incomePercentIncreasePerLevel;
    }

    public float GetNextIncomeBonusPercent()
    {
        return incomeLevel * incomePercentIncreasePerLevel;
    }

    public float GetCurrentIncomeMultiplier()
    {
        return 1f + (GetCurrentIncomeBonusPercent() / 100f);
    }
    // --- KØBS-FUNKTIONER ---
    public void UpgradeAtmosphereShield()
    {
        if (GameManager.instance.SpendCoins(GetAtmosphereShieldUpgradeCost()))
        {
            atmosphereShieldLevel++;

            SaveUpgrades();
            UpdatePlayerStats();
            UpdateUI();

            TutorialManager.Instance?.ReportUpgradeBought();
        }
    }
    public bool GetAtmosphereShieldUnlocked()
    {
        return atmosphereShieldLevel > 0;
    }

    public float GetAtmosphereShieldUpgradeCost()
    {
        return baseAtmosphereShieldCost + atmosphereShieldLevel * atmosphereShieldCostIncreasePerLevel;
    }

    public float GetCurrentAtmosphereShieldRadius()
    {
        if (atmosphereShieldLevel <= 0)
            return 0f;

        return baseAtmosphereShieldRadius + (atmosphereShieldLevel - 1) * atmosphereShieldRadiusIncreasePerLevel;
    }

    public float GetNextAtmosphereShieldRadius()
    {
        return baseAtmosphereShieldRadius + atmosphereShieldLevel * atmosphereShieldRadiusIncreasePerLevel;
    }

    public float GetCurrentAtmosphereShieldDamage()
    {
        if (atmosphereShieldLevel <= 0)
            return 0f;

        return baseAtmosphereShieldDamagePerSecond + (atmosphereShieldLevel - 1) * atmosphereShieldDamageIncreasePerLevel;
    }

    public float GetNextAtmosphereShieldDamage()
    {
        return baseAtmosphereShieldDamagePerSecond + atmosphereShieldLevel * atmosphereShieldDamageIncreasePerLevel;
    }

    public float GetCurrentAtmosphereShieldTickInterval()
    {
        return atmosphereShieldTickInterval;
    }
    public void UpgradeSpeed()
    {
        if (GameManager.instance.SpendCoins(GetSpeedUpgradeCost()))
        {
            speedLevel++;
            SaveUpgrades();
            UpdatePlayerStats();
            UpdateUI();

            TutorialManager.Instance?.ReportUpgradeBought();
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

            TutorialManager.Instance?.ReportUpgradeBought();
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

                TutorialManager.Instance?.ReportUpgradeBought();
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

                TutorialManager.Instance?.ReportUpgradeBought();
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

            TutorialManager.Instance?.ReportUpgradeBought();
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
                bool wasLockedBefore = autoPilotLevel == 0;

                GameManager.instance.AddDiamonds(-diamondCost);
                autoPilotLevel++;
                SaveUpgrades();
                UpdatePlayerStats();
                UpdateUI();

                TutorialManager.Instance?.ReportUpgradeBought();

                if (wasLockedBefore)
                    TutorialManager.Instance?.ReportAutopilotBoughtFirstTime();
            }
        }
    }
    public float GetCurrentMaxSpeed() => baseMaxSpeed + (speedLevel - 1) * speedIncreasePerLevel;
    public float GetNextMaxSpeed() => baseMaxSpeed + (speedLevel) * speedIncreasePerLevel;
    //public float GetSpeedUpgradeCost() => baseSpeedCost * Mathf.Pow(3f, speedLevel - 1);

    public float GetCurrentAcceleration() => baseAcceleration + (accelLevel - 1) * accelIncreasePerLevel;
    public float GetNextAcceleration() => baseAcceleration + (accelLevel) * accelIncreasePerLevel;
    public float GetAccelUpgradeCost() => baseAccelCost * accelLevel;

    public float GetCurrentMass() => baseMass + (massLevel - 1) * massIncreasePerLevel;
    public float GetNextMass() => baseMass + (massLevel) * massIncreasePerLevel;
    public float GetMassCoinCost() => baseMassCost + (massLevel - 1) * 500f;
    //public int GetMassDiamondCost() => (massLevel < 3) ? 0 : Mathf.FloorToInt(massLevel / 10);

    public float GetEnduranceMultiplier() => Mathf.Clamp(1f - (enduranceLevel - 1) * enduranceReductionPerLevel, 0.5f, 1f);
    public float GetEnduranceUpgradeCost() => baseEnduranceCost * enduranceLevel;
    //public int GetEnduranceDiamondCost() => (enduranceLevel < 3) ? 0 : Mathf.FloorToInt(enduranceLevel / 6); 

    public float GetCurrentMaxHealth() => baseHealth + (healthLevel - 1) * healthIncreasePerLevel;
    public float GetNextMaxHealth() => baseHealth + (healthLevel) * healthIncreasePerLevel;
    public float GetHealthUpgradeCost() => baseHealthCost * healthLevel;

    // BEREGNINGER FOR AUTO-PILOT (Koster 650 + 935 per level, og 2 diamanter for hver 3 level)
    public float GetCurrentAutoPilotTime() => autoPilotLevel == 0 ? 0f : baseAutoPilotTime + (autoPilotLevel - 1) * autoPilotTimeIncrease;
    public float GetNextAutoPilotTime() => baseAutoPilotTime + (autoPilotLevel) * autoPilotTimeIncrease;

    // Prisen stiger nu baseret på det rene level (ingen -1 for at regne pris ud)
    public float GetAutoPilotCoinCost() => baseAutoPilotCoinCost + (autoPilotLevel) * autoPilotCoinCostIncrease;
    //public int GetAutoPilotDiamondCost() => (autoPilotLevel < 3) ? 0 : Mathf.FloorToInt(autoPilotLevel / 3) * 2;
    public float GetSpeedUpgradeCost()
    {
        float extraPercentOfBase = baseSpeedCost * 6.40f; //
        return Mathf.Round(baseSpeedCost + (speedLevel - 1) * (baseSpeedCost + extraPercentOfBase));
    }

    public int GetMassDiamondCost()
    {
        return (massLevel < 3) ? 0 : (massLevel - 2);
    }

    public int GetEnduranceDiamondCost()
    {
        return (enduranceLevel < 3) ? 0 : (enduranceLevel - 2);
    }

    public int GetAutoPilotDiamondCost()
    {
        return (autoPilotLevel < 3) ? 0 : (autoPilotLevel - 2);
    }
    void Update()
    {
        UpdateDifficultyDebugInspector();
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

            if (atmosphereShieldUpgradeButton != null)
                atmosphereShieldUpgradeButton.interactable = (GameManager.instance.coins >= GetAtmosphereShieldUpgradeCost());

            // Auto-Pilot knap (mønter og diamanter)
            if (autoPilotUpgradeButton != null)
                autoPilotUpgradeButton.interactable = (GameManager.instance.coins >= GetAutoPilotCoinCost() && GameManager.instance.diamonds >= GetAutoPilotDiamondCost());

            if (incomeUpgradeButton != null)
                incomeUpgradeButton.interactable = (GameManager.instance.coins >= GetIncomeUpgradeCost());

            if (liveMassDisplay != null)
            {
                MeteorController player = Object.FindFirstObjectByType<MeteorController>();
                if (player != null)
                {
                    liveMassDisplay.text = "Aktuel Masse: " + player.currentLiveMass.ToString("F1");
                    liveMassDisplay.color = player.currentLiveMass < 5f ? Color.black : Color.black;
                }
            }
        }
    }

    public void UpdateUI()
    {

        // Speed
        if (speedStatsText != null) speedStatsText.text = "Speed: " + GetCurrentMaxSpeed().ToString("F1") + " -> " + GetNextMaxSpeed().ToString("F1");
        if (speedCostText != null) speedCostText.text = "Price: " + GetSpeedUpgradeCost().ToString("F0");
        if (speedLevelText != null) speedLevelText.text = "Lvl: " + speedLevel;

        // Accel
        if (accelStatsText != null) accelStatsText.text = "Acc: " + GetCurrentAcceleration().ToString("F2") + " -> " + GetNextAcceleration().ToString("F2");
        if (accelCostText != null) accelCostText.text = "Price: " + GetAccelUpgradeCost().ToString("F0");
        if (accelLevelText != null) accelLevelText.text = "Lvl: " + accelLevel;

        // Mass
        if (massStatsText != null) massStatsText.text = "Mass: " + GetCurrentMass().ToString("F0") + " -> " + GetNextMass().ToString("F0");
        if (massCostText != null)
        {
            int dCost = GetMassDiamondCost();
            massCostText.text = "Price: " + GetMassCoinCost().ToString("F0") + (dCost > 0 ? " & " + dCost + " Dia" : "");
        }
        if (massLevelText != null) massLevelText.text = "Lvl: " + massLevel;

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
            enduranceCostText.text = "Price: " + GetEnduranceUpgradeCost().ToString("F0") + (dCost > 0 ? " & " + dCost + " Dia" : "");
        }
        if (enduranceLevelText != null) enduranceLevelText.text = "Lvl: " + enduranceLevel;

        // --- HEALTH UI ---
        if (healthStatsText != null)
            healthStatsText.text = "Max HP: " + GetCurrentMaxHealth().ToString("F0") + " -> " + GetNextMaxHealth().ToString("F0");
        if (healthCostText != null)
            healthCostText.text = "Price: " + GetHealthUpgradeCost().ToString("F0"); // Kun mønter!
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
            autoPilotCostText.text = "Price: " + GetAutoPilotCoinCost().ToString("F0") + (dCost > 0 ? " & " + dCost + " Dia" : "");
        }
        if (autoPilotLevelText != null)
            autoPilotLevelText.text = "Lvl: " + autoPilotLevel;

        // Income
        if (incomeStatsText != null)
        {
            incomeStatsText.text =
                "Income: +" + GetCurrentIncomeBonusPercent().ToString("0.##") + "% " +
                "-> +" + GetNextIncomeBonusPercent().ToString("0.##") + "%";
        }
        if (incomeCostText != null)
            incomeCostText.text = "Price: " + GetIncomeUpgradeCost().ToString("F0");

        if (incomeLevelText != null)
            incomeLevelText.text = "Lvl: " + incomeLevel;
        if (atmosphereShieldStatsText != null)
        {
            if (atmosphereShieldLevel <= 0)
            {
                atmosphereShieldStatsText.text =
                    "Atmosphere Shield: LOCKED -> " +
                    "Radius " + GetNextAtmosphereShieldRadius().ToString("F1") +
                    " / DPS " + GetNextAtmosphereShieldDamage().ToString("F1");
            }
            else
            {
                atmosphereShieldStatsText.text =
                    "Atmosphere Shield: " +
                    "Radius " + GetCurrentAtmosphereShieldRadius().ToString("F1") +
                    " -> " + GetNextAtmosphereShieldRadius().ToString("F1") +
                    " / DPS " + GetCurrentAtmosphereShieldDamage().ToString("F1") +
                    " -> " + GetNextAtmosphereShieldDamage().ToString("F1");
            }
        }

        if (atmosphereShieldCostText != null)
            atmosphereShieldCostText.text = "Price: " + GetAtmosphereShieldUpgradeCost().ToString("F0");

        if (atmosphereShieldLevelText != null)
            atmosphereShieldLevelText.text = "Lvl: " + atmosphereShieldLevel;
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
            AtmosphereShieldWeapon shield = player.GetComponent<AtmosphereShieldWeapon>();

            if (shield != null)
            {
                shield.SetShieldUnlocked(GetAtmosphereShieldUnlocked());

                if (GetAtmosphereShieldUnlocked())
                {
                    shield.SetShieldStats(
                        GetCurrentAtmosphereShieldRadius(),
                        GetCurrentAtmosphereShieldDamage(),
                        GetCurrentAtmosphereShieldTickInterval()
                    );
                }
            }

            player.RefreshMeteorScale();
        }
    }
    public int GetTotalUpgradesBought()
    {
        int total = 0;

        total += Mathf.Max(0, speedLevel - 1);
        total += Mathf.Max(0, accelLevel - 1);
        total += Mathf.Max(0, massLevel - 1);
        total += Mathf.Max(0, enduranceLevel - 1);
        total += Mathf.Max(0, healthLevel - 1);
        total += Mathf.Max(0, incomeLevel - 1);
        total += Mathf.Max(0, atmosphereShieldLevel);

        // AutoPilot starter på 0, så den tælles direkte
        total += Mathf.Max(0, autoPilotLevel);
        // AutoPilot starter på 0, så den tælles direkte
        total += Mathf.Max(0, autoPilotLevel);

        return total;
    }

    public int GetEnemyDifficultyTier()
    {
        // 0 = normal enemies
        // 1 = efter 70 upgrades
        // 2 = efter 140 upgrades
        // 3 = efter 210 upgrades osv.
        return GetTotalUpgradesBought() / upgradesPerEnemyTier;
    }

    private void UpdateDifficultyDebugInspector()
    {
        debugTotalUpgradeLevel = GetTotalUpgradesBought();
        debugEnemyDifficultyTier = GetEnemyDifficultyTier();

        int progressInCurrentTier = debugTotalUpgradeLevel % upgradesPerEnemyTier;
        debugUpgradesUntilNextTier = upgradesPerEnemyTier - progressInCurrentTier;

        if (progressInCurrentTier == 0 && debugTotalUpgradeLevel > 0)
            debugUpgradesUntilNextTier = upgradesPerEnemyTier;
    }
}