using UnityEngine;

public class PlayerHitState : PlayerState
{
    // NEW: hash stored once instead of calling StringToHash on every hit
    private static readonly int HitHash = Animator.StringToHash("Hit");

    // NEW: safety net. Normally the hit animation calls FinishHit() through an animation event.
    // If that event ever doesn't fire (animation interrupted, event missing), the player
    // would be stuck in this state forever. After this many seconds it ends on its own.
    // Set it a little longer than your hit animation.
    private const float MaxHitDuration = 1.2f;

    private bool hitFinished;
    private float hitTimer;

    public PlayerHitState(PlayerController player) : base(player)
    {
    }

    public override void Enter()
    {
        hitFinished = false;
        hitTimer = 0f;

        // Cancel any ongoing attack when the player is hit
        player.CombatController.CancelAttak();

        // NEW: also cancel a dodge in progress (this used to be in PlayerController.EnterHitState)
        player.DodgeController.EndDodge();

        // NEW: if the hit interrupted drawing/sheathing the sword, its end event never fires
        // and the player could no longer toggle combat. This clears that.
        player.CancelCombatTransition();

        // Stop the player's movement when hit
        player.StopMoving();

        player.Animator.SetTrigger(HitHash);
    }

    public override void Update()
    {
        // CHANGED: removed the Debug.Log calls that printed every frame

        // NEW: failsafe timer
        hitTimer += Time.deltaTime;
        if (hitTimer >= MaxHitDuration)
        {
            hitFinished = true;
        }

        if (!hitFinished)
            return;

        if (player.InCombat)
        {
            player.StateMachine.ChangeState(player.CombatState);
        }
        else
        {
            player.StateMachine.ChangeState(player.IdleState);
        }
    }

    // Called by the hit animation's event (through PlayerController.FinishHit)
    public void FinishHit()
    {
        hitFinished = true;
    }
}