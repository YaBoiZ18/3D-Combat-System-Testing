using UnityEngine;

public class AnimationEventReceiver : MonoBehaviour
{
    private PlayerController player;

    private void Awake()
    {
        player = GetComponentInParent<PlayerController>();
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
}
