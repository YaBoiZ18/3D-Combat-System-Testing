using UnityEngine;

public class PlayerStateMachine
{
    // Reference to the current state of the player
    public PlayerState CurrentState { get; private set; }

    // Initialize the state machine with a starting state
    public void Initialize(PlayerState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    // Change the current state to a new state
    public void ChangeState(PlayerState newState)
    {
        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    // Update the current state (called every frame)
    public void Update()
    {
        CurrentState?.Update();
    }
}