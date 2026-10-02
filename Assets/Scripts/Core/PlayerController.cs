using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    public float rotationSpeed = 10f;

    float verticalVelocity;

    [SerializeField]
    float gravity = -20f;

    // Component references
    CharacterController controller;
    InputReader input;

    [SerializeField] // Reference to the LockOnController for handling lock-on mechanics
    private LockOnController lockOn;

    // State instances for the state machine
    public IdleState IdleState;
    public MoveState MoveState;
    public SprintState SprintState;
    public CombatState CombatState;
    private PlayerHealth playerHealth;
    public PlayerHitState HitState { get; private set; }
    public PlayerBlockState BlockState { get; private set; }
    public PlayerGuardBreakState GuardBreakState { get; private set; }

    // State machine and public accessors
    public PlayerStateMachine StateMachine { get; private set; }
    public CharacterController Controller => controller;
    public InputReader Input => input;
    public Animator Animator => animator;
    public Vector3 MoveDirection { get; private set; }

    private float previousCameraYaw;

    public bool InCombat { get; private set; }
    public bool IsChangingCombatState { get; private set; }
    public bool IsBlocking => StateMachine.CurrentState == BlockState;

    [SerializeField] // Reference to the camera transform for movement direction
    private Transform cameraRoot;

    [SerializeField]
    private CombatController combatController;

    [SerializeField]
    private DodgeController dodgeController;

    public CombatController CombatController => combatController;

    public DodgeController DodgeController => dodgeController;

    public LockOnController LockOn => lockOn;

    // Animator reference for handling animations
    Animator animator;
    // Animator parameter hashes
    private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");
    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveYHash = Animator.StringToHash("MoveY");
    private static readonly int LockedOnHash = Animator.StringToHash("IsLockedOn");

    private void Awake()
    {
        // Initialize required components
        controller = GetComponent<CharacterController>();
        input = GetComponent<InputReader>();
        StateMachine = new PlayerStateMachine();
        playerHealth = GetComponent<PlayerHealth>();

        animator = GetComponentInChildren<Animator>();

        previousCameraYaw = transform.eulerAngles.y;

        // Initialize state instances
        IdleState = new IdleState(this);
        MoveState = new MoveState(this);
        SprintState = new SprintState(this);
        CombatState = new CombatState(this);
        HitState = new PlayerHitState(this);
        BlockState = new PlayerBlockState(this);
        GuardBreakState = new PlayerGuardBreakState(this);
    }

    // Initialize state machine with idle state
    void Start()
    {
        StateMachine.Initialize(IdleState);
    }

    // Update state machine each frame
    void Update()
    {
        ApplyGravity();

        // Toggle combat mode if the combat button is pressed and the player is not in the hit state
        if (input.CombatPressed && StateMachine.CurrentState != HitState)
        {
            ToggleCombat();
        }

        RotateTowardsTarget();
        StateMachine.Update();

        if (input.MoveInput.magnitude < 0.1f)
        {
            UpdateAnimator(0);
        }
    }

    // Move player based on camera direction and input
    public void Move(float speed)
    {
        Vector3 moveDirection;
        Vector3 preMoveOffset = Vector3.zero;
        bool orbiting = lockOn.IsLockedOn && lockOn.CurrentTarget != null;

        if (orbiting)
        {
            preMoveOffset = transform.position - lockOn.CurrentTarget.position;
            preMoveOffset.y = 0f;
        }

        moveDirection = orbiting ? CombatMovement() : ExplorationMovement();
        MoveDirection = moveDirection;

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            controller.Move(moveDirection.normalized * speed * Time.deltaTime);

            // Only correct outward drift when strafing — skip it when the player
            // is deliberately changing distance to the target.
            if (orbiting && Mathf.Approximately(input.MoveInput.y, 0f))
            {
                Vector3 offset = transform.position - lockOn.CurrentTarget.position;
                offset.y = 0f;
                if (offset.sqrMagnitude > 0.0001f)
                {
                    Vector3 corrected = offset.normalized * preMoveOffset.magnitude;
                    controller.Move(corrected - offset);
                }
            }
        }

        UpdateAnimator(speed);
    }

    // Stop the player's movement and reset animator parameters
    public void StopMoving()
    {
        MoveDirection = Vector3.zero;

        animator.SetFloat(MoveXHash, 0f);
        animator.SetFloat(MoveYHash, 0f);
        animator.SetFloat(MoveSpeedHash, 0f);
    }

    // Calculate movement direction based on camera orientation and input when not locked on
    private Vector3 ExplorationMovement()
    {
        // Get the forward and right vectors of the camera, ignoring the vertical component
        Vector3 forward = cameraRoot.forward;
        Vector3 right = cameraRoot.right;

        // Set the y component to 0 to ensure movement is horizontal
        forward.y = 0;
        right.y = 0;

        // Normalize the forward and right vectors to ensure consistent movement speed
        forward.Normalize();
        right.Normalize();

        Vector2 inputMove = input.MoveInput;

        // Calculate the movement direction based on input and camera orientation
        Vector3 direction =
            forward * inputMove.y +
            right * inputMove.x;

        // Rotate the player towards the movement direction if there is significant input
        if (direction.sqrMagnitude > 0.01f)
        {
            RotateTowardsMovement(direction);
        }

        return direction;
    }

    // Calculate movement direction based on player orientation and input when locked on
    private Vector3 CombatMovement()
    {
        if (!lockOn.IsLockedOn || lockOn.CurrentTarget == null)
            return Vector3.zero;

        // Direction from the player toward the target
        Vector3 toTarget =
            lockOn.CurrentTarget.position - transform.position;

        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.001f)
            return Vector3.zero;

        toTarget.Normalize();

        // Tangent direction around the target
        Vector3 right = Vector3.Cross(Vector3.up, toTarget);

        // Y input = forward/back (toward/away from target), X input = strafe around target
        Vector3 direction =
            toTarget * input.MoveInput.y +
            right * input.MoveInput.x;

        return direction;
    }

    // Apply gravity to the player
    void ApplyGravity()
    {
        // If the player is grounded and falling, reset vertical velocity to a small negative value to keep them grounded
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f; // Small negative value to keep grounded
        }

        // Apply gravity to vertical velocity
        verticalVelocity += gravity * Time.deltaTime;
        // Move the player vertically based on vertical velocity
        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    // Rotate the player towards the movement direction when not locked on
    private void RotateTowardsMovement(Vector3 moveDirection)
    {
        if (moveDirection.sqrMagnitude < 0.01f) { return; } // Avoid rotating if the movement direction is too small

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * 360f * Time.deltaTime);
    }

    // Rotate the player towards the current lock-on target if locked on
    void RotateTowardsTarget()
    {
        if (!lockOn.IsLockedOn || lockOn.CurrentTarget == null)
            return;

        Vector3 direction =
            lockOn.CurrentTarget.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = targetRotation;

        Debug.DrawRay(
            transform.position,
            transform.forward * 2f,
            Color.blue
        );

        Debug.DrawRay(
            transform.position,
            transform.right * 2f,
            Color.red
        );
    }

    // Update the animator parameters based on the current speed and input
    private void UpdateAnimator(float currentSpeed)
    {
        // Calculate the normalized speed for animation blending
        float normalizedSpeed = currentSpeed / sprintSpeed;

        // Get the movement input from the InputReader
        Vector2 moveInput = input.MoveInput;

        // Update the animator parameters for movement and lock-on state with smoothing
        animator.SetFloat(
            MoveXHash,
            moveInput.x,
            0.15f,
            Time.deltaTime
        );

        // Update the animator's MoveY parameter with smoothing
        animator.SetFloat(
            MoveYHash,
            moveInput.y,
            0.15f,
            Time.deltaTime
        );

        // Update the animator's MoveSpeed parameter with smoothing
        animator.SetFloat(
            MoveSpeedHash,
            normalizedSpeed,
            0.2f,
            Time.deltaTime
        );

        // Update the animator's IsLockedOn parameter based on the lock-on state
        animator.SetBool(
            LockedOnHash,
            lockOn.IsLockedOn
        );
    }

    // Combat state management methods to enter, exit, and toggle combat mode
    public void EnterCombat()
    {
        if (InCombat || IsChangingCombatState)
            return;

        IsChangingCombatState = true;

        animator.SetTrigger("DrawSword");
    }

    public void EnterHitState()
    {
        StateMachine.ChangeState(HitState);
    }

    // Exit combat mode and update the animator
    public void ExitCombat()
    {
        if (!InCombat || IsChangingCombatState)
            return;

        IsChangingCombatState = true;

        animator.SetTrigger("SheathSword");
    }

    // Toggle combat mode based on the current state
    public void ToggleCombat()
    {
        if (IsChangingCombatState)
            return;

        if (DodgeController.IsDodging)
            return;

        if (InCombat)
            ExitCombat();
        else
            EnterCombat();
    }

    // Finish drawing the sword and enter combat state
    public void FinishDrawingSword()
    {
        animator.ResetTrigger("DrawSword");

        InCombat = true;
        IsChangingCombatState = false;

        animator.SetBool("InCombat", true);

        StateMachine.ChangeState(CombatState);
    }

    // Finish sheathing the sword and return to idle state
    public void FinishSheathingSword()
    {
        animator.ResetTrigger("SheathSword");

        InCombat = false;
        IsChangingCombatState = false;

        animator.SetBool("InCombat", false);

        StateMachine.ChangeState(IdleState);
    }

    public void FinishHit()
    {
        HitState.FinishHit();
    }

    public void FinishGuardBreak()
    {
        GuardBreakState.FinishGuardBreak();
    }

    public void EnterGuardBreakState()
    {
        if (playerHealth != null)
        {
            playerHealth.StartGuardRecoveryDelay();
        }

        StateMachine.ChangeState(GuardBreakState);
    }
}
