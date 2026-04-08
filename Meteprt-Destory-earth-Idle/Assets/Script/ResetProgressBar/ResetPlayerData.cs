using UnityEngine;

public class ResetPlayerData : MonoBehaviour
{
    [Header("Reset Options")]
    [Tooltip("If true, it will delete absolutely ALL saved data (including coins, settings, etc). If false, it only resets the upgrades.")]
    public bool wipeAbsolutelyEverything = false;

    // Denne funktion kører KUN, når en UI Button kalder den
    public void ResetAllUpgrades()
    {
        if (wipeAbsolutelyEverything)
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("ALL PlayerPrefs data has been completely wiped.");
        }
        else
        {
            PlayerPrefs.SetInt("SpeedLevel", 1);
            PlayerPrefs.SetInt("AccelLevel", 1);
            PlayerPrefs.SetInt("MassLevel", 1);
            PlayerPrefs.SetInt("EnduranceLevel", 1);
            PlayerPrefs.SetInt("HealthLevel", 1);
            PlayerPrefs.SetInt("AutoPilotLevel", 0);
            Debug.Log("Player upgrades have been reset to default.");
        }

        PlayerPrefs.Save();

        if (UpgradeManager.Instance != null)
        {
            if (wipeAbsolutelyEverything)
            {
                UpgradeManager.Instance.speedLevel = 1;
                UpgradeManager.Instance.accelLevel = 1;
                UpgradeManager.Instance.massLevel = 1;
                UpgradeManager.Instance.enduranceLevel = 1;
                UpgradeManager.Instance.healthLevel = 1;
                UpgradeManager.Instance.autoPilotLevel = 0;
            }
            else
            {
                UpgradeManager.Instance.speedLevel = PlayerPrefs.GetInt("SpeedLevel", 1);
                UpgradeManager.Instance.accelLevel = PlayerPrefs.GetInt("AccelLevel", 1);
                UpgradeManager.Instance.massLevel = PlayerPrefs.GetInt("MassLevel", 1);
                UpgradeManager.Instance.enduranceLevel = PlayerPrefs.GetInt("EnduranceLevel", 1);
                UpgradeManager.Instance.healthLevel = PlayerPrefs.GetInt("HealthLevel", 1);
                UpgradeManager.Instance.autoPilotLevel = PlayerPrefs.GetInt("AutoPilotLevel", 0);
            }

            UpgradeManager.Instance.UpdateUI();
        }
    }
}