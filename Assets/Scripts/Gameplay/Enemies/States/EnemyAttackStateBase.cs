using UnityEngine;

public abstract class EnemyAttackStateBase : EnemyStateBase
{
    protected EnemyAttackStateBase(EnemyContext context, EnemyStateMachine stateMachine)
        : base(context, stateMachine)
    {
    }

    public override void Enter()
    {
        Context.SetAnimatorBool("IsRunning", false);
        Context.SetAnimatorBool("IsFleeing", false);
        TryStartAttack();
    }

    public override void LogicUpdate()
    {
        if (!Context.HasPlayer)
        {
            StateMachine.ChangeState(Context.IdleState, "Lost player reference during attack");
            return;
        }

        if (Context.ShouldEnterFlee)
        {
            StateMachine.ChangeState(Context.FleeState, Context.GetFleeReasonLabel());
            return;
        }

        FacePlayer(Time.deltaTime);

        if (!Context.IsPlayerInAttackRange)
        {
            StateMachine.ChangeState(Context.AggressionState, "Player left attack range");
            return;
        }

        TryStartAttack();
    }

    public override void PhysicsUpdate()
    {
        Context.StopMotion();
    }

    protected void TryStartAttack()
    {
        if (Context.IsAnimatorStatePlaying("Attack"))
            return;

        if (!CanAttack())
            return;

        Context.FacePlayerImmediately();
        Context.SetAnimatorTrigger("Attack");
        CommitAttack();
    }

    protected abstract bool CanAttack();
    protected abstract void CommitAttack();
}
