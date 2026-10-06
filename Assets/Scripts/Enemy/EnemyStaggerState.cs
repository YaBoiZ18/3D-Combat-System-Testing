using UnityEngine;

public class EnemyStaggerState : EnemyState
{
    private float staggerTimer;

    private static readonly int HitHash = Animator.StringToHash("Hit");
    // NEW: needed to clear a leftover attack trigger (see Enter)
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    public EnemyStaggerState(EnemyController enemy) : base(enemy)
    {
    }

    public override void Enter()
    {
        staggerTimer = enemy.StaggerDuration;

        // NEW: if the enemy was staggered mid-swing, its weapon hitbox could stay enabled
        // (the animation event that turns it off never plays). This makes sure it's off.
        enemy.DisableWeaponHitbox();

        // NEW: clear an attack trigger that was set but never used, so the enemy
        // doesn't suddenly attack right after the stagger animation.
        enemy.Animator.ResetTrigger(AttackHash);

        enemy.StopMoving();

        enemy.Animator.SetTrigger(HitHash);
    }

    public override void Update()
    {
        staggerTimer -= Time.deltaTime;

        if (staggerTimer > 0f)
            return;

        // Transition to the next state based on the player's distance
        if (enemy.DistanceToPlayer <= enemy.AttackRange)
        {
            enemy.StateMachine.ChangeState(enemy.AttackState);
        }
        else if (enemy.DistanceToPlayer <= enemy.DetectionRange)
        {
            enemy.StateMachine.ChangeState(enemy.ChaseState);
        }
        else
        {
            enemy.StateMachine.ChangeState(enemy.IdleState);
        }
    }
}