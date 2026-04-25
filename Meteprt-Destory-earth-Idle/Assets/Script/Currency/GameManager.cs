using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

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

    [Header("Reward Juice Icons")]
    public Image coinIconImage;
    public Image diamondIconImage;

    [Header("Coin Reward Popup")]
    public RewardPopupText coinRewardPopupPrefab;
    public RectTransform coinPopupParent;

    [Header("Diamond Reward Popup")]
    public RewardPopupText diamondRewardPopupPrefab;
    public RectTransform diamondPopupParent;

    [Header("Reward Popup Spawn Points")]
    public RectTransform coinPopupSpawnPoint;
    public RectTransform diamondPopupSpawnPoint;

    [Header("Reward Shake Settings")]
    public float shakeDuration = 0.25f;
    public float shakeStrength = 8f;
    public float shakeRotation = 8f;

    private RewardPopupText activeCoinPopup;
    private RewardPopupText activeDiamondPopup;

    private double activeCoinPopupAmount;
    private int activeDiamondPopupAmount;

    private Coroutine coinShakeCoroutine;
    private Coroutine diamondShakeCoroutine;

    private Vector2 coinOriginalPosition;
    private Quaternion coinOriginalRotation;
    private bool coinOriginalSaved;

    private Vector2 diamondOriginalPosition;
    private Quaternion diamondOriginalRotation;
    private bool diamondOriginalSaved;
    [Header("Reward Debug")]
    public bool rewardDebug = true;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadData();

        if (playerTransform != null)
            startX = playerTransform.position.x;
    }

    void Update()
    {
        if (playerTransform != null)
        {
            float currentDist = playerTransform.position.x - startX;

            if (currentDist > distanceTraveled)
            {
                float diff = currentDist - distanceTraveled;
                distanceTraveled = currentDist;
                AddCoins(diff);
            }
        }

        saveTimer += Time.deltaTime;

        if (saveTimer >= saveInterval)
        {
            SaveData();
            saveTimer = 0f;
        }

        UpdateUI();
    }
    private RewardPopupText SpawnRewardPopup(RewardPopupText prefab, RectTransform spawnPoint)
    {
        RewardDebug("POPUP SPAWN START",
            "prefab=" + (prefab != null ? prefab.name : "NULL") +
            " | spawnPoint=" + (spawnPoint != null ? spawnPoint.name : "NULL")
        );

        if (prefab == null)
        {
            RewardDebug("POPUP FAILED", "Prefab is NULL");
            return null;
        }

        if (spawnPoint == null)
        {
            RewardDebug("POPUP FAILED", "Spawn point is NULL");
            return null;
        }

        RectTransform parentToUse = GetOrCreateRewardPopupLayer();

        if (parentToUse == null)
        {
            RewardDebug("POPUP FAILED", "RewardPopupLayer is NULL");
            return null;
        }

        RewardPopupText popup = Instantiate(prefab, parentToUse, false);
        popup.gameObject.SetActive(true);

        RectTransform popupRect = popup.GetComponent<RectTransform>();

        if (popupRect == null)
        {
            RewardDebug("POPUP FAILED", "Spawned popup has no RectTransform");
            Destroy(popup.gameObject);
            return null;
        }

        popupRect.anchorMin = new Vector2(0.5f, 0.5f);
        popupRect.anchorMax = new Vector2(0.5f, 0.5f);
        popupRect.pivot = new Vector2(0.5f, 0.5f);
        popupRect.localScale = Vector3.one;

        Vector2 anchoredPosition = GetSpawnPointPositionInPopupLayer(spawnPoint, parentToUse);
        anchoredPosition = ClampPopupPositionToLayer(anchoredPosition, parentToUse, 50f);

        popupRect.anchoredPosition = anchoredPosition;
        popup.SetOriginalPosition(anchoredPosition);

        popup.transform.SetAsLastSibling();

        RewardDebug(
            "POPUP SPAWNED",
            "popup=" + popup.name +
            " | activeSelf=" + popup.gameObject.activeSelf +
            " | activeInHierarchy=" + popup.gameObject.activeInHierarchy +
            " | anchoredPos=" + popupRect.anchoredPosition,
            popup
        );

        return popup;
    }

    private RectTransform GetOrCreateRewardPopupLayer()
    {
        Canvas canvas = null;

        if (coinText != null)
            canvas = coinText.GetComponentInParent<Canvas>();

        if (canvas == null && diamondText != null)
            canvas = diamondText.GetComponentInParent<Canvas>();

        if (canvas == null)
            canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            RewardDebug("POPUP LAYER FAILED", "No Canvas found in scene");
            return null;
        }

        Transform existingLayer = canvas.transform.Find("RewardPopupLayer");

        if (existingLayer != null)
        {
            RectTransform existingRect = existingLayer.GetComponent<RectTransform>();

            if (existingRect != null)
            {
                existingRect.gameObject.SetActive(true);
                existingRect.SetAsLastSibling();
                return existingRect;
            }
        }

        GameObject layerObject = new GameObject("RewardPopupLayer", typeof(RectTransform));
        layerObject.transform.SetParent(canvas.transform, false);

        RectTransform layerRect = layerObject.GetComponent<RectTransform>();

        layerRect.anchorMin = Vector2.zero;
        layerRect.anchorMax = Vector2.one;
        layerRect.offsetMin = Vector2.zero;
        layerRect.offsetMax = Vector2.zero;
        layerRect.localScale = Vector3.one;
        layerRect.SetAsLastSibling();

        RewardDebug("POPUP LAYER CREATED", "parentCanvas=" + canvas.name);

        return layerRect;
    }

    private Vector2 GetSpawnPointPositionInPopupLayer(RectTransform spawnPoint, RectTransform popupLayer)
    {
        Canvas canvas = popupLayer.GetComponentInParent<Canvas>();

        Camera canvasCamera = null;

        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            canvasCamera = canvas.worldCamera;

            if (canvasCamera == null)
                canvasCamera = Camera.main;
        }

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvasCamera, spawnPoint.position);

        Vector2 localPoint;

        bool converted = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            popupLayer,
            screenPoint,
            canvasCamera,
            out localPoint
        );

        RewardDebug(
            "POPUP POSITION CONVERT",
            "spawnPoint=" + spawnPoint.name +
            " | screenPoint=" + screenPoint +
            " | localPoint=" + localPoint +
            " | converted=" + converted
        );

        return localPoint;
    }

    private Vector2 ClampPopupPositionToLayer(Vector2 position, RectTransform layer, float margin)
    {
        Rect rect = layer.rect;

        float minX = rect.xMin + margin;
        float maxX = rect.xMax - margin;
        float minY = rect.yMin + margin;
        float maxY = rect.yMax - margin;

        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);

        return position;
    }

    private bool IsUsablePopup(RewardPopupText popup)
    {
        if (popup == null)
            return false;

        if (popup.gameObject == null)
            return false;

        if (!popup.gameObject.activeInHierarchy)
            return false;

        return true;
    }
    public void ResetEconomyAndProgress()
    {
        coins = 0;
        diamonds = 0;
        distanceTraveled = 0;

        if (playerTransform != null)
            startX = playerTransform.position.x;

        PlayerPrefs.SetString("TotalCoins", "0");
        PlayerPrefs.SetInt("PermanentDiamonds", 0);
        PlayerPrefs.Save();

        UpdateUI();

        RewardDebug("RESET ECONOMY", "Coins, diamonds and distance have been reset.");
    }
    private void ShowOrUpdateCoinPopup(double amount)
    {
        RewardDebug(
            "COIN POPUP REQUEST",
            "amount=" + amount +
            " | activePopup=" + IsUsablePopup(activeCoinPopup) +
            " | activeAmountBefore=" + activeCoinPopupAmount
        );

        if (amount <= 0)
        {
            RewardDebug("COIN POPUP BLOCKED", "amount was 0 or below");
            return;
        }

        if (!IsUsablePopup(activeCoinPopup))
        {
            activeCoinPopup = null;
            activeCoinPopupAmount = 0;

            RewardDebug("COIN POPUP CREATE", "No usable active coin popup, spawning new one");

            activeCoinPopup = SpawnRewardPopup(
                coinRewardPopupPrefab,
                coinPopupSpawnPoint
            );

            if (activeCoinPopup != null)
                activeCoinPopup.Finished = OnCoinPopupFinished;
        }

        if (!IsUsablePopup(activeCoinPopup))
        {
            RewardDebug("COIN POPUP FAILED", "activeCoinPopup is still not usable after spawn attempt");
            activeCoinPopup = null;
            activeCoinPopupAmount = 0;
            return;
        }

        activeCoinPopupAmount += amount;

        string popupText = "+" + System.Math.Floor(activeCoinPopupAmount).ToString("N0") + "$";

        RewardDebug(
            "COIN POPUP SETUP",
            "text=" + popupText +
            " | activeAmountAfter=" + activeCoinPopupAmount +
            " | popupActive=" + activeCoinPopup.gameObject.activeInHierarchy,
            activeCoinPopup
        );

        activeCoinPopup.Setup(popupText);
    }

    private void ShowOrUpdateDiamondPopup(int amount)
    {
        RewardDebug(
            "DIAMOND POPUP REQUEST",
            "amount=" + amount +
            " | activePopup=" + IsUsablePopup(activeDiamondPopup) +
            " | activeAmountBefore=" + activeDiamondPopupAmount
        );

        if (amount <= 0)
        {
            RewardDebug("DIAMOND POPUP BLOCKED", "amount was 0 or below");
            return;
        }

        if (!IsUsablePopup(activeDiamondPopup))
        {
            activeDiamondPopup = null;
            activeDiamondPopupAmount = 0;

            RewardDebug("DIAMOND POPUP CREATE", "No usable active diamond popup, spawning new one");

            activeDiamondPopup = SpawnRewardPopup(
                diamondRewardPopupPrefab,
                diamondPopupSpawnPoint
            );

            if (activeDiamondPopup != null)
                activeDiamondPopup.Finished = OnDiamondPopupFinished;
        }

        if (!IsUsablePopup(activeDiamondPopup))
        {
            RewardDebug("DIAMOND POPUP FAILED", "activeDiamondPopup is still not usable after spawn attempt");
            activeDiamondPopup = null;
            activeDiamondPopupAmount = 0;
            return;
        }

        activeDiamondPopupAmount += amount;

        string popupText = "+" + activeDiamondPopupAmount.ToString();

        RewardDebug(
            "DIAMOND POPUP SETUP",
            "text=" + popupText +
            " | activeAmountAfter=" + activeDiamondPopupAmount +
            " | popupActive=" + activeDiamondPopup.gameObject.activeInHierarchy,
            activeDiamondPopup
        );

        activeDiamondPopup.Setup(popupText);
    }

    private void OnCoinPopupFinished(RewardPopupText popup)
    {
        RewardDebug("COIN POPUP FINISHED",
            "popup=" + (popup != null ? popup.name : "NULL") +
            " | wasActive=" + (popup == activeCoinPopup)
        );

        if (popup == activeCoinPopup)
        {
            activeCoinPopup = null;
            activeCoinPopupAmount = 0;
        }
    }

    private void OnDiamondPopupFinished(RewardPopupText popup)
    {
        RewardDebug("DIAMOND POPUP FINISHED",
            "popup=" + (popup != null ? popup.name : "NULL") +
            " | wasActive=" + (popup == activeDiamondPopup)
        );

        if (popup == activeDiamondPopup)
        {
            activeDiamondPopup = null;
            activeDiamondPopupAmount = 0;
        }
    }
    public void RewardDebug(string source, string message, Object context = null)
    {
        if (!rewardDebug)
            return;

        Debug.Log("<color=#00E5FF>[REWARD DEBUG]</color> <b>" + source + "</b> | " + message, context);
    }

    public void AddCoins(double amount)
    {
        coins += amount;
    }

    public void AddCoinsFromEnemy(double amount)
    {
        AddRewardCoins(amount, "KILL");
    }

    public void AddCoinsFromHit(double amount)
    {
        AddRewardCoins(amount, "HIT");
    }

    private void AddRewardCoins(double amount, string rewardSource)
    {
        if (amount <= 0)
            return;

        double baseAmount = amount;
        double finalAmount = baseAmount;

        float incomePercent = 0f;
        float incomeMultiplier = 1f;

        // Income bonus kun på KILL reward, ikke meter coins
        if (rewardSource == "KILL" && UpgradeManager.Instance != null)
        {
            incomePercent = UpgradeManager.Instance.GetCurrentIncomeBonusPercent();
            incomeMultiplier = UpgradeManager.Instance.GetCurrentIncomeMultiplier();
            finalAmount = baseAmount * incomeMultiplier;
        }

        double before = coins;
        coins += finalAmount;

        RewardDebug("ADD REWARD COINS",
            "source=" + rewardSource +
            " | baseAmount=" + baseAmount +
            " | incomePercent=" + incomePercent +
            "% | multiplier=" + incomeMultiplier +
            " | finalAmount=" + finalAmount +
            " | before=" + before +
            " | after=" + coins
        );

        ShakeCoinIcon();
        ShowOrUpdateCoinPopup(finalAmount);
        UpdateUI();
        SaveData();
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

    public void AddDiamondsFromEnemy(int amount)
    {
        diamonds += amount;
        ShakeDiamondIcon();
        ShowOrUpdateDiamondPopup(amount);
        SaveData();
    }

    private void ShakeCoinIcon()
    {
        if (coinIconImage == null)
            return;

        if (coinShakeCoroutine != null)
            StopCoroutine(coinShakeCoroutine);

        coinShakeCoroutine = StartCoroutine(ShakeIcon(coinIconImage.rectTransform, true));
    }

    private void ShakeDiamondIcon()
    {
        if (diamondIconImage == null)
            return;

        if (diamondShakeCoroutine != null)
            StopCoroutine(diamondShakeCoroutine);

        diamondShakeCoroutine = StartCoroutine(ShakeIcon(diamondIconImage.rectTransform, false));
    }

    private IEnumerator ShakeIcon(RectTransform icon, bool isCoinIcon)
    {
        if (icon == null)
            yield break;

        if (isCoinIcon)
        {
            if (!coinOriginalSaved)
            {
                coinOriginalPosition = icon.anchoredPosition;
                coinOriginalRotation = icon.localRotation;
                coinOriginalSaved = true;
            }

            icon.anchoredPosition = coinOriginalPosition;
            icon.localRotation = coinOriginalRotation;
        }
        else
        {
            if (!diamondOriginalSaved)
            {
                diamondOriginalPosition = icon.anchoredPosition;
                diamondOriginalRotation = icon.localRotation;
                diamondOriginalSaved = true;
            }

            icon.anchoredPosition = diamondOriginalPosition;
            icon.localRotation = diamondOriginalRotation;
        }

        float timer = 0f;

        Vector2 startPosition = isCoinIcon ? coinOriginalPosition : diamondOriginalPosition;
        Quaternion startRotation = isCoinIcon ? coinOriginalRotation : diamondOriginalRotation;

        while (timer < shakeDuration)
        {
            timer += Time.unscaledDeltaTime;

            float progress = timer / shakeDuration;
            float currentStrength = Mathf.Lerp(shakeStrength, 0f, progress);
            float currentRotation = Mathf.Lerp(shakeRotation, 0f, progress);

            icon.anchoredPosition = startPosition + Random.insideUnitCircle * currentStrength;

            float randomZ = Random.Range(-currentRotation, currentRotation);
            icon.localRotation = Quaternion.Euler(0f, 0f, randomZ);

            yield return null;
        }

        icon.anchoredPosition = startPosition;
        icon.localRotation = startRotation;

        if (isCoinIcon)
            coinShakeCoroutine = null;
        else
            diamondShakeCoroutine = null;
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

    void OnApplicationQuit()
    {
        SaveData();
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)
            SaveData();
    }

    void UpdateUI()
    {
        if (coinText != null)
            coinText.text = ": " + System.Math.Floor(coins).ToString("N0") + "$";

        if (diamondText != null)
            diamondText.text = ": " + diamonds.ToString();

        if (distanceText != null)
            distanceText.text = ": " + Mathf.Floor(distanceTraveled).ToString() + "m";
    }
}