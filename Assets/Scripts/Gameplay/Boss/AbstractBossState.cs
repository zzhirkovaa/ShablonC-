using UnityEngine;

// Base state for the boss State pattern implementation.
public abstract class AbstractBossState : IBossState
{
    protected AbstractBossState(BossController boss, BossStateMachine stateMachine)
    {
        Boss = boss;
        StateMachine = stateMachine;
    }

    protected BossController Boss { get; }
    protected BossStateMachine StateMachine { get; }

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

    protected bool TryEnterRage()
    {
        if (!Boss.ShouldEnterRage)
            return false;

        StateMachine.ChangeState(Boss.RageState, "Boss HP dropped below 50%");
        return true;
    }

    protected bool TryEnterDeath()
    {
        if (!Boss.IsDead || ReferenceEquals(StateMachine.CurrentState, Boss.DeathState))
            return false;

        StateMachine.ChangeState(Boss.DeathState, "Boss health depleted");
        return true;
    }
}
