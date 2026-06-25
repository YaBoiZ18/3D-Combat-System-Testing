using UnityEngine;

public class MoveState : PlayerState
{
    public MoveState(PlayerController player) : base(player)
    {

    }

    public override void Update()
    {
        // Get the player's movement input
        Vector2 move = player.Input.MoveInput;

        // If the player is not moving, change to IdleState
        if (move.magnitude < 0.1f)
        {
            player.StateMachine.ChangeState(player.IdleState);
            return;
        }

        // If the player is sprinting, change to SprintState
        if (player.Input.SprintHeld)
        {
            player.StateMachine.ChangeState(player.SprintState);
            return;
        }

        // Move the player based on the input
        player.Move(player.walkSpeed);

        player.Animator.SetFloat("Speed", 1);

    }

    // Override the Enter method to set the player's animation speed to 1 when entering the MoveState
    public override void Enter()
    {
        player.Animator.SetBool("IsSprinting", false);
    }
}
