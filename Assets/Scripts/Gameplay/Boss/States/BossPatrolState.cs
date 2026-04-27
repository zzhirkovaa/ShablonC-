public sealed class BossPatrolState : AbstractBossState
{
    public BossPatrolState(BossContext context, BossStateMachine stateMachine) : base(context, stateMachine)
    {
    }

    public override void Enter()
    {
        StateMachine.ChangeState(Boss.IdleState, "Patrol is disabled for the giant boss");
    }
}
