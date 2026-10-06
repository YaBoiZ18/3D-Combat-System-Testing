using UnityEngine;

public class LockOnController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader input;
    [SerializeField] private Transform cameraRoot; // used to pick the target you're looking at

    [Header("Settings")]
    [SerializeField] private float lockRadius = 15f;      // How close a target must be to lock on
    [SerializeField] private float breakDistance = 20f;   // Lock drops if the target gets farther than this
    [SerializeField] private float maxLockAngle = 90f;    // Ignore targets this far off from where the camera faces
    [SerializeField] private float eyeHeight = 1.5f;      // Height the line-of-sight check starts from
    [SerializeField] private LayerMask obstacleMask;      // Layers that block line of sight (walls, ground)

    // NEW: switching between targets while locked on
    [Header("Switching Targets")]
    [SerializeField] private float switchCooldown = 0.25f; // Minimum time between switches (stops one scroll notch skipping several targets)

    private LockOnTarget currentTarget;
    private float switchTimer; // NEW

    public bool IsLockedOn => currentTarget != null;
    public Transform CurrentTarget => currentTarget ? currentTarget.LockPoint : null;

    private void Update()
    {
        // Automatically drop the lock if the target died, was disabled, or got too far away
        if (currentTarget != null && !IsStillValid(currentTarget))
        {
            ClearTarget();
        }

        if (switchTimer > 0f)
        {
            // Unscaled, so hit stop doesn't stretch the cooldown
            switchTimer -= Time.unscaledDeltaTime;
        }

        if (input.LockPressed)
        {
            ToggleLock();
        }

        // NEW
        if (currentTarget != null)
        {
            HandleSwitchInput();
        }
    }

    private void ToggleLock()
    {
        if (currentTarget != null)
        {
            ClearTarget();
            return;
        }

        currentTarget = FindBestTarget();
    }

    // Public so other scripts (e.g. player death) can force-unlock
    public void ClearTarget()
    {
        currentTarget = null;
    }

    private bool IsStillValid(LockOnTarget target)
    {
        if (!target.isActiveAndEnabled)
            return false;

        float distance = Vector3.Distance(transform.position, target.transform.position);
        return distance <= breakDistance;
    }

    // NEW: reads the switch input and moves the lock to the next target on that side
    private void HandleSwitchInput()
    {
        if (switchTimer > 0f)
            return;

        int direction = 0; // -1 = target to the left, +1 = target to the right

        float scroll = input.ScrollInput;
        if (scroll > 0.1f) direction = -1;        // wheel up = left
        else if (scroll < -0.1f) direction = 1;   // wheel down = right (swap these two lines if you prefer the opposite)

        if (direction == 0)
            return;

        LockOnTarget next = FindTargetInDirection(direction);

        if (next != null)
        {
            currentTarget = next;
            switchTimer = switchCooldown;
        }
    }

    // NEW: the best target on the given side of the current one (-1 = left, +1 = right),
    // as seen from the camera. Returns null if there is nothing on that side.
    private LockOnTarget FindTargetInDirection(int direction)
    {
        float currentAngle = GetSignedAngle(currentTarget);

        LockOnTarget best = null;
        float bestScore = Mathf.Infinity;

        foreach (LockOnTarget target in LockOnTarget.ActiveTargets)
        {
            if (target == currentTarget)
                continue;

            if (!IsCandidate(target, out float angle, out float distance))
                continue;

            // How far (in degrees) this target is from the current one, in the wanted direction
            float step = Mathf.DeltaAngle(currentAngle, GetSignedAngle(target)) * direction;

            // Not on that side (the small margin ignores targets standing almost in line)
            if (step <= 1f)
                continue;

            // The nearest one in angle wins; distance only breaks near-ties
            float score = step + distance * 0.1f;

            if (score < bestScore)
            {
                bestScore = score;
                best = target;
            }
        }

        return best;
    }

    // NEW: angle of the target relative to where the camera faces. Negative = left, positive = right.
    private float GetSignedAngle(LockOnTarget target)
    {
        Vector3 toTarget = target.LockPoint.position - transform.position;
        toTarget.y = 0f;

        return Vector3.SignedAngle(cameraRoot.forward, toTarget, Vector3.up);
    }

    // CHANGED: the checks that decide whether a target can be locked onto now live in one place,
    // so locking on and switching targets use exactly the same rules.
    private bool IsCandidate(LockOnTarget target, out float angle, out float distance)
    {
        Vector3 toTarget = target.LockPoint.position - transform.position;
        toTarget.y = 0f;

        distance = toTarget.magnitude;
        angle = 0f;

        if (distance > lockRadius || distance < 0.01f)
            return false;

        angle = Vector3.Angle(cameraRoot.forward, toTarget);
        if (angle > maxLockAngle)
            return false;

        return HasLineOfSight(target);
    }

    // Picks the target closest to where the CAMERA is facing (not just the closest one),
    // and skips targets hidden behind walls.
    private LockOnTarget FindBestTarget()
    {
        LockOnTarget best = null;
        float bestScore = Mathf.Infinity;

        foreach (LockOnTarget target in LockOnTarget.ActiveTargets)
        {
            if (!IsCandidate(target, out float angle, out float distance))
                continue;

            // Lower score = better. Angle matters most, distance breaks ties.
            // Tweak these weights if the picking doesn't feel right.
            float score = angle + distance;

            if (score < bestScore)
            {
                bestScore = score;
                best = target;
            }
        }

        return best;
    }

    // Returns false if a wall/obstacle is between the player and the target
    private bool HasLineOfSight(LockOnTarget target)
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 end = target.LockPoint.position;

        if (Physics.Linecast(origin, end, out RaycastHit hit, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            // Something was hit. It only counts as blocking if it isn't the target itself.
            return hit.transform.IsChildOf(target.transform);
        }

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lockRadius);

        // Shows where the lock will break
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, breakDistance);
    }
}