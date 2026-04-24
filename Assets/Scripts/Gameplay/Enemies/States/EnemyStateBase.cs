using UnityEngine;

public abstract class EnemyStateBase : IEnemyState
{
    protected EnemyStateBase(EnemyContext context, EnemyStateMachine stateMachine)
    {
        Context = context;
        StateMachine = stateMachine;
    }

    protected EnemyContext Context { get; }
    protected EnemyStateMachine StateMachine { get; }

    public virtual void Enter()
    {
    }

    public virtual void Exit()
    {
    }

    public virtual void LogicUpdate()
    {
    }

    public virtual void PhysicsUpdate()
    {
    }

    protected void FacePlayer(float deltaTime)
    {
        Context.FaceDirection(Context.GetDirectionToPlayer(), deltaTime);
    }
}
