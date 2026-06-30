using UnityEngine;

public class IdleState : PlayerState
{
    public IdleState(PlayerController player) : base(player)
    {
        
    }

    // Override the Update method to check for movement input and transition to MoveState if necessary
    public override void Update()
    {
        // Get the player's movement input
        Vector2 move = player.Input.MoveInput;

        // If the player is moving, change to MoveState
        if (move.magnitude > 0.1f)
        {
            player.StateMachine.ChangeState(player.MoveState);
        }
    }

    // Override the Enter method to set the player's animation speed to 0 when entering the IdleState
    public override void Enter()
    {
        
    }
}
