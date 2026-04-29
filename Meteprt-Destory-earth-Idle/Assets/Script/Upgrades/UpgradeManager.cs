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
    public int autoRetryLevel = 0;
    public int defaultMeteorDefenceLevel = 0;
    public int rareMeteorDefenceLevel = 0;

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
    [Tooltip("Fast ekstra pris per Endurance level. 300 betyder +300 hver gang.")]
    public float enduranceFlatCostIncreasePerLevel = 300f;

    [Tooltip("Procent ekstra pris per Endurance level. 5 betyder 5% dyrere per level.")]
    public float endurancePercentCostIncreasePerLevel = 5f;
    [Tooltip("Max level for Endurance. Level 11 = 50% resistance hvis reduction per level er 0.05.")]
    public int maxEnduranceLevel = 11;

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
    [Tooltip("Procent ekstra pris per Atmosphere Shield level. 9 betyder 9% dyrere per level.")]
    public float atmosphereShieldPercentCostIncreasePerLevel = 9f;

    public float baseAtmosphereShieldRadius = 3f;
    public float atmosphereShieldRadiusIncreasePerLevel = 0.35f;

    public float baseAtmosphereShieldDamagePerSecond = 5f;
    public float atmosphereShieldDamageIncreasePerLevel = 2f;

    public float atmosphereShieldTickInterval = 0.25f;
    [Header("Settings: Auto Retry")]
    public float autoRetryCost = 100000f;

    [Header("Settings: Default Meteor Defence")]
    public float baseDefaultMeteorDefenceCost = 2000f;
    public float defaultMeteorDefenceCostIncreasePerLevel = 1250f;

    [Tooltip("Hvor mange procent mindre skade fra default meteorer per level. 3 betyder 3%.")]
    public float defaultMeteorDefenceReductionPerLevel = 3f;

    [Tooltip("Max procent skade-reduktion, så upgraden ikke bliver for OP.")]
    public float maxDefaultMeteorDefenceReduction = 35f;
    [Tooltip("Procent ekstra pris per Default Defence level. 5 betyder 5% dyrere per level.")]
    public float defaultMeteorDefencePercentCostIncreasePerLevel = 5f;

    [Tooltip("Max level for Default Meteor Defence. Hvis reduction er 3% per level og max er 60%, så bør max level være 20.")]
    public int maxDefaultMeteorDefenceLevel = 20;

    [Header("Settings: Rare Meteor Defence")]
    public float baseRareMeteorDefenceCost = 10000f;
    public float rareMeteorDefenceFlatCostIncreasePerLevel = 5000f;

    [Tooltip("Procent ekstra pris per Rare Meteor Defence level. 15 betyder 15% dyrere per level.")]
    public float rareMeteorDefencePercentCostIncreasePerLevel = 15f;

    [Tooltip("Hvor mange procent mindre skade fra rare meteorer per level. 2 betyder 2%.")]
    public float rareMeteorDefenceReductionPerLevel = 2f;

    [Tooltip("Max procent skade-reduktion mod rare meteorer.")]
    public float maxRareMeteorDefenceReduction = 50f;

    [Tooltip("Max level for Rare Meteor Defence.")]
    public int maxRareMeteorDefenceLevel = 25;

    [Tooltip("Diamond cost ved første køb.")]
    public int baseRareMeteorDefenceDiamondCost = 1;

    [Tooltip("Hvor mange ekstra diamanter pr level. 2 betyder +2 diamonds per level.")]
    public int rareMeteorDefenceDiamondIncreasePerLevel = 2;

    [Header("UI References: Rare Meteor Defence")]
    public TextMeshProUGUI rareMeteorDefenceStatsText;
    public TextMeshProUGUI rareMeteorDefenceCostText;
    public TextMeshProUGUI rareMeteorDefenceLevelText;
    public Button rareMeteorDefenceUpgradeButton;

    [Header("UI References: Default Meteor Defence")]
    public TextMeshProUGUI defaultMeteorDefenceStatsText;
    public TextMeshProUGUI defaultMeteorDefenceCostText;
    public TextMeshProUGUI defaultMeteorDefenceLevelText;
    public Button defaultMeteorDefenceUpgradeButton;

    [Header("UI References: Auto Retry")]
    public TextMeshProUGUI autoRetryStatsText;
    public TextMeshProUGUI autoRetryCostText;
    public TextMeshProUGUI autoRetryLevelText;
    public Button autoRetryUpgradeButton;

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
        PlayerPrefs.SetInt("AutoRetryLevel", autoRetryLevel);
        PlayerPrefs.SetInt("DefaultMeteorDefenceLevel", defaultMeteorDefenceLevel);
        PlayerPrefs.SetInt("RareMeteorDefenceLevel", rareMeteorDefenceLevel);
        PlayerPrefs.Save();
    }

    void LoadUpgrades()
    {
        speedLevel = PlayerPrefs.GetInt("SpeedLevel", 1);
        accelLevel = PlayerPrefs.GetInt("AccelLevel", 1);
        massLevel = PlayerPrefs.GetInt("MassLevel", 1);
        enduranceLevel = Mathf.Clamp(
        PlayerPrefs.GetInt("EnduranceLevel", 1),
        1,
         maxEnduranceLevel
        );
        healthLevel = PlayerPrefs.GetInt("HealthLevel", 1);
        autoPilotLevel = PlayerPrefs.GetInt("AutoPilotLevel", 0);
        incomeLevel = PlayerPrefs.GetInt("IncomeLevel", 1);
        atmosphereShieldLevel = PlayerPrefs.GetInt("AtmosphereShieldLevel", 0);
        autoRetryLevel = PlayerPrefs.GetInt("AutoRetryLevel", 0);
        defaultMeteorDefenceLevel = Mathf.Clamp(
        PlayerPrefs.GetInt("DefaultMeteorDefenceLevel", 0),
        0,
        maxDefaultMeteorDefenceLevel
        );
        rareMeteorDefenceLevel = PlayerPrefs.GetInt("RareMeteorDefenceLevel", 0);

    }
    public void UpgradeRareMeteorDefence()
    {
        if (rareMeteorDefenceLevel >= maxRareMeteorDefenceLevel)
        {
            UpdateUI();
            return;
        }

        float coinCost = GetRareMeteorDefenceCost();
        int diamondCost = GetRareMeteorDefenceDiamondCost();

        if (GameManager.instance != null &&
            GameManager.instance.coins >= coinCost &&
            GameManager.instance.diamonds >= diamondCost)
        {
            if (GameManager.instance.SpendCoins(coinCost))
            {
                GameManager.instance.AddDiamonds(-diamondCost);

                rareMeteorDefenceLevel++;
                rareMeteorDefenceLevel = Mathf.Min(rareMeteorDefenceLevel, maxRareMeteorDefenceLevel);

                SaveUpgrades();
                UpdatePlayerStats();
                UpdateUI();

                TutorialManager.Instance?.ReportUpgradeBought();
            }
        }
    }

    public float GetRareMeteorDefenceCost()
    {
        float cost = baseRareMeteorDefenceCost;

        for (int i = 0; i < rareMeteorDefenceLevel; i++)
        {
            cost += rareMeteorDefenceFlatCostIncreasePerLevel;
            cost *= 1f + (rareMeteorDefencePercentCostIncreasePerLevel / 100f);
        }

        return Mathf.Round(cost);
    }

    public int GetRareMeteorDefenceDiamondCost()
    {
        return baseRareMeteorDefenceDiamondCost + rareMeteorDefenceLevel * rareMeteorDefenceDiamondIncreasePerLevel;
    }

    public float GetCurrentRareMeteorDefenceReductionPercent()
    {
        return Mathf.Clamp(
            rareMeteorDefenceLevel * rareMeteorDefenceReductionPerLevel,
            0f,
            maxRareMeteorDefenceReduction
        );
    }

    public float GetNextRareMeteorDefenceReductionPercent()
    {
        return Mathf.Clamp(
            (rareMeteorDefenceLevel + 1) * rareMeteorDefenceReductionPerLevel,
            0f,
            maxRareMeteorDefenceReduction
        );
    }

    public float GetRareMeteorDefenceDamageMultiplier()
    {
        float reductionPercent = GetCurrentRareMeteorDefenceReductionPercent();
        return 1f - (reductionPercent / 100f);
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
        float cost = baseAtmosphereShieldCost;

        for (int i = 0; i < atmosphereShieldLevel; i++)
        {
            cost += atmosphereShieldCostIncreasePerLevel;
            cost *= 1f + (atmosphereShieldPercentCostIncreasePerLevel / 100f);
        }

        return Mathf.Round(cost);
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
    public bool GetAutoRetryUnlocked()
    {
        return autoRetryLevel > 0;
    }

    public float GetAutoRetryCost()
    {
        return autoRetryCost;
    }

    public void UpgradeAutoRetry()
    {
        if (autoRetryLevel > 0)
            return;

        if (GameManager.instance != null && GameManager.instance.SpendCoins(GetAutoRetryCost()))
        {
            autoRetryLevel = 1;

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
        if (enduranceLevel >= maxEnduranceLevel)
        {
            UpdateUI();
            return;
        }

        float coinCost = GetEnduranceUpgradeCost();
        int diamondCost = GetEnduranceDiamondCost();

        if (GameManager.instance.coins >= coinCost && GameManager.instance.diamonds >= diamondCost)
        {
            if (GameManager.instance.SpendCoins(coinCost))
            {
                GameManager.instance.AddDiamonds(-diamondCost);

                enduranceLevel++;
                enduranceLevel = Mathf.Min(enduranceLevel, maxEnduranceLevel);

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
    public void UpgradeDefaultMeteorDefence()
    {
        if (IsDefaultMeteorDefenceMaxed())
        {
            UpdateUI();
            return;
        }

        if (GameManager.instance != null && GameManager.instance.SpendCoins(GetDefaultMeteorDefenceCost()))
        {
            defaultMeteorDefenceLevel++;
            defaultMeteorDefenceLevel = Mathf.Min(defaultMeteorDefenceLevel, maxDefaultMeteorDefenceLevel);

            SaveUpgrades();
            UpdatePlayerStats();
            UpdateUI();

            TutorialManager.Instance?.ReportUpgradeBought();
        }
    }
    public bool IsDefaultMeteorDefenceMaxed()
    {
        return defaultMeteorDefenceLevel >= maxDefaultMeteorDefenceLevel ||
               GetCurrentDefaultMeteorDefenceReductionPercent() >= maxDefaultMeteorDefenceReduction;
    }
    public float GetDefaultMeteorDefenceCost()
    {
        float cost = baseDefaultMeteorDefenceCost;

        for (int i = 0; i < defaultMeteorDefenceLevel; i++)
        {
            cost += defaultMeteorDefenceCostIncreasePerLevel;
            cost *= 1f + (defaultMeteorDefencePercentCostIncreasePerLevel / 100f);
        }

        return Mathf.Round(cost);
    }

    public float GetCurrentDefaultMeteorDefenceReductionPercent()
    {
        return Mathf.Clamp(
            defaultMeteorDefenceLevel * defaultMeteorDefenceReductionPerLevel,
            0f,
            maxDefaultMeteorDefenceReduction
        );
    }

    public float GetNextDefaultMeteorDefenceReductionPercent()
    {
        return Mathf.Clamp(
            (defaultMeteorDefenceLevel + 1) * defaultMeteorDefenceReductionPerLevel,
            0f,
            maxDefaultMeteorDefenceReduction
        );
    }

    public float GetDefaultMeteorDefenceDamageMultiplier()
    {
        float reductionPercent = GetCurrentDefaultMeteorDefenceReductionPercent();
        return 1f - (reductionPercent / 100f);
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

    public float GetEnduranceMultiplier() => Mathf.Clamp(1f - (enduranceLevel - 1) * enduranceReductionPerLevel, 0.3f, 1f);
    public float GetEnduranceUpgradeCost()
    {
        float cost = baseEnduranceCost;

        for (int i = 1; i < enduranceLevel; i++)
        {
            cost += enduranceFlatCostIncreasePerLevel;
            cost *= 1f + (endurancePercentCostIncreasePerLevel / 100f);
        }

        return Mathf.Round(cost);
    }
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
                enduranceUpgradeButton.interactable =
                    enduranceLevel < maxEnduranceLevel &&
                    GameManager.instance.coins >= GetEnduranceUpgradeCost() &&
                    GameManager.instance.diamonds >= GetEnduranceDiamondCost();
            if (rareMeteorDefenceUpgradeButton != null)
            {
                rareMeteorDefenceUpgradeButton.interactable =
                    rareMeteorDefenceLevel < maxRareMeteorDefenceLevel &&
                    GameManager.instance.coins >= GetRareMeteorDefenceCost() &&
                    GameManager.instance.diamonds >= GetRareMeteorDefenceDiamondCost();
            }
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

            if (incomeUpgradeButton != null)
                incomeUpgradeButton.interactable = (GameManager.instance.coins >= GetIncomeUpgradeCost());

            if (defaultMeteorDefenceUpgradeButton != null)
                defaultMeteorDefenceUpgradeButton.interactable =
                    !IsDefaultMeteorDefenceMaxed() &&
                    GameManager.instance.coins >= GetDefaultMeteorDefenceCost();

            if (autoRetryUpgradeButton != null)
            {
                autoRetryUpgradeButton.interactable =
                    autoRetryLevel <= 0 &&
                    GameManager.instance.coins >= GetAutoRetryCost();
            }

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
        // Default Meteor Defence
        if (defaultMeteorDefenceStatsText != null)
        {
            if (IsDefaultMeteorDefenceMaxed())
            {
                defaultMeteorDefenceStatsText.text =
                    "Default Defence: " +
                    GetCurrentDefaultMeteorDefenceReductionPercent().ToString("0.#") +
                    "% MAX";
            }
            else
            {
                defaultMeteorDefenceStatsText.text =
                    "Default Defence: " +
                    GetCurrentDefaultMeteorDefenceReductionPercent().ToString("0.#") + "% -> " +
                    GetNextDefaultMeteorDefenceReductionPercent().ToString("0.#") + "%";
            }
        }

        if (defaultMeteorDefenceCostText != null)
        {
            if (IsDefaultMeteorDefenceMaxed())
            {
                defaultMeteorDefenceCostText.text = "MAX";
            }
            else
            {
                defaultMeteorDefenceCostText.text =
                    "Price: " + GetDefaultMeteorDefenceCost().ToString("F0");
            }
        }

        if (defaultMeteorDefenceLevelText != null)
        {
            defaultMeteorDefenceLevelText.text =
                "Lvl: " + defaultMeteorDefenceLevel + " / " + maxDefaultMeteorDefenceLevel;
        }
        if (autoRetryStatsText != null)
        {
            autoRetryStatsText.text =
                autoRetryLevel > 0
                ? "Auto Retry: UNLOCKED"
                : "Auto Retry: LOCKED";
        }

        if (autoRetryCostText != null)
        {
            autoRetryCostText.text =
                autoRetryLevel > 0
                ? "Bought"
                : "Price: " + GetAutoRetryCost().ToString("F0");
        }

        if (autoRetryLevelText != null)
        {
            autoRetryLevelText.text = "Lvl: " + autoRetryLevel + " / 1";
        }
        // Rare Meteor Defence
        if (rareMeteorDefenceStatsText != null)
        {
            if (rareMeteorDefenceLevel >= maxRareMeteorDefenceLevel)
            {
                rareMeteorDefenceStatsText.text =
                    "Rare Defence: " +
                    GetCurrentRareMeteorDefenceReductionPercent().ToString("0.#") +
                    "% MAX";
            }
            else
            {
                rareMeteorDefenceStatsText.text =
                    "Rare Defence: " +
                    GetCurrentRareMeteorDefenceReductionPercent().ToString("0.#") +
                    "% -> " +
                    GetNextRareMeteorDefenceReductionPercent().ToString("0.#") +
                    "%";
            }
        }

        if (rareMeteorDefenceCostText != null)
        {
            if (rareMeteorDefenceLevel >= maxRareMeteorDefenceLevel)
            {
                rareMeteorDefenceCostText.text = "MAX";
            }
            else
            {
                rareMeteorDefenceCostText.text =
                    "Price: " +
                    GetRareMeteorDefenceCost().ToString("F0") +
                    " & " +
                    GetRareMeteorDefenceDiamondCost() +
                    " Dia";
            }
        }

        if (rareMeteorDefenceLevelText != null)
        {
            rareMeteorDefenceLevelText.text =
                "Lvl: " + rareMeteorDefenceLevel + " / " + maxRareMeteorDefenceLevel;
        }
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

            if (enduranceLevel >= maxEnduranceLevel)
            {
                enduranceStatsText.text = "Endurance: " + currentResist.ToString("F0") + "% MAX";
            }
            else
            {
                float nextResist = (1f - Mathf.Clamp(
                    1f - enduranceLevel * enduranceReductionPerLevel,
                    0.5f,
                    1f
                )) * 100f;

                enduranceStatsText.text =
                    "Endurance: " + currentResist.ToString("F0") +
                    "% -> " + nextResist.ToString("F0") + "%";
            }
        }

        if (enduranceCostText != null)
        {
            if (enduranceLevel >= maxEnduranceLevel)
            {
                enduranceCostText.text = "MAX";
            }
            else
            {
                int dCost = GetEnduranceDiamondCost();
                enduranceCostText.text =
                    "Price: " + GetEnduranceUpgradeCost().ToString("F0") +
                    (dCost > 0 ? " & " + dCost + " Dia" : "");
            }
        }

        if (enduranceLevelText != null)
            enduranceLevelText.text = "Lvl: " + enduranceLevel + " / " + maxEnduranceLevel;

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
        total += Mathf.Max(0, autoRetryLevel);
        total += Mathf.Max(0, defaultMeteorDefenceLevel);
        total += Mathf.Max(0, rareMeteorDefenceLevel);
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