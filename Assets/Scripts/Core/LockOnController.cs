using Unity.VisualScripting;
using UnityEngine;

public class LockOnController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader input; // Reference to input reader component

    [Header("Settings")]
    [SerializeField] private float lockRadius = 15f; // Search radius for lock-on targets

    private LockOnTarget currentTarget; // Currently locked target (null if none)

    // Public helper to check lock state
    public bool IsLockedOn => currentTarget != null;

    // Public access to the current target's transform (null if none)
    public Transform CurrentTarget => currentTarget ? currentTarget.transform : null;

    // Update is called once per frame
    private void Update()
    {
        // When lock button is pressed, toggle lock state
        if (input.LockPressed)
        {
            ToggleLock();
        }
    }

    // Toggle locking: unlock if locked, otherwise attempt to find a target
    void ToggleLock()
    {
        if (currentTarget != null)
        {
            // Clear the current target to unlock
            currentTarget = null;
            Debug.Log("Unlocked");
            return;
        }

        // Attempt to find a nearby target to lock onto
        FindTarget();
    }

    // Find the closest LockOnTarget within lockRadius
    void FindTarget()
    {
        // Get all colliders in the radius around this object
        Collider[] hits = Physics.OverlapSphere(transform.position, lockRadius);

        float closestDistance = Mathf.Infinity;
        LockOnTarget bestTarget = null;

        // Iterate through all colliders and look for LockOnTarget components
        foreach (Collider hit in hits)
        {
            LockOnTarget target = hit.GetComponent<LockOnTarget>();

            if (target == null)
            {
                // Skip colliders that don't represent lock-on targets
                continue;
            }

            // Compute distance to the candidate target
            float distance = Vector3.Distance(transform.position, target.transform.position);

            // Keep the closest valid target
            if (distance < closestDistance)
            {
                closestDistance = distance;
                bestTarget = target;
            }
        }

        // Assign the best candidate (or null if none found)
        currentTarget = bestTarget;

        if (currentTarget != null)
        {
            Debug.Log($"Locked on to {currentTarget.name}");
        }
    }

    // Draw the lock radius in the editor for debugging
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lockRadius);
    }
}
