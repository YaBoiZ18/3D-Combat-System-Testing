using UnityEngine;

public class CombatController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private InputReader input;
    [SerializeField] private WeaponHitbox weaponHitbox;
    [SerializeField] private PlayerController player;

    [Header("Light Attack Damage")]
    [SerializeField] private float lightAttack1Damage = 10f;
    [SerializeField] private float lightAttack2Damage = 12f;
    [SerializeField] private float lightAttack3Damage = 15f;

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
        if (player.DodgeController.IsDodging)
            return;

        // First attack
        if (!isAttacking)
        {
            isAttacking = true;
            comboStep = 1;

            animator.SetInteger("ComboStep", comboStep);
            animator.SetTrigger(AttackHash);

            SetCurrentAttackDamage();

            return;
        }


        // Queue next attack
        if (canCombo)
        {
            comboStep++;

            animator.SetInteger("ComboStep", comboStep);
            animator.SetTrigger(AttackHash);

            SetCurrentAttackDamage();

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

        animator.ResetTrigger(AttackHash);
        animator.SetInteger("ComboStep", 0);
    }

    // This method sets the damage of the weapon hitbox based on the current combo step
    private void SetCurrentAttackDamage()
    {
        switch (comboStep)
        {
            case 1:
                weaponHitbox.SetDamage(lightAttack1Damage);
                break;
            case 2:
                weaponHitbox.SetDamage(lightAttack2Damage);
                break;
            case 3:
                weaponHitbox.SetDamage(lightAttack3Damage);
                break;
        }

        Debug.Log($"Combo Step {comboStep} damage set.");
    }
}