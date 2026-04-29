using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlanetCore : MonoBehaviour
{
    [Header("Planet Indstillinger")]
    public float maxPlanetHealth = 2000f;
    private float currentPlanetHealth;

    [Header("Visuals")]
    public GameObject giantExplosionPrefab;

    [Header("Win UI")]
    [Tooltip("Drag your You Win panel here from the Canvas.")]
    public GameObject winPanel;

    [Tooltip("How long after Earth is destroyed before the win menu appears.")]
    public float winMenuDelay = 2.5f;

    [Header("Win Slow Motion")]
    public float winSlowMotionScale = 0.25f;

    private bool hasWon = false;

    void Start()
    {
        currentPlanetHealth = maxPlanetHealth;

        if (winPanel != null)
            winPanel.SetActive(false);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasWon)
            return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

            if (playerHealth != null && playerHealth.currentHealth > 0)
            {
                float impactForce = playerHealth.currentHealth;

                currentPlanetHealth -= impactForce;

                Debug.Log(
                    "<color=orange>Meteoren ramte Jorden med " +
                    impactForce +
                    " kraft! Jordens HP er nu: " +
                    currentPlanetHealth +
                    "</color>"
                );

                if (currentPlanetHealth <= 0)
                {
                    DestroyPlanet(collision.gameObject);
                }
                else
                {
                    Debug.Log("<color=red>Meteoren var for lille og blev knust mod Jordens overflade!</color>");

                    playerHealth.TakeDamage(999999f);
                }
            }
        }
    }

    void DestroyPlanet(GameObject playerObject)
    {
        if (hasWon)
            return;

        hasWon = true;

        Debug.Log("<color=green>KABOOM! JORDEN ER UDSLETTET! DU VANDT!</color>");

        if (giantExplosionPrefab != null)
        {
            Instantiate(giantExplosionPrefab, transform.position, Quaternion.identity);
        }

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
            sprite.enabled = false;

        MeshRenderer mesh = GetComponent<MeshRenderer>();
        if (mesh != null)
            mesh.enabled = false;

        Collider2D coll = GetComponent<Collider2D>();
        if (coll != null)
            coll.enabled = false;

        DisablePlayerAfterWin(playerObject);

        StartCoroutine(WinRoutine());
    }

    void DisablePlayerAfterWin(GameObject playerObject)
    {
        if (playerObject == null)
            return;

        MeteorController controller = playerObject.GetComponent<MeteorController>();
        if (controller != null)
            controller.enabled = false;

        AutoPilot pilot = playerObject.GetComponent<AutoPilot>();
        if (pilot != null)
            pilot.enabled = false;

        PlayerSkade playerDamage = playerObject.GetComponent<PlayerSkade>();
        if (playerDamage != null)
            playerDamage.enabled = false;

        AtmosphereShieldWeapon shield = playerObject.GetComponent<AtmosphereShieldWeapon>();
        if (shield != null)
            shield.enabled = false;

        Collider2D playerCollider = playerObject.GetComponent<Collider2D>();
        if (playerCollider != null)
            playerCollider.enabled = false;
    }

    IEnumerator WinRoutine()
    {
        Time.timeScale = winSlowMotionScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(winMenuDelay);

        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0.02f;

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("WinPanel is not assigned on PlanetCore.");
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        SceneManager.LoadScene("MainMenu");
    }
}