public sealed class BossRageState : AbstractBossState
{
    public BossRageState(BossContext context, BossStateMachine stateMachine) : base(context, stateMachine)
    {
    }

    public override void Enter()
    {
        Boss.EnterPhaseTwo();
        StateMachine.ChangeState(Boss.AggroState, "Enrage transition handled by BossController");
    }
}
