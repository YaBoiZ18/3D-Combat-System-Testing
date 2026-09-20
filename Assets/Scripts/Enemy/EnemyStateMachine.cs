using UnityEngine;

public class EnemyStateMachine
{
    // The current state of the enemy
    public EnemyState CurrentState { get; private set; }

    // Initialize the state machine with a starting state
    public void Initialize(EnemyState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    // Change the current state to a new state
    public void ChangeState(EnemyState newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    // Update the current state
    public void Update()
    {
        CurrentState?.Update();
    }
}
