using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Guard Settings")]
    [SerializeField] private float maxGuard = 100f;
    [SerializeField] private float guardDamagePerBlock = 20f;
    [SerializeField] private float guardRecoveryDelay = 2f;
    [SerializeField] private float guardRecoveryRate = 25f;

    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Animator animator;

    private float currentHealth;
    private bool isDead;
    private float currentGuard;
    private float guardRecoveryTimer;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    public float CurrentGuard => currentGuard;
    public float MaxGuard => maxGuard;

    private static readonly int BlockImpactHash = Animator.StringToHash("BlockImpact");

    private void Awake()
    {
        currentHealth = maxHealth;
        currentGuard = maxGuard;
    }

    public void TakeDamage(float damage)
    {

        if (isDead)
            return;

        if (playerController != null && playerController.IsBlocking)
        {
            Debug.Log("Attack blocked!");

            animator.SetTrigger(BlockImpactHash);
            return;
        }

        currentHealth -= damage;

        Debug.Log(
            $"{gameObject.name} took {damage} damage. " +
            $"Current health: {currentHealth}/{maxHealth}"
        );

        if (currentHealth <= 0f)
        {
            Die();
            return;
        }

        if (playerController != null)
        {
            playerController.EnterHitState();
        }
    }

    // This method is called when the player successfully blocks an attack.
    public void TakeBlockedHit(GameObject attacker)
    {
        if (playerController == null || !playerController.IsBlocking)
            return;

        EnemyController enemy = attacker.GetComponentInParent<EnemyController>();

        // Stagger the enemy if it exists
        if (enemy != null)
        {
            enemy.Stagger();
        }

        currentGuard -= guardDamagePerBlock;
        guardRecoveryTimer = guardRecoveryDelay;

        Debug.Log(
            $"Attack blocked! Guard: {currentGuard}/{maxGuard}"
        );

        if (currentGuard <= 0f)
        {
            currentGuard = 0f;

            playerController.EnterGuardBreakState();

            return;
        }

        if (animator != null)
        {
            animator.SetTrigger(BlockImpactHash);
        }
    }

    private void Update()
    {
        if (isDead)
            return;

        if(currentGuard >= maxGuard)
            return;

        if(playerController != null && playerController.IsBlocking)
            return;

        // Guard recovery logic
        if (guardRecoveryTimer > 0f)
        {
            guardRecoveryTimer -= Time.deltaTime;
            return;
        }
        // Recover guard over time
        currentGuard += guardRecoveryRate * Time.deltaTime;
        currentGuard = Mathf.Min(currentGuard, maxGuard);
    }

    public void ResetGuardRecovery()
    {
        guardRecoveryTimer = guardRecoveryDelay;
    }

    public void StartGuardRecoveryDelay()
    {
        guardRecoveryTimer = guardRecoveryDelay;
    }

    private void Die()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} has died.");
        // Add death logic here (e.g., play death animation, disable player controls, etc.)
    }
}
