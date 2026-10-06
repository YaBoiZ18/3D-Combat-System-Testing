using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private EnemyController enemyController;
    // NEW: found automatically on this object or its children if left empty
    [SerializeField] private LockOnTarget lockOnTarget;

    private float currentHealth;
    private bool isDead;

    // NEW: handy for health bars and other scripts
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;

    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int DieHash = Animator.StringToHash("Die");

    private void Awake()
    {
        currentHealth = maxHealth;

        if (lockOnTarget == null)
        {
            lockOnTarget = GetComponentInChildren<LockOnTarget>();
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
            return;
        }

        // CHANGED: taking damage now briefly stuns the enemy (it goes through the stagger state,
        // which also plays the Hit animation and cancels any attack in progress).
        // Before, the enemy played the Hit animation but kept walking and could keep attacking.
        if (enemyController != null)
        {
            enemyController.HitStun();
        }
        else if (animator != null)
        {
            animator.SetTrigger(HitHash);
        }
    }

    private void Die()
    {
        isDead = true;

        // NEW: stop the AI. Before, a dying enemy kept chasing and swinging at you
        // for the whole death animation.
        if (enemyController != null)
        {
            enemyController.Die();
        }

        // NEW: release the lock-on right away. LockOnController drops targets that get disabled.
        if (lockOnTarget != null)
        {
            lockOnTarget.enabled = false;
        }

        if (animator != null)
        {
            animator.ResetTrigger(HitHash); // a queued hit reaction must not override the death
            animator.SetTrigger(DieHash);
        }
    }

    // Called by an animation event at the end of the death animation
    public void FinishDeath()
    {
        // Placeholder: just hides the enemy. Replace with a corpse, loot drop, pooling, etc.
        gameObject.SetActive(false);
    }
}