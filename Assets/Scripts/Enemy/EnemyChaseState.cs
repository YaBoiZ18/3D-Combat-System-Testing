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

        if (distance > enemy.DetectionRange)
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
