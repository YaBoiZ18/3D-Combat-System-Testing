using UnityEngine;

public abstract class PlayerState
{
    // Reference to the player controller
    protected PlayerController player;

    // Constructor to initialize the player state with a reference to the player controller
    public PlayerState(PlayerController player)
    {
        this.player = player;
    }

    // Virtual methods to be overridden by derived states
    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }

}
