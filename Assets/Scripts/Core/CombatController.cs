using UnityEngine;

public class CombatController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private InputReader input;

    [SerializeField] private PlayerController player;

    // Flag to indicate if the player is currently attacking
    private bool isAttacking;
    // Flag to indicate if the player can perform a combo attack
    private bool canCombo;
    // Step in the combo sequence
    private int comboStep;

    // Property to check if the player is currently attacking
    public bool IsAttacking => isAttacking;

    // Animator hash for the attack trigger
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    // Update is called once per frame
    void Update()
    {
        // Only allow attacks if the player is in combat
        if (!player.InCombat)
            return;

        if (input.AttackPressed)
        {
            LightAttack();
        }
    }

    // This method is called when the player presses the attack button
    public void LightAttack()
    {
        // First attack
        if (!isAttacking)
        {
            isAttacking = true;
            comboStep = 1;

            animator.SetInteger("ComboStep", comboStep);
            animator.SetTrigger(AttackHash);

            return;
        }


        // Queue next attack
        if (canCombo)
        {
            comboStep++;

            animator.SetInteger("ComboStep", comboStep);
            animator.SetTrigger(AttackHash);

            canCombo = false;
        }
    }

    public void EnableCombo()
    {
        canCombo = true;
    }

    public void DisableCombo()
    {
        canCombo = false;
    }

    // This method is called by an animation event at the end of the attack animation
    public void EndAttack()
    {
        isAttacking = false;
        canCombo = false;
        comboStep = 0;

        animator.SetInteger("ComboStep", 0);
    }
}