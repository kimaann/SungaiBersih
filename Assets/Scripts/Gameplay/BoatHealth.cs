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

    [Header("Game Flow")]
    [SerializeField] private RiverFinishController finishController;

    private Rigidbody rb;
    private BoatMovement boatMovement;

    private int currentHealth;
    private float nextDamageTime;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsAlive => currentHealth > 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        boatMovement = GetComponent<BoatMovement>();

        if (finishController == null)
            finishController = FindFirstObjectByType<RiverFinishController>();

        maxHealth = Mathf.Max(1, maxHealth);
        damagePerHit = Mathf.Max(1, damagePerHit);
        damageCooldown = Mathf.Max(0f, damageCooldown);

        currentHealth = maxHealth;

        UpdateHealthText();
    }

    private void Start()
    {
        // Fallback jika RiverFinishController belum ditemukan saat Awake.
        if (finishController == null)
            finishController = FindFirstObjectByType<RiverFinishController>();
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

        if (currentHealth <= 0)
            GameOver();
    }

    private void UpdateHealthText()
    {
        if (healthText == null)
            return;

        healthText.text = currentHealth > 0
            ? $"HP: {currentHealth}/{maxHealth}"
            : $"HP: 0/{maxHealth}";
    }

    private void GameOver()
    {
        if (finishController == null)
            finishController = FindFirstObjectByType<RiverFinishController>();

        if (finishController != null)
        {
            finishController.NotifyBoatHealthDepleted();
            return;
        }

        // Fallback jika controller akhir permainan tidak tersedia.
        if (boatMovement != null)
            boatMovement.enabled = false;

        if (TryGetComponent(out BoatTrashCollector collector))
            collector.enabled = false;

#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = Vector3.zero;
#else
        rb.velocity = Vector3.zero;
#endif

        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        Debug.LogError(
            "BoatHealth: HP habis, tetapi RiverFinishController tidak ditemukan."
        );
    }
}
