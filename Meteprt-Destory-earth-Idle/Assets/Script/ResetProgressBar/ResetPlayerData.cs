using UnityEngine;

public class ResetPlayerData : MonoBehaviour
{
    [Header("Reset Options")]
    [Tooltip("If true, it will delete absolutely ALL saved data including coins, diamonds, distance and upgrades. If false, it only resets the upgrades.")]
    public bool wipeAbsolutelyEverything = false;

    public void ResetAllUpgrades()
    {
        if (wipeAbsolutelyEverything)
        {
            PlayerPrefs.DeleteAll();

            ResetUpgradeValuesInMemory();

            if (GameManager.instance != null)
            {
                GameManager.instance.ResetEconomyAndProgress();
            }

            Debug.Log("ALL PlayerPrefs data has been completely wiped, including coins and diamonds.");
        }
        else
        {
            PlayerPrefs.SetInt("SpeedLevel", 1);
            PlayerPrefs.SetInt("AccelLevel", 1);
            PlayerPrefs.SetInt("MassLevel", 1);
            PlayerPrefs.SetInt("EnduranceLevel", 1);
            PlayerPrefs.SetInt("HealthLevel", 1);
            PlayerPrefs.SetInt("AutoPilotLevel", 0);

            ResetUpgradeValuesInMemory();

            Debug.Log("Player upgrades have been reset to default. Coins were NOT reset.");
        }

        PlayerPrefs.Save();

        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.UpdateUI();
        }
    }

    private void ResetUpgradeValuesInMemory()
    {
        if (UpgradeManager.Instance == null)
            return;

        UpgradeManager.Instance.speedLevel = 1;
        UpgradeManager.Instance.accelLevel = 1;
        UpgradeManager.Instance.massLevel = 1;
        UpgradeManager.Instance.enduranceLevel = 1;
        UpgradeManager.Instance.healthLevel = 1;
        UpgradeManager.Instance.autoPilotLevel = 0;
    }
}