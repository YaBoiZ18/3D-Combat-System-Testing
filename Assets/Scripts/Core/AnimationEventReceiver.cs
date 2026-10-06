using UnityEngine;

// Receives Unity animation events and forwards them to the appropriate controllers.
public class AnimationEventReceiver : MonoBehaviour
{
    // Cached references to nearby controllers for quick event forwarding.
    private PlayerController player;
    private WeaponController weapons;
    private CombatController combat;
    private DodgeController dodge;

    // Assigned in inspector: the weapon's hitbox component used by attack animations.
    [SerializeField] private WeaponHitbox weaponHitbox;

    private void Awake()
    {
        // Resolve controller references from parent objects.
        player = GetComponentInParent<PlayerController>();
        weapons = GetComponentInParent<WeaponController>();
        dodge = GetComponentInParent<DodgeController>();
        combat = GetComponentInParent<CombatController>();

        Debug.Log("Dodge Controller Found: " + dodge);
    }

    // Called by animation when sword draw animation completes.
    public void FinishDrawingSword()
    {
        player.FinishDrawingSword();
    }

    // Called by animation when sword sheath animation completes.
    public void FinishSheathingSword()
    {
        player.FinishSheathingSword();
    }

    // Called by animation to mark the weapon as equipped (visual + logic).
    public void EquipWeapon()
    {
        weapons.EquipWeapon();
    }

    // Called by animation to mark the weapon as unequipped.
    public void UnequipWeapon()
    {
        weapons.UnequipWeapon();
    }

    // Enable combo chaining during an attack animation.
    public void EnableCombo()
    {
        combat.EnableCombo();
    }

    // Disable combo chaining (used at the end of combo window).
    public void DisableCombo()
    {
        combat.DisableCombo();
    }

    // Signal that the current attack animation has finished.
    public void EndAttack()
    {
        combat.EndAttack();
    }

    // Called at dodge animation end to finalize dodge state.
    public void EndDodge()
    {
        Debug.Log("Animation Event: End Dodge");

        dodge.EndDodge();
    }

    // Enable the weapon hitbox at the correct animation frame.
    public void EnableHitbox()
    {
        weaponHitbox.EnableHitbox();
    }

    // Disable the weapon hitbox after the hit frame passes.
    public void DisableHitbox()
    {
        weaponHitbox.DisableHitbox();
    }

    // Called on enemy attack animations to inform the enemy controller the attack finished.
    public void FinishEnemyAttack()
    {
        EnemyController enemy = GetComponentInParent<EnemyController>();

        if (enemy != null)
        {
            enemy.FinishAttack();
        }
    }

    // Called when player hit animation finishes to reset hit state.
    public void FinishPlayerHit()
    {
        PlayerController player = GetComponentInParent<PlayerController>();

        if (player != null)
            player.FinishHit();
    }

    // Called when guard break animation finishes to restore player state.
    public void FinishGuardBreak()
    {
        PlayerController player = GetComponentInParent<PlayerController>();

        if (player != null)
            player.FinishGuardBreak();
    }
}
