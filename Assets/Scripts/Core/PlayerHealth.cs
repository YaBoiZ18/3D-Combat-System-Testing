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

    // NEW: lets other scripts (like WeaponHitbox) ask "is the player blocking?" without
    // needing to know about PlayerController
    public bool IsBlocking => playerController != null && playerController.IsBlocking;

    // NEW: true once the player has died
    public bool IsDead => isDead;

    // NEW: raised once, when the player dies
    public event System.Action Died;

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

        // NEW: dodge invincibility frames. The hit is simply ignored.
        if (playerController != null &&
            playerController.DodgeController != null &&
            playerController.DodgeController.IsInvulnerable)
        {
            return;
        }

        if (IsBlocking)
        {
            // CHANGED: null check, so a missing Animator doesn't crash here
            if (animator != null)
            {
                animator.SetTrigger(BlockImpactHash);
            }
            return;
        }

        currentHealth -= damage;

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

    // Called when the player successfully blocks an attack.
    public void TakeBlockedHit(GameObject attacker)
    {
        if (!IsBlocking)
            return;

        EnemyController enemy = attacker.GetComponentInParent<EnemyController>();

        // Stagger the enemy if it exists
        if (enemy != null)
        {
            enemy.Stagger();
        }

        currentGuard -= guardDamagePerBlock;
        guardRecoveryTimer = guardRecoveryDelay;

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

        if (currentGuard >= maxGuard)
            return;

        if (IsBlocking)
            return;

        // Wait for the recovery delay before guard starts refilling
        if (guardRecoveryTimer > 0f)
        {
            guardRecoveryTimer -= Time.deltaTime;
            return;
        }

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

        // NEW: a moment of slow motion makes the death hit harder
        HitStop.Trigger(0.6f, 0.25f);

        // NEW: switch the player into the death state (stops input, plays the death animation)
        if (playerController != null)
        {
            playerController.EnterDeathState();
        }

        // NEW: lets other scripts react (the DeathScreen listens to this)
        Died?.Invoke();
    }
}