using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Profiling;

public class WeaponHitbox : MonoBehaviour
{
    [Header("Hitbox Settings")]
    [SerializeField] private float damage = 10f;

    private bool isActive;

    // Use a HashSet to keep track of hit objects to avoid hitting the same object multiple times in a single activation
    private HashSet<GameObject> hitObjects = new HashSet<GameObject>();

    public void EnableHitbox()
    {
        isActive = true;
        hitObjects.Clear();

        Debug.Log("Hitbox enabled.");
    }

    public void DisableHitbox()
    {
        isActive = false;

        Debug.Log("Hitbox disabled.");
    }

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    // This method is called when another collider stays within the trigger collider attached to this GameObject
    private void OnTriggerStay(Collider other)
    {
        if (!isActive)
            return;

        // Ignore the player and anything else belonging
        // to the same root as the hitbox.
        if (other.transform.root == transform.root)
            return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();

        // If no IDamageable component is found, log a warning and return early.
        if (damageable == null)
        {
            // Log a warning message to the console indicating that the hit object does not have an IDamageable component.
            Debug.LogWarning(
                $"Hit {other.name}, but no IDamageable was found."
            );

            return;
        }

        // Use the GameObject containing the damageable component
        // as the unique target.
        GameObject target = ((MonoBehaviour)damageable).gameObject;

        if (hitObjects.Contains(target))
            return;

        hitObjects.Add(target);

        Debug.Log($"Hit {target.name} for {damage} damage.");

        damageable.TakeDamage(damage);
    }
}
