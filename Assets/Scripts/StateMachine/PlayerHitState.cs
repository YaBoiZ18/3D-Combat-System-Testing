using UnityEngine;

public class PlayerHitState : PlayerState
{
    private bool hitFinished;

    public PlayerHitState(PlayerController player) : base(player)
    {
    }

    public override void Enter()
    {
        hitFinished = false;

        // Cancel any ongoing attack when the player is hit
        player.CombatController.CancelAttak();

        // Stop the player's movement when hit
        player.StopMoving();

        // Trigger the hit animation
        player.Animator.SetTrigger(
            Animator.StringToHash("Hit")
        );
    }

    public override void Update()
    {
        Debug.Log(
            $"Player HitState Update | Finished: {hitFinished}"
        );

        if (!hitFinished)
            return;

        Debug.Log("Player HitState finished.");

        if (player.InCombat)
        {
            Debug.Log("Returning to CombatState.");
            player.StateMachine.ChangeState(player.CombatState);
        }
        else
        {
            Debug.Log("Returning to IdleState.");
            player.StateMachine.ChangeState(player.IdleState);
        }
    }

    public void FinishHit()
    {
        hitFinished = true;
    }
}