using UnityEngine;

public class EnemyAttackState : EnemyState
{
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    // CHANGED: hash instead of the "AttackStep" string
    private static readonly int AttackStepHash = Animator.StringToHash("AttackStep");

    private const int MaxAttackSteps = 3;

    // NEW: safety net. Normally the attack animation calls FinishAttack() through an event.
    // If that event doesn't fire, the enemy would freeze in this state forever.
    // Set it a little longer than your longest attack animation.
    private const float MaxAttackDuration = 2.5f;

    private bool isAttacking;     // NEW: true while an attack animation is playing
    private bool attackFinished;
    private int attackStep;
    private float attackTimer;    // NEW: how long the current attack has been running (failsafe)
    private float recoveryTimer;  // NEW: pause before the next attack
    private bool isTelegraphing;  // NEW: true during the warning right before a swing
    private bool warnedTriggerStuck; // NEW: so the "Animator didn't use the trigger" warning prints once per swing

    public EnemyAttackState(EnemyController enemy)
        : base(enemy)
    {
    }

    public override void Enter()
    {
        isAttacking = false;
        attackFinished = false;
        attackStep = 0;
        attackTimer = 0f;

        // CHANGED: the enemy no longer swings the instant it enters this state (for example
        // right after being staggered). It waits a short moment first, which gives the player
        // time to react.
        recoveryTimer = enemy.TimeBetweenAttacks;

        enemy.StopMoving();
        enemy.FacePlayer();
    }

    public override void Update()
    {
        // An attack is playing: let it finish. CHANGED: the enemy used to leave this state
        // (and start chasing) the moment the player stepped out of range, even mid-swing.
        if (isAttacking)
        {
            attackTimer += Time.deltaTime;

            // NEW: diagnostic. A trigger stays "on" until an Animator transition uses it. If it is
            // still on after a moment, the Animator has no transition that accepts it from the
            // state it's in, so the swing animation can never start. This says where it's stuck.
            if (!warnedTriggerStuck && attackTimer > 0.3f && enemy.Animator.GetBool(AttackHash))
            {
                warnedTriggerStuck = true;

                AnimatorClipInfo[] clips = enemy.Animator.GetCurrentAnimatorClipInfo(0);
                string clipName = clips.Length > 0 ? clips[0].clip.name : "unknown";

                Debug.LogWarning(
                    $"{enemy.name}: the Animator did not use the 'Attack' trigger " +
                    $"(AttackStep = {attackStep}). It is currently playing '{clipName}'. " +
                    "Check the transitions leaving that state (or from Any State) into your attack states.",
                    enemy
                );
            }

            if (attackTimer >= MaxAttackDuration)
            {
                attackFinished = true; // failsafe

                // NEW: clear the unused trigger and make sure the weapon is off, so a stuck
                // trigger can't fire a swing later at a random moment
                enemy.Animator.ResetTrigger(AttackHash);
                enemy.DisableWeaponHitbox();

                if (enemy.DebugAttackLogs)
                {
                    Debug.Log($"[Enemy:{enemy.name}] failsafe: the FinishAttack event never arrived, ending the swing after {MaxAttackDuration}s", enemy);
                }
            }

            if (!attackFinished)
                return;

            isAttacking = false;
            attackFinished = false;

            // NEW: short pause between hits, longer pause after the last hit of the chain
            recoveryTimer = attackStep >= MaxAttackSteps
                ? enemy.ChainCooldown
                : enemy.TimeBetweenAttacks;
        }

        // Between attacks: chase again if the player moved away.
        // CHANGED: uses AttackExitRange (a bit bigger than AttackRange) so the enemy
        // doesn't flicker between Chase and Attack when the player stands at the edge.
        if (enemy.DistanceToPlayer > enemy.AttackExitRange)
        {
            enemy.StateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        // NEW: once the warning starts, the enemy stops tracking you (if Lock Facing During
        // Telegraph is on), so moving sideways during the warning can make the swing miss.
        bool facingLocked = isTelegraphing && enemy.LockFacingDuringTelegraph;

        if (!facingLocked)
        {
            enemy.FacePlayer();
        }

        recoveryTimer -= Time.deltaTime;

        // NEW: the warning is the last part of the pause before a swing
        if (!isTelegraphing &&
            enemy.TelegraphDuration > 0f &&
            recoveryTimer <= enemy.TelegraphDuration)
        {
            isTelegraphing = true;
            enemy.BeginTelegraph();
        }

        if (isTelegraphing)
        {
            // 0 when the warning starts, 1 when the swing is about to begin
            float progress = 1f - Mathf.Clamp01(recoveryTimer / enemy.TelegraphDuration);
            enemy.SetTelegraphProgress(progress);
        }

        if (recoveryTimer > 0f)
            return;

        isTelegraphing = false;
        enemy.EndTelegraph();

        attackStep = attackStep >= MaxAttackSteps ? 1 : attackStep + 1;

        // CHANGED: only snap to face the player if the enemy was still tracking them.
        // Snapping after a locked-facing warning would cancel the sidestep.
        if (!facingLocked)
        {
            enemy.FacePlayerImmediately();
        }

        StartAttack();
    }

    // NEW: however the enemy leaves this state (staggered, player walked away...),
    // the warning tint must not stay on.
    public override void Exit()
    {
        isTelegraphing = false;
        enemy.EndTelegraph();
    }

    private void StartAttack()
    {
        isAttacking = true;
        attackFinished = false;
        attackTimer = 0f;
        warnedTriggerStuck = false;

        enemy.Animator.SetInteger(AttackStepHash, attackStep);
        enemy.Animator.SetTrigger(AttackHash);

        if (enemy.DebugAttackLogs)
        {
            Debug.Log($"[Enemy:{enemy.name}] swing {attackStep}: Attack trigger sent to the Animator", enemy);
        }
    }

    public void FinishAttack()
    {
        attackFinished = true;
    }

    public void InterruptAttack()
    {
        // NEW: also make sure the weapon can't keep dealing damage after an interrupt
        enemy.Animator.ResetTrigger(AttackHash);
        enemy.DisableWeaponHitbox();

        attackFinished = true;
        attackStep = 0;
    }
}