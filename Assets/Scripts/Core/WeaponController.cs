using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private GameObject swordHip;
    [SerializeField] private GameObject swordHand;

    // Called by AnimationEventReceiver when the sword draw animation completes.
    public void EquipWeapon()
    {
        swordHip.SetActive(false);
        swordHand.SetActive(true);
    }

    // Called by AnimationEventReceiver when the sword sheath animation completes.
    public void UnequipWeapon()
    {
        swordHand.SetActive(false);
        swordHip.SetActive(true);
    }
}
