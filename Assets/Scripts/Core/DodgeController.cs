using UnityEngine;

public class DodgeController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerController player;
    [SerializeField] private InputReader input;

    [SerializeField] private float dodgeSpeed = 8f;
    [SerializeField] private float dodgeDuration = 0.5f;

    private float dodgeTimer;

    private bool isDodging;

    private Vector3 dodgeDirection;

    public bool IsDodging => isDodging;

    private static readonly int DodgeForwardHash = Animator.StringToHash("DodgeForward");

    private static readonly int DodgeBackwardHash = Animator.StringToHash("DodgeBackward");

    private static readonly int DodgeLeftHash = Animator.StringToHash("DodgeLeft");

    private static readonly int DodgeRightHash = Animator.StringToHash("DodgeRight");

    void Update()
    {
        if (!player.InCombat) return;
        if (input.DodgePressed) Dodge();

        if (isDodging)
        {
            float step = dodgeSpeed * Time.deltaTime;

            if (player.LockOn.IsLockedOn && player.LockOn.CurrentTarget != null)
            {
                Transform target = player.LockOn.CurrentTarget;
                Vector3 toTarget = target.position - player.transform.position;
                toTarget.y = 0f;
                float radius = toTarget.magnitude;
                Vector3 forward = radius > 0.001f ? toTarget / radius : player.transform.forward;

                // Forward/back: change distance directly — this part is fine as-is.
                Vector3 radialMove = forward * dodgeInputAxis.y * step;

                // Left/right: rotate around the target instead of sliding along
                // the tangent, so the radius stays constant.
                Vector3 tangentialMove = Vector3.zero;
                if (radius > 0.001f && Mathf.Abs(dodgeInputAxis.x) > 0.0001f)
                {
                    float arcAngle = (-dodgeInputAxis.x * step / radius) * Mathf.Rad2Deg;
                    Vector3 rotatedOffset = Quaternion.AngleAxis(arcAngle, Vector3.up) * -toTarget;
                    Vector3 newPos = target.position + rotatedOffset;
                    tangentialMove = newPos - player.transform.position;
                }

                player.Controller.Move(radialMove + tangentialMove);
            }
            else
            {
                player.Controller.Move(dodgeDirection * dodgeSpeed * Time.deltaTime);
            }

            dodgeTimer -= Time.deltaTime;
            if (dodgeTimer <= 0) EndDodge();
        }
    }

    // This method is called by the animation event at the end of the dodge animation

    private Vector2 dodgeInputAxis;

    public void Dodge()
    {
        if (player.CombatController.IsAttacking) return;
        if (isDodging) return;

        isDodging = true;
        dodgeTimer = dodgeDuration;

        Vector2 inputMove = input.MoveInput;
        dodgeInputAxis = inputMove.sqrMagnitude < 0.1f
            ? new Vector2(0f, 1f)
            : inputMove.normalized;

        // Standing still = dodge forward
        if (inputMove.sqrMagnitude < 0.1f)
        {
            animator.SetTrigger(DodgeForwardHash);
        }
        else
        {
            // Determine the dodge direction based on input and trigger the appropriate animation
            float x = inputMove.x;
            float y = inputMove.y;

            // Determine the dodge direction based on input and trigger the appropriate animation
            if (Mathf.Abs(y) > Mathf.Abs(x))
            {
                if (y > 0)
                    animator.SetTrigger(DodgeForwardHash);
                else
                    animator.SetTrigger(DodgeBackwardHash);
            }
            else // Horizontal dodge
            {
                if (x > 0)
                    animator.SetTrigger(DodgeRightHash);
                else
                    animator.SetTrigger(DodgeLeftHash);
            }
        }
    }

    // Calculate the dodge direction based on player input and orientation
    private void CalculateDodgeDirection()
    {
        Vector2 inputMove = input.MoveInput;


        if (inputMove.sqrMagnitude < 0.1f)
        {
            dodgeDirection = player.transform.forward;
            return;
        }


        Vector3 forward;
        Vector3 right;


        if (player.LockOn.IsLockedOn)
        {
            Vector3 targetDirection =
                player.LockOn.CurrentTarget.position -
                player.transform.position;

            targetDirection.y = 0f;
            targetDirection.Normalize();


            forward = targetDirection;
            right = Vector3.Cross(Vector3.up, forward);
        }
        else
        {
            forward = player.transform.forward;
            right = player.transform.right;
        }


        dodgeDirection =
            forward * inputMove.y +
            right * inputMove.x;


        dodgeDirection.Normalize();
    }

    public void EndDodge()
    {
        Debug.Log("End Dodge");
        isDodging = false;
    }
}
