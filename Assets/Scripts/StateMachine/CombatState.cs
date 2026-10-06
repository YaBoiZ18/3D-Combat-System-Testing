using UnityEngine;

public class CombatState : PlayerState
{
    public CombatState(PlayerController player) : base(player)
    {
    }

    public override void Enter()
    {
        // Combat animation already handled by FinishDrawingSword()
        // CHANGED: removed the Debug.Log
    }

    public override void Exit()
    {
        // Sheathing handled by PlayerController
    }

    public override void Update()
    {
        // CHANGED: removed the Debug.Log that printed every frame

        if (!player.InCombat)
            return;

        if (player.DodgeController.IsDodging)
            return;

        bool isAttacking = player.CombatController.IsAttacking;

        // CHANGED: blocking waits until the attack is over. Before, holding block mid-swing
        // switched states while the attack (and its hitbox) was still running.
        // Since block is "held", you'll start blocking as soon as the attack ends.
        if (player.Input.BlockHeld && !isAttacking)
        {
            player.StateMachine.ChangeState(player.BlockState);
            return;
        }

        if (player.Input.MoveInput.magnitude > 0.1f)
        {
            float speed = player.walkSpeed;

            // NEW: slow down while attacking so the player doesn't slide around during a swing
            if (isAttacking)
            {
                speed *= player.AttackMoveMultiplier;
            }

            player.Move(speed);
        }
    }
}