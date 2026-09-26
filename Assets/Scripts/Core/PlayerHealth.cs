using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;

    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Animator animator;

    private float currentHealth;
    private bool isDead;

    private static readonly int BlockImpactHash = Animator.StringToHash("BlockImpact");

    private void Awake()
    {
        currentHealth = maxHealth;
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

        if (animator != null)
        {
            animator.SetTrigger(BlockImpactHash);
        }

        Debug.Log("Attack blocked! Enemy staggered.");
    }

    private void Die()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} has died.");
        // Add death logic here (e.g., play death animation, disable player controls, etc.)
    }
}
