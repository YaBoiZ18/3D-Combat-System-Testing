using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private GameObject swordHip;
    [SerializeField] private GameObject swordHand;

    public void EquipWeapon()
    {
        swordHip.SetActive(false);
        swordHand.SetActive(true);
    }

    public void UnequipWeapon()
    {
        swordHand.SetActive(false);
        swordHip.SetActive(true);
    }
}
