using UnityEngine;

public class EnemyIdleState : EnemyState
{
    public EnemyIdleState(EnemyController enemy) 
        : base(enemy) 
    {
        
    }

    public override void Enter()
    {
        enemy.StopMoving();
    }

    public override void Update()
    {
        float distance = enemy.DistanceToPlayer;

        if (distance <= enemy.DetectionRange)
        {
            enemy.StateMachine.ChangeState(enemy.ChaseState);
        }
    }
}