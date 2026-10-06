using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[DefaultExecutionOrder(-50)] // NEW: makes sure the player moves before the camera's LateUpdate runs
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    public float rotationSpeed = 10f; // CHANGED: now a smoothing speed. Higher = snappier turning (try 10-20)

    // NEW: how fast the player turns to face the lock-on target
    [SerializeField] private float lockOnRotationSpeed = 20f;

    // NEW: how much of your walk speed you keep while attacking (0 = planted, 1 = full speed)
    [SerializeField, Range(0f, 1f)] private float attackMoveMultiplier = 0.3f;
    public float AttackMoveMultiplier => attackMoveMultiplier;

    float verticalVelocity;

    [SerializeField]
    float gravity = -20f;

    // NEW: movement requested this frame. Everything gets applied in ONE controller.Move call.
    private Vector3 pendingMove;

    // Component references
    CharacterController controller;
    InputReader input;

    [SerializeField]
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

    // CHANGED: removed unused previousCameraYaw

    public bool InCombat { get; private set; }
    public bool IsChangingCombatState { get; private set; }
    public bool IsBlocking => StateMachine.CurrentState == BlockState;

    // NEW: death state
    public PlayerDeathState DeathState { get; private set; }
    public bool IsDead => StateMachine.CurrentState == DeathState;

    [SerializeField]
    private Transform cameraRoot;

    [SerializeField]
    private CombatController combatController;

    [SerializeField]
    private DodgeController dodgeController;

    public CombatController CombatController => combatController;

    public DodgeController DodgeController => dodgeController;

    public LockOnController LockOn => lockOn;

    Animator animator;

    // Animator parameter hashes
    private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");
    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveYHash = Animator.StringToHash("MoveY");
    private static readonly int LockedOnHash = Animator.StringToHash("IsLockedOn");
    // NEW: hashes for the strings that were used directly before
    private static readonly int DrawSwordHash = Animator.StringToHash("DrawSword");
    private static readonly int SheathSwordHash = Animator.StringToHash("SheathSword");
    private static readonly int InCombatHash = Animator.StringToHash("InCombat");

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<InputReader>();
        StateMachine = new PlayerStateMachine();
        playerHealth = GetComponent<PlayerHealth>();

        animator = GetComponentInChildren<Animator>();

        IdleState = new IdleState(this);
        MoveState = new MoveState(this);
        SprintState = new SprintState(this);
        CombatState = new CombatState(this);
        HitState = new PlayerHitState(this);
        BlockState = new PlayerBlockState(this);
        GuardBreakState = new PlayerGuardBreakState(this);
        DeathState = new PlayerDeathState(this); // NEW
    }

    void Start()
    {
        StateMachine.Initialize(IdleState);
    }

    void Update()
    {
        if (input.CombatPressed && StateMachine.CurrentState != HitState && !IsDead)
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

    // CHANGED: runs after every script's Update, so movement requested by other scripts
    // (like DodgeController) lands in the same single controller.Move call.
    private void LateUpdate()
    {
        ApplyMovement();
    }

    // NEW: other scripts add movement here instead of calling controller.Move themselves
    public void AddMovement(Vector3 delta)
    {
        pendingMove += delta;
    }

    // CHANGED: replaces ApplyGravity(). Before, the controller was moved twice per frame
    // (once for gravity, once for walking), which makes isGrounded unreliable.
    // Now there is a single controller.Move call.
    private void ApplyMovement()
    {
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f; // small push down to stay stuck to the ground
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 totalMove = pendingMove + Vector3.up * verticalVelocity * Time.deltaTime;
        controller.Move(totalMove);

        pendingMove = Vector3.zero;
    }

    // CHANGED: Move() now only works out the step and stores it in pendingMove.
    // The actual movement happens in ApplyMovement().
    public void Move(float speed)
    {
        bool orbiting = lockOn.IsLockedOn && lockOn.CurrentTarget != null;

        Vector3 moveDirection = orbiting ? CombatMovement() : ExplorationMovement();
        MoveDirection = moveDirection;

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            // CHANGED: ClampMagnitude instead of normalized, so a half-tilted
            // gamepad stick walks slower (keyboard still gives full speed).
            Vector3 step = Vector3.ClampMagnitude(moveDirection, 1f) * speed * Time.deltaTime;

            // Keep the player at the same distance from the target while strafing,
            // but skip it when they're deliberately moving toward/away from it.
            if (orbiting && Mathf.Approximately(input.MoveInput.y, 0f))
            {
                Vector3 before = transform.position - lockOn.CurrentTarget.position;
                before.y = 0f;

                Vector3 after = before + step;
                after.y = 0f;

                if (after.sqrMagnitude > 0.0001f)
                {
                    // Pull the "after" position back onto the circle around the target
                    Vector3 corrected = after.normalized * before.magnitude;
                    step += corrected - after;
                }
            }

            pendingMove += step;
        }

        UpdateAnimator(speed);
    }

    public void StopMoving()
    {
        MoveDirection = Vector3.zero;

        animator.SetFloat(MoveXHash, 0f);
        animator.SetFloat(MoveYHash, 0f);
        animator.SetFloat(MoveSpeedHash, 0f);
    }

    private Vector3 ExplorationMovement()
    {
        Vector3 forward = cameraRoot.forward;
        Vector3 right = cameraRoot.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector2 inputMove = input.MoveInput;

        Vector3 direction =
            forward * inputMove.y +
            right * inputMove.x;

        if (direction.sqrMagnitude > 0.01f)
        {
            RotateTowardsMovement(direction);
        }

        return direction;
    }

    private Vector3 CombatMovement()
    {
        if (!lockOn.IsLockedOn || lockOn.CurrentTarget == null)
            return Vector3.zero;

        Vector3 toTarget = lockOn.CurrentTarget.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.001f)
            return Vector3.zero;

        toTarget.Normalize();

        // Tangent direction around the target
        Vector3 right = Vector3.Cross(Vector3.up, toTarget);

        // Y input = toward/away from target, X input = strafe around target
        Vector3 direction =
            toTarget * input.MoveInput.y +
            right * input.MoveInput.x;

        return direction;
    }

    // NEW: frame-rate independent smooth turning. Higher speed = faster turn.
    private void SmoothRotateTo(Quaternion targetRotation, float speed)
    {
        float t = 1f - Mathf.Exp(-speed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, t);
    }

    // CHANGED: the old version used rotationSpeed * 360, which turned the player
    // ~3600 degrees per second (basically an instant snap). Now it eases into the turn.
    private void RotateTowardsMovement(Vector3 moveDirection)
    {
        if (moveDirection.sqrMagnitude < 0.01f) { return; }

        SmoothRotateTo(Quaternion.LookRotation(moveDirection), rotationSpeed);
    }

    // CHANGED: the old version set the rotation directly, so the player teleported to face
    // the target the moment you locked on. Now it turns quickly but smoothly.
    void RotateTowardsTarget()
    {
        if (!lockOn.IsLockedOn || lockOn.CurrentTarget == null)
            return;

        Vector3 direction = lockOn.CurrentTarget.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            return;

        SmoothRotateTo(Quaternion.LookRotation(direction), lockOnRotationSpeed);

        // CHANGED: removed the Debug.DrawRay calls
    }

    private void UpdateAnimator(float currentSpeed)
    {
        float normalizedSpeed = currentSpeed / sprintSpeed;

        Vector2 moveInput = input.MoveInput;

        animator.SetFloat(MoveXHash, moveInput.x, 0.15f, Time.deltaTime);
        animator.SetFloat(MoveYHash, moveInput.y, 0.15f, Time.deltaTime);
        animator.SetFloat(MoveSpeedHash, normalizedSpeed, 0.2f, Time.deltaTime);

        animator.SetBool(LockedOnHash, lockOn.IsLockedOn);
    }

    // Combat state management methods to enter, exit, and toggle combat mode
    public void EnterCombat()
    {
        if (InCombat || IsChangingCombatState)
            return;

        IsChangingCombatState = true;

        animator.SetTrigger(DrawSwordHash); // CHANGED: uses the hash
    }

    public void EnterHitState()
    {
        // NEW: a dead player can't be staggered back to life
        if (IsDead)
            return;

        // CHANGED: cancelling the dodge/attack now happens in PlayerHitState.Enter,
        // so it all lives in one place.
        StateMachine.ChangeState(HitState);
    }

    // NEW: called by PlayerHealth when health reaches 0
    public void EnterDeathState()
    {
        if (IsDead)
            return;

        StateMachine.ChangeState(DeathState);
    }

    // NEW: called when the player gets hit. If a draw/sheath animation was interrupted,
    // its end event never fires and IsChangingCombatState would stay true forever.
    public void CancelCombatTransition()
    {
        if (!IsChangingCombatState)
            return;

        IsChangingCombatState = false;
        animator.ResetTrigger(DrawSwordHash);
        animator.ResetTrigger(SheathSwordHash);
    }

    public void ExitCombat()
    {
        if (!InCombat || IsChangingCombatState)
            return;

        IsChangingCombatState = true;

        animator.SetTrigger(SheathSwordHash); // CHANGED: uses the hash
    }

    public void ToggleCombat()
    {
        if (IsChangingCombatState)
            return;

        if (DodgeController.IsDodging)
            return;

        // NEW: no sheathing/drawing mid-attack. If the sheath animation interrupted an attack,
        // the attack's end event could be skipped and the player would be stuck unable to attack.
        if (CombatController.IsAttacking)
            return;

        if (InCombat)
            ExitCombat();
        else
            EnterCombat();
    }

    public void FinishDrawingSword()
    {
        // NEW: a late animation event must not move a dead player out of the death state
        if (IsDead)
            return;

        animator.ResetTrigger(DrawSwordHash);

        InCombat = true;
        IsChangingCombatState = false;

        animator.SetBool(InCombatHash, true);

        StateMachine.ChangeState(CombatState);
    }

    public void FinishSheathingSword()
    {
        // NEW: same protection as FinishDrawingSword
        if (IsDead)
            return;

        animator.ResetTrigger(SheathSwordHash);

        InCombat = false;
        IsChangingCombatState = false;

        animator.SetBool(InCombatHash, false);

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
        // NEW: a dead player can't be guard-broken back to life
        if (IsDead)
            return;

        if (playerHealth != null)
        {
            playerHealth.StartGuardRecoveryDelay();
        }

        StateMachine.ChangeState(GuardBreakState);
    }
}