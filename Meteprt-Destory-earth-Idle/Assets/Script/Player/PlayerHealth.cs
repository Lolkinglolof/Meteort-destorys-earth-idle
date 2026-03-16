using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    private bool isDead = false;

    [HideInInspector] public float currentMassMultiplier = 1f;

    [Header("Visual Feedback")]
    public Image healthBarFill;
    public TextMeshProUGUI healthText;
    public Image damageVignette;
    public GameObject debrisPrefab;

    [Header("Game Over UI")]
    public GameObject gameOverPanel; // Træk dit Game Over UI Panel herind fra Inspectoren!

    private Vector3 originalScale;

    void Start()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        if (UpgradeManager.Instance != null)
        {
            maxHealth = UpgradeManager.Instance.GetCurrentMaxHealth();
        }

        currentHealth = maxHealth;
        originalScale = transform.localScale;

        UpdateHealthBar();

        if (damageVignette != null)
        {
            Color c = damageVignette.color;
            c.a = 0;
            damageVignette.color = c;
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    void Update()
    {
        if (damageVignette != null && damageVignette.color.a > 0)
        {
            Color c = damageVignette.color;
            c.a -= Time.unscaledDeltaTime * 2f;
            damageVignette.color = c;
        }
    }

    public void UpgradeMaxHealth(float newMaxHealth)
    {
        float healthIncrease = newMaxHealth - maxHealth;
        maxHealth = newMaxHealth;
        currentHealth += healthIncrease;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthBar();
        UpdateMeteorVisuals();
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        SpawnDebris(3);

        UpdateHealthBar();
        UpdateMeteorVisuals();

        if (damageVignette != null)
        {
            Color c = damageVignette.color;
            c.a = 0.5f;
            damageVignette.color = c;
        }

        if (currentHealth <= 0) GameOver();
    }

    void SpawnDebris(int amount)
    {
        if (debrisPrefab != null)
        {
            for (int i = 0; i < amount; i++)
            {
                GameObject piece = Instantiate(debrisPrefab, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));

                Rigidbody2D rb = piece.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 randomDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                    rb.AddForce(randomDir * 4f, ForceMode2D.Impulse);
                }
                Destroy(piece, 3f);
            }
        }
    }

    void UpdateHealthBar()
    {
        if (healthBarFill != null) healthBarFill.fillAmount = currentHealth / maxHealth;
        if (healthText != null) healthText.text = Mathf.Ceil(currentHealth) + " / " + maxHealth;
    }

    void UpdateMeteorVisuals()
    {
        float healthPercent = currentHealth / maxHealth;
        currentMassMultiplier = Mathf.Max(healthPercent, 0.1f);
    }

    void GameOver()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("<color=red>SPILLEREN ER DØD - STARTER SLOW MOTION!</color>");

        Time.timeScale = 0.3f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale; // Gør fysikken smooth i slowmotion

        MeshRenderer mesh = GetComponent<MeshRenderer>();
        if (mesh != null) mesh.enabled = false;

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null) sprite.enabled = false;

        SpawnDebris(20);

        Collider2D coll = GetComponent<Collider2D>();
        if (coll != null) coll.enabled = false;

        MeteorController controller = GetComponent<MeteorController>();
        if (controller != null) controller.enabled = false;

        AutoPilot pilot = GetComponent<AutoPilot>();
        if (pilot != null) pilot.enabled = false;

        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        yield return new WaitForSecondsRealtime(2.5f);

        // Tænd for Game Over menuen 
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        else
        {
            RestartGame();
        }
    }

    public void RestartGame()
    {
        // Nulstil tiden INDEN vi loader banen igen!
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        // Nulstil tiden INDEN vi går til menuen!
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        SceneManager.LoadScene("MainMenu");
    }
}