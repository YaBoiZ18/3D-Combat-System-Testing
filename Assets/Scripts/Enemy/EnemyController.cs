using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EnemyController : MonoBehaviour, IKnockbackable
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 2.5f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float gravity = -20f; // NEW

    [Header("Stagger Settings")]
    [SerializeField] private float staggerDuration = 1f; // used when this enemy's attack gets blocked
    // NEW: short stun when this enemy takes damage. Lower = enemies stay more aggressive.
    [SerializeField] private float hitStunDuration = 0.4f;

    // CHANGED: the stagger state reads this, so a blocked attack and a normal hit
    // can stun for different lengths of time.
    private float currentStaggerDuration = 1f;
    public float StaggerDuration => currentStaggerDuration;

    // NEW: set by Die(). A dead enemy stops thinking and attacking.
    public bool IsDead { get; private set; }

    // NEW: attack pacing. Set both times to 0 to get the old non-stop attacking back.
    [Header("Attack Settings")]
    [SerializeField] private float timeBetweenAttacks = 0.5f; // pause between hits of a chain
    [SerializeField] private float chainCooldown = 1.5f;      // pause after the last hit of a chain
    [SerializeField] private float attackRangeBuffer = 0.5f;  // extra distance before it stops attacking
    [SerializeField] private WeaponHitbox weaponHitbox;       // assign this enemy's weapon hitbox

    // NEW: the warning the player gets before each swing
    [Header("Attack Telegraph")]
    [SerializeField] private float telegraphDuration = 0.4f;          // seconds of warning before a swing. 0 = no telegraph
    [SerializeField] private bool lockFacingDuringTelegraph = true;   // enemy stops tracking you once it starts charging, so you can sidestep
    [SerializeField] private AudioClip telegraphClip;                 // optional sound played when the warning starts
    [SerializeField] private string telegraphTrigger = "";            // optional Animator trigger for a wind-up animation (leave empty to skip)
    [SerializeField] private TelegraphFlash telegraphFlash;           // found or added automatically if left empty

    // TEMPORARY: prints each swing to the Console so we can see how far an attack gets.
    // Turn it off once enemies attack correctly.
    [Header("Debug")]
    [SerializeField] private bool debugAttackLogs = true;
    public bool DebugAttackLogs => debugAttackLogs;

    // NEW: how quickly knockback slows down (higher = shorter slide)
    [Header("Knockback")]
    [SerializeField] private float knockbackDrag = 8f;

    public float TimeBetweenAttacks => timeBetweenAttacks;
    public float ChainCooldown => chainCooldown;
    public float AttackExitRange => attackRange + attackRangeBuffer;
    public float LoseTargetRange => detectionRange * 1.3f;

    public float TelegraphDuration => telegraphDuration;
    public bool LockFacingDuringTelegraph => lockFacingDuringTelegraph;

    // NEW: the warning starts: play the optional sound and wind-up animation
    public void BeginTelegraph()
    {
        if (telegraphClip != null)
        {
            AudioSource.PlayClipAtPoint(telegraphClip, transform.position);
        }

        if (!string.IsNullOrEmpty(telegraphTrigger))
        {
            animator.SetTrigger(telegraphTrigger);
        }
    }

    // NEW: progress goes from 0 (warning just started) to 1 (swing is about to begin)
    public void SetTelegraphProgress(float progress)
    {
        if (telegraphFlash != null)
        {
            telegraphFlash.SetIntensity(progress);
        }
    }

    // NEW: the warning is over (the swing started, or it got interrupted)
    public void EndTelegraph()
    {
        if (telegraphFlash != null)
        {
            telegraphFlash.Clear();
        }

        if (!string.IsNullOrEmpty(telegraphTrigger))
        {
            animator.ResetTrigger(telegraphTrigger);
        }
    }

    private Vector3 knockbackVelocity;

    // NEW: turns the weapon hitbox off. Used when an attack gets interrupted.
    public void DisableWeaponHitbox()
    {
        if (weaponHitbox != null)
        {
            weaponHitbox.DisableHitbox();
        }
    }

    // NEW: called by WeaponHitbox when this enemy gets hit
    public void ApplyKnockback(Vector3 direction, float force)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        knockbackVelocity = direction.normalized * force;
    }

    private CharacterController controller;

    // NEW: found automatically from the player reference
    private PlayerHealth playerHealth;

    // NEW: the enemy had no gravity before. These two work like the player's:
    // movement is collected during the frame and applied in ONE controller.Move call.
    private float verticalVelocity;
    private Vector3 pendingMove;

    public EnemyStateMachine StateMachine { get; private set; }

    public EnemyIdleState IdleState { get; private set; }

    public EnemyChaseState ChaseState { get; private set; }

    public EnemyAttackState AttackState { get; private set; }
    public EnemyStaggerState StaggerState { get; private set; }

    public float DetectionRange => detectionRange;

    public float AttackRange => attackRange;

    public Animator Animator => animator;

    public float DistanceToPlayer
    {
        get
        {
            if (player == null)
                return Mathf.Infinity;

            Vector3 offset = player.position - transform.position;
            offset.y = 0f;

            return offset.magnitude;
        }
    }

    private static readonly int MoveXHash =
        Animator.StringToHash("MoveX");

    private static readonly int MoveYHash =
        Animator.StringToHash("MoveY");

    private static readonly int MoveSpeedHash =
        Animator.StringToHash("MoveSpeed");

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        // NEW: used to notice when the player has died
        if (player != null)
        {
            playerHealth = player.GetComponentInParent<PlayerHealth>();
        }

        // NEW: use the TelegraphFlash on this enemy, or add one so the warning works with no setup
        if (telegraphFlash == null)
        {
            telegraphFlash = GetComponentInChildren<TelegraphFlash>();
        }

        if (telegraphFlash == null)
        {
            telegraphFlash = gameObject.AddComponent<TelegraphFlash>();
        }

        StateMachine = new EnemyStateMachine();

        IdleState = new EnemyIdleState(this);
        ChaseState = new EnemyChaseState(this);
        AttackState = new EnemyAttackState(this);
        StaggerState = new EnemyStaggerState(this);
    }

    // CHANGED: the state machine starts in Start() instead of Awake(), same as the player,
    // so every other script has finished setting up before the first state's Enter() runs.
    private void Start()
    {
        StateMachine.Initialize(IdleState);
    }

    private void Update()
    {
        // CHANGED: also stops when dead
        if (player == null || IsDead)
            return;

        // NEW: the player is dead, so stop chasing and attacking. The enemy goes idle and stays there.
        if (playerHealth != null && playerHealth.IsDead)
        {
            if (StateMachine.CurrentState != IdleState)
            {
                AttackState.InterruptAttack(); // weapon off, attack trigger cleared
                StateMachine.ChangeState(IdleState);
            }

            return;
        }

        StateMachine.Update();
    }

    // NEW: runs after Update, so everything requested this frame is applied together
    private void LateUpdate()
    {
        ApplyMovement();
    }

    private void ApplyMovement()
    {
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f; // small push down to stay stuck to the ground
        }

        verticalVelocity += gravity * Time.deltaTime;

        // NEW: knockback was stored by ApplyKnockback() but never actually used.
        // Push the enemy with it, then let it fade out (higher knockbackDrag = shorter slide).
        pendingMove += knockbackVelocity * Time.deltaTime;
        knockbackVelocity = Vector3.Lerp(
            knockbackVelocity,
            Vector3.zero,
            1f - Mathf.Exp(-knockbackDrag * Time.deltaTime)
        );

        controller.Move(pendingMove + Vector3.up * verticalVelocity * Time.deltaTime);

        pendingMove = Vector3.zero;
    }

    // NEW: other scripts can push the enemy through this (knockback later, for example)
    public void AddMovement(Vector3 delta)
    {
        pendingMove += delta;
    }

    // CHANGED: removed GetDistanceToPlayer(). It was unused and did the same thing
    // as the DistanceToPlayer property above.

    // Move the enemy towards the player while facing them.
    public void ChasePlayer()
    {
        Vector3 direction = player.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
        {
            StopMoving();
            return;
        }

        direction.Normalize();

        FaceDirection(direction);

        Vector3 movement = direction * moveSpeed;

        // CHANGED: was controller.Move(...). Now it's queued and applied in LateUpdate.
        pendingMove += movement * Time.deltaTime;

        UpdateAnimator(movement);
    }

    // Rotate the enemy to face the player smoothly.
    public void FacePlayer()
    {
        Vector3 direction = player.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            return;

        FaceDirection(direction.normalized);
    }

    // Rotate the enemy to face the given direction smoothly.
    private void FaceDirection(Vector3 direction)
    {
        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        // CHANGED: the old version used rotationSpeed * Time.deltaTime as the blend amount,
        // which turns at a different speed depending on frame rate. This version is
        // frame-rate independent and about the same speed as before at 60 FPS.
        float t = 1f - Mathf.Exp(-rotationSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            t
        );
    }

    public void FacePlayerImmediately()
    {
        Vector3 direction = player.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            return;

        transform.rotation = Quaternion.LookRotation(direction.normalized);
    }

    // FIXED: this used to call UpdateAnimator(Vector3.zero), which DAMPS the values toward 0.
    // Damping only works when it's called every frame, but the states call StopMoving() once,
    // so MoveSpeed/MoveX/MoveY stayed stuck near their walking values. The enemy kept playing a
    // walk animation while standing still, and any Animator transition that needs
    // MoveSpeed to be ~0 could never happen. Now the values go straight to 0.
    public void StopMoving()
    {
        animator.SetFloat(MoveXHash, 0f);
        animator.SetFloat(MoveYHash, 0f);
        animator.SetFloat(MoveSpeedHash, 0f);
    }

    // Update the animator parameters based on the movement vector.
    private void UpdateAnimator(Vector3 movement)
    {
        Vector3 localMovement =
            transform.InverseTransformDirection(movement);

        // CHANGED: damping added (like the player has) so the animation blends
        // smoothly instead of snapping between idle and walking.
        animator.SetFloat(MoveXHash, localMovement.x, 0.1f, Time.deltaTime);
        animator.SetFloat(MoveYHash, localMovement.z, 0.1f, Time.deltaTime);
        animator.SetFloat(MoveSpeedHash, movement.magnitude, 0.1f, Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );
    }

    public void FinishAttack()
    {
        if (debugAttackLogs)
        {
            Debug.Log($"[Enemy:{name}] FinishAttack animation event received", this);
        }

        AttackState.FinishAttack();
    }

    // Long stagger: used when this enemy's attack gets blocked by the player
    public void Stagger()
    {
        StaggerFor(staggerDuration);
    }

    // NEW: short stun when this enemy takes damage (called by EnemyHealth)
    public void HitStun()
    {
        // Already staggered (e.g. from a block): don't shorten or restart it
        if (StateMachine.CurrentState == StaggerState)
            return;

        StaggerFor(hitStunDuration);
    }

    private void StaggerFor(float duration)
    {
        if (IsDead)
            return;

        currentStaggerDuration = duration;
        StateMachine.ChangeState(StaggerState);
    }

    // NEW: called by EnemyHealth when the enemy dies
    public void Die()
    {
        if (IsDead)
            return;

        IsDead = true;

        // Turns off the weapon hitbox and clears any queued attack
        AttackState.InterruptAttack();

        // NEW: a dying enemy shouldn't stay tinted
        EndTelegraph();

        StopMoving();
    }

    public void InterruptAttack()
    {
        if (StateMachine.CurrentState == AttackState)
        {
            StateMachine.ChangeState(ChaseState);
        }
    }
}