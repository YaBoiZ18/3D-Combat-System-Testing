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

    // State instances for the state machine
    public IdleState IdleState;
    public MoveState MoveState;
    public SprintState SprintState;

    // State machine and public accessors
    public PlayerStateMachine StateMachine { get; private set; }
    public CharacterController Controller => controller;
    public InputReader Input => input;
    public Animator Animator => animator;

    // Camera reference for direction calculation
    Camera mainCamera;

    // Animator reference for handling animations
    Animator animator;

    private void Awake()
    {
        // Initialize required components
        controller = GetComponent<CharacterController>();
        input = GetComponent<InputReader>();
        StateMachine = new PlayerStateMachine();
        mainCamera = Camera.main;

        animator = GetComponentInChildren<Animator>();

        // Initialize state instances
        IdleState = new IdleState(this);
        MoveState = new MoveState(this);
        SprintState = new SprintState(this);
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
        StateMachine.Update();

        if (input.MoveInput.magnitude < 0.1f)
        {
            UpdateAnimation(0);
        }
    }

    // Move player based on camera direction and input
    public void Move(float speed)
    {
        // Get camera directions and remove vertical component
        Vector3 forward = mainCamera.transform.forward;
        Vector3 right = mainCamera.transform.right;

        forward.y = 0;
        right.y = 0;

        // Normalize directional vectors
        forward.Normalize();
        right.Normalize();

        // Calculate movement direction from input
        Vector2 moveInput = input.MoveInput;
        Vector3 moveDirection = forward * moveInput.y + right * moveInput.x;

        // Rotate player towards movement direction if moving
        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Apply movement to character controller
        controller.Move(moveDirection.normalized * speed * Time.deltaTime);
        UpdateAnimation(speed);
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

    void UpdateAnimation(float speed)
    {
        float normalizedSpeed = speed / sprintSpeed;

        animator.SetFloat(
            "MoveSpeed",
            normalizedSpeed,
            0.25f,
            Time.deltaTime
        );
    }
}
