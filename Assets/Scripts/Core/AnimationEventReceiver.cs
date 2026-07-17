using UnityEngine;

public class AnimationEventReceiver : MonoBehaviour
{
    private PlayerController player;
    private WeaponController weapons;
    private CombatController combat;

    private void Awake()
    {
        player = GetComponentInParent<PlayerController>();
        weapons = GetComponentInParent<WeaponController>();
        combat = GetComponentInParent<CombatController>();
    }

    // This method is called when the sword drawing animation is finished.
    public void FinishDrawingSword()
    {
        player.FinishDrawingSword();
    }

    // This method is called when the sword sheathing animation is finished.
    public void FinishSheathingSword()
    {
        player.FinishSheathingSword();
    }

    public void EquipWeapon()
    {
        weapons.EquipWeapon();
    }

    public void UnequipWeapon()
    {
        weapons.UnequipWeapon();
    }

    public void EnableCombo()
    {
        combat.EnableCombo();
    }


    public void DisableCombo()
    {
        combat.DisableCombo();
    }


    public void EndAttack()
    {
        combat.EndAttack();
    }
}
