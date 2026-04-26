using UnityEngine;

public abstract class AbstractBossState : IBossState
{
    protected AbstractBossState(BossContext context, BossStateMachine stateMachine)
    {
        Context = context;
        StateMachine = stateMachine;
    }

    protected BossContext Context { get; }
    protected BossController Boss => Context.Controller;
    protected BossStateMachine StateMachine { get; }

    public virtual void Enter()
    {
    }

    public virtual void Exit()
    {
    }

    public virtual void Tick()
    {
    }

    public virtual void FixedTick()
    {
    }

    protected bool TryEnterDeath()
    {
        if (!Context.IsDead)
            return false;

        StateMachine.ChangeState(Boss.DeathState, "Boss health depleted");
        return true;
    }

    protected bool TryEnterEnrage()
    {
        if (!Context.ShouldEnterEnrage)
            return false;

        StateMachine.ChangeState(Boss.EnrageState, "Boss HP dropped below enrage threshold");
        return true;
    }

    protected bool TryEnterHeal()
    {
        if (!Context.CanEnterHeal)
            return false;

        StateMachine.ChangeState(Boss.HealState, "Boss HP dropped below heal threshold");
        return true;
    }

    protected void FaceTarget(float deltaTime)
    {
        Vector3 direction = Context.DirectionToTarget;
        if (direction != Vector3.zero)
            Boss.FaceDirection(direction, deltaTime);
    }
}
