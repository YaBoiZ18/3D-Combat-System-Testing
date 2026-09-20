using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;

    [Header("References")]
    [SerializeField] private Animator animator;

    private float currentHealth;
    private bool isDead;

    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int DieHash = Animator.StringToHash("Die");

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        Debug.Log($"{gameObject.name} took {damage} damage. Current health: {currentHealth}/{maxHealth}");
        

        if (currentHealth <= 0f)
        {
            Die();
            return;
        }

        animator.SetTrigger(HitHash);
    }

    private void Die()
    {
        isDead = true;

        Debug.Log($"{gameObject.name} has died.");

        animator.SetTrigger(DieHash);      
    }

    public void FinishDeath()
    {
        Debug.Log($"{gameObject.name} death animation finished.");
        // Here you can add logic to disable the enemy, play a death animation, or remove it from the scene.
        // For example:
        gameObject.SetActive(false);
    }
}

