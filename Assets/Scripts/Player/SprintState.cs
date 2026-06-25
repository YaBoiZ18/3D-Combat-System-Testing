using UnityEngine;

public class SprintState : PlayerState
{
    public SprintState(PlayerController player) : base(player)
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

        // If the player is not sprinting, change to MoveState
        if (!player.Input.SprintHeld)
        {
            player.StateMachine.ChangeState(player.MoveState);
            return;
        }

        // Move the player based on the input
        player.Move(player.sprintSpeed);

        // Set the player's animation speed to 1 when sprinting
        player.Animator.SetFloat("Speed", 1);
    }

    // Override the Enter method to set the player's animation speed to 1 when entering the SprintState
    public override void Enter()
    {
        player.Animator.SetBool("IsSprinting", true);
    }
}