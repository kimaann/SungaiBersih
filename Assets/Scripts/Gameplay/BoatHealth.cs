
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class BoatHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int damagePerHit = 20;
    [SerializeField] private float damageCooldown = 1f;

    [Header("UI")]
    [SerializeField] private Text healthText;

    private Rigidbody rb;
    private BoatMovement boatMovement;

    private int currentHealth;
    private float nextDamageTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        boatMovement = GetComponent<BoatMovement>();

        maxHealth = Mathf.Max(1, maxHealth);
        damagePerHit = Mathf.Max(1, damagePerHit);

        currentHealth = maxHealth;

        UpdateHealthText();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("Obstacle"))
            return;

        if (currentHealth <= 0 || Time.time < nextDamageTime)
            return;

        nextDamageTime = Time.time + damageCooldown;

        currentHealth = Mathf.Max(
            0,
            currentHealth - damagePerHit
        );

        UpdateHealthText();

        if (currentHealth == 0)
            GameOver();
    }

    private void UpdateHealthText()
    {
        if (healthText == null)
            return;

        healthText.text = currentHealth > 0
            ? $"HP: {currentHealth}/{maxHealth}"
            : $"HP: 0/{maxHealth} - GAME OVER (Tekan R Restart)";
    }

    private void GameOver()
    {
        if (boatMovement != null)
            boatMovement.enabled = false;

        if (TryGetComponent(out BoatTrashCollector collector))
            collector.enabled = false;

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        Debug.Log("GAME OVER: HP perahu habis.");
    }

    private void Update()
    {
        if (currentHealth <= 0 && Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
        }
    }
}
