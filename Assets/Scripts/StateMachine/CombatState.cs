using UnityEngine;

public class CombatState : PlayerState
{
    // Constructor for the CombatState class, which takes a PlayerController instance as a parameter and passes it to the base PlayerState class.
    public CombatState(PlayerController player) : base(player)
    {
    }

    public override void Enter()
    {
        player.Animator.SetBool("InCombat", true);
    }

    public override void Exit()
    {
        player.Animator.SetBool("InCombat", false);
    }

    public override void Update()
    {
        // Check if the player is still holding the combat input. If not, transition back to IdleState.
        if (!player.Input.CombatHeld)
        {
            player.StateMachine.ChangeState(player.IdleState);

            return;
        }

        // Check if the player is moving. If so, call the Move method with the player's walk speed.
        if (player.Input.MoveInput.magnitude > 0.1f)
        {
            player.Move(player.walkSpeed);
        }
    }
}
