using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EnemyController : MonoBehaviour
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

    private CharacterController controller;

    public EnemyStateMachine StateMachine { get; private set; }

    public EnemyIdleState IdleState { get; private set; }

    public EnemyChaseState ChaseState { get; private set; }

    public EnemyAttackState AttackState { get; private set; }

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

        StateMachine = new EnemyStateMachine();

        IdleState = new EnemyIdleState(this);
        ChaseState = new EnemyChaseState(this);
        AttackState = new EnemyAttackState(this);

        StateMachine.Initialize(IdleState);
    }

    private void Update()
    {
        if (player == null)
            return;

        StateMachine.Update();
    }

    private float GetDistanceToPlayer()
    {
        Vector3 offset = player.position - transform.position;

        // Ignore vertical distance.
        offset.y = 0f;

        return offset.magnitude;
    }

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

        controller.Move(movement * Time.deltaTime);

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

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
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

    public void StopMoving()
    {
        UpdateAnimator(Vector3.zero);
    }

    // Update the animator parameters based on the movement vector.
    private void UpdateAnimator(Vector3 movement)
    {
        Vector3 localMovement =
            transform.InverseTransformDirection(movement);

        animator.SetFloat(MoveXHash, localMovement.x);
        animator.SetFloat(MoveYHash, localMovement.z);
        animator.SetFloat(MoveSpeedHash, movement.magnitude);
    }

    // Draw gizmos in the editor to visualize detection and attack ranges.
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
        AttackState.FinishAttack();
    }
}