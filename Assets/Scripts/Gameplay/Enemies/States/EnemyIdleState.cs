public sealed class EnemyIdleState : EnemyStateBase
{
    public EnemyIdleState(EnemyContext context, EnemyStateMachine stateMachine)
        : base(context, stateMachine)
    {
    }

    public override void Enter()
    {
        Context.SetAnimatorBool("IsRunning", false);
        Context.SetAnimatorBool("IsFleeing", false);
        Context.ResetAnimatorTrigger("Attack");
        Context.StopMotion();
    }

    public override void Tick()
    {
        if (!Context.HasPlayer)
            return;

        if (Context.ShouldEnterFlee)
        {
            StateMachine.ChangeState(Context.FleeState, Context.GetFleeReasonLabel());
            return;
        }

        if (Context.IsPeacefulMode)
            return;

        if (Context.IsPlayerInAttackRange)
        {
            StateMachine.ChangeState(Context.AttackState, "Player entered attack range from idle");
            return;
        }

        if (Context.HasDetectedPlayer)
            StateMachine.ChangeState(Context.AggressionState, "Player detected from idle");
    }

    public override void FixedTick()
    {
        Context.StopMotion();
    }
}
