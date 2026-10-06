using UnityEngine;

// Controls the player's light-attack combo flow, buffering, and hitbox damage.
public class CombatController : MonoBehaviour
{
    [Header("References")]
    // Component references resolved in the inspector.
    [SerializeField] private Animator animator;
    [SerializeField] private InputReader input;
    [SerializeField] private WeaponHitbox weaponHitbox;
    [SerializeField] private PlayerController player;

    [Header("Light Attack Damage")]
    // Damage values applied to the weapon hitbox per combo step.
    [SerializeField] private float lightAttack1Damage = 10f;
    [SerializeField] private float lightAttack2Damage = 12f;
    [SerializeField] private float lightAttack3Damage = 15f;

    // NEW: how long (in seconds) an attack press is remembered if it can't be used yet
    [Header("Feel")]
    // Short window to buffer an attack press so inputs feel responsive.
    [SerializeField] private float attackBufferTime = 0.3f;

    // NEW: the combo has 3 hits. Before, nothing stopped comboStep from going past 3.
    // Upper bound for combo steps to avoid invalid animator states.
    private const int MaxComboSteps = 3;

    private bool isAttacking;
    private bool canCombo;
    private int comboStep;

    // NEW: counts down after an attack press. While it's above 0, we keep trying to attack.
    // Tracks how long the recent attack press should be remembered.
    private float bufferTimer;

    // Public accessor used by other systems to check if an attack is active.
    public bool IsAttacking => isAttacking;

    private static readonly int AttackHash = Animator.StringToHash("Attack");
    // NEW: hash instead of the "ComboStep" string
    private static readonly int ComboStepHash = Animator.StringToHash("ComboStep");

    // Main update loop for input buffering and preventing actions while hit/dead.
    void Update()
    {
        // CHANGED: also clears the buffer, so a press from before you got hit
        // doesn't suddenly fire afterwards.
        if (!player.InCombat || player.IsDead || player.StateMachine.CurrentState == player.HitState)
        {
            bufferTimer = 0f;
            return;
        }

        // CHANGED: instead of attacking right now or losing the press, remember it briefly.
        if (input.AttackPressed)
        {
            bufferTimer = attackBufferTime;
        }

        if (bufferTimer > 0f)
        {
            bufferTimer -= Time.deltaTime;

            // If the attack went through, the press is used up.
            if (TryLightAttack())
            {
                bufferTimer = 0f;
            }
        }
    }

    // Kept so any other script that calls LightAttack() still compiles
    // Legacy compatibility wrapper that attempts an attack.
    public void LightAttack()
    {
        TryLightAttack();
    }

    // CHANGED: was LightAttack(). Now returns true if an attack actually started.
    // Tries to start an attack and returns true on success (used by the buffer).
    private bool TryLightAttack()
    {
        if (player.DodgeController.IsDodging)
            return false;

        // NEW: no attacking while blocking. Before, clicking while holding block started an
        // attack animation while the player was still in BlockState (so hits were still "blocked"
        // during the swing). A press made while blocking stays buffered for a moment, so it
        // fires right after you let go of block.
        if (player.IsBlocking)
            return false;

        // First attack
        if (!isAttacking)
        {
            StartAttack(1);
            return true;
        }

        // Next hit in the combo (only inside the combo window, and only up to 3 hits)
        if (canCombo && comboStep < MaxComboSteps)
        {
            canCombo = false;
            StartAttack(comboStep + 1);
            return true;
        }

        return false;
    }

    // NEW: the first attack and the combo attacks did the same 4 things, so it's one method now
    // Initializes attack state, updates animator parameters and sets hit damage.
    private void StartAttack(int step)
    {
        isAttacking = true;
        comboStep = step;

        animator.SetInteger(ComboStepHash, comboStep);
        animator.SetTrigger(AttackHash);

        SetCurrentAttackDamage();
    }

    // Animation event: the combo window opens
    // Animation hook that allows the next combo input.
    public void EnableCombo()
    {
        canCombo = true;
    }

    // Animation event: the combo window closes
    // Animation hook that disables combo chaining.
    public void DisableCombo()
    {
        canCombo = false;
    }

    // Animation event at the end of the attack animation
    // Resets attack-related flags and animator state.
    public void EndAttack()
    {
        isAttacking = false;
        canCombo = false;
        comboStep = 0;

        animator.ResetTrigger(AttackHash);
        animator.SetInteger(ComboStepHash, 0);
    }

    // Cancels the current attack, e.g. when the player is hit.
    // (The name has a typo, "Attak". Rename it with your IDE's rename tool so every
    // script that calls it updates automatically.)
    // Force-cancels the attack, clears buffer and disables hitbox immediately.
    public void CancelAttak()
    {
        if (!isAttacking)
            return;

        isAttacking = false;
        canCombo = false;
        comboStep = 0;
        bufferTimer = 0f; // NEW

        weaponHitbox.DisableHitbox();

        animator.ResetTrigger(AttackHash);
        animator.SetInteger(ComboStepHash, 0);
    }

    // Sets the weapon hitbox damage based on the active combo step.
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

        // CHANGED: removed the Debug.Log calls that printed on every attack
    }
}