using UnityEngine;

// NEW: the state the player is in after dying. Nothing ever leaves this state;
// the DeathScreen restarts the scene.
public class PlayerDeathState : PlayerState
{
    private static readonly int DieHash = Animator.StringToHash("Die");
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int IsBlockingHash = Animator.StringToHash("IsBlocking");

    public PlayerDeathState(PlayerController player) : base(player)
    {
    }

    public override void Enter()
    {
        // Stop everything that could still be going on
        player.CombatController.CancelAttak();
        player.DodgeController.EndDodge();
        player.CancelCombatTransition();
        player.StopMoving();

        // Let go of the lock-on, and switch the lock-on script off so it can't lock again
        player.LockOn.ClearTarget();
        player.LockOn.enabled = false;

        Animator animator = player.Animator;

        animator.SetBool(IsBlockingHash, false);
        animator.ResetTrigger(HitHash); // a queued hit reaction must not override the death

        if (HasParameter(animator, DieHash))
        {
            animator.SetTrigger(DieHash);
        }
        else
        {
            Debug.LogWarning(
                "The player's Animator has no 'Die' trigger parameter, so no death animation will play. " +
                "Add a trigger called Die and a transition from Any State to your death animation."
            );
        }
    }

    // Checks if the Animator has a parameter with the given name hash.
    private static bool HasParameter(Animator animator, int nameHash)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.nameHash == nameHash)
                return true;
        }

        return false;
    }
}