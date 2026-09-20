using UnityEngine;

public abstract class EnemyState // Base class for enemy states
{
    protected EnemyController enemy;

    public EnemyState(EnemyController enemy)
    {
        this.enemy = enemy;
    }

    public virtual void Enter() { } // Called when the state is entered

    public virtual void Exit() { } // Called when the state is exited

    public abstract void Update(); // Called every frame
}
