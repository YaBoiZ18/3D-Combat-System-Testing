using UnityEngine;

public class CombatController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private InputReader input;

    // Flag to indicate if the player is currently attacking
    private bool isAttacking;

    // Property to check if the player is currently attacking
    public bool IsAttacking => isAttacking;

    // Animator hash for the attack trigger
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    // Update is called once per frame
    void Update()
    {
        if (input.AttackPressed)
        {
            LightAttack();
        }
    }

    // This method is called when the player presses the attack button
    public void LightAttack()
    {
        if (isAttacking) 
            return;

        isAttacking = true;

        animator.SetTrigger(AttackHash);
    }

    // This method is called by an animation event at the end of the attack animation
    public void EndAttack()
    {
        isAttacking = false;
    }
}