using UnityEngine;

public class PlayerGuardBreakState : PlayerState
{
    private bool breakFinished;

    private static readonly int GuardBreakHash = Animator.StringToHash("GuardBreak");

    public PlayerGuardBreakState(PlayerController player) : base(player)
    {
    }

    public override void Enter()
    {
        breakFinished = false;

        player.StopMoving();

        player.Animator.SetBool(
            Animator.StringToHash("IsBlocking"),
            false
        );

        player.Animator.SetTrigger(GuardBreakHash);
    }

    public override void Update()
    {
        if (!breakFinished)
            return;

        if(player.InCombat)
        {
            player.StateMachine.ChangeState(player.CombatState);
        }
        else
        {
            player.StateMachine.ChangeState(player.IdleState);
        }
    }

    public void FinishGuardBreak()
    {
        breakFinished = true;
    }
}
