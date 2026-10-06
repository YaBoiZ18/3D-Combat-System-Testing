using UnityEngine;

public class PlayerGuardBreakState : PlayerState
{
    private static readonly int GuardBreakHash = Animator.StringToHash("GuardBreak");
    // CHANGED: hash stored once (it used to be created inside Enter every time)
    private static readonly int IsBlockingHash = Animator.StringToHash("IsBlocking");

    // NEW: safety net. Normally the guard break animation calls FinishGuardBreak() through an
    // animation event. If that event doesn't fire, the player would be stuck in this state.
    // Set it a little longer than your guard break animation.
    private const float MaxGuardBreakDuration = 2.5f;

    private bool breakFinished;
    private float breakTimer;

    public PlayerGuardBreakState(PlayerController player) : base(player)
    {
    }

    public override void Enter()
    {
        breakFinished = false;
        breakTimer = 0f;

        player.StopMoving();

        player.Animator.SetBool(IsBlockingHash, false);

        player.Animator.SetTrigger(GuardBreakHash);
    }

    public override void Update()
    {
        // NEW: failsafe timer
        breakTimer += Time.deltaTime;
        if (breakTimer >= MaxGuardBreakDuration)
        {
            breakFinished = true;
        }

        if (!breakFinished)
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

    // Called by the guard break animation's event (through PlayerController.FinishGuardBreak)
    public void FinishGuardBreak()
    {
        breakFinished = true;
    }
}