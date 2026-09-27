using UnityEngine;

public class EnemyStaggerState : EnemyState
{
    //private float staggerDuration = 1f; // Duration of the stagger state
    private float staggerTimer;

    private static readonly int HitHash = Animator.StringToHash("Hit");

    public EnemyStaggerState(EnemyController enemy) : base(enemy)
    {
    }

    public override void Enter()
    {   
        staggerTimer = enemy.StaggerDuration;

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
