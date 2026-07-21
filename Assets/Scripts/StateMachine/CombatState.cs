using UnityEngine;

public class CombatState : PlayerState
{
    // Constructor for the CombatState class, which takes a PlayerController instance as a parameter and passes it to the base PlayerState class.
    public CombatState(PlayerController player) : base(player)
    {
    }

    public override void Enter()
    {
        // Combat animation already handled
        // by FinishDrawingSword()
    }

    public override void Exit()
    {
        // Sheathing handled by PlayerController
    }

    public override void Update()
    {
        if (!player.InCombat)
            return;

        if (player.GetComponent<DodgeController>().IsDodging)
            return;

        // Check if the player is moving. If so, call the Move method with the player's walk speed.
        if (player.Input.MoveInput.magnitude > 0.1f)
        {
            player.Move(player.walkSpeed);
        }
    }
}
