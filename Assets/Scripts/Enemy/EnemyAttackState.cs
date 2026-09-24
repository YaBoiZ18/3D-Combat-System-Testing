using UnityEngine;

public class EnemyAttackState : EnemyState
{
    private static readonly int AttackHash =
        Animator.StringToHash("Attack");

    private bool attackFinished;
    private int attackStep;

    public EnemyAttackState(EnemyController enemy)
        : base(enemy)
    {
    }

    // The Enter method is called when the enemy enters the attack state.
    public override void Enter()
    {
        attackFinished = false;
        attackStep = 1;

        enemy.StopMoving();
        enemy.FacePlayer();

        PlayCurrentAttack();
    }

    public override void Update()
    {
        // Keep checking whether the player has left attack range.
        if (enemy.DistanceToPlayer > enemy.AttackRange)
        {
            enemy.StateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        // If the current attack has finished,
        // turn toward the player's current position
        // before starting the next attack.
        if (!attackFinished)
            return;

        attackFinished = false;

        enemy.FacePlayerImmediately();

        attackStep++;

        if (attackStep > 3)
        {
            attackStep = 1;
        }

        PlayCurrentAttack();
    }

    private void PlayCurrentAttack()
    {
        enemy.Animator.SetInteger("AttackStep", attackStep);
        enemy.Animator.SetTrigger(AttackHash);
    }

    public void FinishAttack()
    {
        attackFinished = true;
    }
}