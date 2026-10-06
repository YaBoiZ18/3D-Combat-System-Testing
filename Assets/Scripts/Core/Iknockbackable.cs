using UnityEngine;

// NEW: anything that can be pushed back by a hit implements this.
// It's a separate interface on purpose, so IDamageable (and every script that
// implements it) doesn't have to change.
public interface IKnockbackable
{
    // direction: which way to push (the height is ignored). force: how hard.
    void ApplyKnockback(Vector3 direction, float force);
}