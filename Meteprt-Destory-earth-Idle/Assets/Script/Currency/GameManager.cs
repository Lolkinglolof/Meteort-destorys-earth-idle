using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Economy")]
    public double coins;
    public int diamonds;

    [Header("Auto-Save Settings")]
    public float saveInterval = 5f;
    private float saveTimer;

    [Header("Tracking")]
    public float distanceTraveled;
    private float startX;
    public Transform playerTransform;

    [Header("UI Reference")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI diamondText;
    public TextMeshProUGUI distanceText;

    void Awake()
    {
        if (instance == null) instance = this;
        LoadData();
        if (playerTransform != null) startX = playerTransform.position.x;
    }

    void Update()
    {
        // 1. DISTANCE LOGIK
        float currentDist = playerTransform.position.x - startX;
        if (currentDist > distanceTraveled)
        {
            float diff = currentDist - distanceTraveled;
            distanceTraveled = currentDist;
            AddCoins(diff);
        }

        // 2. AUTO-SAVE PULS
        saveTimer += Time.deltaTime;
        if (saveTimer >= saveInterval)
        {
            SaveData();
            saveTimer = 0;
        }

        UpdateUI();
    }

    public void AddCoins(double amount)
    {
        coins += amount;
    }

    public bool SpendCoins(double amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            SaveData();
            return true;
        }
        return false;
    }

    public void AddDiamonds(int amount)
    {
        diamonds += amount;
        SaveData();
    }

    public void SaveData()
    {
        PlayerPrefs.SetString("TotalCoins", coins.ToString());
        PlayerPrefs.SetInt("PermanentDiamonds", diamonds);
        PlayerPrefs.Save();
    }

    void LoadData()
    {
        string savedCoins = PlayerPrefs.GetString("TotalCoins", "0");
        double.TryParse(savedCoins, out coins);
        diamonds = PlayerPrefs.GetInt("PermanentDiamonds", 0);
    }

    void OnApplicationQuit() { SaveData(); }
    void OnApplicationPause(bool pause) { if (pause) SaveData(); }

    void UpdateUI()
    {
        if (coinText != null)
            coinText.text = "Coins: " + System.Math.Floor(coins).ToString("N0");

        if (diamondText != null)
            diamondText.text = "Dia: " + diamonds.ToString();

        if (distanceText != null)
            distanceText.text = "Distance: " + Mathf.Floor(distanceTraveled).ToString() + "m";
    }
}