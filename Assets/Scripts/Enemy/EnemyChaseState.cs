using UnityEngine;

public class EnemyChaseState : EnemyState
{
    public EnemyChaseState(EnemyController enemy)
        : base(enemy)
    {
    }

    public override void Enter()
    {
    }

    public override void Update()
    {
        float distance = enemy.DistanceToPlayer;

        // CHANGED: gives up at LoseTargetRange (a bit bigger than DetectionRange).
        // Before, both used the same distance, so a player standing right at the edge made
        // the enemy flicker between Idle and Chase.
        if (distance > enemy.LoseTargetRange)
        {
            enemy.StateMachine.ChangeState(enemy.IdleState);
            return;
        }

        if (distance <= enemy.AttackRange)
        {
            enemy.StateMachine.ChangeState(enemy.AttackState);
            return;
        }

        enemy.ChasePlayer();
    }
}