using UnityEngine;
using System.Collections.Generic;
// FIXED: removed "using UnityEditor.Profiling;". UnityEditor only exists inside the editor,
// so this line would make your game fail to BUILD (even though it runs fine in the editor).

public class WeaponHitbox : MonoBehaviour
{
    [Header("Hitbox Settings")]
    [SerializeField] private float damage = 10f;

    // NEW: length of the freeze-frame on a hit, in seconds. Set to 0 to turn it off
    // (e.g. on enemy weapons if you don't want the game to freeze when you get hit).
    [Header("Feel")]
    [SerializeField] private float hitStopDuration = 0.06f;

    // NEW: how hard hits push enemies back (in units/second, fades out quickly). 0 = no knockback.
    [SerializeField] private float knockbackForce = 4f;

    // TEMPORARY: prints what this hitbox is doing to the Console, to find out why a weapon
    // isn't hurting anything. Turn it off (or delete the Report/Debug lines) once it works.
    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private bool isActive;

    // Keeps track of what was already hit so one swing can't hit the same target twice
    private HashSet<GameObject> hitObjects = new HashSet<GameObject>();

    // Debug only: colliders already reported during the current swing (so the Console isn't flooded)
    private HashSet<Collider> reported = new HashSet<Collider>();

    public void EnableHitbox()
    {
        isActive = true;
        hitObjects.Clear();
        reported.Clear();

        if (debugLogs)
        {
            Debug.Log($"[Hitbox:{name}] ENABLED (an animation event called EnableHitbox)", this);
        }
    }

    public void DisableHitbox()
    {
        isActive = false;

        if (debugLogs)
        {
            Debug.Log($"[Hitbox:{name}] disabled", this);
        }
    }

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    // Debug only: says what happened with each collider the weapon touched (once per swing)
    private void Report(Collider other, string outcome)
    {
        if (!debugLogs)
            return;

        if (reported.Add(other))
        {
            Debug.Log($"[Hitbox:{name}] touching '{other.name}' -> {outcome}", this);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isActive)
            return;

        // Ignore the player and anything else belonging to the same root as the hitbox.
        if (other.transform.root == transform.root)
        {
            Report(other, $"ignored: same root as the weapon's owner ('{transform.root.name}')");
            return;
        }

        IDamageable damageable = other.GetComponentInParent<IDamageable>();

        if (damageable == null)
        {
            Report(other, "ignored: no IDamageable on it or its parents");
            return;
        }

        GameObject target = ((MonoBehaviour)damageable).gameObject;

        if (hitObjects.Contains(target))
            return;

        hitObjects.Add(target);

        // CHANGED: the old version looked up PlayerHealth and PlayerController separately.
        // Now it just asks the PlayerHealth if the player is blocking.
        if (damageable is PlayerHealth playerHealth && playerHealth.IsBlocking)
        {
            Report(other, $"BLOCKED by '{target.name}'");

            playerHealth.TakeBlockedHit(gameObject);

            // NEW: a shorter freeze on a blocked hit
            HitStop.Trigger(hitStopDuration * 0.5f);
            return;
        }

        Report(other, $"HIT '{target.name}' for {damage} damage");

        damageable.TakeDamage(damage);

        // NEW: push the target away from whoever swung the weapon (only things that
        // implement IKnockbackable are pushed, e.g. enemies; the player is not affected).
        IKnockbackable knockbackable = other.GetComponentInParent<IKnockbackable>();
        if (knockbackable != null && knockbackForce > 0f)
        {
            Vector3 pushDirection = target.transform.position - transform.root.position;
            knockbackable.ApplyKnockback(pushDirection, knockbackForce);
        }

        // NEW: freeze-frame on impact
        HitStop.Trigger(hitStopDuration);
    }
}