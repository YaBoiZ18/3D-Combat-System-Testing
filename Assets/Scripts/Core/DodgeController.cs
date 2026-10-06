using UnityEngine;

// Handles player dodge input, timers, movement and i-frames.
public class DodgeController : MonoBehaviour
{
    [SerializeField] private Animator animator; // Animator for dodge animations.
    [SerializeField] private PlayerController player; // Reference to the player.
    [SerializeField] private InputReader input; // Source of player input.

    [SerializeField] private float dodgeSpeed = 8f; // Base dodge speed (units/sec).
    [SerializeField] private float dodgeDuration = 0.5f; // How long a dodge lasts.
    // NEW: speed multiplier over the dodge. X = 0 (start) to 1 (end), Y = multiplier.
    // Default is a fast start that slows down toward the end.
    [SerializeField] private AnimationCurve speedOverTime = AnimationCurve.Linear(0f, 1.5f, 1f, 0.3f);

    // NEW: how long a dodge press is remembered if it can't be used yet (e.g. mid-attack)
    [SerializeField] private float dodgeBufferTime = 0.2f;

    // Tracks remaining dodge time.
    private float dodgeTimer;
    // Countdown for buffered dodge input.
    private float bufferTimer; // NEW

    // True while the player is performing a dodge.
    private bool isDodging;

    // Direction the player will move when dodging (world-space).
    private Vector3 dodgeDirection;
    // CHANGED: moved up here with the other fields
    private Vector2 dodgeInputAxis;

    public bool IsDodging => isDodging;

    // NEW: the first part of the dodge can't be hurt (i-frames). Set to 0 to turn this off.
    [SerializeField] private float invulnerableTime = 0.3f;
    // True while the dodge is currently granting invulnerability.
    public bool IsInvulnerable => isDodging && (dodgeDuration - dodgeTimer) < invulnerableTime;

    // Animation parameter hashes for performance.
    private static readonly int DodgeForwardHash = Animator.StringToHash("DodgeForward");
    private static readonly int DodgeBackwardHash = Animator.StringToHash("DodgeBackward");
    private static readonly int DodgeLeftHash = Animator.StringToHash("DodgeLeft");
    private static readonly int DodgeRightHash = Animator.StringToHash("DodgeRight");

    // Main loop handling input buffering and dodge movement updates.
    void Update()
    {
        if (!player.InCombat)
        {
            bufferTimer = 0f;
            return;
        }

        // CHANGED: remember the press briefly instead of throwing it away
        if (input.DodgePressed)
        {
            bufferTimer = dodgeBufferTime;
        }

        if (bufferTimer > 0f)
        {
            bufferTimer -= Time.deltaTime;

            if (TryDodge())
            {
                bufferTimer = 0f;
            }
        }

        if (isDodging)
        {
            MoveDodge();
        }
    }

    // CHANGED: the movement code moved out of Update into its own method
    // Applies dodge movement each frame while dodging.
    private void MoveDodge()
    {
        // NEW: speed follows the curve instead of being constant
        float progress = 1f - (dodgeTimer / dodgeDuration);
        float step = dodgeSpeed * speedOverTime.Evaluate(progress) * Time.deltaTime;

        if (player.LockOn.IsLockedOn && player.LockOn.CurrentTarget != null)
        {
            Transform target = player.LockOn.CurrentTarget;
            Vector3 toTarget = target.position - player.transform.position;
            toTarget.y = 0f;
            float radius = toTarget.magnitude;
            Vector3 forward = radius > 0.001f ? toTarget / radius : player.transform.forward;

            // Forward/back: change distance directly
            Vector3 radialMove = forward * dodgeInputAxis.y * step;

            // Left/right: rotate around the target so the distance to it stays constant
            Vector3 tangentialMove = Vector3.zero;
            if (radius > 0.001f && Mathf.Abs(dodgeInputAxis.x) > 0.0001f)
            {
                float arcAngle = (-dodgeInputAxis.x * step / radius) * Mathf.Rad2Deg;
                Vector3 rotatedOffset = Quaternion.AngleAxis(arcAngle, Vector3.up) * -toTarget;
                Vector3 newPos = target.position + rotatedOffset;
                tangentialMove = newPos - player.transform.position;

                // FIXED: newPos used the target's height, so a sideways dodge also pushed the
                // player up or down toward that height. Only move on the ground plane.
                tangentialMove.y = 0f;
            }

            // CHANGED: goes through PlayerController so there is still only ONE controller.Move per frame
            player.AddMovement(radialMove + tangentialMove);
        }
        else
        {
            // Free-space dodge moves in the computed dodgeDirection.
            player.AddMovement(dodgeDirection * step);
        }

        dodgeTimer -= Time.deltaTime;
        if (dodgeTimer <= 0f) EndDodge();
    }

    // Kept so any other script that calls Dodge() still compiles
    public void Dodge()
    {
        TryDodge();
    }

    // CHANGED: was Dodge(). Now returns true if the dodge actually started.
    // Validate state and begin dodge if allowed.
    private bool TryDodge()
    {
        if (isDodging) return false;
        if (player.CombatController.IsAttacking) return false;

        // NEW: no dodging while stunned
        var state = player.StateMachine.CurrentState;
        if (state == player.HitState || state == player.GuardBreakState || player.IsDead) return false;

        isDodging = true;
        dodgeTimer = dodgeDuration;

        Vector2 inputMove = input.MoveInput;
        bool hasInput = inputMove.sqrMagnitude >= 0.1f;

        // Standing still = dodge forward
        dodgeInputAxis = hasInput ? inputMove.normalized : Vector2.up;

        // FIXED: dodgeDirection was never assigned (it was always zero), so dodging
        // without a lock-on played the animation but didn't move the player.
        // It's now based on the way the player is facing, to match the dodge animations.
        dodgeDirection =
            player.transform.forward * dodgeInputAxis.y +
            player.transform.right * dodgeInputAxis.x;
        dodgeDirection.y = 0f;
        dodgeDirection.Normalize();

        animator.SetTrigger(GetDodgeTrigger(dodgeInputAxis));
        return true;
    }

    // CHANGED: the repeated if/else animation picking is in one place now
    // Chooses which dodge trigger to use based on input axis.
    private int GetDodgeTrigger(Vector2 axis)
    {
        if (Mathf.Abs(axis.y) > Mathf.Abs(axis.x))
        {
            return axis.y > 0f ? DodgeForwardHash : DodgeBackwardHash;
        }

        return axis.x > 0f ? DodgeRightHash : DodgeLeftHash;
    }

    // Called when the dodge timer runs out. Can also be called by other scripts
    // (e.g. when the player gets hit) to cancel a dodge early.
    public void EndDodge()
    {
        isDodging = false;
    }
}