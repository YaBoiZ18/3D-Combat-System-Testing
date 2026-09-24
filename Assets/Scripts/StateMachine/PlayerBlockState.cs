using UnityEngine;

public class PlayerBlockState : PlayerState
{
    private static readonly int BlockHash = Animator.StringToHash("Block");
    private static readonly int IsBlockingHash = Animator.StringToHash("IsBlocking");

    public PlayerBlockState(PlayerController player) : base(player)
    {
    }

    public override void Enter()
    {
        player.StopMoving();

        player.Animator.SetBool(IsBlockingHash, true);
        player.Animator.SetTrigger(BlockHash);
    }

    public override void Update()
    {
        // If the player is not holding the block input, transition back to the combat state
        if (!player.Input.BlockHeld)
        {
            player.StateMachine.ChangeState(player.CombatState);
            return;
        }
    }

    public override void Exit()
    {
        player.Animator.SetBool(IsBlockingHash, false);
        player.Animator.ResetTrigger(BlockHash);
    }
}
