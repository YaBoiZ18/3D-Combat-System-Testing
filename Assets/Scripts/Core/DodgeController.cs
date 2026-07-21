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

    private static readonly int DodgeHash = Animator.StringToHash("Dodge");

    void Update()
    {
        // Check if the player is in combat before allowing dodge
        if (!player.InCombat) 
            return;

        if (input.DodgePressed)
        {
            Dodge();
        }

        if (isDodging)
        {
            player.Controller.Move(
                dodgeDirection * dodgeSpeed * Time.deltaTime
            );

            dodgeTimer -= Time.deltaTime;

            if (dodgeTimer <= 0)
            {
                EndDodge();
            }
        }
    }

    // This method is called by the animation event at the end of the dodge animation
    public void Dodge()
    {
        // Check if the player is already attacking or dodging before allowing a new dodge
        if (player.CombatController.IsAttacking)
            return;

        if (isDodging)
            return;


        isDodging = true;
        dodgeTimer = dodgeDuration;


        Vector2 inputMove = input.MoveInput;

        // If there's no input, dodge forward; otherwise, dodge in the direction of the input
        if (inputMove.sqrMagnitude < 0.1f)
        {
            dodgeDirection = player.transform.forward;
        }
        else // Calculate dodge direction based on input and player orientation
        {
            Vector3 forward = player.transform.forward;
            Vector3 right = player.transform.right;

            dodgeDirection =
                forward * inputMove.y +
                right * inputMove.x;

            dodgeDirection.Normalize();
        }

        // Reset the dodge timer
        animator.ResetTrigger(DodgeHash);
        animator.SetTrigger(DodgeHash);
    }

    public void EndDodge()
    {
        Debug.Log("End Dodge");
        isDodging = false;
    }
}
