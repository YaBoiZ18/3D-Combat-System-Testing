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

    // State machine and public accessors
    public PlayerStateMachine StateMachine { get; private set; }
    public CharacterController Controller => controller;
    public InputReader Input => input;
    public Animator Animator => animator;
    public Vector3 MoveDirection { get; private set; }

    private float previousCameraYaw;

    public bool InCombat { get; private set; }
    public bool IsChangingCombatState { get; private set; }

    [SerializeField] // Reference to the camera transform for movement direction
    private Transform cameraRoot;

    [SerializeField]
    private CombatController combatController;

    public CombatController CombatController => combatController;

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

        animator = GetComponentInChildren<Animator>();

        previousCameraYaw = transform.eulerAngles.y;

        // Initialize state instances
        IdleState = new IdleState(this);
        MoveState = new MoveState(this);
        SprintState = new SprintState(this);
        CombatState = new CombatState(this);
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

        if (input.CombatPressed)
        {
            ToggleCombat();
        }

        StateMachine.Update();

        RotateTowardsTarget();

        if (input.MoveInput.magnitude < 0.1f)
        {
            UpdateAnimator(0);
        }
    }

    // Move player based on camera direction and input
    public void Move(float speed)
    {
        // Get camera directions and remove vertical component
        Vector3 forward = cameraRoot.forward;
        Vector3 right = cameraRoot.right;

        forward.y = 0;
        right.y = 0;

        // Normalize directional vectors
        forward.Normalize();
        right.Normalize();

        // Calculate movement direction from input
        Vector2 moveInput = input.MoveInput;
        MoveDirection = forward * moveInput.y + right * moveInput.x;

        if (!lockOn.IsLockedOn)
        {
            RotateTowardsMovement(MoveDirection);
        }

        // Apply movement to character controller
        controller.Move(MoveDirection.normalized * speed * Time.deltaTime);
        UpdateAnimator(speed);
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
        if (!lockOn.IsLockedOn) { return; }

        Vector3 direction = lockOn.CurrentTarget.position - transform.position;

        direction.y = 0f; // Ignore vertical difference for rotation

        if (direction.sqrMagnitude < 0.01f) { return; } // Avoid rotating if the target is too close

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * 360f * Time.deltaTime);
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

    //// Update the turn animation based on the change in camera yaw
    //private void UpdateTurnAnimation()
    //{
    //    // Calculate the change in camera yaw since the last frame
    //    float turnAmount = Mathf.DeltaAngle(previousCameraYaw, transform.eulerAngles.y);

    //    // Update the animator's "Turn" parameter with the calculated turn amount
    //    animator.SetFloat(
    //        "Turn",
    //        turnAmount
    //    );

    //    // Update the previous camera yaw for the next frame
    //    previousCameraYaw = transform.eulerAngles.y;
    //}


}
